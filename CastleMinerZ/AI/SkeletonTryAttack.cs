using System;

namespace DNA.CastleMinerZ.AI
{
	public class SkeletonTryAttack : BaseTryAttack
	{
		protected override string[] AttackArray
		{
			get
			{
				return this._attacks;
			}
		}

		protected override string RageAnimation
		{
			get
			{
				return "enraged";
			}
		}

		protected override float[][] HitTimes
		{
			get
			{
				return this._hitTimes;
			}
		}

		protected override float[] HitDamages
		{
			get
			{
				return this._hitDamages;
			}
		}

		protected override float[] HitRanges
		{
			get
			{
				return this._hitRanges;
			}
		}

		protected override float HitDotMultiplier
		{
			get
			{
				return 1f;
			}
		}

		private string[] _attacks = new string[] { "attack1", "attack2", "attack1", "attack2", "attack3" };

		private float[][] _hitTimes = new float[][]
		{
			new float[] { 0.7f },
			new float[] { 0.9333f },
			new float[] { 0.7f },
			new float[] { 0.9333f },
			new float[] { 1.5667f }
		};

		private float[] _hitDamages = new float[] { 0.4f, 0.4f, 0.4f, 0.4f, 0.6f };

		private float[] _hitRanges = new float[] { 1.6f, 2f, 1.6f, 2f, 2.1f };
	}
}
