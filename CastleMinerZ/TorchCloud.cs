using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class TorchCloud : Entity
	{
		public static void Init()
		{
			TorchCloud._torchModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Items\\Torch\\Model");
			TorchCloud.instancedModelBones = new Matrix[TorchCloud._torchModel.Bones.Count];
			TorchCloud._torchModel.CopyAbsoluteBoneTransformsTo(TorchCloud.instancedModelBones);
			TorchCloud._smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchSmoke");
			TorchCloud._fireEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchFire");
			ModelEntity modelEnt = new ModelEntity(TorchCloud._torchModel);
			Matrix flameTrans = modelEnt.Skeleton["Flame"].Transform;
			TorchCloud.Offsets[4].Offset = new Vector3(0f, -0.5f, 0f);
			TorchCloud.Offsets[4].TorchRotation = Quaternion.Identity;
			TorchCloud.Offsets[4].SetFlameOffset(flameTrans);
			TorchCloud.Offsets[2].Offset = new Vector3(0.5f, -0.25f, 0f);
			TorchCloud.Offsets[2].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.7853982f);
			TorchCloud.Offsets[2].SetFlameOffset(flameTrans);
			TorchCloud.Offsets[1].Offset = new Vector3(0f, -0.25f, 0.5f);
			TorchCloud.Offsets[1].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -0.7853982f);
			TorchCloud.Offsets[1].SetFlameOffset(flameTrans);
			TorchCloud.Offsets[0].Offset = new Vector3(-0.5f, -0.25f, 0f);
			TorchCloud.Offsets[0].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.7853982f);
			TorchCloud.Offsets[0].SetFlameOffset(flameTrans);
			TorchCloud.Offsets[3].Offset = new Vector3(0f, -0.25f, -0.5f);
			TorchCloud.Offsets[3].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.7853982f);
			TorchCloud.Offsets[3].SetFlameOffset(flameTrans);
			TorchCloud.Offsets[5].Offset = new Vector3(0f, 0.5f, 0f);
			TorchCloud.Offsets[5].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 3.1415927f);
			TorchCloud.Offsets[5].SetFlameOffset(flameTrans);
		}

		public TorchCloud(DNAGame game)
		{
			this.DrawPriority = 900;
			this._smokeEmitter = TorchCloud._smokeEffect.CreateEmitter(game);
			this._smokeEmitter.DrawPriority = 900;
			this._smokeEmitter.Emitting = true;
			this._fireEmitter = TorchCloud._fireEffect.CreateEmitter(game);
			this._fireEmitter.DrawPriority = 900;
			this._fireEmitter.Emitting = true;
			ModelEntity modelEnt = new ModelEntity(TorchCloud._torchModel);
			Matrix flameToParent = modelEnt.Skeleton["Flame"].Transform * modelEnt.LocalToParent;
			this._smokeEmitter.LocalToParent = flameToParent;
			this._fireEmitter.LocalToParent = flameToParent;
			base.Children.Add(this._smokeEmitter);
			base.Children.Add(this._fireEmitter);
		}

		public bool ContainsTorch(Vector3 blockCenter)
		{
			for (int i = 0; i < this.TorchReferences.Count; i++)
			{
				if ((double)Vector3.DistanceSquared(blockCenter, this.TorchReferences[i].Position) < 0.0625)
				{
					return true;
				}
			}
			return false;
		}

		public void AddTorch(Vector3 blockCenter, BlockFace facing)
		{
			if (!this.ContainsTorch(blockCenter))
			{
				this._listsDirty = true;
				if (facing >= BlockFace.NUM_FACES)
				{
					facing = BlockFace.POSY;
				}
				this.TorchReferences.Add(new TorchReference(blockCenter, facing));
			}
		}

		public void RemoveTorch(Vector3 blockCenter)
		{
			for (int i = 0; i < this.TorchReferences.Count; i++)
			{
				if ((double)Vector3.DistanceSquared(blockCenter, this.TorchReferences[i].Position) < 0.0625)
				{
					int c = this.TorchReferences.Count - 1;
					if (i < c)
					{
						this.TorchReferences[i] = this.TorchReferences[c];
					}
					this.TorchReferences.RemoveAt(c);
					this._listsDirty = true;
				}
			}
		}

		private void ComputeLists()
		{
			int count = this.TorchReferences.Count;
			if (this.torchMats.Length != count)
			{
				this.torchMats = new Matrix[count];
			}
			if (count != 0)
			{
				Matrix[] effectPos = new Matrix[count];
				for (int i = 0; i < count; i++)
				{
					TorchReference tref = this.TorchReferences[i];
					TorchCloud.TorchOffset offset = TorchCloud.Offsets[(int)tref.Facing];
					effectPos[i] = Matrix.CreateTranslation(tref.Position + offset.FlameOffset + new Vector3(0f, -0.5f, 0f));
					Matrix worldPos = Matrix.CreateFromQuaternion(offset.TorchRotation);
					worldPos.Translation = tref.Position + offset.Offset;
					this.torchMats[i] = worldPos;
				}
				if (this.instanceVertexBuffer == null || this.torchMats.Length > this.instanceVertexBuffer.VertexCount)
				{
					if (this.instanceVertexBuffer != null)
					{
						this.instanceVertexBuffer.Dispose();
					}
					this.instanceVertexBuffer = new VertexBuffer(CastleMinerZGame.Instance.GraphicsDevice, TorchCloud.instanceVertexDeclaration, this.torchMats.Length, BufferUsage.WriteOnly);
				}
				this.instanceVertexBuffer.SetData<Matrix>(this.torchMats, 0, count);
				this._fireEmitter.Instances = (this._smokeEmitter.Instances = effectPos);
			}
			else
			{
				this._fireEmitter.Instances = (this._smokeEmitter.Instances = new Matrix[0]);
			}
			this._listsDirty = false;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (this._listsDirty)
			{
				this.ComputeLists();
			}
			base.OnUpdate(gameTime);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			int count = this.TorchReferences.Count;
			if (this.torchMats.Length == 0)
			{
				return;
			}
			foreach (ModelMesh mesh in TorchCloud._torchModel.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					device.SetVertexBuffers(new VertexBufferBinding[]
					{
						new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
						new VertexBufferBinding(this.instanceVertexBuffer, 0, 1)
					});
					device.Indices = meshPart.IndexBuffer;
					DNAEffect effect = (DNAEffect)meshPart.Effect;
					effect.World = TorchCloud.instancedModelBones[mesh.ParentBone.Index];
					effect.View = view;
					effect.Projection = projection;
					effect.AmbientColor = ColorF.FromARGB(1f, 0.75f, 0.75f, 0.75f);
					effect.DiffuseColor = Color.Gray;
					effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];
					foreach (EffectPass pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, this.torchMats.Length);
					}
					effect.CurrentTechnique = effect.Techniques["NoInstancing"];
				}
			}
			base.Draw(device, gameTime, view, projection);
		}

		private static TorchCloud.TorchOffset[] Offsets = new TorchCloud.TorchOffset[6];

		private List<TorchReference> TorchReferences = new List<TorchReference>();

		public static Model _torchModel;

		public static Texture2D _fireTexture;

		private ParticleEmitter _smokeEmitter;

		private ParticleEmitter _fireEmitter;

		private static Matrix[] instancedModelBones;

		private static ParticleEffect _smokeEffect;

		private static ParticleEffect _fireEffect;

		private bool _listsDirty = true;

		private Matrix[] torchMats = new Matrix[0];

		private VertexBuffer instanceVertexBuffer;

		private static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration(new VertexElement[]
		{
			new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
			new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
			new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
			new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
		});

		public struct TorchOffset
		{
			public void SetFlameOffset(Matrix flameTrans)
			{
				this.FlameOffset = (flameTrans * Matrix.CreateFromQuaternion(this.TorchRotation) * Matrix.CreateTranslation(this.Offset)).Translation;
			}

			public Vector3 FlameOffset;

			public Vector3 Offset;

			public Quaternion TorchRotation;
		}
	}
}
