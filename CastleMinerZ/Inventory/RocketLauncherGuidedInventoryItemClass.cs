using System;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherGuidedInventoryItemClass : RocketLauncherBaseInventoryItemClass
	{
		public bool LockedOnToDragon
		{
			get
			{
				return this._lockedOntoDragon;
			}
		}

		public Rectangle LockedOnSpriteLocation
		{
			get
			{
				return this._lockedOnSpriteLocation;
			}
		}

		public RocketLauncherGuidedInventoryItemClass(InventoryItemIDs id, string name, string description, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\RPG\\Model"), name, description, TimeSpan.FromMinutes(0.016666666666666666), damage, durabilitydamage, ammotype, "RPGLaunch", "ShotGunReload")
		{
			this.ShoulderMagnification = 2f;
			this._game = CastleMinerZGame.Instance;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new RocketLauncherGuidedItem(this, stackCount);
		}

		public override void OnItemUnequipped()
		{
			if (this._toneCue != null && this._toneCue.IsPlaying)
			{
				this._toneCue.Stop(AudioStopOptions.Immediate);
			}
		}

		public void CheckIfLocked(TimeSpan elapsedGameTime)
		{
			if (this._game.LocalPlayer.Shouldering && EnemyManager.Instance.DragonPosition != Vector3.Zero)
			{
				Vector3 position = this._game.LocalPlayer.FPSCamera.WorldPosition;
				LineF3D lineToDragon = new LineF3D(position, EnemyManager.Instance.DragonPosition);
				Angle angle = this._game.LocalPlayer.FPSCamera.LocalToWorld.Forward.AngleBetween(lineToDragon.Direction);
				Angle maxAngle = 0.3f * this._game.LocalPlayer.FPSCamera.FieldOfView;
				if (lineToDragon.Length < 250f && angle < maxAngle)
				{
					RocketLauncherGuidedInventoryItemClass.tp.Init(position, EnemyManager.Instance.DragonPosition);
					IShootableEnemy e = EnemyManager.Instance.Trace(RocketLauncherGuidedInventoryItemClass.tp, false);
					if (!RocketLauncherGuidedInventoryItemClass.tp._collides || (e != null && e is DragonClientEntity))
					{
						Rectangle screenRect = Screen.Adjuster.ScreenRect;
						Matrix viewMat = this._game.LocalPlayer.FPSCamera.View;
						Matrix projMat = this._game.LocalPlayer.FPSCamera.GetProjection(this._game.GraphicsDevice);
						Matrix viewProj = viewMat * projMat;
						Vector3 worldPos = EnemyManager.Instance.DragonPosition;
						Vector4 spos = Vector4.Transform(worldPos, viewProj);
						Vector3 screenPos = new Vector3(spos.X / spos.W, spos.Y / spos.W, spos.Z / spos.W);
						screenPos *= new Vector3(0.5f, -0.5f, 1f);
						screenPos += new Vector3(0.5f, 0.5f, 0f);
						screenPos *= new Vector3((float)this._game.GraphicsDevice.Viewport.Width, (float)this._game.GraphicsDevice.Viewport.Height, 1f);
						int spriteWidth = (int)(35f + 215f * (1f - lineToDragon.Length / 250f));
						this._lockedOnSpriteLocation = new Rectangle((int)screenPos.X - spriteWidth / 2, (int)screenPos.Y - spriteWidth / 2, spriteWidth, spriteWidth);
						this._lockingTime += elapsedGameTime;
						TimeSpan timeToLock = TimeSpan.FromSeconds(1.5 + (double)(4f * (lineToDragon.Length / 250f)) + (double)(4f * (angle / maxAngle)));
						if (timeToLock <= this._lockingTime)
						{
							this._lockedOntoDragon = true;
							if (this._toneCue == null || !this._toneCue.IsPlaying)
							{
								this._toneCue = SoundManager.Instance.PlayInstance("SolidTone");
								return;
							}
						}
						else
						{
							if (this._toneCue != null && this._toneCue.IsPlaying)
							{
								this._toneCue.Stop(AudioStopOptions.Immediate);
							}
							this._lockedOntoDragon = false;
							TimeSpan timeToBeep = TimeSpan.FromSeconds(0.15600000321865082 + 0.844 * ((timeToLock - this._lockingTime).TotalSeconds / 9.5));
							this._beepTimer.MaxTime = timeToBeep;
							this._beepTimer.Update(elapsedGameTime);
							if (this._beepTimer.Expired)
							{
								SoundManager.Instance.PlayInstance("Beep");
								this._beepTimer.Reset();
							}
						}
						return;
					}
				}
			}
			this.ResetLockingBox();
		}

		public void StopSound()
		{
			if (this._toneCue != null)
			{
				this._toneCue.Stop(AudioStopOptions.Immediate);
			}
		}

		private void ResetLockingBox()
		{
			this._lockedOntoDragon = false;
			this._lockingTime = TimeSpan.Zero;
			this._lockedOnSpriteLocation = Rectangle.Empty;
			if (this._toneCue != null && this._toneCue.IsPlaying)
			{
				this._toneCue.Stop(AudioStopOptions.Immediate);
			}
		}

		public override bool IsGuided()
		{
			return this._lockedOntoDragon;
		}

		private static TracerManager.TracerProbe tp = new TracerManager.TracerProbe();

		private TimeSpan _lockingTime = TimeSpan.Zero;

		private bool _lockedOntoDragon;

		private Rectangle _lockedOnSpriteLocation = Rectangle.Empty;

		private OneShotTimer _beepTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		private Cue _toneCue;

		private CastleMinerZGame _game;
	}
}
