using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class GPSMarkerEntity : ModelEntity
	{
		public GPSMarkerEntity()
			: base(GPSMarkerEntity.MarkerModel)
		{
			base.EnableDefaultLighting();
			base.EnablePerPixelLighting();
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll((float)gameTime.TotalGameTime.TotalSeconds * 2f % 6.2831855f, 0f, 0f);
			base.Update(game, gameTime);
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect oeffect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (base.SetEffectParams(mesh, oeffect, gameTime, world, view, projection))
			{
				BasicEffect basicEffect = mesh.Effects[0] as BasicEffect;
				if (basicEffect != null)
				{
					if (mesh.Name.Contains("recolor_"))
					{
						if (this.color.A == 0)
						{
							return false;
						}
						basicEffect.DiffuseColor = this.color.ToVector3();
					}
					else
					{
						basicEffect.DiffuseColor = Color.Gray.ToVector3();
					}
				}
				return true;
			}
			return false;
		}

		private static Model MarkerModel = CastleMinerZGame.Instance.Content.Load<Model>("Marker");

		public Color color = Color.Gray;
	}
}
