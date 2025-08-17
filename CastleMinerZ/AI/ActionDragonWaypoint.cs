using System;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public struct ActionDragonWaypoint
	{
		public static ActionDragonWaypoint ReadActionWaypoint(BinaryReader reader)
		{
			ActionDragonWaypoint actionDragonWaypoint;
			actionDragonWaypoint.BaseWpt = BaseDragonWaypoint.ReadBaseWaypoint(reader);
			actionDragonWaypoint.Action = (DragonWaypointActionEnum)reader.ReadByte();
			actionDragonWaypoint.ActionPosition = reader.ReadVector3();
			actionDragonWaypoint.FireballIndex = 0;
			return actionDragonWaypoint;
		}

		public void Write(BinaryWriter writer)
		{
			this.BaseWpt.Write(writer);
			writer.Write((byte)this.Action);
			writer.Write(this.ActionPosition);
		}

		public static void InterpolatePositionVelocity(float time, ActionDragonWaypoint wpt1, ActionDragonWaypoint wpt2, out Vector3 outpos, out Vector3 outvel)
		{
			BaseDragonWaypoint.InterpolatePositionVelocity(time, wpt1.BaseWpt, wpt2.BaseWpt, out outpos, out outvel);
		}

		public static void Extrapolate(float time, ActionDragonWaypoint wpt, out Vector3 outpos, out Vector3 outvel)
		{
			BaseDragonWaypoint.Extrapolate(time, wpt.BaseWpt, out outpos, out outvel);
		}

		public static ActionDragonWaypoint CreateFromBase(BaseDragonWaypoint wpt)
		{
			ActionDragonWaypoint actionDragonWaypoint;
			actionDragonWaypoint.BaseWpt = wpt;
			actionDragonWaypoint.Action = DragonWaypointActionEnum.GOTO;
			actionDragonWaypoint.ActionPosition = Vector3.Zero;
			actionDragonWaypoint.FireballIndex = 0;
			return actionDragonWaypoint;
		}

		public static ActionDragonWaypoint Create(BaseDragonWaypoint wpt, Vector3 target, DragonWaypointActionEnum action, int index)
		{
			ActionDragonWaypoint actionDragonWaypoint;
			actionDragonWaypoint.BaseWpt = wpt;
			actionDragonWaypoint.Action = action;
			actionDragonWaypoint.ActionPosition = target;
			actionDragonWaypoint.FireballIndex = index;
			return actionDragonWaypoint;
		}

		public BaseDragonWaypoint BaseWpt;

		public DragonWaypointActionEnum Action;

		public Vector3 ActionPosition;

		public int FireballIndex;
	}
}
