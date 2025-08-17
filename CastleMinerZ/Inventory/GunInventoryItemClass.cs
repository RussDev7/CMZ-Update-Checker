using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class GunInventoryItemClass : ModelInventoryItemClass
	{
		public string ReloadSound
		{
			get
			{
				return this._reloadSound;
			}
		}

		public TimeSpan FireRate
		{
			get
			{
				return this._coolDownTime;
			}
		}

		public GunInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, TimeSpan fireRate, ToolMaterialTypes material, float bulletDamage, float itemSelfDamage, InventoryItem.InventoryItemClass ammoClass, string shootSound, string reloadSound)
			: base(id, model, name, description, 1, fireRate, CMZColors.GetMaterialcColor(material), shootSound)
		{
			this._reloadSound = reloadSound;
			this.AmmoType = ammoClass;
			this.Material = material;
			this.ItemSelfDamagePerUse = itemSelfDamage;
			this.TracerColor = CMZColors.GetMaterialcColor(this.Material).ToVector4();
			this.ToolColor = CMZColors.GetMaterialcColor(this.Material);
			this._playerMode = PlayerMode.Assault;
			this.EnemyDamage = bulletDamage;
			this.EnemyDamageType = DamageType.BULLET;
		}

		public override bool IsMeleeWeapon
		{
			get
			{
				return false;
			}
		}

		public virtual bool NeedsDropCompensation
		{
			get
			{
				return true;
			}
		}

		public int AmmoCount(PlayerInventory inventory)
		{
			if (this.AmmoType == null)
			{
				return 1000;
			}
			return inventory.CountItems(this.AmmoType);
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new GunInventoryItem(this, stackCount);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			GunEntity gunEntity = new GunEntity(this._model, use, attachedToLocalPlayer);
			gunEntity.EnablePerPixelLighting();
			gunEntity.ToolColor = this.ToolColor;
			gunEntity.EmissiveColor = this.EmissiveColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.7853982f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(32f / gunEntity.GetLocalBoundingSphere().Radius), quaternion);
				gunEntity.LocalToParent = matrix;
				gunEntity.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				gunEntity.EnablePerPixelLighting();
				break;
			case ItemUse.Pickup:
				gunEntity.EnablePerPixelLighting();
				break;
			}
			return gunEntity;
		}

		private string _reloadSound;

		public ToolMaterialTypes Material;

		public TimeSpan ReloadTime = TimeSpan.FromSeconds(3.0);

		public Angle Recoil = Angle.FromDegrees(3f);

		public bool Automatic;

		public int RoundsPerReload = 32;

		public int ClipCapacity = 32;

		public float ShoulderMagnification = 1f;

		public bool Scoped;

		public Angle ShoulderedMinAccuracy = Angle.FromDegrees(0.625);

		public Angle ShoulderedMaxAccuracy = Angle.FromDegrees(1.25);

		public Angle MinInnaccuracy = Angle.FromDegrees(1.25);

		public Angle MaxInnaccuracy = Angle.FromDegrees(2.5);

		public float InnaccuracySpeed = 5f;

		public float FlightTime = 2f;

		public float Velocity = 100f;

		public Vector4 TracerColor = Color.White.ToVector4();

		public InventoryItem.InventoryItemClass AmmoType;
	}
}
