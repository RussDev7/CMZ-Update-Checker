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
			Vector3 dir = this._target.LocalPosition - base.LocalPosition;
			float lengthSQ = dir.LengthSquared();
			if (lengthSQ < 0.5f)
			{
				base.RemoveFromParent();
				return;
			}
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			this._velocity += 20f * dt;
			this._velocity = Math.Min(this._velocity, 100f);
			float dv = this._velocity * dt;
			if (dv * dv > lengthSQ)
			{
				base.RemoveFromParent();
				return;
			}
			dir.Normalize();
			dir *= dv;
			base.LocalPosition += dir;
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
