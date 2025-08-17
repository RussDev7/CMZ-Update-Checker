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
		static TorchCloud()
		{
			TorchCloud._torchModel.CopyAbsoluteBoneTransformsTo(TorchCloud.instancedModelBones);
			TorchCloud._smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchSmoke");
			TorchCloud._fireEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\TorchFire");
			ModelEntity modelEntity = new ModelEntity(TorchCloud._torchModel);
			Matrix transform = modelEntity.Skeleton["Flame"].Transform;
			TorchCloud.Offsets[4].Offset = new Vector3(0f, -0.5f, 0f);
			TorchCloud.Offsets[4].TorchRotation = Quaternion.Identity;
			TorchCloud.Offsets[4].SetFlameOffset(transform);
			TorchCloud.Offsets[2].Offset = new Vector3(0.5f, -0.25f, 0f);
			TorchCloud.Offsets[2].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.7853982f);
			TorchCloud.Offsets[2].SetFlameOffset(transform);
			TorchCloud.Offsets[1].Offset = new Vector3(0f, -0.25f, 0.5f);
			TorchCloud.Offsets[1].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -0.7853982f);
			TorchCloud.Offsets[1].SetFlameOffset(transform);
			TorchCloud.Offsets[0].Offset = new Vector3(-0.5f, -0.25f, 0f);
			TorchCloud.Offsets[0].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.7853982f);
			TorchCloud.Offsets[0].SetFlameOffset(transform);
			TorchCloud.Offsets[3].Offset = new Vector3(0f, -0.25f, -0.5f);
			TorchCloud.Offsets[3].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.7853982f);
			TorchCloud.Offsets[3].SetFlameOffset(transform);
			TorchCloud.Offsets[5].Offset = new Vector3(0f, 0.5f, 0f);
			TorchCloud.Offsets[5].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 3.1415927f);
			TorchCloud.Offsets[5].SetFlameOffset(transform);
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
			ModelEntity modelEntity = new ModelEntity(TorchCloud._torchModel);
			Matrix matrix = modelEntity.Skeleton["Flame"].Transform * modelEntity.LocalToParent;
			this._smokeEmitter.LocalToParent = matrix;
			this._fireEmitter.LocalToParent = matrix;
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
					int num = this.TorchReferences.Count - 1;
					if (i < num)
					{
						this.TorchReferences[i] = this.TorchReferences[num];
					}
					this.TorchReferences.RemoveAt(num);
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
				Matrix[] array = new Matrix[count];
				for (int i = 0; i < count; i++)
				{
					TorchReference torchReference = this.TorchReferences[i];
					TorchCloud.TorchOffset torchOffset = TorchCloud.Offsets[(int)torchReference.Facing];
					array[i] = Matrix.CreateTranslation(torchReference.Position + torchOffset.FlameOffset + new Vector3(0f, -0.5f, 0f));
					Matrix matrix = Matrix.CreateFromQuaternion(torchOffset.TorchRotation);
					matrix.Translation = torchReference.Position + torchOffset.Offset;
					this.torchMats[i] = matrix;
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
				this._fireEmitter.Instances = (this._smokeEmitter.Instances = array);
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
			foreach (ModelMesh modelMesh in TorchCloud._torchModel.Meshes)
			{
				foreach (ModelMeshPart modelMeshPart in modelMesh.MeshParts)
				{
					device.SetVertexBuffers(new VertexBufferBinding[]
					{
						new VertexBufferBinding(modelMeshPart.VertexBuffer, modelMeshPart.VertexOffset, 0),
						new VertexBufferBinding(this.instanceVertexBuffer, 0, 1)
					});
					device.Indices = modelMeshPart.IndexBuffer;
					DNAEffect dnaeffect = (DNAEffect)modelMeshPart.Effect;
					dnaeffect.World = TorchCloud.instancedModelBones[modelMesh.ParentBone.Index];
					dnaeffect.View = view;
					dnaeffect.Projection = projection;
					dnaeffect.AmbientColor = ColorF.FromARGB(1f, 0.75f, 0.75f, 0.75f);
					dnaeffect.DiffuseColor = Color.Gray;
					dnaeffect.CurrentTechnique = dnaeffect.Techniques["HardwareInstancing"];
					foreach (EffectPass effectPass in dnaeffect.CurrentTechnique.Passes)
					{
						effectPass.Apply();
						device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, modelMeshPart.NumVertices, modelMeshPart.StartIndex, modelMeshPart.PrimitiveCount, this.torchMats.Length);
					}
					dnaeffect.CurrentTechnique = dnaeffect.Techniques["NoInstancing"];
				}
			}
			base.Draw(device, gameTime, view, projection);
		}

		private static TorchCloud.TorchOffset[] Offsets = new TorchCloud.TorchOffset[6];

		private List<TorchReference> TorchReferences = new List<TorchReference>();

		public static Model _torchModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Items\\Torch\\Model");

		public static Texture2D _fireTexture;

		private ParticleEmitter _smokeEmitter;

		private ParticleEmitter _fireEmitter;

		private static Matrix[] instancedModelBones = new Matrix[TorchCloud._torchModel.Bones.Count];

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
