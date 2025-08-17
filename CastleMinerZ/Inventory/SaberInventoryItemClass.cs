using System;
using DNA.Audio;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class SaberInventoryItemClass : PickInventoryItemClass
	{
		public SaberInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, Model model, string name, string description, float meleeDamage)
			: base(id, material, model, name, description, 1f)
		{
			this.BeamColor = CMZColors.GetLaserMaterialcColor(this.Material);
			this.ToolColor = Color.Gray;
			this._activeSound = "LightSaber";
			this.ItemSelfDamagePerUse = 0.005f;
		}

		public override void OnItemEquipped()
		{
			if (this._activeSound != null)
			{
				this._activeSoundCue = SoundManager.Instance.PlayInstance(this._activeSound);
			}
			base.OnItemEquipped();
		}

		public override void OnItemUnequipped()
		{
			if (this._activeSoundCue != null && this._activeSoundCue.IsPlaying)
			{
				this._activeSoundCue.Stop(AudioStopOptions.Immediate);
			}
			base.OnItemUnequipped();
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			SaberInventoryItemClass.SaberModelEntity saberModelEntity = new SaberInventoryItemClass.SaberModelEntity(this._model, use, attachedToLocalPlayer);
			saberModelEntity.BeamColor = this.BeamColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, -0.871792f) * Quaternion.CreateFromYawPitchRoll(-0.98174775f, 0f, 0f);
				Matrix matrix = Matrix.Transform(Matrix.CreateScale(89.6f / saberModelEntity.GetLocalBoundingSphere().Radius), quaternion);
				matrix.Translation += matrix.Translation + new Vector3(11f, -22f, 0f);
				saberModelEntity.LocalToParent = matrix;
				saberModelEntity.EnableDefaultLighting();
				break;
			}
			}
			return saberModelEntity;
		}

		private Color BeamColor;

		private Cue _activeSoundCue;

		private string _activeSound;

		public class SaberModelEntity : CastleMinerToolModel
		{
			public SaberModelEntity(Model model, ItemUse use, bool attachedToLocalPlayer)
				: base(model, use, attachedToLocalPlayer)
			{
			}

			public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
			{
				base.CalculateLighting();
				int count = base.Model.Meshes.Count;
				int i = 0;
				IL_00C4:
				while (i < count)
				{
					ModelMesh modelMesh = base.Model.Meshes[i];
					Matrix matrix = this._worldBoneTransforms[modelMesh.ParentBone.Index];
					int count2 = modelMesh.Effects.Count;
					for (int j = 0; j < count2; j++)
					{
						if (!this.SetEffectParams(modelMesh, modelMesh.Effects[j], gameTime, matrix, view, projection))
						{
							IL_00C0:
							i++;
							goto IL_00C4;
						}
					}
					if (modelMesh.Name.Contains("Beam"))
					{
						BlendState blendState = device.BlendState;
						device.BlendState = BlendState.Additive;
						modelMesh.Draw();
						device.BlendState = blendState;
						goto IL_00C0;
					}
					modelMesh.Draw();
					goto IL_00C0;
				}
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect oeffect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				bool flag = base.SetEffectParams(mesh, oeffect, gameTime, world, view, projection);
				DNAEffect dnaeffect = oeffect as DNAEffect;
				if (dnaeffect != null && mesh.Name.Contains("Beam"))
				{
					if (this.ToolColor.A == 0)
					{
						return false;
					}
					dnaeffect.DiffuseColor = this.BeamColor;
				}
				return flag;
			}

			public Color BeamColor;
		}
	}
}
