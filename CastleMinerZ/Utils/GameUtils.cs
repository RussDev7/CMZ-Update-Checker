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
			for (int yLoc = Location.Y + 1; yLoc < clearRadius + Location.Y + 1; yLoc++)
			{
				for (int xLoc = Location.X - clearRadius; xLoc < clearRadius + Location.X; xLoc++)
				{
					for (int zLoc = Location.Z - clearRadius; zLoc < clearRadius + Location.Z; zLoc++)
					{
						IntVector3 loc = new IntVector3(xLoc, yLoc, zLoc);
						if (!(loc == Location))
						{
							BlockTypeEnum blockType = InGameHUD.GetBlock(loc);
							if (!BlockType.IsSpawnerClickable(blockType) && !BlockType.ShouldDropLoot(blockType))
							{
								AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, loc, BlockTypeEnum.Empty);
							}
						}
					}
				}
			}
		}
	}
}
