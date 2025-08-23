using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class ClockEntity : CastleMinerToolModel
	{
		public ClockEntity(Model model, ItemUse use, bool attachedToLocalPlayer)
			: base(model, use, attachedToLocalPlayer)
		{
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (this.TrackPosition && mesh.Name.Contains("Needle") && CastleMinerZGame.Instance.GameScreen != null)
			{
				Quaternion rot = Quaternion.CreateFromAxisAngle(Vector3.Down, 6.2831855f * CastleMinerZGame.Instance.GameScreen.TimeOfDay);
				float temp = rot.Z;
				rot.Z = rot.Y;
				rot.Y = temp;
				rot.Normalize();
				world = Matrix.CreateFromQuaternion(rot) * world;
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public bool TrackPosition = true;
	}
}
