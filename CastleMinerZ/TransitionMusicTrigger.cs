using System;

namespace DNA.CastleMinerZ
{
	public class TransitionMusicTrigger : DistanceTrigger
	{
		protected override bool IsSastisfied()
		{
			return !CastleMinerZGame.Instance.MusicCue.IsPlaying && !CastleMinerZGame.Instance.MusicCue.IsPreparing && base.IsSastisfied();
		}

		public TransitionMusicTrigger(string songName, float distance)
			: base(true, distance)
		{
			this._songName = songName;
		}

		public override void OnTriggered()
		{
			CastleMinerZGame.Instance.PlayMusic(this._songName);
			base.OnTriggered();
		}

		public string _songName;
	}
}
