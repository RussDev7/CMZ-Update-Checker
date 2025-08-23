using System;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class RocketEntity : Entity
	{
		public static void Init()
		{
			RocketEntity.sFireTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\RocketFireTrail");
			RocketEntity.sSmokeTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\RocketSmokeTrail");
		}

		public RocketEntity(Vector3 position, Vector3 vector, InventoryItemIDs weaponType, bool guided, bool doExplosion)
		{
			this._rocket = new ModelEntity(CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\RPG\\RPGGrenade"));
			this._rocket.EnableDefaultLighting();
			this._rocket.LocalRotation = Quaternion.CreateFromYawPitchRoll(-1.5707964f, 0f, 0f);
			this._active = true;
			base.Children.Add(this._rocket);
			this._emittedDirection = vector;
			this._startPoint = position + vector;
			this._doExplosion = doExplosion;
			this._weaponType = weaponType;
			base.LocalToParent = MathTools.CreateWorld(this._startPoint, this._emittedDirection);
			this._runningTime = -0.25f;
			this._guidedPosition = this._startPoint;
			this._startingVelocity = Vector3.Normalize(Vector3.Lerp(base.LocalToWorld.Forward, base.LocalToWorld.Up, 0.75f)) * 3.5f;
			this._audioEmitter.Forward = this._emittedDirection;
			this._audioEmitter.Position = base.LocalPosition;
			this._audioEmitter.Up = Vector3.Up;
			this._audioEmitter.Velocity = this._startingVelocity;
			this._whooshCue = SoundManager.Instance.PlayInstance("RocketWhoosh", this._audioEmitter);
			if (weaponType == InventoryItemIDs.RocketLauncherGuided)
			{
				this._guided = guided;
				this._maxSpeed = 50f;
				this._timeToFullGuidance = 2.5f;
				this._timeToMaxSpeed = 1f;
			}
			else
			{
				this._guided = false;
				this._maxSpeed = 25f;
				this._timeToFullGuidance = 1f;
				this._timeToMaxSpeed = 1f;
			}
			if (CastleMinerZGame.Instance.IsActive)
			{
				this._smokeEmitter = RocketEntity.sSmokeTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
				this._smokeEmitter.Emitting = false;
				this._smokeEmitter.DrawPriority = 900;
				base.Children.Add(this._smokeEmitter);
				this._fireEmitter = RocketEntity.sFireTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
				this._fireEmitter.Emitting = false;
				this._fireEmitter.DrawPriority = 900;
				base.Children.Add(this._fireEmitter);
			}
			this.Collidee = false;
			this.Collider = false;
		}

		protected override void OnParentChanged(Entity oldParent, Entity newParent)
		{
			if (newParent == null && this._whooshCue != null && this._whooshCue.IsPlaying)
			{
				this._whooshCue.Stop(AudioStopOptions.Immediate);
			}
			base.OnParentChanged(oldParent, newParent);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			if (!this._active)
			{
				if (this._fireEmitter != null && this._smokeEmitter != null && !this._fireEmitter.HasActiveParticles && !this._smokeEmitter.HasActiveParticles)
				{
					this._fireEmitter.RemoveFromParent();
					this._smokeEmitter.RemoveFromParent();
					base.RemoveFromParent();
				}
				return;
			}
			bool validTarget = false;
			float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			this._runningTime += deltaTime;
			Vector3 ballisticPosition = Vector3.Zero;
			float speed = 0f;
			if (this._runningTime < this._timeToFullGuidance)
			{
				float absTime = this._runningTime + 0.25f;
				ballisticPosition = this._startPoint + Vector3.Multiply(this._startingVelocity, absTime) + Vector3.Multiply(Vector3.Down, 4.9f * absTime * absTime);
			}
			Vector3 pointingDirection;
			Vector3 newPosition;
			if (this._runningTime >= 0f)
			{
				if (this._fireEmitter != null && this._smokeEmitter != null)
				{
					this._fireEmitter.Emitting = true;
					this._smokeEmitter.Emitting = true;
				}
				Vector3 newDirection;
				if (this._guided)
				{
					Vector3 targetLocation;
					if (EnemyManager.Instance != null && EnemyManager.Instance.DragonIsAlive)
					{
						validTarget = true;
						targetLocation = EnemyManager.Instance.DragonPosition;
					}
					else
					{
						targetLocation = base.WorldPosition + base.LocalToWorld.Forward;
					}
					Vector3 targetDirection = Vector3.Normalize(targetLocation - base.WorldPosition);
					if (this._runningTime >= this._timeToFullGuidance)
					{
						newDirection = targetDirection;
						pointingDirection = newDirection;
					}
					else
					{
						float lerpValue = this._runningTime / this._timeToFullGuidance;
						newDirection = Vector3.Normalize(Vector3.Lerp(this._emittedDirection, targetDirection, lerpValue));
						pointingDirection = Vector3.Normalize(Vector3.Lerp(this._emittedDirection, targetDirection, (float)Math.Sqrt((double)lerpValue)));
					}
				}
				else
				{
					newDirection = this._emittedDirection;
					pointingDirection = this._emittedDirection;
				}
				if (this._runningTime < this._timeToMaxSpeed)
				{
					float lerpValue2 = (float)Math.Sqrt((double)(this._runningTime / this._timeToMaxSpeed));
					speed = lerpValue2 * this._maxSpeed;
					this._guidedPosition += newDirection * (speed * deltaTime);
					newPosition = Vector3.Lerp(ballisticPosition, this._guidedPosition, lerpValue2);
				}
				else
				{
					speed = this._maxSpeed;
					this._guidedPosition += newDirection * (speed * deltaTime);
					newPosition = this._guidedPosition;
				}
			}
			else
			{
				newPosition = ballisticPosition;
				pointingDirection = this._emittedDirection;
			}
			if (speed > 0f)
			{
				Quaternion q = Quaternion.CreateFromYawPitchRoll(0f, 0f, deltaTime * 0.5f * speed);
				this._rocket.LocalRotation = Quaternion.Concatenate(this._rocket.LocalRotation, q);
			}
			Vector3 lastPosition = base.WorldPosition;
			base.LocalToParent = MathTools.CreateWorld(newPosition, pointingDirection);
			bool deactivate = false;
			if (!validTarget && (BlockTerrain.Instance == null || !BlockTerrain.Instance.IsTracerStillInWorld(newPosition)))
			{
				deactivate = true;
			}
			else
			{
				IShootableEnemy z = null;
				Vector3 detonationPosition = base.WorldPosition;
				bool explode = false;
				if (this._runningTime > 10f)
				{
					explode = true;
				}
				else
				{
					RocketEntity.tp.Init(lastPosition, base.WorldPosition);
					z = EnemyManager.Instance.Trace(RocketEntity.tp, false);
					if (RocketEntity.tp._collides)
					{
						explode = true;
						detonationPosition = RocketEntity.tp.GetIntersection();
					}
				}
				if (explode)
				{
					deactivate = true;
					if (this._doExplosion)
					{
						bool hitDragon = z is DragonClientEntity;
						DetonateRocketMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, detonationPosition, ExplosiveTypes.Rocket, this._weaponType, hitDragon);
						Explosive.FindBlocksToRemove(IntVector3.FromVector3(detonationPosition), ExplosiveTypes.Rocket, false);
					}
				}
			}
			this._audioEmitter.Forward = this._guidedPosition;
			this._audioEmitter.Position = base.LocalPosition;
			if (deactivate)
			{
				if (this._fireEmitter != null && this._smokeEmitter != null)
				{
					this._fireEmitter.Emitting = false;
					this._smokeEmitter.Emitting = false;
				}
				this._rocket.RemoveFromParent();
				this._rocket = null;
				this._active = false;
			}
		}

		public const string RocketModelName = "Props\\Weapons\\Conventional\\RPG\\RPGGrenade";

		private const float cMaxVelocity = 50f;

		private const float cFuseTime = 0.25f;

		private const float cLaunchVelocity = 3.5f;

		private const float cLaunchBlend = 0.75f;

		private const float cRotationalSpeedMultiplier = 0.5f;

		private const float cRocketLifetime = 10f;

		private static ParticleEffect sSmokeTrailEffect;

		private static ParticleEffect sFireTrailEffect;

		private static TracerManager.TracerProbe tp = new TracerManager.TracerProbe();

		private ModelEntity _rocket;

		private ParticleEmitter _smokeEmitter;

		private ParticleEmitter _fireEmitter;

		private Vector3 _startingVelocity;

		private Vector3 _emittedDirection;

		private Vector3 _startPoint;

		private Vector3 _guidedPosition;

		private InventoryItemIDs _weaponType;

		private float _runningTime;

		private float _maxSpeed;

		private float _timeToFullGuidance;

		private float _timeToMaxSpeed;

		private bool _guided;

		private bool _doExplosion;

		private bool _active;

		private AudioEmitter _audioEmitter = new AudioEmitter();

		private SoundCue3D _whooshCue;
	}
}
