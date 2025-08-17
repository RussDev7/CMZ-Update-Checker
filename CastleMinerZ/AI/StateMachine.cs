using System;

namespace DNA.CastleMinerZ.AI
{
	public class StateMachine<T>
	{
		public StateMachine(T owner)
		{
			this._owner = owner;
			this._currentState = null;
			this._previousState = null;
			this._globalState = null;
		}

		public void ChangeState(IFSMState<T> newState)
		{
			this._previousState = this._currentState;
			if (this._currentState != null)
			{
				this._currentState.Exit(this._owner);
			}
			this._currentState = newState;
			if (this._currentState != null)
			{
				this._currentState.Enter(this._owner);
			}
		}

		public void Revert()
		{
			this.ChangeState(this._previousState);
		}

		public void Update(float dt)
		{
			if (this._globalState != null)
			{
				this._globalState.Update(this._owner, dt);
			}
			if (this._currentState != null)
			{
				this._currentState.Update(this._owner, dt);
			}
		}

		public T _owner;

		public IFSMState<T> _currentState;

		public IFSMState<T> _previousState;

		public IFSMState<T> _globalState;
	}
}
