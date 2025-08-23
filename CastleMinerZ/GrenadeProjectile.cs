using System;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class GrenadeProjectile : Entity
	{
		public static void Init()
		{
			GrenadeProjectile._grenadeModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Grenade\\Model");
			GrenadeProjectile._smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeEffect");
			GrenadeProjectile._sparkEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SparksEffect");
			GrenadeProjectile._spashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BlasterFlash");
		}

		public static void HandleDetonateGrenadeMessage(DetonateGrenadeMessage msg)
		{
			switch (msg.GrenadeType)
			{
			case GrenadeTypeEnum.HE:
			case GrenadeTypeEnum.Sticky:
				HEGrenadeProjectile.InternalHandleDetonateGrenadeMessage(msg);
				return;
			case GrenadeTypeEnum.Smoke:
				SmokeGrenadeProjectile.InternalHandleDetonateGrenadeMessage(msg);
				return;
			case GrenadeTypeEnum.Flash:
				FlashGrenadeProjectile.InternalHandleDetonateGrenadeMessage(msg);
				return;
			default:
				return;
			}
		}

		public static GrenadeProjectile Create(Vector3 position, Vector3 velocity, float timeLeft, GrenadeTypeEnum grenadeType, bool isLocal)
		{
			GrenadeProjectile result = null;
			switch (grenadeType)
			{
			case GrenadeTypeEnum.HE:
			case GrenadeTypeEnum.Sticky:
				result = new HEGrenadeProjectile();
				break;
			case GrenadeTypeEnum.Smoke:
				result = new SmokeGrenadeProjectile();
				break;
			case GrenadeTypeEnum.Flash:
				result = new FlashGrenadeProjectile();
				break;
			}
			result.LocalToParent = MathTools.CreateWorld(position, velocity);
			result._rotationAxis = result.LocalToWorld.Right;
			result._rotationSpeed = 6f;
			result._linearVelocity = velocity;
			result._timeLeft = timeLeft;
			result._isLocal = isLocal;
			result.GrenadeType = grenadeType;
			return result;
		}

		protected GrenadeProjectile()
		{
			this._grenadeEntity = new ModelEntity(GrenadeProjectile._grenadeModel);
			float s = 0.3f / this._grenadeEntity.GetLocalBoundingSphere().Radius;
			this._grenadeEntity.LocalPosition = new Vector3(0f, -0.08f, 0f) * s;
			this._grenadeEntity.LocalScale = new Vector3(s);
			base.Children.Add(this._grenadeEntity);
			this._stopped = false;
			this._audioEmitter.Position = base.WorldPosition;
			this._audioEmitter.Up = Vector3.Up;
			this._audioEmitter.Velocity = this._linearVelocity;
		}

		protected virtual bool ReadyToBeRemoved()
		{
			return this._exploded;
		}

		protected virtual void Explode()
		{
			this._exploded = true;
		}

		protected void MoveToNewPosition(Vector3 newPosition)
		{
			GrenadeProjectile.tp._lastEnemy = null;
			int count = 10;
			if (this.GrenadeType == GrenadeTypeEnum.Sticky)
			{
				count = 1;
			}
			do
			{
				GrenadeProjectile.tp.Init(this._lastPosition, newPosition, this.cGrenadeAABB);
				IShootableEnemy z = EnemyManager.Instance.Trace(GrenadeProjectile.tp, false);
				GrenadeProjectile.tp._lastEnemy = z;
				if (GrenadeProjectile.tp._collides)
				{
					Vector3 collisionPoint = GrenadeProjectile.tp.GetIntersection();
					count--;
					if (count == 0)
					{
						newPosition = collisionPoint;
						this._stopped = true;
						this._linearVelocity = Vector3.Zero;
						this._rotationSpeed = 0f;
						this._stickyPosition = collisionPoint;
						if (this.GrenadeType == GrenadeTypeEnum.Sticky && GrenadeProjectile.tp._lastEnemy != null && !this._attached)
						{
							GrenadeProjectile.tp._lastEnemy.AttachProjectile(this._grenadeEntity);
							this._attached = true;
						}
					}
					else
					{
						float restitution = 0.1f;
						if (z == null)
						{
							this._audioEmitter.Position = base.WorldPosition;
							this._audioEmitter.Velocity = this._linearVelocity;
							if ((double)this._linearVelocity.LengthSquared() > 0.01)
							{
								SoundManager.Instance.PlayInstance("BulletHitDirt", this._audioEmitter);
							}
							BlockType bt = BlockType.GetType(BlockTerrain.Instance.GetBlockWithChanges(GrenadeProjectile.tp._worldIndex));
							restitution = bt.BounceRestitution;
						}
						this.HandleCollision(ref newPosition, GrenadeProjectile.tp._inNormal, collisionPoint, restitution);
					}
				}
				else
				{
					count = 0;
				}
			}
			while (!this._stopped && count > 0);
			base.LocalPosition = newPosition;
		}

		protected Vector3 ReflectVectorWithRestitution(Vector3 inVec, Vector3 normal, float restitution)
		{
			Vector3 perp = normal * -Vector3.Dot(inVec, normal);
			Vector3 par = inVec + perp;
			return perp * restitution + par * MathHelper.Lerp(1f, restitution, Math.Max(normal.Y, 0f));
		}

		protected virtual void HandleCollision(ref Vector3 newPosition, Vector3 collisionNormal, Vector3 collisionPoint, float restitution)
		{
			this._linearVelocity = this.ReflectVectorWithRestitution(this._linearVelocity, collisionNormal, restitution);
			if (collisionNormal.Y >= 0.75f && this._linearVelocity.LengthSquared() < 1f)
			{
				newPosition = collisionPoint;
				this._stopped = true;
				this._linearVelocity = Vector3.Zero;
				this._rotationSpeed = 0f;
				return;
			}
			this._lastPosition = collisionPoint;
			Vector3 offset = this.ReflectVectorWithRestitution(newPosition - collisionPoint, collisionNormal, restitution);
			newPosition = offset + collisionPoint;
			Vector3 vel = this._linearVelocity - collisionNormal * Vector3.Dot(this._linearVelocity, collisionNormal);
			Vector3 rotAxis = Vector3.Cross(collisionNormal, vel);
			if (rotAxis.LengthSquared() > 0f)
			{
				this._rotationAxis = Vector3.Normalize(rotAxis);
				this._rotationSpeed = vel.Length() / this.cGrenadeAABB.Max.X;
				return;
			}
			this._rotationSpeed = 0f;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (this._exploded && this.ReadyToBeRemoved())
			{
				if (this._attached)
				{
					base.AdoptChild(this._grenadeEntity);
				}
				base.RemoveFromParent();
			}
			if (this._stopped)
			{
				Vector3 positionToCheck = base.WorldPosition;
				positionToCheck.Y -= 0.5f;
				BlockType bt = BlockType.GetType(BlockTerrain.Instance.GetBlockWithChanges(positionToCheck));
				if (this.GrenadeType != GrenadeTypeEnum.Sticky && !bt.BlockPlayer)
				{
					this._stopped = false;
				}
			}
			if (!this._stopped && !this._attached)
			{
				if (this._rotationSpeed != 0f)
				{
					Quaternion deltaRotation = Quaternion.CreateFromAxisAngle(this._rotationAxis, this._rotationSpeed * dt);
					base.LocalRotation = Quaternion.Concatenate(base.LocalRotation, deltaRotation);
				}
				this._linearVelocity += Vector3.Down * (9.8f * dt);
				this._lastPosition = base.WorldPosition;
				Vector3 newPosition = this._lastPosition + this._linearVelocity * dt;
				this.MoveToNewPosition(newPosition);
			}
			if (!this._exploded)
			{
				this._timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (this._timeLeft < 0f)
				{
					this.Explode();
				}
			}
			base.OnUpdate(gameTime);
		}

		public const string cGrenadeModelName = "Props\\Weapons\\Conventional\\Grenade\\Model";

		private const float cMinSpeedSquared = 1f;

		private readonly BoundingBox cGrenadeAABB = new BoundingBox(new Vector3(-0.1f, -0.1f, -0.1f), new Vector3(0.1f, 0.1f, 0.1f));

		public GrenadeTypeEnum GrenadeType;

		protected static ParticleEffect _spashEffect;

		protected static ParticleEffect _sparkEffect;

		protected static ParticleEffect _smokeEffect;

		protected static Model _grenadeModel;

		private static GrenadeProjectile.GrenadeTraceProbe tp = new GrenadeProjectile.GrenadeTraceProbe();

		protected ModelEntity _grenadeEntity;

		protected Vector3 _linearVelocity;

		protected Vector3 _lastPosition;

		protected Vector3 _rotationAxis;

		protected float _rotationSpeed;

		protected bool _isLocal;

		protected float _timeLeft;

		protected bool _attached;

		protected bool _stopped;

		protected bool _exploded;

		protected Vector3 _stickyPosition;

		private AudioEmitter _audioEmitter = new AudioEmitter();

		private class GrenadeTraceProbe : AABBTraceProbe
		{
			public override bool TestThisType(BlockTypeEnum e)
			{
				BlockType bt = BlockType.GetType(e);
				return bt.CanBeTouched && bt.BlockPlayer;
			}

			public override bool TestThisEnemy(IShootableEnemy enemy)
			{
				return this._lastEnemy == null || this._lastEnemy != enemy;
			}

			public IShootableEnemy _lastEnemy;
		}
	}
}
