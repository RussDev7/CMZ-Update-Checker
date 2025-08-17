using System;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class FlyingPickupEntity : Entity
	{
		public FlyingPickupEntity(InventoryItem item, Player player, Vector3 pos)
		{
			this._displayEntity = item.CreateEntity(ItemUse.Pickup, false);
			this._target = player;
			base.Children.Add(this._displayEntity);
			this._velocity = 0f;
			base.LocalPosition = pos;
			this.Collidee = false;
			this.Collider = false;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this._displayEntity.LocalRotation *= Quaternion.CreateFromAxisAngle(Vector3.Down, (float)gameTime.ElapsedGameTime.TotalSeconds);
			if (!this._target.ValidLivingGamer)
			{
				base.RemoveFromParent();
				return;
			}
			Vector3 vector = this._target.LocalPosition - base.LocalPosition;
			float num = vector.LengthSquared();
			if (num < 0.5f)
			{
				base.RemoveFromParent();
				return;
			}
			float num2 = (float)gameTime.ElapsedGameTime.TotalSeconds;
			this._velocity += 20f * num2;
			this._velocity = Math.Min(this._velocity, 100f);
			float num3 = this._velocity * num2;
			if (num3 * num3 > num)
			{
				base.RemoveFromParent();
				return;
			}
			vector.Normalize();
			vector *= num3;
			base.LocalPosition += vector;
			base.OnUpdate(gameTime);
		}

		private const float Accel = 20f;

		private const float MaxVel = 100f;

		private const float PickupRadSqu = 4f;

		private Entity _displayEntity;

		private Player _target;

		private float _velocity;
	}
}
