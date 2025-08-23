using System;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DragonBaseState : IFSMState<DragonEntity>
	{
		protected static bool CanSeePosition(DragonEntity entity, Vector3 target)
		{
			Vector3 pfrom = entity.WorldPosition;
			pfrom += entity.LocalToWorld.Forward * 7f;
			DragonBaseState.tp.Init(pfrom, target);
			BlockTerrain.Instance.Trace(DragonBaseState.tp);
			return !DragonBaseState.tp._collides;
		}

		public static DragonBaseState GetNextAttackType(DragonEntity entity)
		{
			if (entity.ChancesToNotAttack == 0 && entity.EType.ChanceOfHoverAttack > MathTools.RandomFloat())
			{
				return DragonStates.HoverAttack;
			}
			return DragonStates.StrafeAttack;
		}

		public static bool DoViewCheck(DragonEntity entity, float dt, float interval)
		{
			entity.TimeLeftBeforeNextViewCheck -= dt;
			if (entity.TimeLeftBeforeNextViewCheck <= 0f)
			{
				entity.TimeLeftBeforeNextViewCheck += DragonBaseState.CalculateNextCheckInterval(entity, interval);
				return true;
			}
			return false;
		}

		public static bool SearchForNewTarget(DragonEntity entity, float dt)
		{
			if (DragonBaseState.DoViewCheck(entity, dt, entity.EType.SlowViewCheckInterval))
			{
				TargetSearchResult searchResult = TargetUtils.FindBestTarget(entity, entity.LocalToWorld.Forward, entity.EType.MaxViewDistance);
				if (searchResult.player != null)
				{
					entity.Target = searchResult.player;
					entity.TravelTarget = searchResult.player.WorldPosition;
					if (entity.ChancesToNotAttack != 0)
					{
						entity.NextSound = DragonSoundEnum.CRY;
					}
					entity.ChancesToNotAttack = 0;
					entity.TimeLeftTilShotsHeard = 0f;
					if (entity.Target != CastleMinerZGame.Instance.LocalPlayer && CastleMinerZGame.Instance.LocalPlayer != null && CastleMinerZGame.Instance.LocalPlayer.ValidGamer && Vector3.DistanceSquared(entity.TravelTarget, CastleMinerZGame.Instance.LocalPlayer.WorldPosition) > 22500f)
					{
						entity.MigrateDragonTo = entity.Target;
						entity.MigrateDragon = true;
					}
					return true;
				}
			}
			if (entity.ChancesToNotAttack != 0)
			{
				entity.TimeLeftTilShotsHeard -= dt;
				if (entity.TimeLeftTilShotsHeard <= 0f)
				{
					float maxdistsq = entity.EType.MaxViewDistance * entity.EType.MaxViewDistance * 2.25f;
					float closestGSDist = float.MaxValue;
					Vector3 closestGS = Vector3.Zero;
					Vector3 anchorSpot = ((entity.ChancesToNotAttack == entity.EType.ChancesToNotAttack) ? entity.WorldPosition : entity.TravelTarget);
					for (int i = 0; i < entity.Gunshots.Count; i++)
					{
						Vector3 gs = entity.Gunshots[i];
						float distsq = Vector3.DistanceSquared(anchorSpot, gs);
						if (distsq < maxdistsq && distsq < closestGSDist && BlockTerrain.Instance.RegionIsLoaded(gs))
						{
							closestGSDist = distsq;
							closestGS = gs;
						}
					}
					if (closestGSDist != 3.4028235E+38f)
					{
						entity.ChancesToNotAttack--;
						if (entity.ChancesToNotAttack == 0)
						{
							entity.NextSound = DragonSoundEnum.CRY;
						}
						entity.TravelTarget = closestGS;
						entity.Target = null;
						entity.TimeLeftTilShotsHeard = entity.EType.ShotHearingInterval;
						return true;
					}
				}
			}
			return false;
		}

		public static float CalculateNextCheckInterval(DragonEntity entity, float interval)
		{
			return MathTools.RandomFloat(interval * 0.75f, interval * 1.25f);
		}

		public static float GetHeading(Vector3 forward, float defaultHeading)
		{
			if (forward.X != 0f || forward.Z != 0f)
			{
				return (float)Math.Atan2((double)(-(double)forward.X), (double)(-(double)forward.Z));
			}
			return defaultHeading;
		}

		public static Vector3 MakeYawVector(float yaw)
		{
			return new Vector3((float)(-(float)Math.Sin((double)yaw)), 0f, (float)(-(float)Math.Cos((double)yaw)));
		}

		public static BaseDragonWaypoint GetBaseWaypoint(DragonEntity entity, DragonAnimEnum nextanim)
		{
			BaseDragonWaypoint wpt = default(BaseDragonWaypoint);
			wpt.Position = entity.LocalPosition;
			wpt.Velocity = entity.LocalToWorld.Forward * entity.Velocity;
			wpt.HostTime = entity.DragonTime;
			wpt.Animation = nextanim;
			wpt.TargetRoll = entity.TargetRoll;
			wpt.Sound = entity.NextSound;
			entity.NextSound = DragonSoundEnum.NONE;
			return wpt;
		}

		public static void SendAttack(DragonEntity entity, bool animatedAttack, DragonAnimEnum nextanim)
		{
			BaseDragonWaypoint wpt = DragonBaseState.GetBaseWaypoint(entity, nextanim);
			Vector3 target;
			if (animatedAttack)
			{
				target = entity.TravelTarget;
			}
			else
			{
				target = entity.ShootTarget;
				entity.ShotPending = false;
			}
			DragonAttackMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, wpt, target, entity.GetNextFireballIndex(), animatedAttack);
			entity.UpdatesSent++;
		}

		public static void SendRegularUpdate(DragonEntity entity, DragonAnimEnum nextanim)
		{
			if (entity.CurrentAnimation != DragonAnimEnum.ATTACK && entity.ShotPending)
			{
				DragonBaseState.SendAttack(entity, false, nextanim);
				return;
			}
			UpdateDragonMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, DragonBaseState.GetBaseWaypoint(entity, nextanim));
			entity.UpdatesSent++;
		}

		public virtual void SendUpdateMessage(DragonEntity entity)
		{
			DragonBaseState.SendRegularUpdate(entity, entity.CurrentAnimation);
		}

		public static float SteerTowardTarget(DragonEntity entity, out Vector3 dest)
		{
			Vector3 pos = entity.WorldPosition;
			if (entity.Target != null)
			{
				Vector3 target = entity.Target.WorldPosition;
				target.Y += 1.5f;
				if (DragonBaseState.CanSeePosition(entity, target))
				{
					entity.TravelTarget = entity.Target.WorldPosition;
				}
			}
			dest = entity.TravelTarget - pos;
			dest.Y = 0f;
			float dsq = dest.Length();
			if (dsq > 10f)
			{
				entity.TargetYaw = MathHelper.WrapAngle(DragonBaseState.GetHeading(dest, entity.TargetYaw));
			}
			return dsq;
		}

		public virtual void Enter(DragonEntity entity)
		{
		}

		public virtual void Update(DragonEntity entity, float dt)
		{
		}

		public virtual void Exit(DragonEntity entity)
		{
		}

		protected const int SHOTS_DURING_HOVER_MIN = 3;

		protected const int SHOTS_DURING_HOVER_MAX = 6;

		protected const float TIME_BETWEEN_SHOTS = 1f;

		protected const float MIN_STEER_DIST = 10f;

		private const float MAX_VIEW_DOT = 0.17f;

		protected static TraceProbe tp = new TraceProbe();
	}
}
