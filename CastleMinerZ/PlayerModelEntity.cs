using System;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class PlayerModelEntity : SkinnedModelEntity
	{
		public PlayerModelEntity(Model model)
			: base(model)
		{
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			DNAEffect dft = effect as DNAEffect;
			if (dft != null)
			{
				effect.Parameters["LightDirection1"].SetValue(-this.DirectLightDirection[0]);
				effect.Parameters["LightColor1"].SetValue(this.DirectLightColor[0]);
				effect.Parameters["LightDirection2"].SetValue(-this.DirectLightDirection[1]);
				effect.Parameters["LightColor2"].SetValue(this.DirectLightColor[1]);
				dft.AmbientColor = ColorF.FromVector3(this.AmbientLight);
				dft.EmissiveColor = Color.Black;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Color.Gray.ToVector3();
	}
}
