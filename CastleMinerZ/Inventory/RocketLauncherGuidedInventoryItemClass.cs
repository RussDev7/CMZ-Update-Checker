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
				Vector3 worldPosition = this._game.LocalPlayer.FPSCamera.WorldPosition;
				LineF3D lineF3D = new LineF3D(worldPosition, EnemyManager.Instance.DragonPosition);
				Angle angle = this._game.LocalPlayer.FPSCamera.LocalToWorld.Forward.AngleBetween(lineF3D.Direction);
				Angle angle2 = 0.3f * this._game.LocalPlayer.FPSCamera.FieldOfView;
				if (lineF3D.Length < 250f && angle < angle2)
				{
					RocketLauncherGuidedInventoryItemClass.tp.Init(worldPosition, EnemyManager.Instance.DragonPosition);
					IShootableEnemy shootableEnemy = EnemyManager.Instance.Trace(RocketLauncherGuidedInventoryItemClass.tp, false);
					if (!RocketLauncherGuidedInventoryItemClass.tp._collides || (shootableEnemy != null && shootableEnemy is DragonClientEntity))
					{
						Rectangle screenRect = Screen.Adjuster.ScreenRect;
						Matrix view = this._game.LocalPlayer.FPSCamera.View;
						Matrix projection = this._game.LocalPlayer.FPSCamera.GetProjection(this._game.GraphicsDevice);
						Matrix matrix = view * projection;
						Vector3 dragonPosition = EnemyManager.Instance.DragonPosition;
						Vector4 vector = Vector4.Transform(dragonPosition, matrix);
						Vector3 vector2 = new Vector3(vector.X / vector.W, vector.Y / vector.W, vector.Z / vector.W);
						vector2 *= new Vector3(0.5f, -0.5f, 1f);
						vector2 += new Vector3(0.5f, 0.5f, 0f);
						vector2 *= new Vector3((float)this._game.GraphicsDevice.Viewport.Width, (float)this._game.GraphicsDevice.Viewport.Height, 1f);
						int num = (int)(35f + 215f * (1f - lineF3D.Length / 250f));
						this._lockedOnSpriteLocation = new Rectangle((int)vector2.X - num / 2, (int)vector2.Y - num / 2, num, num);
						this._lockingTime += elapsedGameTime;
						TimeSpan timeSpan = TimeSpan.FromSeconds(1.5 + (double)(4f * (lineF3D.Length / 250f)) + (double)(4f * (angle / angle2)));
						if (timeSpan <= this._lockingTime)
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
							TimeSpan timeSpan2 = TimeSpan.FromSeconds(0.15600000321865082 + 0.844 * ((timeSpan - this._lockingTime).TotalSeconds / 9.5));
							this._beepTimer.MaxTime = timeSpan2;
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
			this._toneCue.Stop(AudioStopOptions.Immediate);
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
