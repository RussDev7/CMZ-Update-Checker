using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class PickupManager : Entity
	{
		public static PickupManager Instance
		{
			get
			{
				return PickupManager._instance.Target;
			}
		}

		public PickupManager()
		{
			this.Visible = false;
			this.Collidee = false;
			this.Collider = false;
			PickupManager._instance.Target = this;
		}

		public void HandleMessage(CastleMinerZMessage message)
		{
			if (message is CreatePickupMessage)
			{
				this.HandleCreatePickupMessage((CreatePickupMessage)message);
				return;
			}
			if (message is ConsumePickupMessage)
			{
				this.HandleConsumePickupMessage((ConsumePickupMessage)message);
				return;
			}
			if (message is RequestPickupMessage)
			{
				this.HandleRequestPickupMessage((RequestPickupMessage)message);
			}
		}

		public void CreateUpwardPickup(InventoryItem item, Vector3 location, float vel, bool displayOnPickup = false)
		{
			Vector3 vec = new Vector3(MathTools.RandomFloat(-0.5f, 0.501f), 0.1f, MathTools.RandomFloat(-0.5f, 0.501f));
			vec.Normalize();
			vec *= vel;
			CreatePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, location, vec, this._nextPickupID++, item, false, displayOnPickup);
		}

		public void CreatePickup(InventoryItem item, Vector3 location, bool dropped, bool displayOnPickup = false)
		{
			if (dropped)
			{
				Player p = CastleMinerZGame.Instance.LocalPlayer;
				Matrix i = p.FPSCamera.LocalToWorld;
				Vector3 vec = i.Forward;
				vec.Y = 0f;
				vec.Normalize();
				vec.Y = 0.1f;
				vec += i.Left * (MathTools.RandomFloat() * 0.25f - 0.12f);
				float vel = 4f;
				vec.Normalize();
				vec *= vel;
				CreatePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, location, vec, this._nextPickupID++, item, dropped, displayOnPickup);
				return;
			}
			this.CreateUpwardPickup(item, location, 1.5f, displayOnPickup);
		}

		public void PlayerTouchedPickup(PickupEntity pickup)
		{
			RequestPickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, pickup.SpawnerID, pickup.PickupID);
		}

		private void HandleCreatePickupMessage(CreatePickupMessage msg)
		{
			int spawnID = (int)msg.Sender.Id;
			PickupEntity entity = new PickupEntity(msg.Item, msg.PickupID, spawnID, msg.Dropped, msg.SpawnPosition);
			entity.Item.DisplayOnPickup = msg.DisplayOnPickup;
			entity.PlayerPhysics.LocalVelocity = msg.SpawnVector;
			entity.LocalPosition = msg.SpawnPosition + new Vector3(0.5f, 0.5f, 0.5f);
			this.Pickups.Add(entity);
			Scene scene = base.Scene;
			if (scene != null && scene.Children != null)
			{
				scene.Children.Add(entity);
			}
		}

		public void RemovePickup(PickupEntity pe)
		{
			this.Pickups.Remove(pe);
			pe.RemoveFromParent();
			if (CastleMinerZGame.Instance.IsGameHost)
			{
				this.PendingPickupList.Remove(pe);
			}
		}

		private void HandleRequestPickupMessage(RequestPickupMessage msg)
		{
			if (!CastleMinerZGame.Instance.IsGameHost)
			{
				return;
			}
			for (int i = 0; i < this.Pickups.Count; i++)
			{
				if (this.Pickups[i].PickupID == msg.PickupID && this.Pickups[i].SpawnerID == msg.SpawnerID)
				{
					PickupEntity pickup = this.Pickups[i];
					if (!this.PendingPickupList.Contains(pickup))
					{
						this.PendingPickupList.Add(pickup);
						ConsumePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, msg.Sender.Id, pickup.GetActualGraphicPos(), pickup.SpawnerID, pickup.PickupID, pickup.Item, pickup.Item.DisplayOnPickup);
						return;
					}
				}
			}
		}

		private void HandleConsumePickupMessage(ConsumePickupMessage msg)
		{
			Vector3 position = Vector3.Zero;
			PickupEntity pe = null;
			Player player = null;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer nwg = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (nwg != null && nwg.Id == msg.PickerUpper)
					{
						Player p = (Player)nwg.Tag;
						if (p != null)
						{
							player = p;
						}
					}
				}
			}
			for (int j = 0; j < this.Pickups.Count; j++)
			{
				if (this.Pickups[j].PickupID == msg.PickupID && this.Pickups[j].SpawnerID == msg.SpawnerID)
				{
					pe = this.Pickups[j];
					this.RemovePickup(pe);
				}
			}
			if (pe != null)
			{
				position = pe.GetActualGraphicPos();
			}
			else
			{
				position = msg.PickupPosition;
			}
			if (player != null)
			{
				if (player == CastleMinerZGame.Instance.LocalPlayer)
				{
					CastleMinerZGame.Instance.GameScreen.HUD.PlayerInventory.AddInventoryItem(msg.Item, msg.DisplayOnPickup);
					SoundManager.Instance.PlayInstance("pickupitem");
				}
				FlyingPickupEntity fpe = new FlyingPickupEntity(msg.Item, player, position);
				Scene scene = base.Scene;
				if (scene != null && scene.Children != null)
				{
					scene.Children.Add(fpe);
				}
			}
		}

		private static WeakReference<PickupManager> _instance = new WeakReference<PickupManager>(null);

		private int _nextPickupID;

		public List<PickupEntity> Pickups = new List<PickupEntity>();

		public List<PickupEntity> PendingPickupList = new List<PickupEntity>();
	}
}
