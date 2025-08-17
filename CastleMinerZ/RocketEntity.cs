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
			this._smokeEmitter = RocketEntity.sSmokeTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
			this._smokeEmitter.Emitting = false;
			this._smokeEmitter.DrawPriority = 900;
			base.Children.Add(this._smokeEmitter);
			this._fireEmitter = RocketEntity.sFireTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
			this._fireEmitter.Emitting = false;
			this._fireEmitter.DrawPriority = 900;
			base.Children.Add(this._fireEmitter);
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
				if (!this._fireEmitter.HasActiveParticles && !this._smokeEmitter.HasActiveParticles)
				{
					this._fireEmitter.RemoveFromParent();
					this._smokeEmitter.RemoveFromParent();
					base.RemoveFromParent();
				}
				return;
			}
			bool flag = false;
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			this._runningTime += num;
			Vector3 vector = Vector3.Zero;
			float num2 = 0f;
			if (this._runningTime < this._timeToFullGuidance)
			{
				float num3 = this._runningTime + 0.25f;
				vector = this._startPoint + Vector3.Multiply(this._startingVelocity, num3) + Vector3.Multiply(Vector3.Down, 4.9f * num3 * num3);
			}
			Vector3 vector5;
			Vector3 vector6;
			if (this._runningTime >= 0f)
			{
				this._fireEmitter.Emitting = true;
				this._smokeEmitter.Emitting = true;
				Vector3 vector4;
				if (this._guided)
				{
					Vector3 vector2;
					if (EnemyManager.Instance != null && EnemyManager.Instance.DragonIsAlive)
					{
						flag = true;
						vector2 = EnemyManager.Instance.DragonPosition;
					}
					else
					{
						vector2 = base.WorldPosition + base.LocalToWorld.Forward;
					}
					Vector3 vector3 = Vector3.Normalize(vector2 - base.WorldPosition);
					if (this._runningTime >= this._timeToFullGuidance)
					{
						vector4 = vector3;
						vector5 = vector4;
					}
					else
					{
						float num4 = this._runningTime / this._timeToFullGuidance;
						vector4 = Vector3.Normalize(Vector3.Lerp(this._emittedDirection, vector3, num4));
						vector5 = Vector3.Normalize(Vector3.Lerp(this._emittedDirection, vector3, (float)Math.Sqrt((double)num4)));
					}
				}
				else
				{
					vector4 = this._emittedDirection;
					vector5 = this._emittedDirection;
				}
				if (this._runningTime < this._timeToMaxSpeed)
				{
					float num5 = (float)Math.Sqrt((double)(this._runningTime / this._timeToMaxSpeed));
					num2 = num5 * this._maxSpeed;
					this._guidedPosition += vector4 * (num2 * num);
					vector6 = Vector3.Lerp(vector, this._guidedPosition, num5);
				}
				else
				{
					num2 = this._maxSpeed;
					this._guidedPosition += vector4 * (num2 * num);
					vector6 = this._guidedPosition;
				}
			}
			else
			{
				vector6 = vector;
				vector5 = this._emittedDirection;
			}
			if (num2 > 0f)
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, num * 0.5f * num2);
				this._rocket.LocalRotation = Quaternion.Concatenate(this._rocket.LocalRotation, quaternion);
			}
			Vector3 worldPosition = base.WorldPosition;
			base.LocalToParent = MathTools.CreateWorld(vector6, vector5);
			bool flag2 = false;
			if (!flag && (BlockTerrain.Instance == null || !BlockTerrain.Instance.IsTracerStillInWorld(vector6)))
			{
				flag2 = true;
			}
			else
			{
				IShootableEnemy shootableEnemy = null;
				Vector3 vector7 = base.WorldPosition;
				bool flag3 = false;
				if (this._runningTime > 10f)
				{
					flag3 = true;
				}
				else
				{
					RocketEntity.tp.Init(worldPosition, base.WorldPosition);
					shootableEnemy = EnemyManager.Instance.Trace(RocketEntity.tp, false);
					if (RocketEntity.tp._collides)
					{
						flag3 = true;
						vector7 = RocketEntity.tp.GetIntersection();
					}
				}
				if (flag3)
				{
					flag2 = true;
					if (this._doExplosion)
					{
						bool flag4 = shootableEnemy is DragonClientEntity;
						DetonateRocketMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, vector7, ExplosiveTypes.Rocket, this._weaponType, flag4);
						Explosive.FindBlocksToRemove(IntVector3.FromVector3(vector7), ExplosiveTypes.Rocket, false);
					}
				}
			}
			this._audioEmitter.Forward = this._guidedPosition;
			this._audioEmitter.Position = base.LocalPosition;
			if (flag2)
			{
				this._fireEmitter.Emitting = false;
				this._smokeEmitter.Emitting = false;
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
