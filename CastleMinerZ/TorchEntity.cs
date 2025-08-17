using System;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class TorchEntity : Entity
	{
		public bool HasFlame
		{
			get
			{
				return this._hasFlame;
			}
			set
			{
				if (value)
				{
					this.AddFlame();
					return;
				}
				this.RemoveFlame();
			}
		}

		public TorchEntity(bool hasParticles)
		{
			this._modelEnt = new TorchEntity.TorchModelEntity();
			base.Children.Add(this._modelEnt);
			this.HasFlame = hasParticles;
			this.SetPosition(this.AttachedFace);
		}

		protected override void OnParentChanged(Entity oldParent, Entity newParent)
		{
			if (newParent == null)
			{
				this.RemoveFlame();
			}
			base.OnParentChanged(oldParent, newParent);
		}

		public void RemoveFlame()
		{
			if (this._hasFlame)
			{
				this._hasFlame = false;
				if (this._smokeEmitter != null)
				{
					this._smokeEmitter.RemoveFromParent();
				}
				if (this._fireEmitter != null)
				{
					this._fireEmitter.RemoveFromParent();
				}
			}
		}

		public void AddFlame()
		{
			if (!this._hasFlame)
			{
				this._hasFlame = true;
				this._smokeEmitter = TorchEntity._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				this._smokeEmitter.Emitting = true;
				this._smokeEmitter.DrawPriority = 900;
				this._modelEnt.Children.Add(this._smokeEmitter);
				this._fireEmitter = TorchEntity._fireEffect.CreateEmitter(CastleMinerZGame.Instance);
				this._fireEmitter.Emitting = true;
				this._fireEmitter.DrawPriority = 900;
				this._modelEnt.Children.Add(this._fireEmitter);
				Matrix transform = this._modelEnt.Skeleton["Flame"].Transform;
				this._smokeEmitter.LocalToParent = transform;
				this._fireEmitter.LocalToParent = transform;
			}
		}

		public void SetPosition(BlockFace face)
		{
			this.AttachedFace = face;
			switch (this.AttachedFace)
			{
			case BlockFace.POSX:
				this._modelEnt.LocalPosition = new Vector3(-0.5f, -0.25f, 0f);
				this._modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.7853982f);
				return;
			case BlockFace.NEGZ:
				this._modelEnt.LocalPosition = new Vector3(0f, -0.25f, 0.5f);
				this._modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -0.7853982f);
				return;
			case BlockFace.NEGX:
				this._modelEnt.LocalPosition = new Vector3(0.5f, -0.25f, 0f);
				this._modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.7853982f);
				return;
			case BlockFace.POSZ:
				this._modelEnt.LocalPosition = new Vector3(0f, -0.25f, -0.5f);
				this._modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.7853982f);
				return;
			case BlockFace.POSY:
				this._modelEnt.LocalPosition = new Vector3(0f, -0.5f, 0f);
				this._modelEnt.LocalRotation = Quaternion.Identity;
				return;
			case BlockFace.NEGY:
				this._modelEnt.LocalPosition = new Vector3(0f, 0.5f, 0f);
				this._modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 3.1415927f);
				return;
			case BlockFace.NUM_FACES:
				this._modelEnt.LocalPosition = Vector3.Zero;
				this._modelEnt.LocalRotation = Quaternion.Identity;
				return;
			default:
				return;
			}
		}

		public static Model _torchModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Items\\Torch\\Model");

		private static ParticleEffect _smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchSmoke");

		private static ParticleEffect _fireEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchFire");

		private BlockFace AttachedFace = BlockFace.NUM_FACES;

		private ModelEntity _modelEnt;

		private ParticleEmitter _smokeEmitter;

		private ParticleEmitter _fireEmitter;

		private bool _hasFlame;

		private class TorchModelEntity : ModelEntity
		{
			public TorchModelEntity()
				: base(TorchEntity._torchModel)
			{
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dnaeffect = effect as DNAEffect;
				if (dnaeffect != null)
				{
					dnaeffect.EmissiveColor = Color.Black;
					if (dnaeffect.Parameters["LightDirection1"] != null)
					{
						dnaeffect.Parameters["LightDirection1"].SetValue(-this.DirectLightDirection[0]);
						dnaeffect.Parameters["LightColor1"].SetValue(this.DirectLightColor[0]);
						dnaeffect.Parameters["LightDirection2"].SetValue(-this.DirectLightDirection[1]);
						dnaeffect.Parameters["LightColor2"].SetValue(this.DirectLightColor[1]);
						dnaeffect.AmbientColor = ColorF.FromVector3(this.AmbientLight);
					}
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}

			protected override void OnUpdate(GameTime gameTime)
			{
				Vector3 vector = Vector3.Transform(new Vector3(0.1f, -0.3f, -0.25f), CastleMinerZGame.Instance.LocalPlayer.FPSCamera.LocalToWorld);
				BlockTerrain.Instance.GetEnemyLighting(vector, ref this.DirectLightDirection[0], ref this.DirectLightColor[0], ref this.DirectLightDirection[1], ref this.DirectLightColor[1], ref this.AmbientLight);
				base.OnUpdate(gameTime);
			}

			public Vector3[] DirectLightColor = new Vector3[2];

			public Vector3[] DirectLightDirection = new Vector3[2];

			public Vector3 AmbientLight = Color.Gray.ToVector3();
		}
	}
}
