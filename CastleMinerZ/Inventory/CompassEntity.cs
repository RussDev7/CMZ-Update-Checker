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
			for (Entity entity = base.Parent; entity != null; entity = entity.Parent)
			{
				if (entity is Player)
				{
					return (Player)entity;
				}
			}
			return null;
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (this.TrackPosition && mesh.Name.Contains("Needle"))
			{
				Player player = this.GetPlayer();
				if (player == null)
				{
					player = CastleMinerZGame.Instance.LocalPlayer;
				}
				if (player != null)
				{
					Vector3 vector = -player.WorldPosition;
					Vector3 vector2 = Vector3.TransformNormal(Vector3.Forward, player.LocalToWorld);
					vector.Y = 0f;
					vector2.Y = 0f;
					Quaternion quaternion = vector2.RotationBetween(vector);
					float z = quaternion.Z;
					quaternion.Z = quaternion.Y;
					quaternion.Y = z;
					quaternion.Normalize();
					world = Matrix.CreateFromQuaternion(quaternion) * world;
				}
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}

		public bool TrackPosition = true;
	}
}
