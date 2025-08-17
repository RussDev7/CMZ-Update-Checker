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
				Vector3 vector = base.WorldPosition;
				if (this.AttachedToLocalPlayer)
				{
					vector = Vector3.Transform(new Vector3(0.1f, -0.3f, -0.25f), CastleMinerZGame.Instance.LocalPlayer.FPSCamera.LocalToWorld);
				}
				BlockTerrain.Instance.GetEnemyLighting(vector, ref this.DirectLightDirection[0], ref this.DirectLightColor[0], ref this.DirectLightDirection[1], ref this.DirectLightColor[1], ref this.AmbientLight);
				if (this.Context == ItemUse.Pickup)
				{
					float num = (float)Math.IEEERemainder(CastleMinerZGame.Instance.CurrentGameTime.TotalGameTime.TotalSeconds, 1.0) * 3.1415927f * 2f;
					num = 0.55f + 0.45f * (float)Math.Sin((double)num);
					this.AmbientLight = Vector3.Lerp(this.AmbientLight, Vector3.One, num);
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
				BasicEffect basicEffect = oeffect as BasicEffect;
				if (basicEffect != null)
				{
					if (mesh.Name.Contains("recolor_"))
					{
						if (this.ToolColor.A == 0)
						{
							return false;
						}
						basicEffect.DiffuseColor = this.ToolColor.ToVector3();
					}
					else if (mesh.Name.Contains("recolor2_"))
					{
						if (this.ToolColor2.A == 0)
						{
							return false;
						}
						basicEffect.DiffuseColor = this.ToolColor2.ToVector3();
					}
					else
					{
						basicEffect.DiffuseColor = Color.White.ToVector3();
					}
				}
				else
				{
					DNAEffect dnaeffect = oeffect as DNAEffect;
					if (dnaeffect != null)
					{
						dnaeffect.EmissiveColor = this.EmissiveColor;
						if (mesh.Name.Contains("recolor_"))
						{
							if (this.ToolColor.A == 0)
							{
								return false;
							}
							dnaeffect.DiffuseColor = this.ToolColor;
						}
						else if (mesh.Name.Contains("recolor2_"))
						{
							if (this.ToolColor2.A == 0)
							{
								return false;
							}
							dnaeffect.DiffuseColor = this.ToolColor2;
						}
						else
						{
							dnaeffect.DiffuseColor = Color.Gray;
						}
						if (dnaeffect.Parameters["LightDirection1"] != null)
						{
							dnaeffect.Parameters["LightDirection1"].SetValue(-this.DirectLightDirection[0]);
							dnaeffect.Parameters["LightColor1"].SetValue(this.DirectLightColor[0]);
							dnaeffect.Parameters["LightDirection2"].SetValue(-this.DirectLightDirection[1]);
							dnaeffect.Parameters["LightColor2"].SetValue(this.DirectLightColor[1]);
							dnaeffect.AmbientColor = ColorF.FromVector3(this.AmbientLight);
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

		public bool AttachedToLocalPlayer;

		public ItemUse Context = ItemUse.Hand;

		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Color.Gray.ToVector3();
	}
}
