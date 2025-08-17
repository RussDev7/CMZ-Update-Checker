using System;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class DamageLOSProbe : TraceProbe
	{
		public override void Reset()
		{
			this.TotalDistance = Vector3.Distance(this._start, this._end);
			this.TotalDamageMultiplier = 1f;
			this.TraceCompletePath = true;
			base.Reset();
		}

		public override bool TestThisType(BlockTypeEnum e)
		{
			return e != BlockTypeEnum.NumberOfBlocks && BlockType.GetType(e).BlockPlayer;
		}

		public override bool TouchesBlock(float inT, ref Vector3 inNormal, bool startsIn, BlockFace inFace, float outT, ref Vector3 outNormal, bool endsIn, BlockFace outFace, IntVector3 worldindex)
		{
			float num = (outT - inT) * this.TotalDistance;
			if (num <= 0f)
			{
				return true;
			}
			if (DragonType.BreakLookup[this.DragonTypeIndex, (int)this._currentTestingBlockType])
			{
				this.TotalDamageMultiplier = 0f;
				return false;
			}
			float num2 = 1f - BlockType.GetType(this._currentTestingBlockType).DamageTransmision;
			if (num2 <= 0f)
			{
				return true;
			}
			num2 *= num;
			this.TotalDamageMultiplier *= (1f - num2).Clamp(0f, 1f);
			return this.TotalDamageMultiplier > 0f;
		}

		private float TotalDistance;

		public float TotalDamageMultiplier;

		public int DragonTypeIndex;
	}
}
