using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class CrackBoxEntity : ModelEntity
	{
		public CrackBoxEntity()
			: base(CrackBoxEntity._model)
		{
			base.BlendState = new BlendState();
			base.BlendState.AlphaSourceBlend = Blend.Zero;
			base.BlendState.AlphaDestinationBlend = Blend.SourceAlpha;
			base.BlendState.ColorSourceBlend = Blend.Zero;
			base.BlendState.ColorDestinationBlend = Blend.SourceColor;
			base.DepthStencilState = DepthStencilState.DepthRead;
			this.DrawPriority = 300;
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (this.CrackAmount <= 0.1f)
			{
				return false;
			}
			float amount = this.CrackAmount;
			if (amount >= 1f)
			{
				amount = 0.99f;
			}
			effect.Parameters["crackAmount"].SetValue(amount);
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public float CrackAmount = 0.9f;

		private static Model _model = CastleMinerZGame.Instance.Content.Load<Model>("CrackBox");
	}
}
