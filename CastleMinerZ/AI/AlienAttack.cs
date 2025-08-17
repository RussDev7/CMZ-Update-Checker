using System;

namespace DNA.CastleMinerZ.AI
{
	public class AlienAttack : BaseTryAttack
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
				return "Attack1";
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

		private string[] _attacks = new string[] { "Attack1", "Attack2" };

		private float[][] _hitTimes = new float[][]
		{
			new float[] { 0.7f },
			new float[] { 0.7f }
		};

		private float[] _hitDamages = new float[] { 0.6f, 0.8f };

		private float[] _hitRanges = new float[] { 1.6f, 2.1f };
	}
}
