using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class GPSEntity : CastleMinerToolModel
	{
		public GPSEntity(Model model, ItemUse use, bool attachedToLocalPlayer)
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
					Vector3 toLocation;
					Vector3 forward;
					if (holder == CastleMinerZGame.Instance.LocalPlayer && CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem is GPSItem)
					{
						GPSItem gps = (GPSItem)CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem;
						toLocation = gps.PointToLocation - holder.WorldPosition;
						forward = Vector3.TransformNormal(Vector3.Forward, holder.LocalToWorld);
					}
					else
					{
						toLocation = -holder.WorldPosition;
						forward = Vector3.TransformNormal(Vector3.Forward, holder.LocalToWorld);
					}
					toLocation.Y = 0f;
					forward.Y = 0f;
					Quaternion rot = forward.RotationBetween(toLocation);
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
