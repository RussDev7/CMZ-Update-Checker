using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Animation;
using DNA.Drawing.Particles;
using DNA.Net.GamerServices;
using DNA.Profiling;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class DragonClientEntity : Entity, IShootableEnemy
	{
		public static void Init()
		{
			DragonClientEntity.ParticlePackages = new DragonClientEntity.ParticlePackage[2];
			DragonClientEntity.ParticlePackages[0]._flashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FlashEffect");
			DragonClientEntity.ParticlePackages[0]._firePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FirePuff");
			DragonClientEntity.ParticlePackages[0]._smokePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigSmokePuff");
			DragonClientEntity.ParticlePackages[1]._flashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\IceFlash");
			DragonClientEntity.ParticlePackages[1]._firePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\IcePuff");
			DragonClientEntity.ParticlePackages[1]._smokePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigPuff");
			DragonClientEntity.DeathAnimationPackages = new DragonClientEntity.DeathAnimationPackage[2];
			DragonClientEntity.DeathAnimationPackages[0].Name = "death_air_1";
			DragonClientEntity.DeathAnimationPackages[0].BeginPositionChangeTime = 1f;
			DragonClientEntity.DeathAnimationPackages[0].PauseTime = 1.3666667f;
			DragonClientEntity.DeathAnimationPackages[0].TimeToWaitAfterStop = 5f;
			DragonClientEntity.DeathAnimationPackages[0].BoxOffset = new Vector3(0f, 0f, 7f);
			DragonClientEntity.DeathAnimationPackages[1].Name = "death_air_2";
			DragonClientEntity.DeathAnimationPackages[1].BeginPositionChangeTime = 0.9f;
			DragonClientEntity.DeathAnimationPackages[1].PauseTime = 1.4f;
			DragonClientEntity.DeathAnimationPackages[1].TimeToWaitAfterStop = 5f;
			DragonClientEntity.DeathAnimationPackages[1].BoxOffset = new Vector3(0f, 0f, -3.5f);
			DragonClientEntity.DragonBody = CastleMinerZGame.Instance.Content.Load<Model>("Enemies\\Dragon\\DragonBodyHigh");
			DragonClientEntity.DragonFeet = CastleMinerZGame.Instance.Content.Load<Model>("Enemies\\Dragon\\DragonFeetHigh");
		}

		public DragonClientEntity(DragonTypeEnum type, float health)
		{
			DragonPartEntity[] array = new DragonPartEntity[2];
			this._dragonModel = array;
			AnimationPlayer[] array2 = new AnimationPlayer[2];
			this.CurrentPlayer = array2;
			this.Waypoints = new List<ActionDragonWaypoint>();
			this.NextFireballIndex = new List<int>();
			this.SoundEmitter = new AudioEmitter();
			this.FlapTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));
			this._dragonProbe = new TraceProbe();
			base..ctor();
			this.EType = DragonType.GetDragonType(type);
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f);
			base.LocalPosition = Vector3.Zero;
			this.Health = ((health == -1f) ? this.EType.StartingHealth : health);
			this.DeathState = DragonClientEntity.DeathAnimationState.STILL_ALIVE;
			this.DrawPriority = (int)(520 + type);
			this._dragonModel[0] = new DragonPartEntity(this.EType, DragonClientEntity.DragonBody);
			this._dragonModel[1] = new DragonPartEntity(this.EType, DragonClientEntity.DragonFeet);
			this.CurrentInterpolationTime = 0f;
			base.Children.Add(this._dragonModel[0]);
			base.Children.Add(this._dragonModel[1]);
			this.OnGround = false;
			this.GotInitialWaypoints = false;
			this.DrawPriority = (int)(520 + type);
			this.Collider = false;
			this.Visible = false;
			this.HeadHitVolume = new Plane[6];
			this.BodyHitVolume = new Plane[6];
			TraceProbe.MakeOrientedBox(base.LocalToWorld, DragonClientEntity.HeadBox, this.HeadHitVolume);
			TraceProbe.MakeOrientedBox(base.LocalToWorld, DragonClientEntity.BodyBox, this.BodyHitVolume);
			this.TimeoutTimer = 0f;
			this.WaitingToShoot = false;
			this.SpawnPickups = false;
		}

		public void AddActionWaypoint(ActionDragonWaypoint inwpt)
		{
			this.TimeoutTimer = 0f;
			for (int i = 0; i < this.Waypoints.Count; i++)
			{
				if (this.Waypoints[i].BaseWpt.HostTime > inwpt.BaseWpt.HostTime)
				{
					if (i >= 2)
					{
						this.Waypoints.Insert(i, inwpt);
					}
					return;
				}
			}
			if (this.Waypoints.Count > 0 && this.CurrentInterpolationTime > this.Waypoints[this.Waypoints.Count - 1].BaseWpt.HostTime)
			{
				this.Waypoints.Clear();
				ActionDragonWaypoint wpt = default(ActionDragonWaypoint);
				wpt.Action = DragonWaypointActionEnum.GOTO;
				wpt.BaseWpt.Position = base.WorldPosition;
				wpt.BaseWpt.Velocity = this.CurrentVelocity;
				wpt.BaseWpt.HostTime = this.CurrentInterpolationTime;
				wpt.BaseWpt.Animation = this.CurrentAnimation;
				wpt.BaseWpt.Sound = DragonSoundEnum.NONE;
				wpt.FireballIndex = 0;
				this.Waypoints.Add(wpt);
			}
			this.Waypoints.Add(inwpt);
		}

		public bool Trace(TraceProbe tp)
		{
			this._dragonProbe.Init(tp._start, tp._end);
			this._dragonProbe.TestShape(this.HeadHitVolume, IntVector3.Zero);
			if (this._dragonProbe._collides)
			{
				tp._collides = true;
				tp._end = this._dragonProbe._end;
				tp._inT = this._dragonProbe._inT;
				tp._inNormal = this._dragonProbe._inNormal;
				tp._inFace = this._dragonProbe._inFace;
			}
			this._dragonProbe.Reset();
			this._dragonProbe.TestShape(this.BodyHitVolume, IntVector3.Zero);
			if (this._dragonProbe._collides && this._dragonProbe._inT < tp._inT)
			{
				tp._collides = true;
				tp._end = this._dragonProbe._end;
				tp._inT = this._dragonProbe._inT;
				tp._inNormal = this._dragonProbe._inNormal;
				tp._inFace = this._dragonProbe._inFace;
			}
			return tp._collides;
		}

		public bool IsHeadshot(Vector3 position)
		{
			return this.BodyHitVolume[1].DotCoordinate(position) > 0f;
		}

		public void AttachProjectile(Entity projectile)
		{
			base.AdoptChild(projectile);
		}

		public void TakeDamage(Vector3 damagePosition, Vector3 damageDirection, InventoryItem.InventoryItemClass itemClass, byte shooterID)
		{
			DamageType enemyDamageType = itemClass.EnemyDamageType;
			float damageAmount = itemClass.EnemyDamage;
			if (CastleMinerZGame.Instance.IsLocalPlayerId(shooterID))
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(itemClass.ID);
				itemStats.Hits++;
			}
			if (!this.Dead)
			{
				this.Health -= damageAmount;
				EnemyManager.Instance.DragonHasBeenHit();
				if (this.Health <= 0f)
				{
					KillDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, base.WorldPosition, shooterID, itemClass.ID);
				}
			}
		}

		public void TakeExplosiveDamage(Vector3 position, float damageAmount, byte shooterID, InventoryItemIDs itemID)
		{
			if (!this.Dead)
			{
				float damageMultiplier = (this.IsHeadshot(position) ? 1f : 2.5f);
				damageAmount *= damageMultiplier;
				this.Health -= damageAmount;
				EnemyManager.Instance.DragonHasBeenHit();
				if (this.Health <= 0f)
				{
					if (itemID == InventoryItemIDs.RocketLauncherGuided && CastleMinerZGame.Instance.GameMode == GameModeTypes.Endurance && shooterID == CastleMinerZGame.Instance.LocalPlayer.Gamer.Id)
					{
						CastleMinerZGame.Instance.PlayerStats.DragonsKilledWithGuidedMissile++;
					}
					KillDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, base.WorldPosition, shooterID, itemID);
				}
			}
		}

		public bool Dead
		{
			get
			{
				return this.DeathState != DragonClientEntity.DeathAnimationState.STILL_ALIVE;
			}
		}

		public void Kill(bool spawnPickups)
		{
			if (!this.Dead)
			{
				this.DeathPackageIndex = 0;
				this.SpawnPickups = spawnPickups;
				this.PlaySingleClip(DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].Name, false, 0.25f);
				this.DeathState = DragonClientEntity.DeathAnimationState.FIRST_REACTION;
				this.DeathTimer = DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].PauseTime;
				float yaw = DragonBaseState.GetHeading(base.LocalToWorld.Forward, 0f);
				base.LocalRotation = Quaternion.CreateFromYawPitchRoll(yaw, 0f, 0f);
				int particlePackageIndex = (int)this.EType.DamageType;
				if (CastleMinerZGame.Instance.IsActive)
				{
					ParticleEmitter emitter = DragonClientEntity.ParticlePackages[particlePackageIndex]._flashEffect.CreateEmitter(CastleMinerZGame.Instance);
					emitter.Reset();
					emitter.Emitting = true;
					emitter.LocalPosition = Vector3.Zero;
					emitter.DrawPriority = 900;
					base.Children.Add(emitter);
					emitter = DragonClientEntity.ParticlePackages[particlePackageIndex]._firePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
					emitter.Reset();
					emitter.Emitting = true;
					emitter.LocalPosition = Vector3.Zero;
					emitter.DrawPriority = 900;
					base.Children.Add(emitter);
					emitter = DragonClientEntity.ParticlePackages[particlePackageIndex]._smokePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
					emitter.Reset();
					emitter.Emitting = true;
					emitter.LocalPosition = Vector3.Zero;
					emitter.DrawPriority = 900;
					base.Children.Add(emitter);
				}
			}
		}

		public void HandleUpdateDragonMessage(UpdateDragonMessage msg)
		{
			this.AddActionWaypoint(ActionDragonWaypoint.CreateFromBase(msg.Waypoint));
		}

		public void HandleDragonAttackMessage(DragonAttackMessage msg)
		{
			DragonWaypointActionEnum actionType = (msg.AnimatedAttack ? DragonWaypointActionEnum.ANIMSHOOT : DragonWaypointActionEnum.QUICKSHOOT);
			this.AddActionWaypoint(ActionDragonWaypoint.Create(msg.Waypoint, msg.Target, actionType, msg.FireballIndex));
		}

		public DragonAnimEnum CurrentAnimation
		{
			get
			{
				return this._currentAnimation;
			}
			set
			{
				this._currentAnimation = value;
				this.NextAnimation = value;
				this.PlaySingleClip(DragonEntity.AnimNames[(int)value], false, 0.5f);
				if (this._currentAnimation == DragonAnimEnum.FLAP)
				{
					this.TimeLeftBeforeFlap = 11f;
				}
			}
		}

		public void ShootFireball(Vector3 targetPosition)
		{
			Bone mouthBone = this._dragonModel[0].Skeleton["Bip01 Ponytail1"];
			Matrix mouthXform = this._dragonModel[0].WorldBoneTransforms[mouthBone.Index];
			Vector3 mouthPosition = mouthXform.Translation;
			FireballEntity fbe = new FireballEntity(mouthPosition, targetPosition, this.NextFireballIndex[0], this.EType, EnemyManager.Instance.DragonControlledLocally);
			this.NextFireballIndex.RemoveAt(0);
			Scene scene = base.Scene;
			if (scene != null && scene.Children != null)
			{
				scene.Children.Add(fbe);
			}
		}

		public void ProcessWaypoint(ActionDragonWaypoint waypoint)
		{
			switch (waypoint.Action)
			{
			case DragonWaypointActionEnum.ANIMSHOOT:
				this.TargetPosition = waypoint.ActionPosition;
				this.WaitingToShoot = true;
				this.CurrentAnimation = DragonAnimEnum.ATTACK;
				this.NextFireballIndex.Add(waypoint.FireballIndex);
				break;
			case DragonWaypointActionEnum.QUICKSHOOT:
				this.NextFireballIndex.Add(waypoint.FireballIndex);
				this.ShootFireball(waypoint.ActionPosition);
				break;
			}
			if (waypoint.BaseWpt.Sound != DragonSoundEnum.NONE)
			{
				DragonSoundEnum sound = waypoint.BaseWpt.Sound;
				if (sound != DragonSoundEnum.CRY)
				{
					return;
				}
				SoundManager.Instance.PlayInstance("DragonScream", this.SoundEmitter);
			}
		}

		private void UpdateWhileDead(DNAGame game, GameTime gameTime)
		{
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (this.DeathState < DragonClientEntity.DeathAnimationState.POSTCOLLISION)
			{
				this.CurrentVelocity.Y = this.CurrentVelocity.Y - 10f * dt;
				if (this.DeathState == DragonClientEntity.DeathAnimationState.FALLING)
				{
					this.MoveDragonWithCollision(dt);
					if (this.OnGround)
					{
						if (!this.ClipFinished && !this.CurrentPlayer[0].Playing)
						{
							SoundManager.Instance.PlayInstance("DragonFall", this.SoundEmitter);
							for (int i = 0; i < 2; i++)
							{
								this.CurrentPlayer[i].Play();
							}
						}
						this.CurrentVelocity.X = this.CurrentVelocity.X * 0.7f;
						this.CurrentVelocity.Z = this.CurrentVelocity.Z * 0.7f;
						if (this.CurrentVelocity.LengthSquared() < 0.5f)
						{
							this.DeathState = DragonClientEntity.DeathAnimationState.POSTCOLLISION;
							this.DeathTimer = DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].TimeToWaitAfterStop;
						}
					}
				}
				else
				{
					base.LocalPosition += this.CurrentVelocity * dt;
				}
			}
			switch (this.DeathState)
			{
			case DragonClientEntity.DeathAnimationState.FIRST_REACTION:
			{
				float t = (float)this.ClipCurrentTime.TotalSeconds;
				float h = MathTools.MapAndLerp(t, DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].BeginPositionChangeTime, DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].PauseTime, -11.75f, 0f);
				bool timeToPause = t >= DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].PauseTime;
				Vector3 v = this._dragonModel[0].LocalPosition;
				v.Y = h;
				for (int j = 0; j < 2; j++)
				{
					this._dragonModel[j].LocalPosition = v;
					if (timeToPause)
					{
						this.CurrentPlayer[j].Pause();
					}
				}
				if (timeToPause)
				{
					this.DeathState = DragonClientEntity.DeathAnimationState.FALLING;
				}
				break;
			}
			case DragonClientEntity.DeathAnimationState.POSTCOLLISION:
				if (this.ClipFinished)
				{
					this.DeathTimer -= dt;
				}
				break;
			}
			for (int k = 0; k < base.Children.Count; k++)
			{
				base.Children[k].Update(game, gameTime);
			}
			if (this.DeathState == DragonClientEntity.DeathAnimationState.POSTCOLLISION && this.DeathTimer < 0f)
			{
				if (this.SpawnPickups)
				{
					Vector3 offsetToBox = DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].BoxOffset;
					offsetToBox = Vector3.TransformNormal(offsetToBox, base.LocalToWorld);
					Vector3 worldPos = base.WorldPosition + offsetToBox;
					EnemyManager.Instance.SpawnDragonPickups(worldPos);
				}
				EnemyManager.Instance.RemoveDragonEntity();
			}
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (this.Dead)
			{
				this.UpdateWhileDead(game, gameTime);
				return;
			}
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			this.TimeoutTimer += dt;
			if (this.TimeoutTimer > 10f)
			{
				this.TimeoutTimer = -3600f;
				RemoveDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer);
			}
			if (this.Waypoints.Count < 3 && !this.GotInitialWaypoints)
			{
				return;
			}
			this.GotInitialWaypoints = true;
			this.Visible = true;
			if (this.CurrentInterpolationTime == 0f)
			{
				this.CurrentAnimation = DragonAnimEnum.SOAR;
				this.CurrentInterpolationTime = this.Waypoints[0].BaseWpt.HostTime;
			}
			else if (this.Waypoints[this.Waypoints.Count - 1].BaseWpt.HostTime - this.CurrentInterpolationTime < 0.1f)
			{
				this.CurrentInterpolationTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.8f;
			}
			else
			{
				this.CurrentInterpolationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
			}
			while (this.Waypoints.Count > 1 && this.CurrentInterpolationTime >= this.Waypoints[1].BaseWpt.HostTime)
			{
				this.Waypoints.RemoveAt(0);
				this.ProcessWaypoint(this.Waypoints[0]);
			}
			this.NextAnimation = this.Waypoints[0].BaseWpt.Animation;
			Vector3 pos;
			Vector3 vel;
			if (this.Waypoints.Count >= 2)
			{
				ActionDragonWaypoint.InterpolatePositionVelocity(this.CurrentInterpolationTime, this.Waypoints[0], this.Waypoints[1], out pos, out vel);
			}
			else
			{
				ActionDragonWaypoint.Extrapolate(this.CurrentInterpolationTime, this.Waypoints[0], out pos, out vel);
			}
			this.Roll = MathTools.MoveTowardTarget(this.Roll, this.Waypoints[0].BaseWpt.TargetRoll, this.EType.RollRate, (float)gameTime.ElapsedGameTime.TotalSeconds);
			this.CurrentVelocity = vel;
			Vector3 forward = Vector3.Normalize(vel);
			Vector3 newfwd = forward;
			Vector3 oldfwd = base.LocalToWorld.Forward;
			newfwd.Y = 0f;
			oldfwd.Y = 0f;
			newfwd.Normalize();
			oldfwd.Normalize();
			Vector3.Cross(newfwd, oldfwd);
			Vector3 up = Vector3.Up;
			Vector3 right = Vector3.Normalize(Vector3.Cross(forward, up));
			up = Vector3.Cross(right, forward);
			Matrix newmat = Matrix.Identity;
			newmat.Forward = forward;
			newmat.Right = right;
			newmat.Up = up;
			newmat = Matrix.Multiply(Matrix.CreateFromYawPitchRoll(0f, 0f, this.Roll), newmat);
			newmat.Translation = pos;
			base.LocalToParent = newmat;
			this.SoundEmitter.Position = newmat.Translation;
			this.SoundEmitter.Forward = newmat.Forward;
			this.SoundEmitter.Up = newmat.Up;
			this.SoundEmitter.Velocity = Vector3.Zero;
			TraceProbe.MakeOrientedBox(base.LocalToWorld, DragonClientEntity.HeadBox, this.HeadHitVolume);
			TraceProbe.MakeOrientedBox(base.LocalToWorld, DragonClientEntity.BodyBox, this.BodyHitVolume);
			for (int i = 0; i < base.Children.Count; i++)
			{
				base.Children[i].Update(game, gameTime);
			}
			if (this.WaitingToShoot)
			{
				if (this.CurrentAnimation == DragonAnimEnum.ATTACK)
				{
					if (this.ClipCurrentTime.TotalSeconds > 1.1333333333333333)
					{
						this.ShootFireball(this.TargetPosition);
						this.WaitingToShoot = false;
					}
				}
				else
				{
					this.WaitingToShoot = false;
				}
			}
			if (this.ClipFinished)
			{
				this.CurrentAnimation = this.NextAnimation;
			}
			this.FlapTimer.Update(gameTime.ElapsedGameTime);
			if (this.FlapTimer.Expired && this.CurrentAnimation != DragonAnimEnum.SOAR)
			{
				SoundManager.Instance.PlayInstance("WingFlap", this.SoundEmitter);
				this.FlapTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));
			}
		}

		public void PlaySingleClip(string name, bool loop, float blendTime)
		{
			for (int i = 0; i < 2; i++)
			{
				this.CurrentPlayer[i] = this._dragonModel[i].PlayClip(name, loop, TimeSpan.FromSeconds((double)blendTime));
			}
		}

		public float ClipSpeed
		{
			get
			{
				if (this.CurrentPlayer[0] != null)
				{
					return this.CurrentPlayer[0].Speed;
				}
				return 1f;
			}
			set
			{
				for (int i = 0; i < 2; i++)
				{
					if (this.CurrentPlayer[i] != null)
					{
						this.CurrentPlayer[i].Speed = value;
					}
				}
			}
		}

		public bool ClipFinished
		{
			get
			{
				return this.CurrentPlayer[0] == null || (this.CurrentPlayer[0].Duration - this.CurrentPlayer[0].CurrentTime).TotalSeconds < 0.5;
			}
		}

		public TimeSpan ClipCurrentTime
		{
			get
			{
				if (this.CurrentPlayer[0] != null)
				{
					return this.CurrentPlayer[0].CurrentTime;
				}
				return TimeSpan.FromSeconds(0.0);
			}
		}

		public bool MoveDragonWithCollision(float dt)
		{
			bool flag;
			using (Profiler.TimeSection("Pickup Collision", ProfilerThreadEnum.MAIN))
			{
				bool result = false;
				Vector3 offsetToBox = DragonClientEntity.DeathAnimationPackages[this.DeathPackageIndex].BoxOffset;
				offsetToBox = Vector3.TransformNormal(offsetToBox, base.LocalToWorld);
				float t = dt;
				Vector3 worldPos = base.WorldPosition + offsetToBox;
				Vector3 nextPos = worldPos;
				Vector3 velocity = this.CurrentVelocity;
				this.OnGround = false;
				DragonClientEntity.MovementProbe.SkipEmbedded = true;
				int iteration = 0;
				for (;;)
				{
					Vector3 prevPos = nextPos;
					Vector3 movement = Vector3.Multiply(velocity, t);
					nextPos += movement;
					DragonClientEntity.MovementProbe.Init(prevPos, nextPos, DragonClientEntity.DeadBodyBox);
					BlockTerrain.Instance.Trace(DragonClientEntity.MovementProbe);
					if (DragonClientEntity.MovementProbe._collides)
					{
						result = true;
						if (DragonClientEntity.MovementProbe._inFace == BlockFace.POSY)
						{
							this.OnGround = true;
						}
						if (DragonClientEntity.MovementProbe._startsIn)
						{
							goto IL_017F;
						}
						float realt = Math.Max(DragonClientEntity.MovementProbe._inT - 0.001f, 0f);
						nextPos = prevPos + movement * realt;
						velocity -= Vector3.Multiply(DragonClientEntity.MovementProbe._inNormal, Vector3.Dot(DragonClientEntity.MovementProbe._inNormal, velocity));
						t *= 1f - realt;
						if (t <= 1E-07f)
						{
							goto IL_017F;
						}
						if (velocity.LengthSquared() <= 1E-06f || Vector3.Dot(this.CurrentVelocity, velocity) <= 1E-06f)
						{
							break;
						}
					}
					iteration++;
					if (!DragonClientEntity.MovementProbe._collides || iteration >= 4)
					{
						goto IL_017F;
					}
				}
				velocity = Vector3.Zero;
				IL_017F:
				if (iteration == 4)
				{
					velocity = Vector3.Zero;
				}
				nextPos -= offsetToBox;
				if (nextPos.Y < -64f)
				{
					nextPos.Y = -64f;
					velocity.Y = 0f;
					this.OnGround = true;
				}
				base.LocalPosition = nextPos;
				this.CurrentVelocity = velocity;
				flag = result;
			}
			return flag;
		}

		private const int NUM_DEATH_ANIMATIONS = 2;

		private const float ANIM_BLEND_TIME = 0.5f;

		public const float TIMEOUT_MAX_TIME = 10f;

		public const int NUM_WAYPOINTS = 20;

		private static DragonClientEntity.ParticlePackage[] ParticlePackages;

		private static DragonClientEntity.DeathAnimationPackage[] DeathAnimationPackages;

		private static Model DragonBody;

		public static Model DragonFeet;

		private static DragonClientEntity.DragonTraceProbe MovementProbe = new DragonClientEntity.DragonTraceProbe();

		private static BoundingBox HeadBox = new BoundingBox(new Vector3(-1.5f, 0f, -9.625f), new Vector3(1.5f, 2.5f, -4.125f));

		private static BoundingBox BodyBox = new BoundingBox(new Vector3(-1.75f, -1.05f, -4.2f), new Vector3(1.75f, 2.45f, 4.8f));

		private static BoundingBox DeadBodyBox = new BoundingBox(new Vector3(-2.5f, 0f, -2.5f), new Vector3(2.5f, 10f, 2.5f));

		public Plane[] HeadHitVolume;

		public Plane[] BodyHitVolume;

		public DragonPartEntity[] _dragonModel;

		public AnimationPlayer[] CurrentPlayer;

		public DragonType EType;

		public List<ActionDragonWaypoint> Waypoints;

		public List<int> NextFireballIndex;

		public float CurrentInterpolationTime;

		public Vector3 CurrentVelocity;

		public Vector3 TargetPosition;

		public DragonAnimEnum _currentAnimation;

		public DragonAnimEnum NextAnimation;

		public float TimeLeftBeforeFlap;

		public bool GotInitialWaypoints;

		public float TimeoutTimer;

		public float TargetRoll;

		public float Roll;

		public bool WaitingToShoot;

		public float Health;

		public bool SpawnPickups;

		public AudioEmitter SoundEmitter;

		public SoundCue3D DragonCryCue;

		public SoundCue3D DragonFlapCue;

		public OneShotTimer FlapTimer;

		private int DeathPackageIndex;

		private DragonClientEntity.DeathAnimationState DeathState;

		private float DeathTimer;

		private bool OnGround;

		public TraceProbe _dragonProbe;

		private enum DeathAnimationState
		{
			STILL_ALIVE,
			FIRST_REACTION,
			FALLING,
			POSTCOLLISION,
			PAUSE
		}

		private struct ParticlePackage
		{
			public ParticleEffect _flashEffect;

			public ParticleEffect _firePuffEffect;

			public ParticleEffect _smokePuffEffect;
		}

		private struct DeathAnimationPackage
		{
			public string Name;

			public float BeginPositionChangeTime;

			public float PauseTime;

			public float TimeToWaitAfterStop;

			public Vector3 BoxOffset;
		}

		private class DragonTraceProbe : AABBTraceProbe
		{
			public override bool TestThisType(BlockTypeEnum e)
			{
				return e != BlockTypeEnum.NumberOfBlocks && e != BlockTypeEnum.Log && BlockType.GetType(e).BlockPlayer;
			}
		}
	}
}
