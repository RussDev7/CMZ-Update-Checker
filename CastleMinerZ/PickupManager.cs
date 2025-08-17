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
			Vector3 vector = new Vector3(MathTools.RandomFloat(-0.5f, 0.501f), 0.1f, MathTools.RandomFloat(-0.5f, 0.501f));
			vector.Normalize();
			vector *= vel;
			CreatePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, location, vector, this._nextPickupID++, item, false, displayOnPickup);
		}

		public void CreatePickup(InventoryItem item, Vector3 location, bool dropped, bool displayOnPickup = false)
		{
			if (dropped)
			{
				Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
				Matrix localToWorld = localPlayer.FPSCamera.LocalToWorld;
				Vector3 vector = localToWorld.Forward;
				vector.Y = 0f;
				vector.Normalize();
				vector.Y = 0.1f;
				vector += localToWorld.Left * (MathTools.RandomFloat() * 0.25f - 0.12f);
				float num = 4f;
				vector.Normalize();
				vector *= num;
				CreatePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, location, vector, this._nextPickupID++, item, dropped, displayOnPickup);
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
			int id = (int)msg.Sender.Id;
			PickupEntity pickupEntity = new PickupEntity(msg.Item, msg.PickupID, id, msg.Dropped, msg.SpawnPosition);
			pickupEntity.Item.DisplayOnPickup = msg.DisplayOnPickup;
			pickupEntity.PlayerPhysics.LocalVelocity = msg.SpawnVector;
			pickupEntity.LocalPosition = msg.SpawnPosition + new Vector3(0.5f, 0.5f, 0.5f);
			this.Pickups.Add(pickupEntity);
			Scene scene = base.Scene;
			if (scene != null && scene.Children != null)
			{
				scene.Children.Add(pickupEntity);
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
					PickupEntity pickupEntity = this.Pickups[i];
					if (!this.PendingPickupList.Contains(pickupEntity))
					{
						this.PendingPickupList.Add(pickupEntity);
						ConsumePickupMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, msg.Sender.Id, pickupEntity.GetActualGraphicPos(), pickupEntity.SpawnerID, pickupEntity.PickupID, pickupEntity.Item, pickupEntity.Item.DisplayOnPickup);
						return;
					}
				}
			}
		}

		private void HandleConsumePickupMessage(ConsumePickupMessage msg)
		{
			Vector3 vector = Vector3.Zero;
			PickupEntity pickupEntity = null;
			Player player = null;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (networkGamer != null && networkGamer.Id == msg.PickerUpper)
					{
						Player player2 = (Player)networkGamer.Tag;
						if (player2 != null)
						{
							player = player2;
						}
					}
				}
			}
			for (int j = 0; j < this.Pickups.Count; j++)
			{
				if (this.Pickups[j].PickupID == msg.PickupID && this.Pickups[j].SpawnerID == msg.SpawnerID)
				{
					pickupEntity = this.Pickups[j];
					this.RemovePickup(pickupEntity);
				}
			}
			if (pickupEntity != null)
			{
				vector = pickupEntity.GetActualGraphicPos();
			}
			else
			{
				vector = msg.PickupPosition;
			}
			if (player != null)
			{
				if (player == CastleMinerZGame.Instance.LocalPlayer)
				{
					CastleMinerZGame.Instance.GameScreen.HUD.PlayerInventory.AddInventoryItem(msg.Item, msg.DisplayOnPickup);
					SoundManager.Instance.PlayInstance("pickupitem");
				}
				FlyingPickupEntity flyingPickupEntity = new FlyingPickupEntity(msg.Item, player, vector);
				Scene scene = base.Scene;
				if (scene != null && scene.Children != null)
				{
					scene.Children.Add(flyingPickupEntity);
				}
			}
		}

		private static WeakReference<PickupManager> _instance = new WeakReference<PickupManager>(null);

		private int _nextPickupID;

		public List<PickupEntity> Pickups = new List<PickupEntity>();

		public List<PickupEntity> PendingPickupList = new List<PickupEntity>();
	}
}
