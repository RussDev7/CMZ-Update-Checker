using System;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Utils
{
	public class GameUtils
	{
		public static void ClearSurroundingBlocks(IntVector3 Location, int clearRadius = 3)
		{
			for (int i = Location.Y + 1; i < clearRadius + Location.Y + 1; i++)
			{
				for (int j = Location.X - clearRadius; j < clearRadius + Location.X; j++)
				{
					for (int k = Location.Z - clearRadius; k < clearRadius + Location.Z; k++)
					{
						IntVector3 intVector = new IntVector3(j, i, k);
						if (!(intVector == Location))
						{
							BlockTypeEnum block = InGameHUD.GetBlock(intVector);
							if (!BlockType.IsSpawnerClickable(block) && !BlockType.ShouldDropLoot(block))
							{
								AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, intVector, BlockTypeEnum.Empty);
							}
						}
					}
				}
			}
		}
	}
}
