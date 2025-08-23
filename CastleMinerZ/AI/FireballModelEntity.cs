using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.AI
{
	public class FireballModelEntity : ModelEntity
	{
		public FireballModelEntity(Model fireballModel)
			: base(fireballModel)
		{
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			effect.CurrentTechnique = effect.Techniques[0];
			if (effect is BasicEffect)
			{
				BasicEffect be = (BasicEffect)effect;
				be.World = world;
				be.View = view;
				be.Projection = projection;
				be.EmissiveColor = Vector3.Zero;
				be.DiffuseColor = Vector3.One;
				be.AmbientLightColor = Vector3.One;
				be.DirectionalLight0.Enabled = false;
				be.DirectionalLight1.Enabled = false;
				be.DirectionalLight2.Enabled = false;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
