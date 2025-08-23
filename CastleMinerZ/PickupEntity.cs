using System;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Profiling;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class PickupEntity : Entity
	{
		public PickupEntity(InventoryItem item, int pid, int sid, bool dropped, Vector3 pickupLoc)
		{
			this.Item = item;
			this.PickupID = pid;
			this.SpawnerID = sid;
			item.SetLocation(IntVector3.FromVector3(pickupLoc));
			int modelIndex = this.GetModelIndex(item, IntVector3.FromVector3(pickupLoc));
			if (modelIndex > 0)
			{
				item.SetModelNameIndex(modelIndex);
			}
			this._displayEntity = item.CreateEntity(ItemUse.Pickup, false);
			base.Children.Add(this._displayEntity);
			this._pickedUp = false;
			if (CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				this._readyForPickup = !dropped || sid != (int)CastleMinerZGame.Instance.LocalPlayer.Gamer.Id;
			}
			else
			{
				this._readyForPickup = true;
			}
			this._bouncePhase = MathTools.RandomFloat(0f, 3.1415927f);
			this._timeLeft = item.ItemClass.PickupTimeoutLength;
			this.Collider = true;
			base.Physics = new Player.NoMovePhysics(this);
			this.PlayerPhysics.WorldAcceleration = BasicPhysics.Gravity * 0.25f;
		}

		private int GetModelIndex(InventoryItem item, IntVector3 location)
		{
			int modelIndex = 0;
			if (item is DoorInventoryitem)
			{
				Door door = CastleMinerZGame.Instance.CurrentWorld.GetDoor(location);
				if (door != null)
				{
					modelIndex = (int)door.ModelName;
				}
			}
			return Math.Max(0, modelIndex);
		}

		public BasicPhysics PlayerPhysics
		{
			get
			{
				return (BasicPhysics)base.Physics;
			}
		}

		public Vector3 GetActualGraphicPos()
		{
			return base.LocalPosition + this._displayEntity.LocalPosition;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this._displayEntity.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.Down, (float)gameTime.ElapsedGameTime.TotalSeconds) * this._displayEntity.LocalRotation;
			Vector3 v = this._displayEntity.LocalPosition;
			this._bouncePhase += (float)gameTime.ElapsedGameTime.TotalSeconds * 1.5f;
			v.Y = (float)Math.Sin((double)this._bouncePhase) * 0.1f + 0.1f;
			this._displayEntity.LocalPosition = v;
			v = this.PlayerPhysics.LocalVelocity;
			if (this.OnGround)
			{
				v.X *= 0.9f;
				v.Z *= 0.9f;
			}
			else
			{
				v.X *= 0.99f;
				v.Z *= 0.99f;
			}
			if (Math.Abs(v.X) < 0.1f)
			{
				v.X = 0f;
			}
			if (Math.Abs(v.Z) < 0.1f)
			{
				v.Z = 0f;
			}
			this.PlayerPhysics.LocalVelocity = v;
			this._timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (this._timeLeft <= 0f)
			{
				PickupManager.Instance.RemovePickup(this);
				return;
			}
			if (!BlockTerrain.Instance.RegionIsLoaded(base.LocalPosition))
			{
				this.PlayerPhysics.LocalVelocity = Vector3.Zero;
				this.PlayerPhysics.WorldVelocity = Vector3.Zero;
				this.Visible = false;
			}
			else if (this._timeLeft < 5f)
			{
				this.Visible = ((int)Math.Floor((double)(this._timeLeft * 8f)) & 1) == 0;
			}
			else
			{
				this.Visible = true;
			}
			if (!CastleMinerZGame.Instance.LocalPlayer.ValidLivingGamer)
			{
				this._readyForPickup = true;
			}
			else if (!this._pickedUp)
			{
				Player p = CastleMinerZGame.Instance.LocalPlayer;
				Vector3 pp = p.LocalPosition;
				pp.Y += 1f;
				if (!this._readyForPickup && Vector3.DistanceSquared(pp, base.LocalPosition) > 4f)
				{
					this._readyForPickup = true;
				}
				if (this._readyForPickup && Vector3.DistanceSquared(pp, base.LocalPosition) < 4f)
				{
					this._pickedUp = true;
					PickupManager.Instance.PlayerTouchedPickup(this);
				}
			}
			base.OnUpdate(gameTime);
		}

		public override bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			bool flag;
			using (Profiler.TimeSection("Pickup Collision", ProfilerThreadEnum.MAIN))
			{
				base.ResolveCollsion(e, out collsionPlane, dt);
				bool result = false;
				if (e == BlockTerrain.Instance)
				{
					float t = (float)dt.ElapsedGameTime.TotalSeconds;
					Vector3 worldPos = base.WorldPosition;
					Vector3 nextPos = worldPos;
					Vector3 velocity = this.PlayerPhysics.WorldVelocity;
					this.OnGround = false;
					this.MovementProbe.SkipEmbedded = true;
					int iteration = 0;
					for (;;)
					{
						Vector3 prevPos = nextPos;
						Vector3 movement = Vector3.Multiply(velocity, t);
						nextPos += movement;
						this.MovementProbe.Init(prevPos, nextPos, this.CollisionAABB);
						BlockTerrain.Instance.Trace(this.MovementProbe);
						if (this.MovementProbe._collides)
						{
							result = true;
							if (this.MovementProbe._inFace == BlockFace.POSY)
							{
								this.OnGround = true;
							}
							if (this.MovementProbe._startsIn)
							{
								goto IL_0189;
							}
							float realt = Math.Max(this.MovementProbe._inT - 0.001f, 0f);
							nextPos = prevPos + movement * realt;
							velocity -= Vector3.Multiply(this.MovementProbe._inNormal, Vector3.Dot(this.MovementProbe._inNormal, velocity));
							t *= 1f - realt;
							if (t <= 1E-07f)
							{
								goto IL_0189;
							}
							if (velocity.LengthSquared() <= 1E-06f || Vector3.Dot(this.PlayerPhysics.WorldVelocity, velocity) <= 1E-06f)
							{
								break;
							}
						}
						iteration++;
						if (!this.MovementProbe._collides || iteration >= 4)
						{
							goto IL_0189;
						}
					}
					velocity = Vector3.Zero;
					IL_0189:
					if (iteration == 4)
					{
						velocity = Vector3.Zero;
					}
					base.LocalPosition = nextPos;
					this.PlayerPhysics.WorldVelocity = velocity;
					nextPos.Y += 1.2f;
				}
				flag = result;
			}
			return flag;
		}

		private const float PickupRadSqu = 4f;

		private Entity _displayEntity;

		public InventoryItem Item;

		public int PickupID;

		public int SpawnerID;

		private float _timeLeft;

		private float _bouncePhase;

		private bool _pickedUp;

		private bool _readyForPickup;

		public AABBTraceProbe MovementProbe = new AABBTraceProbe();

		public BoundingBox CollisionAABB = new BoundingBox(new Vector3(-0.3f, -0.3f, -0.3f), new Vector3(0.3f, 0.3f, 0.3f));

		public bool OnGround;
	}
}
