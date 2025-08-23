using System;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class CastleMinerToolModel : ModelEntity
	{
		public CastleMinerToolModel(Model model, ItemUse use, bool attachedToLocalPlayer)
			: base(model)
		{
			this.AttachedToLocalPlayer = false;
			this.Context = use;
			this.AttachedToLocalPlayer = attachedToLocalPlayer;
		}

		public void CalculateLighting()
		{
			if (this.Context != ItemUse.UI)
			{
				Vector3 pos = base.WorldPosition;
				if (this.AttachedToLocalPlayer)
				{
					pos = Vector3.Transform(new Vector3(0.1f, -0.3f, -0.25f), CastleMinerZGame.Instance.LocalPlayer.FPSCamera.LocalToWorld);
				}
				BlockTerrain.Instance.GetEnemyLighting(pos, ref this.DirectLightDirection[0], ref this.DirectLightColor[0], ref this.DirectLightDirection[1], ref this.DirectLightColor[1], ref this.AmbientLight);
				if (this.Context == ItemUse.Pickup)
				{
					float pulse = (float)Math.IEEERemainder(CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime.TotalSeconds, 1.0) * 3.1415927f * 2f;
					pulse = 0.55f + 0.45f * (float)Math.Sin((double)pulse);
					this.AmbientLight = Vector3.Lerp(this.AmbientLight, Vector3.One, pulse);
				}
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this.CalculateLighting();
			base.OnUpdate(gameTime);
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect oeffect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (base.SetEffectParams(mesh, oeffect, gameTime, world, view, projection))
			{
				BasicEffect effect = oeffect as BasicEffect;
				if (effect != null)
				{
					if (mesh.Name.Contains("recolor_"))
					{
						if (this.ToolColor.A == 0)
						{
							return false;
						}
						effect.DiffuseColor = this.ToolColor.ToVector3();
					}
					else if (mesh.Name.Contains("recolor2_"))
					{
						if (this.ToolColor2.A == 0)
						{
							return false;
						}
						effect.DiffuseColor = this.ToolColor2.ToVector3();
					}
					else
					{
						effect.DiffuseColor = Color.White.ToVector3();
					}
				}
				else
				{
					DNAEffect dnaEffect = oeffect as DNAEffect;
					if (dnaEffect != null)
					{
						dnaEffect.EmissiveColor = this.EmissiveColor;
						if (mesh.Name.Contains("recolor_"))
						{
							if (this.ToolColor.A == 0)
							{
								return false;
							}
							dnaEffect.DiffuseColor = this.ToolColor;
						}
						else if (mesh.Name.Contains("recolor2_"))
						{
							if (this.ToolColor2.A == 0)
							{
								return false;
							}
							dnaEffect.DiffuseColor = this.ToolColor2;
						}
						else
						{
							dnaEffect.DiffuseColor = this.DiffuseColor;
						}
						if (dnaEffect.Parameters["LightDirection1"] != null)
						{
							dnaEffect.Parameters["LightDirection1"].SetValue(-this.DirectLightDirection[0]);
							dnaEffect.Parameters["LightColor1"].SetValue(this.DirectLightColor[0]);
							dnaEffect.Parameters["LightDirection2"].SetValue(-this.DirectLightDirection[1]);
							dnaEffect.Parameters["LightColor2"].SetValue(this.DirectLightColor[1]);
							dnaEffect.AmbientColor = ColorF.FromVector3(this.AmbientLight);
						}
					}
				}
				return true;
			}
			return false;
		}

		public Color ToolColor = Color.Gray;

		public Color ToolColor2 = Color.Gray;

		public Color EmissiveColor = Color.Black;

		public Color DiffuseColor = Color.Gray;

		public bool AttachedToLocalPlayer;

		public ItemUse Context = ItemUse.Hand;

		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Color.Gray.ToVector3();
	}
}
