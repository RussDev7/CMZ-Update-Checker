using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Utils
{
	public class TargetUtils
	{
		public static TargetSearchResult FindBestTarget(DragonEntity entity, Vector3 forward, float maxViewDistance)
		{
			float num = float.MaxValue;
			bool flag = true;
			TargetSearchResult targetSearchResult = default(TargetSearchResult);
			targetSearchResult.player = null;
			Vector3 vector = entity.WorldPosition;
			vector += entity.LocalToWorld.Forward * 7f;
			float num2 = maxViewDistance * maxViewDistance;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (networkGamer != null)
					{
						Player player = (Player)networkGamer.Tag;
						if (player != null && player.ValidLivingGamer)
						{
							Vector3 worldPosition = player.WorldPosition;
							worldPosition.Y += 1.5f;
							Vector3 vector2 = worldPosition - vector;
							float num3 = vector2.LengthSquared();
							if (num3 < num2 && num3 > 0.001f)
							{
								flag = false;
								if (BlockTerrain.Instance.RegionIsLoaded(worldPosition))
								{
									float simpleSunlightAtPoint = BlockTerrain.Instance.GetSimpleSunlightAtPoint(worldPosition);
									if (simpleSunlightAtPoint > 0f)
									{
										float num4 = num3 / (simpleSunlightAtPoint * simpleSunlightAtPoint);
										if (num4 < num)
										{
											vector2.Normalize();
											if (Vector3.Dot(vector2, forward) > 0.17f)
											{
												TargetUtils.tp.Init(vector, worldPosition);
												BlockTerrain.Instance.Trace(TargetUtils.tp);
												if (!TargetUtils.tp._collides)
												{
													targetSearchResult.player = player;
													targetSearchResult.distance = (float)Math.Sqrt((double)num3);
													targetSearchResult.light = simpleSunlightAtPoint;
													targetSearchResult.vectorToPlayer = vector2;
													num = num4;
												}
											}
										}
									}
								}
							}
							else if (num3 < MathTools.Square(entity.EType.SpawnDistance * 1.25f))
							{
								flag = false;
							}
						}
					}
				}
			}
			if (flag)
			{
				entity.Removed = true;
				EnemyManager.Instance.RemoveDragon();
			}
			return targetSearchResult;
		}

		public static List<TargetSearchResult> FindTargetsInRange(Vector3 centerPoint, float range, bool ignoreLighting = true)
		{
			List<TargetSearchResult> list = new List<TargetSearchResult>();
			float num = range * range;
			if (CastleMinerZGame.Instance.CurrentNetworkSession == null)
			{
				return list;
			}
			for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
				if (networkGamer != null)
				{
					Player player = (Player)networkGamer.Tag;
					if (player != null && player.ValidLivingGamer)
					{
						Vector3 worldPosition = player.WorldPosition;
						worldPosition.Y += 1.5f;
						Vector3 vector = worldPosition - centerPoint;
						float num2 = vector.LengthSquared();
						if (num2 <= num && BlockTerrain.Instance.RegionIsLoaded(worldPosition))
						{
							float simpleSunlightAtPoint = BlockTerrain.Instance.GetSimpleSunlightAtPoint(worldPosition);
							if (simpleSunlightAtPoint > 0f || ignoreLighting)
							{
								vector.Normalize();
								list.Add(new TargetSearchResult
								{
									player = player,
									distance = (float)Math.Sqrt((double)num2),
									light = simpleSunlightAtPoint,
									vectorToPlayer = vector
								});
							}
						}
					}
				}
			}
			return list;
		}

		public const float c_playerHeightInBlocks = 1.5f;

		private const float MAX_VIEW_DOT = 0.17f;

		protected static TraceProbe tp = new TraceProbe();
	}
}
