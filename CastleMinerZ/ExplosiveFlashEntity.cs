using System;
using DNA.Audio;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class ExplosiveFlashEntity : Entity, IEquatable<ExplosiveFlashEntity>
	{
		public ExplosiveFlashEntity(IntVector3 position)
		{
			base.LocalPosition = position + new Vector3(0.5f, -0.002f, 0.5f);
			this.BlockPosition = position;
			this._emitter = new AudioEmitter();
			this._emitter.Position = base.LocalPosition;
			this._fuseCue = SoundManager.Instance.PlayInstance("Fuse", this._emitter);
			base.BlendState = BlendState.Additive;
			base.DepthStencilState = DepthStencilState.DepthRead;
			this.DrawPriority = 300;
			this._smokeEmitter = ExplosiveFlashEntity._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			this._smokeEmitter.Emitting = true;
			this._smokeEmitter.DrawPriority = 900;
			this._smokeEmitter.LocalPosition += new Vector3(0f, 1f, 0f);
			this._smokeEmitter.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.Left, Angle.FromDegrees(90f).Radians);
			base.Children.Add(this._smokeEmitter);
			this._flashingModel = new ExplosiveFlashEntity.FlashingModelEntity();
			base.Children.Add(this._flashingModel);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			this._timer.Update(gameTime.ElapsedGameTime);
			if (this._timer.Expired)
			{
				this._timer.Reset();
				this._flashOn = !this._flashOn;
				if (this._flashOn)
				{
					this._flashingModel.Visible = true;
				}
				else
				{
					this._flashingModel.Visible = false;
				}
			}
			this._lifeTime += gameTime.ElapsedGameTime;
			if (this._lifeTime > TimeSpan.FromSeconds(3.0))
			{
				this._timer.MaxTime = TimeSpan.FromSeconds(0.125);
			}
			if (this._lifeTime > ExplosiveFlashEntity._maxLifetime && CastleMinerZGame.Instance.GameScreen != null)
			{
				CastleMinerZGame.Instance.GameScreen.RemoveExplosiveFlashModel(this.BlockPosition);
			}
			base.Update(game, gameTime);
		}

		public bool Equals(ExplosiveFlashEntity other)
		{
			return base.LocalPosition == other.LocalPosition;
		}

		private static TimeSpan _maxLifetime = TimeSpan.FromSeconds(8.0);

		private static ParticleEffect _smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchSmoke");

		private bool _flashOn = true;

		private OneShotTimer _timer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private TimeSpan _lifeTime = TimeSpan.Zero;

		public IntVector3 BlockPosition = IntVector3.Zero;

		private SoundCue3D _fuseCue;

		private AudioEmitter _emitter;

		private ParticleEmitter _smokeEmitter;

		private ExplosiveFlashEntity.FlashingModelEntity _flashingModel;

		private class FlashingModelEntity : ModelEntity
		{
			public FlashingModelEntity()
				: base(ExplosiveFlashEntity.FlashingModelEntity._model)
			{
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				BasicEffect basicEffect = (BasicEffect)effect;
				basicEffect.Alpha = 0.5f;
				basicEffect.DiffuseColor = Color.Red.ToVector3();
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}

			private static Model _model = CastleMinerZGame.Instance.Content.Load<Model>("WhiteBox");
		}
	}
}
