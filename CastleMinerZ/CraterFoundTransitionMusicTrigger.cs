using System;
using DNA.CastleMinerZ.Terrain;
using DNA.Triggers;

namespace DNA.CastleMinerZ
{
	public class CraterFoundTransitionMusicTrigger : Trigger
	{
		protected override bool IsSastisfied()
		{
			return !CastleMinerZGame.Instance.MusicCue.IsPlaying && !CastleMinerZGame.Instance.MusicCue.IsPreparing && this._currentDepth >= this._depth;
		}

		public CraterFoundTransitionMusicTrigger(string songName, float depth)
			: base(true)
		{
			this._songName = songName;
			this._depth = depth;
		}

		public override void OnTriggered()
		{
			CastleMinerZGame.Instance.PlayMusic(this._songName);
			base.OnTriggered();
		}

		protected override void OnUpdate()
		{
			this._currentDepth = (float)BlockTerrain.Instance.DepthUnderSpaceRock(CastleMinerZGame.Instance.LocalPlayer.LocalPosition);
			base.OnUpdate();
		}

		private float _depth;

		private float _currentDepth;

		private string _songName;
	}
}
