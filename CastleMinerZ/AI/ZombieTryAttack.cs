using System;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieTryAttack : BaseTryAttack
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

		private string[] _attacks = new string[] { "attack1", "attack2", "attack3", "attack4", "attack5" };

		private float[][] _hitTimes = new float[][]
		{
			new float[] { 0.8333f },
			new float[] { 0.8333f, 1.466667f },
			new float[] { 0.7333f },
			new float[] { 0.9333f },
			new float[] { 1.5f }
		};

		private float[] _hitDamages = new float[] { 0.4f, 0.3f, 0.4f, 0.4f, 0.6f };

		private float[] _hitRanges = new float[] { 1.6f, 1.8f, 1.6f, 2.1f, 2.1f };
	}
}
