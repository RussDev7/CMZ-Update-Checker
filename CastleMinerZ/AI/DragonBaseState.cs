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
			Vector3 vector = entity.WorldPosition;
			vector += entity.LocalToWorld.Forward * 7f;
			DragonBaseState.tp.Init(vector, target);
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
				TargetSearchResult targetSearchResult = TargetUtils.FindBestTarget(entity, entity.LocalToWorld.Forward, entity.EType.MaxViewDistance);
				if (targetSearchResult.player != null)
				{
					entity.Target = targetSearchResult.player;
					entity.TravelTarget = targetSearchResult.player.WorldPosition;
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
					float num = entity.EType.MaxViewDistance * entity.EType.MaxViewDistance * 2.25f;
					float num2 = float.MaxValue;
					Vector3 vector = Vector3.Zero;
					Vector3 vector2 = ((entity.ChancesToNotAttack == entity.EType.ChancesToNotAttack) ? entity.WorldPosition : entity.TravelTarget);
					for (int i = 0; i < entity.Gunshots.Count; i++)
					{
						Vector3 vector3 = entity.Gunshots[i];
						float num3 = Vector3.DistanceSquared(vector2, vector3);
						if (num3 < num && num3 < num2 && BlockTerrain.Instance.RegionIsLoaded(vector3))
						{
							num2 = num3;
							vector = vector3;
						}
					}
					if (num2 != 3.4028235E+38f)
					{
						entity.ChancesToNotAttack--;
						if (entity.ChancesToNotAttack == 0)
						{
							entity.NextSound = DragonSoundEnum.CRY;
						}
						entity.TravelTarget = vector;
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
			BaseDragonWaypoint baseDragonWaypoint = default(BaseDragonWaypoint);
			baseDragonWaypoint.Position = entity.LocalPosition;
			baseDragonWaypoint.Velocity = entity.LocalToWorld.Forward * entity.Velocity;
			baseDragonWaypoint.HostTime = entity.DragonTime;
			baseDragonWaypoint.Animation = nextanim;
			baseDragonWaypoint.TargetRoll = entity.TargetRoll;
			baseDragonWaypoint.Sound = entity.NextSound;
			entity.NextSound = DragonSoundEnum.NONE;
			return baseDragonWaypoint;
		}

		public static void SendAttack(DragonEntity entity, bool animatedAttack, DragonAnimEnum nextanim)
		{
			BaseDragonWaypoint baseWaypoint = DragonBaseState.GetBaseWaypoint(entity, nextanim);
			Vector3 vector;
			if (animatedAttack)
			{
				vector = entity.TravelTarget;
			}
			else
			{
				vector = entity.ShootTarget;
				entity.ShotPending = false;
			}
			DragonAttackMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, baseWaypoint, vector, entity.GetNextFireballIndex(), animatedAttack);
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
			Vector3 worldPosition = entity.WorldPosition;
			if (entity.Target != null)
			{
				Vector3 worldPosition2 = entity.Target.WorldPosition;
				worldPosition2.Y += 1.5f;
				if (DragonBaseState.CanSeePosition(entity, worldPosition2))
				{
					entity.TravelTarget = entity.Target.WorldPosition;
				}
			}
			dest = entity.TravelTarget - worldPosition;
			dest.Y = 0f;
			float num = dest.Length();
			if (num > 10f)
			{
				entity.TargetYaw = MathHelper.WrapAngle(DragonBaseState.GetHeading(dest, entity.TargetYaw));
			}
			return num;
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
