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
			float bestScore = float.MaxValue;
			bool tooFarAway = true;
			TargetSearchResult result = default(TargetSearchResult);
			result.player = null;
			Vector3 pfrom = entity.WorldPosition;
			pfrom += entity.LocalToWorld.Forward * 7f;
			float maxdistsq = maxViewDistance * maxViewDistance;
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
				{
					NetworkGamer nwg = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
					if (nwg != null)
					{
						Player p = (Player)nwg.Tag;
						if (p != null && p.ValidLivingGamer)
						{
							Vector3 pto = p.WorldPosition;
							pto.Y += 1.5f;
							Vector3 delta = pto - pfrom;
							float distancesq = delta.LengthSquared();
							if (distancesq < maxdistsq && distancesq > 0.001f)
							{
								tooFarAway = false;
								if (BlockTerrain.Instance.RegionIsLoaded(pto))
								{
									float light = BlockTerrain.Instance.GetSimpleSunlightAtPoint(pto);
									if (light > 0f)
									{
										float score = distancesq / (light * light);
										if (score < bestScore)
										{
											delta.Normalize();
											if (Vector3.Dot(delta, forward) > 0.17f)
											{
												TargetUtils.tp.Init(pfrom, pto);
												BlockTerrain.Instance.Trace(TargetUtils.tp);
												if (!TargetUtils.tp._collides)
												{
													result.player = p;
													result.distance = (float)Math.Sqrt((double)distancesq);
													result.light = light;
													result.vectorToPlayer = delta;
													bestScore = score;
												}
											}
										}
									}
								}
							}
							else if (distancesq < MathTools.Square(entity.EType.SpawnDistance * 1.25f))
							{
								tooFarAway = false;
							}
						}
					}
				}
			}
			if (tooFarAway)
			{
				entity.Removed = true;
				EnemyManager.Instance.RemoveDragon();
			}
			return result;
		}

		public static List<TargetSearchResult> FindTargetsInRange(Vector3 centerPoint, float range, bool ignoreLighting = true)
		{
			List<TargetSearchResult> results = new List<TargetSearchResult>();
			float rangeSquared = range * range;
			if (CastleMinerZGame.Instance.CurrentNetworkSession == null)
			{
				return results;
			}
			for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer nwg = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
				if (nwg != null)
				{
					Player p = (Player)nwg.Tag;
					if (p != null && p.ValidLivingGamer)
					{
						Vector3 pto = p.WorldPosition;
						pto.Y += 1.5f;
						Vector3 delta = pto - centerPoint;
						float distanceSquared = delta.LengthSquared();
						if (distanceSquared <= rangeSquared && BlockTerrain.Instance.RegionIsLoaded(pto))
						{
							float light = BlockTerrain.Instance.GetSimpleSunlightAtPoint(pto);
							if (light > 0f || ignoreLighting)
							{
								delta.Normalize();
								results.Add(new TargetSearchResult
								{
									player = p,
									distance = (float)Math.Sqrt((double)distanceSquared),
									light = light,
									vectorToPlayer = delta
								});
							}
						}
					}
				}
			}
			return results;
		}

		public const float c_playerHeightInBlocks = 1.5f;

		private const float MAX_VIEW_DOT = 0.17f;

		protected static TraceProbe tp = new TraceProbe();
	}
}
