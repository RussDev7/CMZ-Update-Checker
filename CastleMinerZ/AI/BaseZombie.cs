using System;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Animation;
using DNA.Drawing.Effects;
using DNA.Net.GamerServices;
using DNA.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class BaseZombie : SkinnedModelEntity, IShootableEnemy
	{
		public BasicPhysics PlayerPhysics
		{
			get
			{
				return (BasicPhysics)base.Physics;
			}
		}

		public BaseZombie(EnemyManager mgr, EnemyTypeEnum et, Player target, Vector3 pos, int id, int seed, EnemyType.InitPackage initpkg)
			: base(EnemyType.GetEnemyType(et).Model)
		{
			this.Rnd = new Random(seed);
			this.Target = target;
			this.StateMachine = new StateMachine<BaseZombie>(this);
			this._mgr = mgr;
			this.EType = EnemyType.GetEnemyType(et);
			this.CurrentPlayer = null;
			base.LocalRotation = Quaternion.Identity;
			base.LocalPosition = pos;
			base.LocalScale = new Vector3(this.EType.Scale);
			this.EnemyID = id;
			this.IsBlocking = false;
			this.IsHittable = false;
			this.Health = this.EType.StartingHealth;
			this.PlayerDistanceLimit = this.EType.StartingDistanceLimit;
			this.IsLocalEnemy = target == CastleMinerZGame.Instance.LocalPlayer;
			this.DrawPriority = (int)(501 + this.EType.TextureIndex);
			this.Collider = true;
			base.Physics = new Player.NoMovePhysics(this);
			this.PlayerPhysics.WorldAcceleration = BasicPhysics.Gravity;
			this.SoundUpdateTimer = 0f;
			this.InitPkg = initpkg;
			this.CurrentSpeed = this.InitPkg.SlowSpeed;
			this.TimeLeftTilFast = this.InitPkg.NormalActivationTime;
			this.TimeLeftTilRunFast = this.InitPkg.RunActivationTime;
			this.IsMovingFast = false;
			this.StateMachine.ChangeState(this.EType.GetEmergeState(this));
			this._shadow = new ModelEntity(BaseZombie._shadowModel);
			this._shadow.LocalPosition = new Vector3(0f, 0.05f, 0f);
			this._shadow.BlendState = BlendState.AlphaBlend;
			this._shadow.DepthStencilState = DepthStencilState.DepthRead;
			this._shadow.DrawPriority = 200;
			base.Children.Add(this._shadow);
		}

		public void SetDistanceLimit()
		{
			int num = this.EType.StartingDistanceLimit;
			if (this.SpawnSource != null)
			{
				if (this.SpawnSource.IsHellBlock())
				{
					num = 300;
				}
				else
				{
					num = 100;
				}
			}
			this.PlayerDistanceLimit = num;
		}

		public void ResetFrustration()
		{
			this.FrustrationCount = 2.5f;
		}

		public void SpeedUp()
		{
			if (this.EType.HasRunFast)
			{
				this.IsMovingFast = true;
				this.CurrentSpeed = this.InitPkg.FastSpeed;
				((EnemyBaseState)this.StateMachine._currentState).HandleSpeedUp(this);
			}
		}

		public bool IsNearAnimationEnd
		{
			get
			{
				return (this.CurrentPlayer.Duration - this.CurrentPlayer.CurrentTime).TotalSeconds < 0.25;
			}
		}

		public float TimeToIntercept()
		{
			Vector3 delta = base.WorldPosition - this.Target.WorldPosition;
			delta.Y = 0f;
			float dlsw = delta.LengthSquared();
			if (dlsw < 1f)
			{
				return 0f;
			}
			Vector3 pv = this.Target.PlayerPhysics.WorldVelocity;
			Vector3 dv = pv - this.PlayerPhysics.WorldVelocity;
			dv.Y = 0f;
			if (Vector3.Dot(delta, dv) < 0f)
			{
				return float.MaxValue;
			}
			float speed = dv.LengthSquared();
			if (speed < 0.001f)
			{
				return float.MaxValue;
			}
			speed = (float)Math.Sqrt((double)speed);
			dv *= 1f / speed;
			float distance = (float)Math.Sqrt((double)dlsw);
			delta *= 1f / distance;
			float dot = Vector3.Dot(dv, delta);
			if (dot < 0.01f)
			{
				return float.MaxValue;
			}
			return distance / dot / speed;
		}

		public void Remove()
		{
			this._mgr.RemoveZombie(this);
		}

		public bool IsDead
		{
			get
			{
				return this.StateMachine._currentState == this.EType.GetDieState(this);
			}
		}

		public void Kill()
		{
			if (!this.IsDead)
			{
				this.StateMachine.ChangeState(this.EType.GetDieState(this));
			}
		}

		public void GiveUp()
		{
			if (!this.IsDead && this.StateMachine._currentState != this.EType.GetGiveUpState(this))
			{
				this.StateMachine.ChangeState(this.EType.GetGiveUpState(this));
			}
		}

		public bool IsHeadshot(Vector3 hit)
		{
			return hit.Y - base.LocalPosition.Y > 1.5f;
		}

		public void TakeExplosiveDamage(float damageAmount, byte shooterID, InventoryItemIDs itemID)
		{
			if (this.Health > 0f)
			{
				this.Health -= damageAmount;
				if (this.Health <= 0f)
				{
					if (itemID == InventoryItemIDs.TNT && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance && shooterID == CastleMinerZGame.Instance.LocalPlayer.Gamer.Id)
					{
						CastleMinerZGame.Instance.PlayerStats.EnemiesKilledWithTNT++;
					}
					else if (itemID == InventoryItemIDs.Grenade && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance && shooterID == CastleMinerZGame.Instance.LocalPlayer.Gamer.Id)
					{
						CastleMinerZGame.Instance.PlayerStats.EnemiesKilledWithGrenade++;
					}
					KillEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this.EnemyID, (int)this.Target.Gamer.Id, shooterID, itemID);
					return;
				}
				if (this.StateMachine._currentState != this.EType.GetHitState(this))
				{
					this.StateMachine.ChangeState(this.EType.GetHitState(this));
				}
			}
		}

		public void AttachProjectile(Entity projectile)
		{
			base.AdoptChild(projectile);
		}

		public void TakeDamage(Vector3 damagePosition, Vector3 damageDirection, InventoryItem.InventoryItemClass itemClass, byte shooterID)
		{
			DamageType damageType = itemClass.EnemyDamageType;
			float damageAmount = itemClass.EnemyDamage;
			if (CastleMinerZGame.Instance.IsLocalPlayerId(shooterID))
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(itemClass.ID);
				itemStats.Hits++;
			}
			if (this.Health > 0f)
			{
				float damageMultiplier = this.EType.GetDamageTypeMultiplier(damageType, this.IsHeadshot(damagePosition));
				damageAmount *= damageMultiplier;
				this.Health -= damageAmount;
				if (this.Health <= 0f)
				{
					if (itemClass is LaserGunInventoryItemClass && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance && shooterID == CastleMinerZGame.Instance.LocalPlayer.Gamer.Id)
					{
						CastleMinerZGame.Instance.PlayerStats.EnemiesKilledWithLaserWeapon++;
					}
					KillEnemyMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this.EnemyID, (int)this.Target.Gamer.Id, shooterID, itemClass.ID);
					return;
				}
				if (this.StateMachine._currentState != this.EType.GetHitState(this))
				{
					this.StateMachine.ChangeState(this.EType.GetHitState(this));
				}
			}
		}

		public void CreatePickup()
		{
			if (PickupManager.Instance == null)
			{
				return;
			}
			float dist = base.LocalPosition.Length();
			float blender = (dist / 5000f).Clamp(0f, 1f);
			float dval = MathTools.RandomFloat(blender, 1f);
			InventoryItem item;
			if (this.EType.FoundIn == EnemyType.FoundInEnum.HELL)
			{
				if ((double)dval < 0.5)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1);
				}
				else if ((double)dval < 0.8)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 2);
				}
				else
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 3);
				}
			}
			if (this.EType.FoundIn == EnemyType.FoundInEnum.CRASHSITE)
			{
				if ((double)dval < 0.5)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.Copper, 1);
				}
				else if ((double)dval < 0.8)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.Iron, 1);
				}
				else
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1);
				}
			}
			else
			{
				if ((double)dval < 0.5)
				{
					return;
				}
				bool inhell = base.LocalPosition.Y < -40f;
				if (inhell)
				{
					if ((double)dval < 0.7)
					{
						item = InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 1);
					}
					else if ((double)dval < 0.8)
					{
						item = InventoryItem.CreateItem(InventoryItemIDs.Copper, 1);
					}
					else if ((double)dval < 0.85)
					{
						item = InventoryItem.CreateItem(InventoryItemIDs.Iron, 1);
					}
					else if ((double)dval < 0.9)
					{
						item = InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 1);
					}
					else
					{
						item = InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1);
					}
				}
				else if ((double)dval < 0.7)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1);
				}
				else if ((double)dval < 0.8)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.Coal, 1);
				}
				else if ((double)dval < 0.85)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 1);
				}
				else if ((double)dval < 0.9)
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.Copper, 1);
				}
				else
				{
					item = InventoryItem.CreateItem(InventoryItemIDs.IronOre, 1);
				}
			}
			if (item != null)
			{
				PickupManager.Instance.CreatePickup(item, base.LocalPosition + new Vector3(0f, 1f, 0f), false, false);
			}
		}

		public bool Touches(BoundingBox box)
		{
			BoundingBox bb = this.PlayerAABB;
			Vector3 wp = base.WorldPosition;
			bb.Min += wp;
			bb.Max += wp;
			return bb.Intersects(box);
		}

		public static void Init()
		{
			BaseZombie._shadowModel = CastleMinerZGame.Instance.Content.Load<Model>("Shadow");
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (!BlockTerrain.Instance.RegionIsLoaded(base.LocalPosition))
			{
				this.Remove();
				return;
			}
			Vector3 pos = base.WorldPosition + new Vector3(0f, 1f, 0f);
			this.shadowProbe.Init(pos, base.WorldPosition + new Vector3(0f, -2.5f, 0f));
			this.shadowProbe.SkipEmbedded = true;
			BlockTerrain.Instance.Trace(this.shadowProbe);
			this._shadow.Visible = this.shadowProbe._collides;
			if (this._shadow.Visible)
			{
				Vector3 interSection = this.shadowProbe.GetIntersection();
				Vector3 toGround = interSection - base.WorldPosition;
				float height = Math.Abs(toGround.Y);
				this._shadow.LocalPosition = toGround + new Vector3(0f, 0.05f, 0f);
				int maxHeight = 2;
				float blender = height / (float)maxHeight;
				this._shadow.LocalScale = new Vector3(1f + 2f * blender, 1f, 1f + 2f * blender);
				this._shadow.EntityColor = new Color?(new Color(1f, 1f, 1f, Math.Max(0f, 0.5f * (1f - blender))));
			}
			BlockTerrain.Instance.GetEnemyLighting(pos, ref this.DirectLightDirection[0], ref this.DirectLightColor[0], ref this.DirectLightDirection[1], ref this.DirectLightColor[1], ref this.AmbientLight);
			using (Profiler.TimeSection("Zombie Update", ProfilerThreadEnum.MAIN))
			{
				if (!this.Target.ValidGamer || this.Target.Dead)
				{
					this.GiveUp();
				}
				this.StateMachine.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
				base.OnUpdate(gameTime);
			}
		}

		public override bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			bool flag;
			using (Profiler.TimeSection("Zombie Collision", ProfilerThreadEnum.MAIN))
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
					this.TouchingWall = false;
					this.MovementProbe.SkipEmbedded = true;
					int iteration = 0;
					for (;;)
					{
						Vector3 prevPos = nextPos;
						Vector3 movement = Vector3.Multiply(velocity, t);
						nextPos += movement;
						this.MovementProbe.Init(prevPos, nextPos, this.PlayerAABB);
						BlockTerrain.Instance.Trace(this.MovementProbe);
						if (this.MovementProbe._collides)
						{
							result = true;
							if (this.MovementProbe._inFace == BlockFace.POSY)
							{
								this.OnGround = true;
							}
							else
							{
								this.TouchingWall = true;
							}
							if (this.MovementProbe._startsIn)
							{
								break;
							}
							float realt = Math.Max(this.MovementProbe._inT - 0.001f, 0f);
							nextPos = prevPos + movement * realt;
							velocity -= Vector3.Multiply(this.MovementProbe._inNormal, Vector3.Dot(this.MovementProbe._inNormal, velocity));
							t *= 1f - realt;
							if (t <= 1E-07f)
							{
								goto IL_01A2;
							}
							if (velocity.LengthSquared() <= 1E-06f || Vector3.Dot(this.PlayerPhysics.WorldVelocity, velocity) <= 1E-06f)
							{
								goto IL_017E;
							}
						}
						iteration++;
						if (!this.MovementProbe._collides || iteration >= 4)
						{
							goto IL_01A2;
						}
					}
					this.TouchingWall = true;
					goto IL_01A2;
					IL_017E:
					velocity = Vector3.Zero;
					IL_01A2:
					if (iteration == 4)
					{
						velocity = Vector3.Zero;
					}
					base.LocalPosition = nextPos;
					this.PlayerPhysics.WorldVelocity = velocity;
					this.SoundUpdateTimer -= (float)dt.ElapsedGameTime.TotalSeconds;
					if (this.SoundUpdateTimer < 0.1f)
					{
						this.SoundUpdateTimer += 0.1f;
						this.SoundEmitter.Position = nextPos;
						this.SoundEmitter.Forward = base.LocalToWorld.Forward;
						this.SoundEmitter.Up = Vector3.Up;
						this.SoundEmitter.Velocity = velocity;
					}
					velocity.Y = 0f;
					if (velocity.LengthSquared() < 0.25f)
					{
						this.FrustrationCount -= (float)dt.ElapsedGameTime.TotalSeconds;
					}
					else
					{
						this.ResetFrustration();
					}
					nextPos.Y += 1.2f;
				}
				flag = result;
			}
			return flag;
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is DNAEffect)
			{
				DNAEffect dnaEffect = (DNAEffect)effect;
				if (dnaEffect.Parameters["LightDirection1"] != null)
				{
					dnaEffect.Parameters["LightDirection1"].SetValue(-this.DirectLightDirection[0]);
					dnaEffect.Parameters["LightColor1"].SetValue(this.DirectLightColor[0]);
					dnaEffect.Parameters["LightDirection2"].SetValue(-this.DirectLightDirection[1]);
					dnaEffect.Parameters["LightColor2"].SetValue(this.DirectLightColor[1]);
					dnaEffect.AmbientColor = ColorF.FromVector3(this.AmbientLight);
				}
				dnaEffect.DiffuseMap = this.EType.EnemyTexture;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public StateMachine<BaseZombie> StateMachine;

		public Player Target;

		public Random Rnd;

		public AnimationPlayer CurrentPlayer;

		public bool IsLocalEnemy;

		private EnemyManager _mgr;

		public Spawner SpawnSource;

		public int SpawnValue;

		public int PlayerDistanceLimit = 25;

		public AABBTraceProbe MovementProbe = new AABBTraceProbe();

		public BoundingBox PlayerAABB = new BoundingBox(new Vector3(-0.35f, 0f, -0.35f), new Vector3(0.35f, 1.65f, 0.35f));

		public bool OnGround;

		public bool TouchingWall;

		public AudioEmitter SoundEmitter = new AudioEmitter();

		public SoundCue3D ZombieGrowlCue;

		public EnemyType.InitPackage InitPkg;

		public float TimeLeftTilFast;

		public float TimeLeftTilRunFast;

		public float CurrentSpeed;

		public float FrustrationCount = 15f;

		public float StateTimer;

		public int SwingCount;

		public int AnimationIndex;

		public int HitCount;

		public int MissCount;

		public float SoundUpdateTimer;

		public bool IsBlocking;

		public bool IsHittable;

		public bool IsMovingFast;

		public EnemyType EType;

		public int EnemyID;

		public float Health;

		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Color.Gray.ToVector3();

		private ModelEntity _shadow;

		public static Random _rand = new Random();

		private static Model _shadowModel;

		private TraceProbe shadowProbe = new TraceProbe();
	}
}
