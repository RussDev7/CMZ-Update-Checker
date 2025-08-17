using System;
using System.IO;
using System.Text;
using DNA.CastleMinerZ.UI;
using DNA.Text;

namespace DNA.CastleMinerZ.Inventory
{
	public class GunInventoryItem : InventoryItem
	{
		public string CurrentAmmoName
		{
			get
			{
				return this._currentAmmo;
			}
		}

		public int RoundsInClip
		{
			get
			{
				return this._roundsInClip;
			}
			set
			{
				this._roundsInClip = value;
			}
		}

		public int AmmoCount
		{
			get
			{
				return this._ammoCount;
			}
		}

		public override void GetDisplayText(StringBuilder builder)
		{
			base.GetDisplayText(builder);
			if (base.ItemClass is GrenadeLauncherBaseInventoryItemClass)
			{
				builder.Append(" ");
				builder.Concat(this.RoundsInClip);
				builder.Append("/");
				builder.Concat(this.AmmoCount);
				builder.Append(" ");
				builder.Append(this.CurrentAmmoName);
				return;
			}
			if (!(base.ItemClass is RocketLauncherBaseInventoryItemClass))
			{
				builder.Append(" ");
				builder.Concat(this.RoundsInClip);
				builder.Append("/");
				builder.Concat(this.AmmoCount);
			}
		}

		public GunInventoryItemClass GunClass
		{
			get
			{
				return (GunInventoryItemClass)base.ItemClass;
			}
		}

		public GunInventoryItem(GunInventoryItemClass classtype, int stackCount)
			: base(classtype, stackCount)
		{
			this.RoundsInClip = classtype.ClipCapacity;
		}

		protected override void Read(BinaryReader reader)
		{
			base.Read(reader);
			this.RoundsInClip = reader.ReadInt32();
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(this.RoundsInClip);
		}

		public bool Reload(InGameHUD hud)
		{
			this._ammoCount = this.GunClass.AmmoCount(hud.PlayerInventory);
			int num = this.GunClass.ClipCapacity - this.RoundsInClip;
			if (num > this._ammoCount)
			{
				num = this._ammoCount;
			}
			if (num > this.GunClass.RoundsPerReload)
			{
				num = this.GunClass.RoundsPerReload;
			}
			if (num <= 0)
			{
				return false;
			}
			if (hud.PlayerInventory.Consume(this.GunClass.AmmoType, num))
			{
				this.RoundsInClip += num;
				this._ammoCount -= num;
			}
			return this.RoundsInClip < this.GunClass.ClipCapacity && this._ammoCount > 0;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			hud.LocalPlayer.Shouldering = controller.Shoulder.Held;
			this._ammoCount = this.GunClass.AmmoCount(hud.PlayerInventory);
			bool flag = this.RoundsInClip < this.GunClass.ClipCapacity && this._ammoCount > 0;
			if (flag && controller.Reload.Pressed)
			{
				this.gunReleased = !controller.Use.Held;
				hud.LocalPlayer.Reloading = true;
			}
			if (this.gunReleased && controller.Use.Pressed)
			{
				hud.LocalPlayer.Reloading = false;
			}
			if (!hud.LocalPlayer.Reloading && base.CoolDownTimer.Expired && ((controller.Use.Held && this.GunClass.Automatic) || controller.Use.Pressed))
			{
				RocketLauncherGuidedInventoryItemClass rocketLauncherGuidedInventoryItemClass = base.ItemClass as RocketLauncherGuidedInventoryItemClass;
				if (this.RoundsInClip > 0 && (rocketLauncherGuidedInventoryItemClass == null || rocketLauncherGuidedInventoryItemClass.LockedOnToDragon))
				{
					if (rocketLauncherGuidedInventoryItemClass != null)
					{
						rocketLauncherGuidedInventoryItemClass.StopSound();
					}
					hud.LocalPlayer.Reloading = false;
					hud.Shoot((GunInventoryItemClass)base.ItemClass);
					base.CoolDownTimer.Reset();
					this.RoundsInClip--;
					hud.LocalPlayer.UsingTool = true;
					CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(base.ItemClass.ID);
					itemStats.Used++;
					hud.LocalPlayer.ApplyRecoil(this.GunClass.Recoil);
					if (this.InflictDamage())
					{
						hud.PlayerInventory.Remove(this);
					}
					if (this.RoundsInClip <= 0 && this._ammoCount > 0)
					{
						hud.LocalPlayer.Reloading = true;
					}
					return;
				}
				if (flag)
				{
					hud.LocalPlayer.Reloading = true;
				}
			}
			hud.LocalPlayer.UsingTool = false;
		}

		private string _currentAmmo = "Sticky Grenade";

		private int _roundsInClip;

		private int _ammoCount;

		private bool gunReleased;
	}
}
