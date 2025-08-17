using System;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Utils
{
	public struct TargetSearchResult
	{
		public Vector3 vectorToPlayer;

		public Player player;

		public float distance;

		public float light;
	}
}
