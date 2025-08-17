using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeLauncherInventoryItemClass : GrenadeLauncherBaseInventoryItemClass
	{
		public GrenadeLauncherInventoryItemClass(InventoryItemIDs id, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\Launcher\\Model"), name, description, TimeSpan.FromMinutes(0.016666666666666666), damage, durabilitydamage, ammotype, "RPGLaunch", "ShotGunReload")
		{
		}
	}
}
