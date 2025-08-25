using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class Selector : ModelEntity
	{
		public static void Init()
		{
			Selector._selectorModel = CastleMinerZGame.Instance.Content.Load<Model>("Selector");
		}

		public Selector()
			: base(Selector._selectorModel)
		{
			base.RasterizerState = RasterizerState.CullNone;
			DepthStencilState state = new DepthStencilState();
			state.DepthBufferFunction = CompareFunction.Less;
			state.DepthBufferWriteEnable = false;
			base.BlendState = BlendState.AlphaBlend;
			base.DepthStencilState = state;
			this.DrawPriority = 400;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this.u1 += (float)gameTime.ElapsedGameTime.TotalSeconds / 5f;
			this.u2 -= (float)gameTime.ElapsedGameTime.TotalSeconds / 5f;
			this.u1 -= (float)((int)this.u1);
			this.u2 -= (float)((int)this.u2);
			base.OnUpdate(gameTime);
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (CastleMinerZGame.Instance.DrawingReflection)
			{
				return false;
			}
			if (mesh.Name == "Box001")
			{
				effect.Parameters["uvOffset"].SetValue(new Vector2(this.u1, 0f));
			}
			if (mesh.Name == "Box002")
			{
				effect.Parameters["uvOffset"].SetValue(new Vector2(this.u2, 0f));
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public static Model _selectorModel;

		private float u1;

		private float u2;
	}
}
