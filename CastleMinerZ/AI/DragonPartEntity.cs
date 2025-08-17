using System;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class DragonPartEntity : SkinnedModelEntity
	{
		public DragonPartEntity(DragonType type, Model model)
			: base(model)
		{
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll(3.1415927f, 0f, 0f);
			base.LocalPosition = new Vector3(0f, -23.5f, 4f) * 0.5f;
			this.DragonTexture = type.Texture;
			this.DrawPriority = (int)(520 + type.EType);
			this.Collider = false;
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is DNAEffect)
			{
				DNAEffect dnaeffect = (DNAEffect)effect;
				if (dnaeffect.Parameters["LightDirection1"] != null)
				{
					dnaeffect.Parameters["LightDirection1"].SetValue(BlockTerrain.Instance.VectorToSun);
					dnaeffect.Parameters["LightColor1"].SetValue(BlockTerrain.Instance.SunlightColor.ToVector3());
					dnaeffect.AmbientColor = ColorF.FromVector3(BlockTerrain.Instance.AmbientSunColor.ToVector3() * 0.5f);
				}
				dnaeffect.DiffuseMap = this.DragonTexture;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public const float HEIGHT_OFFSET = -23.5f;

		public const float SUB_PART_SCALE = 0.5f;

		public Texture2D DragonTexture;
	}
}
