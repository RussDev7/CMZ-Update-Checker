using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class CompassEntity : CastleMinerToolModel
	{
		public CompassEntity(Model model, ItemUse use, bool attachedToLocalPlayer)
			: base(model, use, attachedToLocalPlayer)
		{
		}

		public Player GetPlayer()
		{
			for (Entity node = base.Parent; node != null; node = node.Parent)
			{
				if (node is Player)
				{
					return (Player)node;
				}
			}
			return null;
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (this.TrackPosition && mesh.Name.Contains("Needle"))
			{
				Player holder = this.GetPlayer();
				if (holder == null)
				{
					holder = CastleMinerZGame.Instance.LocalPlayer;
				}
				if (holder != null)
				{
					Vector3 toStart = -holder.WorldPosition;
					Vector3 forward = Vector3.TransformNormal(Vector3.Forward, holder.LocalToWorld);
					toStart.Y = 0f;
					forward.Y = 0f;
					Quaternion rot = forward.RotationBetween(toStart);
					float temp = rot.Z;
					rot.Z = rot.Y;
					rot.Y = temp;
					rot.Normalize();
					world = Matrix.CreateFromQuaternion(rot) * world;
				}
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public bool TrackPosition = true;
	}
}
