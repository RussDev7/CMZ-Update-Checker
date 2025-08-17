using System;
using DNA.Triggers;

namespace DNA.CastleMinerZ
{
	public abstract class DistanceTrigger : Trigger
	{
		protected override bool IsSastisfied()
		{
			if (this._neverTrigger)
			{
				return false;
			}
			if (this._currentDistance <= this._distance)
			{
				return false;
			}
			if (this._currentDistance - this._lastDistance > 10f)
			{
				this._neverTrigger = true;
				return false;
			}
			return true;
		}

		public DistanceTrigger(bool oneShot, float distance)
			: base(oneShot)
		{
			this._distance = distance;
		}

		protected override void OnUpdate()
		{
			this._lastDistance = this._currentDistance;
			this._currentDistance = CastleMinerZGame.Instance.LocalPlayer.LocalPosition.Length();
			base.OnUpdate();
		}

		private float _distance;

		private float _currentDistance;

		private float _lastDistance;

		private bool _neverTrigger;
	}
}
