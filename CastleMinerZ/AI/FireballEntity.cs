using System;
using DNA.Audio;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class FireballEntity : Entity
	{
		public static void Init()
		{
			FireballEntity.ParticlePackages = new FireballEntity.ParticlePackage[2];
			FireballEntity.ParticlePackages[0]._fireTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FireTrail");
			FireballEntity.ParticlePackages[0]._smokeTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeTrail");
			FireballEntity.ParticlePackages[0]._fireGlowEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FireGlow");
			FireballEntity.ParticlePackages[0]._fireBallEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FireBallEffect");
			FireballEntity.ParticlePackages[0]._flashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FlashEffect");
			FireballEntity.ParticlePackages[0]._firePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FirePuff");
			FireballEntity.ParticlePackages[0]._smokePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigSmokePuff");
			FireballEntity.ParticlePackages[0]._rockBlastEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigRockBlast");
			FireballEntity.ParticlePackages[0]._fireballModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Projectiles\\FireBall\\FireBall");
			FireballEntity.ParticlePackages[0]._flightSoundName = "Fireball";
			FireballEntity.ParticlePackages[0]._detonateSoundName = "Explosion";
			FireballEntity.ParticlePackages[1]._fireTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\IceTrail");
			FireballEntity.ParticlePackages[1]._smokeTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\PuffTrail");
			FireballEntity.ParticlePackages[1]._fireGlowEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\IceGlow");
			FireballEntity.ParticlePackages[1]._fireBallEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\IceBallEffect");
			FireballEntity.ParticlePackages[1]._flashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\IceFlash");
			FireballEntity.ParticlePackages[1]._firePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\IcePuff");
			FireballEntity.ParticlePackages[1]._smokePuffEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigPuff");
			FireballEntity.ParticlePackages[1]._rockBlastEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BigSnowBlast");
			FireballEntity.ParticlePackages[1]._fireballModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Projectiles\\SnowBall\\Snowball");
			FireballEntity.ParticlePackages[1]._flightSoundName = "Iceball";
			FireballEntity.ParticlePackages[1]._detonateSoundName = "Freeze";
		}

		public FireballEntity(Vector3 spawnposition, Vector3 target, int index, DragonType dragonType, bool spawnedLocally)
		{
			this.EType = dragonType;
			this.ParticleDef = FireballEntity.ParticlePackages[(int)this.EType.DamageType];
			this.model = new FireballModelEntity(this.ParticleDef._fireballModel);
			this.model.EnableDefaultLighting();
			base.Children.Add(this.model);
			this.FireballIndex = index;
			this.SpawnedLocally = spawnedLocally;
			this.WasInLoadedArea = false;
			if (CastleMinerZGame.Instance.IsActive)
			{
				this.SmokeEmitter = this.ParticleDef._smokeTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
				this.SmokeEmitter.Emitting = true;
				this.SmokeEmitter.DrawPriority = 900;
				base.Children.Add(this.SmokeEmitter);
				this.FireEmitter = this.ParticleDef._fireTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
				this.FireEmitter.Emitting = true;
				this.FireEmitter.DrawPriority = 900;
				base.Children.Add(this.FireEmitter);
				this.FireGlowEmitter = this.ParticleDef._fireGlowEffect.CreateEmitter(CastleMinerZGame.Instance);
				this.FireGlowEmitter.Emitting = true;
				this.FireGlowEmitter.DrawPriority = 900;
				base.Children.Add(this.FireGlowEmitter);
				this.FireBallEmitter = this.ParticleDef._fireBallEffect.CreateEmitter(CastleMinerZGame.Instance);
				this.FireBallEmitter.Emitting = true;
				this.FireBallEmitter.DrawPriority = 900;
				base.Children.Add(this.FireBallEmitter);
			}
			base.LocalPosition = spawnposition;
			Vector3 vector = target - spawnposition;
			this.Velocity = vector * (this.EType.FireballVelocity / vector.Length());
			float num = (float)Math.Atan2((double)(-(double)this.Velocity.X), (double)(-(double)this.Velocity.Z));
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(num, 0f, 0f);
			this.Detonated = false;
			EnemyManager.Instance.AddFireball(this);
			this.FireballCue = SoundManager.Instance.PlayInstance(this.ParticleDef._flightSoundName, this.SoundEmitter);
		}

		public void Detonate(Vector3 position)
		{
			if (this.Detonated)
			{
				return;
			}
			this.Detonated = true;
			this.model.RemoveFromParent();
			if (this.SmokeEmitter != null && this.FireEmitter != null)
			{
				this.FireGlowEmitter.Emitting = false;
				this.FireBallEmitter.Emitting = false;
				this.FireEmitter.Emitting = false;
				this.SmokeEmitter.Emitting = false;
			}
			this.FireballCue.Stop(AudioStopOptions.Immediate);
			SoundManager.Instance.PlayInstance(this.ParticleDef._detonateSoundName, this.SoundEmitter);
			Scene scene = base.Scene;
			if (scene == null || scene.Children == null || !CastleMinerZGame.Instance.IsActive)
			{
				return;
			}
			ParticleEmitter particleEmitter = this.ParticleDef._flashEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			particleEmitter.LocalPosition = position;
			particleEmitter.DrawPriority = 900;
			scene.Children.Add(particleEmitter);
			particleEmitter = this.ParticleDef._firePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			particleEmitter.LocalPosition = position;
			particleEmitter.DrawPriority = 900;
			scene.Children.Add(particleEmitter);
			particleEmitter = this.ParticleDef._smokePuffEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			particleEmitter.LocalPosition = position;
			particleEmitter.DrawPriority = 900;
			scene.Children.Add(particleEmitter);
			particleEmitter = this.ParticleDef._rockBlastEffect.CreateEmitter(CastleMinerZGame.Instance);
			particleEmitter.Reset();
			particleEmitter.Emitting = true;
			particleEmitter.LocalPosition = position;
			particleEmitter.DrawPriority = 900;
			scene.Children.Add(particleEmitter);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			this.SoundEmitter.Position = base.LocalPosition;
			this.SoundEmitter.Forward = base.LocalToWorld.Forward;
			this.SoundEmitter.Up = Vector3.Up;
			this.SoundEmitter.Velocity = this.Velocity;
			if (this.Detonated)
			{
				if (this.SmokeEmitter == null && this.FireEmitter == null)
				{
					EnemyManager.Instance.RemoveFireball(this);
				}
				else if (!this.FireEmitter.HasActiveParticles && !this.SmokeEmitter.HasActiveParticles)
				{
					EnemyManager.Instance.RemoveFireball(this);
				}
			}
			else
			{
				Vector3 vector = base.LocalPosition + this.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (vector.Y < -66f)
				{
					EnemyManager.Instance.RemoveFireball(this);
					return;
				}
				if (!BlockTerrain.Instance.IsInsideWorld(vector))
				{
					if (this.WasInLoadedArea)
					{
						EnemyManager.Instance.RemoveFireball(this);
						return;
					}
					base.LocalPosition = vector;
					this.model.LocalRotation = Quaternion.CreateFromYawPitchRoll(0f, (float)gameTime.TotalGameTime.TotalSeconds * 3f % 6.2831855f, 0f);
					base.Update(game, gameTime);
					return;
				}
				else
				{
					this.WasInLoadedArea = true;
					this.TraceProbe.Init(base.LocalPosition, vector, this.FireballAABB);
					Vector3 worldPosition = CastleMinerZGame.Instance.LocalPlayer.WorldPosition;
					BoundingBox playerAABB = CastleMinerZGame.Instance.LocalPlayer.PlayerAABB;
					playerAABB.Min += worldPosition;
					playerAABB.Max += worldPosition;
					this.TraceProbe.TestBoundBox(playerAABB);
					if (!this.TraceProbe._collides)
					{
						this.TraceProbe.Reset();
						BlockTerrain.Instance.Trace(this.TraceProbe);
					}
					if (this.TraceProbe._collides)
					{
						Vector3 intersection = this.TraceProbe.GetIntersection();
						this.Detonate(intersection);
						if (this.SpawnedLocally)
						{
							EnemyManager.Instance.DetonateFireball(intersection, this.FireballIndex, this.EType);
						}
					}
					else
					{
						base.LocalPosition = vector;
						this.model.LocalRotation = Quaternion.CreateFromYawPitchRoll(0f, (float)gameTime.TotalGameTime.TotalSeconds * 3f % 6.2831855f, 0f);
					}
				}
			}
			base.Update(game, gameTime);
		}

		public AudioEmitter SoundEmitter = new AudioEmitter();

		private SoundCue3D FireballCue;

		public FireballModelEntity model;

		private FireballEntity.FireballTraceProbe TraceProbe = new FireballEntity.FireballTraceProbe();

		public BoundingBox FireballAABB = new BoundingBox(new Vector3(-0.57f), new Vector3(0.57f));

		public Vector3 Target;

		public Vector3 Velocity;

		public ParticleEmitter SmokeEmitter;

		public ParticleEmitter FireEmitter;

		public ParticleEmitter FireBallEmitter;

		public ParticleEmitter FireGlowEmitter;

		public int FireballIndex;

		public bool SpawnedLocally;

		public bool WasInLoadedArea;

		public bool Detonated;

		public DragonType EType;

		private FireballEntity.ParticlePackage ParticleDef;

		private static FireballEntity.ParticlePackage[] ParticlePackages;

		private class FireballTraceProbe : AABBTraceProbe
		{
			public override bool TestThisType(BlockTypeEnum e)
			{
				return e != BlockTypeEnum.NumberOfBlocks && BlockType.GetType(e).BlockPlayer;
			}
		}

		private struct ParticlePackage
		{
			public ParticleEffect _fireBallEffect;

			public ParticleEffect _smokeTrailEffect;

			public ParticleEffect _fireTrailEffect;

			public ParticleEffect _fireGlowEffect;

			public ParticleEffect _flashEffect;

			public ParticleEffect _firePuffEffect;

			public ParticleEffect _smokePuffEffect;

			public ParticleEffect _rockBlastEffect;

			public Model _fireballModel;

			public string _flightSoundName;

			public string _detonateSoundName;
		}
	}
}
