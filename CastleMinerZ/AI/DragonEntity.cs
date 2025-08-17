using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.Animation;
using DNA.Timers;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonEntity : Entity
	{
		public DragonEntity(DragonTypeEnum type, bool forBiome, DragonHostMigrationInfo miginfo)
		{
			this.EType = DragonType.GetDragonType(type);
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f);
			base.LocalPosition = Vector3.Zero;
			this.Target = null;
			this.TimeLeftBeforeNextShot = 0f;
			this.ShotsLeft = 0;
			this.FirstTimeForDefaultState = true;
			this.FlapDebt = 0f;
			this.UpdatesSent = 0;
			this.FlapChangeUpdate = 0;
			this.AnimationChangeUpdate = 0;
			this.Removed = false;
			this.NextSound = DragonSoundEnum.NONE;
			this.Velocity = this.EType.Speed;
			this.ShotsLeft = 0;
			this.ShotPending = false;
			this.ShootTarget = Vector3.Zero;
			this.ChancesToNotAttack = this.EType.ChancesToNotAttack;
			this.TimeLeftTilShotsHeard = 0f;
			this.DrawPriority = (int)(520 + type);
			this._dragonModel = new DragonPartEntity(this.EType, DragonClientEntity.DragonFeet);
			base.Children.Add(this._dragonModel);
			this.MigrateDragon = false;
			this.MigrateDragonTo = null;
			this.Collider = false;
			this.HadTargetThisPass = false;
			this.StateMachine = new StateMachine<DragonEntity>(this);
			this.Visible = false;
			this.NextUpdateTime = -1f;
			this.LoiterCount = 0;
			this.DragonTime = 0f;
			this.ForBiome = forBiome;
			if (miginfo == null)
			{
				this.InitSpawnState();
				return;
			}
			this.InitAfterHostChange(miginfo);
		}

		public void InitAfterHostChange(DragonHostMigrationInfo miginfo)
		{
			this.DragonTime = miginfo.NextDragonTime;
			base.LocalPosition = miginfo.Position;
			this.Yaw = miginfo.Yaw;
			this.TargetYaw = miginfo.TargetYaw;
			this.Roll = miginfo.Roll;
			this.TargetRoll = miginfo.TargetRoll;
			this.Pitch = miginfo.Pitch;
			this.TargetPitch = miginfo.TargetPitch;
			this.TargetVelocity = miginfo.TargetVelocity;
			this.DefaultHeading = miginfo.DefaultHeading;
			this.Velocity = miginfo.Velocity;
			this.TargetVelocity = miginfo.TargetVelocity;
			this.NextUpdateTime = miginfo.NextUpdateTime;
			DragonEntity.NextFireballIndex = miginfo.NextFireballIndex;
			this.ForBiome = miginfo.ForBiome;
			this.Target = CastleMinerZGame.Instance.LocalPlayer;
			this.TravelTarget = miginfo.Target;
			this.FirstTimeForDefaultState = false;
			this.ChancesToNotAttack = 0;
			this.LoitersLeft = 0;
			this.CurrentAnimation = miginfo.Animation;
			this.FlapDebt = miginfo.FlapDebt;
			this.StateMachine.ChangeState(DragonBaseState.GetNextAttackType(this));
		}

		public void InitSpawnState()
		{
			Vector3 vector = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
			float num = MathTools.RandomFloat(-1.5707964f, 1.5707964f);
			Vector3 vector2 = DragonBaseState.MakeYawVector(num);
			vector -= vector2 * this.EType.SpawnDistance;
			vector.Y = this.EType.CruisingAltitude;
			this.TargetAltitude = this.EType.CruisingAltitude;
			this.Yaw = (this.DefaultHeading = num);
			this.Pitch = 0f;
			this.CurrentAnimation = DragonAnimEnum.FLAP;
			this.Velocity = this.EType.Speed;
			this.Target = null;
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(this.Yaw, 0f, 0f);
			base.LocalPosition = vector;
			this.StateMachine.ChangeState(DragonStates.Default);
		}

		public void DoMigration()
		{
			DragonHostMigrationInfo dragonHostMigrationInfo = new DragonHostMigrationInfo();
			dragonHostMigrationInfo.Yaw = this.Yaw;
			dragonHostMigrationInfo.TargetYaw = this.TargetYaw;
			dragonHostMigrationInfo.Roll = this.Roll;
			dragonHostMigrationInfo.TargetRoll = this.TargetRoll;
			dragonHostMigrationInfo.Pitch = this.Pitch;
			dragonHostMigrationInfo.TargetPitch = this.TargetPitch;
			dragonHostMigrationInfo.NextDragonTime = this.DragonTime + 0.4f;
			dragonHostMigrationInfo.NextUpdateTime = this.NextUpdateTime + 0.4f;
			dragonHostMigrationInfo.Position = base.LocalPosition;
			dragonHostMigrationInfo.Velocity = this.Velocity;
			dragonHostMigrationInfo.TargetVelocity = this.TargetVelocity;
			dragonHostMigrationInfo.DefaultHeading = this.DefaultHeading;
			dragonHostMigrationInfo.NextFireballIndex = DragonEntity.NextFireballIndex;
			dragonHostMigrationInfo.ForBiome = this.ForBiome;
			dragonHostMigrationInfo.Target = this.TravelTarget;
			dragonHostMigrationInfo.EType = this.EType.EType;
			EnemyManager.Instance.MigrateDragon(this.MigrateDragonTo, dragonHostMigrationInfo);
		}

		public int GetNextFireballIndex()
		{
			return ((int)CastleMinerZGame.Instance.LocalPlayer.Gamer.Id << 23) | DragonEntity.NextFireballIndex++;
		}

		public float Velocity
		{
			get
			{
				return this._velocity;
			}
			set
			{
				this._velocity = value;
				this.TargetVelocity = value;
			}
		}

		public float Yaw
		{
			get
			{
				return this._yaw;
			}
			set
			{
				this._yaw = value;
				this.TargetYaw = value;
			}
		}

		public float Roll
		{
			get
			{
				return this._roll;
			}
			set
			{
				this._roll = value;
				this.TargetRoll = value;
			}
		}

		public float Pitch
		{
			get
			{
				return this._pitch;
			}
			set
			{
				this._pitch = value;
				this.TargetPitch = value;
			}
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
				this.PlaySingleClip(DragonEntity.AnimNames[(int)value], false);
			}
		}

		public void UpdateSounds(TimeSpan ElapsedGameTime)
		{
			this.CryTimer.Update(ElapsedGameTime);
			if (this.NextSound == DragonSoundEnum.NONE && this.CryTimer.Expired)
			{
				this.NextSound = DragonSoundEnum.CRY;
				this.CryTimer = new OneShotTimer(TimeSpan.FromSeconds((double)MathTools.RandomInt(6, 14)));
			}
		}

		public void RegisterGunshot(Vector3 position)
		{
			this.Gunshots.Add(position);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (this.MigrateDragon)
			{
				if (this.Target != null)
				{
					this.MigrateDragonTo = this.Target;
				}
				if (this.MigrateDragonTo != null && this.MigrateDragonTo.ValidLivingGamer)
				{
					this.DoMigration();
					return;
				}
				this.MigrateDragonTo = null;
				this.MigrateDragon = false;
			}
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			this.DragonTime += num;
			this.StateMachine._currentState.Update(this, num);
			this.UpdateSounds(gameTime.ElapsedGameTime);
			float num2 = MathHelper.WrapAngle(this.TargetYaw - this._yaw);
			if (this.CurrentAnimation == DragonAnimEnum.HOVER)
			{
				this.TargetRoll = 0f;
			}
			else
			{
				this.TargetRoll = MathHelper.Clamp(num2, -this.EType.MaxRoll, this.EType.MaxRoll);
			}
			float y = base.WorldPosition.Y;
			if (y > this.TargetAltitude - 2f && y < this.TargetAltitude + 2f)
			{
				this.TargetPitch = 0f;
			}
			else
			{
				float num3 = this.TargetAltitude - y;
				this.TargetPitch = (float)Math.Sign(num3) * Math.Min(num3 * num3 / 30f, this.EType.MaxPitch);
			}
			float yaw = this._yaw;
			this._yaw = MathTools.MoveTowardTargetAngle(this._yaw, this.TargetYaw, this.EType.YawRate, num);
			this._velocity = MathTools.MoveTowardTarget(this._velocity, this.TargetVelocity, this.EType.MaxAccel, num);
			this._roll = MathTools.MoveTowardTarget(this._roll, this.TargetRoll, this.EType.RollRate, num);
			this._pitch = MathTools.MoveTowardTarget(this._pitch, this.TargetPitch, this.EType.PitchRate, num);
			if (this.AnimationChangeUpdate >= this.FlapChangeUpdate && this.UpdatesSent > this.AnimationChangeUpdate)
			{
				float num4 = ((this._pitch > 0f) ? 2f : 0.5f);
				this.FlapDebt += this._pitch * num * num4;
				this.FlapDebt += Math.Abs(MathHelper.WrapAngle(yaw - this.Yaw)) * 1.5f;
				if (Math.Abs(this._pitch) < 0.01f)
				{
					this.FlapDebt += 0.2f * num;
				}
				if (this.CurrentAnimation != DragonAnimEnum.SOAR)
				{
					this.FlapDebt -= 0.5f * num;
					if (this.FlapDebt < -1f && this.NextAnimation == DragonAnimEnum.FLAP)
					{
						this.NextAnimation = DragonAnimEnum.SOAR;
						this.FlapChangeUpdate = this.UpdatesSent;
					}
				}
				else if (this.NextAnimation == DragonAnimEnum.SOAR && this.FlapDebt > 1f)
				{
					this.NextAnimation = DragonAnimEnum.FLAP;
					this.FlapChangeUpdate = this.UpdatesSent;
				}
				this.FlapDebt = this.FlapDebt.Clamp(-1f, 1f);
			}
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(this._yaw, this._pitch, this._roll);
			base.LocalPosition += base.LocalToWorld.Forward * (this._velocity * num);
			for (int i = 0; i < base.Children.Count; i++)
			{
				base.Children[i].Update(game, gameTime);
			}
			if (this.ClipFinished)
			{
				this.CurrentAnimation = this.NextAnimation;
				this.AnimationChangeUpdate = this.UpdatesSent;
			}
			if (this.DragonTime > this.NextUpdateTime)
			{
				if (this.NextUpdateTime == -1f)
				{
					UpdateDragonMessage.UpdateCount = 0;
				}
				((DragonBaseState)this.StateMachine._currentState).SendUpdateMessage(this);
				this.NextUpdateTime = this.DragonTime + 0.2f;
			}
			this.Gunshots.Clear();
		}

		public void PlaySingleClip(string name, bool loop)
		{
			this.CurrentPlayer = this._dragonModel.PlayClip(name, loop, TimeSpan.FromSeconds(0.25));
		}

		public float ClipSpeed
		{
			get
			{
				if (this.CurrentPlayer != null)
				{
					return this.CurrentPlayer.Speed;
				}
				return 1f;
			}
			set
			{
				if (this.CurrentPlayer != null)
				{
					this.CurrentPlayer.Speed = value;
				}
			}
		}

		public bool ClipFinished
		{
			get
			{
				return this.CurrentPlayer == null || this.CurrentPlayer.Finished;
			}
		}

		public TimeSpan ClipCurrentTime
		{
			get
			{
				if (this.CurrentPlayer != null)
				{
					return this.CurrentPlayer.CurrentTime;
				}
				return TimeSpan.FromSeconds(0.0);
			}
		}

		public const float UPDATE_INTERVAL = 0.2f;

		public const float DEBT_BEFORE_FLAPPING = 1f;

		public const float DEBT_ASCEND_MULTIPLIER = 2f;

		public const float DEBT_DESCEND_MULTIPLIER = 0.5f;

		public const float DEBT_YAW_MULTIPLIER = 1.5f;

		public const float DEBT_REMOVAL_RATE = 0.5f;

		public const float DEBT_AMBIENT = 0.2f;

		public const float GUESS_AT_LATENCY = 0.4f;

		public const float DISTANCE_FROM_LP_TO_MIGRATE_DRAGON = 150f;

		public static readonly string[] AnimNames = new string[] { "flying_idle", "fly_forward", "gethit", "Idle" };

		public static int NextFireballIndex = 0;

		public DragonPartEntity _dragonModel;

		public AnimationPlayer CurrentPlayer;

		public StateMachine<DragonEntity> StateMachine;

		public List<Vector3> Gunshots = new List<Vector3>();

		public DragonType EType;

		public float _velocity;

		public float TargetVelocity;

		public bool FirstTimeForDefaultState;

		public bool HadTargetThisPass;

		public int LoiterCount;

		public float LoiterTimer;

		public float NextUpdateTime;

		public Player Target;

		public Vector3 TravelTarget;

		public bool ShotPending;

		public Vector3 ShootTarget;

		public DragonAnimEnum _currentAnimation;

		public DragonAnimEnum NextAnimation;

		public float FlapDebt;

		public int ShotsLeft;

		public float TimeLeftBeforeNextShot;

		public float TimeLeftTilShotsHeard;

		public float TimeLeftBeforeNextViewCheck;

		public float DefaultHeading;

		public float TargetAltitude;

		public float _yaw;

		public float TargetYaw;

		public float _roll;

		public float TargetRoll;

		public float _pitch;

		public float TargetPitch;

		public int UpdatesSent;

		public int AnimationChangeUpdate;

		public int FlapChangeUpdate;

		public int LoitersLeft;

		public bool Removed;

		public DragonSoundEnum NextSound;

		public int ChancesToNotAttack;

		public float DragonTime;

		public bool ForBiome;

		public bool MigrateDragon;

		public Player MigrateDragonTo;

		public OneShotTimer CryTimer = new OneShotTimer(TimeSpan.FromSeconds(5.0));
	}
}
