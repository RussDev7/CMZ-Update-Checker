using System;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public interface IShootableEnemy
	{
		void TakeDamage(Vector3 damagePosition, Vector3 damageDirection, InventoryItem.InventoryItemClass itemClass, byte shooterID);

		void AttachProjectile(Entity projectileEntity);
	}
}
