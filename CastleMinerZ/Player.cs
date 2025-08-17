using System;
using System.IO;
using System.Text;
using DNA.Audio;
using DNA.Avatars;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Animation;
using DNA.Drawing.Particles;
using DNA.Input;
using DNA.IO.Storage;
using DNA.Net;
using DNA.Net.GamerServices;
using DNA.Security.Cryptography;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class Player : FPSRig
	{
		public int MinLightLevel { get; set; }

		private static string GetHashFromGamerTag(string gamerTag)
		{
			Hash hash = Player.hasher.Compute(Encoding.UTF8.GetBytes(gamerTag));
			return hash.ToString();
		}

		public string PlayerHash
		{
			get
			{
				NetworkGamer gamer = this.Gamer;
				if (gamer != null && gamer.AlternateAddress != 0UL)
				{
					return Player.GetHashFromGamerTag(gamer.AlternateAddress.ToString());
				}
				return Player.GetHashFromGamerTag(this.Gamer.Gamertag);
			}
		}

		public string OldPlayerHash
		{
			get
			{
				return Player.GetHashFromGamerTag(this.Gamer.Gamertag);
			}
		}

		private AnimationPlayer LowerAnimation
		{
			get
			{
				return this.Avatar.Animations[0];
			}
		}

		private AnimationPlayer UpperAnimation
		{
			get
			{
				return this.Avatar.Animations[2];
			}
		}

		private AnimationPlayer HeadAnimation
		{
			get
			{
				return this.Avatar.Animations[4];
			}
		}

		public bool LoadInventory(SaveDevice device, string path)
		{
			bool flag = false;
			PlayerInventory playerInventory = new PlayerInventory(this, false);
			try
			{
				try
				{
					string text = Path.Combine(path, this.PlayerHash + ".inv");
					playerInventory.LoadFromStorage(device, text);
				}
				catch
				{
					string text2 = Path.Combine(path, this.OldPlayerHash + ".inv");
					playerInventory.LoadFromStorage(device, text2);
				}
			}
			catch
			{
				flag = true;
				playerInventory = new PlayerInventory(this, true);
			}
			this.PlayerInventory = playerInventory;
			return flag;
		}

		public void SaveInventory(SaveDevice device, string path)
		{
			try
			{
				string text = Path.Combine(path, this.PlayerHash + ".inv");
				this.PlayerInventory.SaveToStorage(device, text);
			}
			catch
			{
			}
		}

		public void DeleteInventory(SaveDevice device, string path)
		{
			try
			{
				string text = Path.Combine(path, this.PlayerHash + ".inv");
				device.Delete(text);
			}
			catch
			{
			}
		}

		public ParticleEffect SparkEffect
		{
			get
			{
				return Player._sparkEffect;
			}
		}

		static Player()
		{
			Player.mouthLevels[0].Mouth = AvatarMouth.Neutral;
			Player.mouthLevels[0].LeftEye = AvatarEye.Neutral;
			Player.mouthLevels[0].RightEye = AvatarEye.Neutral;
			Player.mouthLevels[1].Mouth = AvatarMouth.PhoneticEe;
			Player.mouthLevels[1].LeftEye = AvatarEye.Neutral;
			Player.mouthLevels[1].RightEye = AvatarEye.Neutral;
			Player.mouthLevels[2].Mouth = AvatarMouth.PhoneticAi;
			Player.mouthLevels[2].LeftEye = AvatarEye.Neutral;
			Player.mouthLevels[2].RightEye = AvatarEye.Neutral;
			Player.mouthLevels[3].Mouth = AvatarMouth.Shocked;
			Player.mouthLevels[3].LeftEye = AvatarEye.Blink;
			Player.mouthLevels[3].RightEye = AvatarEye.Blink;
			Player._shadowModel = CastleMinerZGame.Instance.Content.Load<Model>("Shadow");
			Player.ProxyModel = CastleMinerZGame.Instance.Content.Load<Model>("Character\\SWATMale");
		}

		protected override bool CanJump
		{
			get
			{
				return (base.CanJump || this.InWater) && CastleMinerZGame.Instance.GameScreen.HUD.CheckAndUseStamina(this.GetJumpCost(this.m_jumpCount));
			}
		}

		protected float GetJumpCost(int jumpCount)
		{
			if (jumpCount < 1)
			{
				return 0f;
			}
			return this.m_baseJumpCost;
		}

		protected float GetSprintCost()
		{
			float num = this.m_baseSprintCost * (1f + this.m_multSprintCost);
			return num + this.m_modSprintCost;
		}

		public bool IsLocal
		{
			get
			{
				return this.Gamer.IsLocal;
			}
		}

		public bool FPSMode
		{
			get
			{
				return !base.Children.Contains(this.Avatar);
			}
			set
			{
				if (value)
				{
					if (this.Avatar.Parent != null)
					{
						this.Avatar.RemoveFromParent();
					}
					this.FPSNode.Children.Add(this.Avatar);
					this.Avatar.HideHead = true;
					return;
				}
				if (this.Avatar.Parent != null)
				{
					this.Avatar.RemoveFromParent();
				}
				base.Children.Add(this.Avatar);
				this.Avatar.HideHead = false;
			}
		}

		public bool FlyMode
		{
			get
			{
				return this._flyMode;
			}
			set
			{
				this._flyMode = value;
			}
		}

		public bool InWater
		{
			get
			{
				return BlockTerrain.Instance.IsWaterWorld && BlockTerrain.Instance.DepthUnderWater(base.WorldPosition) > 0f;
			}
		}

		public bool Underwater
		{
			get
			{
				return BlockTerrain.Instance.IsWaterWorld && (double)BlockTerrain.Instance.DepthUnderWater(base.WorldPosition) >= 1.5;
			}
		}

		public float PercentSubmergedWater
		{
			get
			{
				if (!BlockTerrain.Instance.IsWaterWorld)
				{
					return 0f;
				}
				float num = BlockTerrain.Instance.DepthUnderWater(base.WorldPosition);
				if (num < 0f)
				{
					return 0f;
				}
				return Math.Min(1f, num);
			}
		}

		public bool InLava
		{
			get
			{
				return this.PercentSubmergedLava > 0f;
			}
		}

		public bool UnderLava
		{
			get
			{
				return this.PercentSubmergedLava >= 1f;
			}
		}

		public float PercentSubmergedLava
		{
			get
			{
				float num = (-60f - base.WorldPosition.Y) / 2f;
				if (num < 0f)
				{
					return 0f;
				}
				return Math.Min(1f, num);
			}
		}

		public bool ValidGamer
		{
			get
			{
				return this.Gamer != null && !this.Gamer.IsDisposed && !this.Gamer.HasLeftSession;
			}
		}

		public bool ValidLivingGamer
		{
			get
			{
				return !this.Dead && this.ValidGamer;
			}
		}

		private void UpdateAudio(GameTime gameTime)
		{
			Vector3 vector = new Vector3(0f, 0f, -1f);
			vector = Vector3.TransformNormal(vector, base.LocalToWorld);
			if (this.Gamer.IsLocal)
			{
				CastleMinerZGame.Instance.Listener.Position = base.WorldPosition + new Vector3(0f, 1.8f, 0f);
				CastleMinerZGame.Instance.Listener.Forward = vector;
				CastleMinerZGame.Instance.Listener.Up = new Vector3(0f, 1f, 0f);
				CastleMinerZGame.Instance.Listener.Velocity = base.PlayerPhysics.WorldVelocity;
			}
			this.SoundEmitter.Position = base.WorldPosition;
			this.SoundEmitter.Forward = vector;
			this.SoundEmitter.Up = Vector3.Up;
			this.SoundEmitter.Velocity = base.PlayerPhysics.WorldVelocity;
			bool prevUnderwaterState = this._prevUnderwaterState;
			bool inWater = this.InWater;
			this._prevUnderwaterState = this.InWater;
			if (this.Underwater && this.IsLocal)
			{
				if (this.UnderwaterCue != null && this.UnderwaterCue.IsPaused)
				{
					this.UnderwaterCue.Resume();
				}
			}
			else if (this.UnderwaterCue != null)
			{
				this.UnderwaterCue.Pause();
			}
			if (this._isMoveing && this.IsActive)
			{
				this._footStepTimer.Update(TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds * (double)this._walkSpeed));
				if (this._footStepTimer.Expired)
				{
					if (!this._flyMode && !this.InWater && this.InContact)
					{
						SoundManager.Instance.PlayInstance("FootStep", this.SoundEmitter);
					}
					else if (!this._flyMode && this.InWater && this.InContact)
					{
						bool underwater = this.Underwater;
					}
					this._footStepTimer.Reset();
					if (this._isRunning)
					{
						this._footStepTimer.MaxTime = TimeSpan.FromSeconds(0.4);
						return;
					}
					this._footStepTimer.MaxTime = TimeSpan.FromSeconds(0.5);
				}
			}
		}

		public void CalculateLightLevel()
		{
			if (this.PlayerInventory != null && this.PlayerInventory.ActiveInventoryItem != null && this.PlayerInventory.ActiveInventoryItem.ItemClass is SaberInventoryItemClass)
			{
				Block.MinLightLevel = 10;
				return;
			}
			Block.MinLightLevel = 0;
		}

		public override void Jump()
		{
			if (this.FlyMode)
			{
				Vector3 worldVelocity = base.PlayerPhysics.WorldVelocity;
				worldVelocity.Y += this.JumpImpulse;
				base.PlayerPhysics.WorldVelocity = worldVelocity;
				return;
			}
			base.Jump();
		}

		public void Equip(InventoryItem item)
		{
			this.Reloading = false;
			ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, item.ItemClass.ID);
		}

		public void PutItemInHand(InventoryItemIDs itemID)
		{
			this.RightHand.Children.Clear();
			InventoryItem.InventoryItemClass @class = InventoryItem.GetClass(itemID);
			if (@class != null)
			{
				this.RightHand.Children.Add(@class.CreateEntity(ItemUse.Hand, this.IsLocal));
				this._playerMode = @class.PlayerAnimationMode;
				return;
			}
			this._playerMode = PlayerMode.Fist;
		}

		public Player(NetworkGamer gamer, AvatarDescription description)
		{
			this.PlayerInventory = new PlayerInventory(this, false);
			this.Gamer = gamer;
			this.Gamer.Tag = this;
			this.Avatar = new Avatar(description);
			this.Avatar.ProxyModelEntity = new PlayerModelEntity(Player.ProxyModel);
			this.Avatar.Tag = this;
			this.RightHand = this.Avatar.GetAvatarPart(AvatarBone.PropRight);
			this.JumpCountLimit = 2;
			this.Avatar.EyePointCamera.FieldOfView = Angle.FromDegrees(90f);
			base.Children.Add(this.Avatar);
			this.Collider = true;
			base.Physics = new Player.NoMovePhysics(this);
			this.FlyMode = false;
			this.SetupAnimation();
			this.Gamer.BeginGetProfile(delegate(IAsyncResult result)
			{
				try
				{
					this.Profile = this.Gamer.EndGetProfile(result);
					Stream gamerPicture = this.Profile.GetGamerPicture();
					this.GamerPicture = Texture2D.FromStream(CastleMinerZGame.Instance.GraphicsDevice, gamerPicture);
				}
				catch
				{
				}
			}, null);
			this._shadow = new ModelEntity(Player._shadowModel);
			this._shadow.LocalPosition = new Vector3(0f, 0.05f, 0f);
			this._shadow.BlendState = BlendState.AlphaBlend;
			this._shadow.DepthStencilState = DepthStencilState.DepthRead;
			this._shadow.DrawPriority = 200;
			base.Children.Add(this._shadow);
			this.Avatar.EyePointCamera.Children.Add(this.GunEyePointCamera);
		}

		public void UpdateGunEyePointCamera(Vector2 location)
		{
			Vector3 localPosition = this.GunEyePointCamera.LocalPosition;
			localPosition.X = location.X;
			localPosition.Y = location.Y;
			this.GunEyePointCamera.LocalPosition = localPosition;
		}

		public void SetupAnimation()
		{
			this.Avatar.Animations.Play("Stand", 0, TimeSpan.Zero);
			this._torsoPitchAnimation = this.Avatar.Animations.Play("Tilt", 1, TimeSpan.Zero);
			this._torsoPitchAnimation.Pause();
			this.Avatar.Animations.Play("GenericIdle", 2, TimeSpan.Zero);
			this.Avatar.Animations.Play("IdleHead", 4, TimeSpan.Zero);
			this.HeadAnimation.PingPong = true;
		}

		public string GetDigSound(BlockTypeEnum blockType)
		{
			if (blockType == BlockTypeEnum.Sand)
			{
				return "Sand";
			}
			if (blockType == BlockTypeEnum.Snow)
			{
				return "Sand";
			}
			if (blockType != BlockTypeEnum.Leaves)
			{
				return "punch";
			}
			return "leaves";
		}

		private Vector3 GetGunTipPosition()
		{
			Matrix matrix;
			Vector3 barrelTipLocation;
			if (this.RightHand.Children.Count > 0 && this.RightHand.Children[0] is GunEntity)
			{
				GunEntity gunEntity = (GunEntity)this.RightHand.Children[0];
				matrix = gunEntity.LocalToWorld;
				if (this.FPSMode)
				{
					Matrix localToWorld = this.RightHand.LocalToWorld;
					Matrix worldToLocal = this.GunEyePointCamera.WorldToLocal;
					Matrix localToWorld2 = this.FPSCamera.LocalToWorld;
					matrix = localToWorld * worldToLocal * localToWorld2;
				}
				barrelTipLocation = gunEntity.BarrelTipLocation;
			}
			else
			{
				matrix = this.RightHand.LocalToWorld;
				barrelTipLocation = new Vector3(0f, 0f, -0.5f);
			}
			return Vector3.Transform(barrelTipLocation, matrix);
		}

		private void ProcessPlayerUpdateMessage(Message message)
		{
			if (!this.Gamer.IsLocal)
			{
				PlayerUpdateMessage playerUpdateMessage = (PlayerUpdateMessage)message;
				playerUpdateMessage.Apply(this);
			}
		}

		private void ProcessGunshotMessage(Message message)
		{
			GunshotMessage gunshotMessage = (GunshotMessage)message;
			InventoryItem.InventoryItemClass @class = InventoryItem.GetClass(gunshotMessage.ItemID);
			if (@class is LaserGunInventoryItemClass)
			{
				Scene scene = base.Scene;
				if (scene != null)
				{
					BlasterShot blasterShot = BlasterShot.Create(this.GetGunTipPosition(), gunshotMessage.Direction, gunshotMessage.ItemID, message.Sender.Id);
					scene.Children.Add(blasterShot);
				}
			}
			else if (TracerManager.Instance != null)
			{
				TracerManager.Instance.AddTracer(this.FPSCamera.WorldPosition, gunshotMessage.Direction, gunshotMessage.ItemID, message.Sender.Id);
			}
			if (SoundManager.Instance != null)
			{
				if (@class.UseSound == null)
				{
					SoundManager.Instance.PlayInstance("GunShot3", this.SoundEmitter);
				}
				else
				{
					SoundManager.Instance.PlayInstance(@class.UseSound, this.SoundEmitter);
				}
			}
			if (this.RightHand.Children.Count > 0 && this.RightHand.Children[0] is GunEntity)
			{
				GunEntity gunEntity = (GunEntity)this.RightHand.Children[0];
				gunEntity.ShowMuzzleFlash();
			}
		}

		private void ProcessShotgunShotMessage(Message message)
		{
			ShotgunShotMessage shotgunShotMessage = (ShotgunShotMessage)message;
			InventoryItem.InventoryItemClass @class = InventoryItem.GetClass(shotgunShotMessage.ItemID);
			if (@class is LaserGunInventoryItemClass)
			{
				this.GetGunTipPosition();
				for (int i = 0; i < 5; i++)
				{
					Scene scene = base.Scene;
					if (scene != null)
					{
						BlasterShot blasterShot = BlasterShot.Create(this.GetGunTipPosition(), shotgunShotMessage.Directions[i], shotgunShotMessage.ItemID, message.Sender.Id);
						scene.Children.Add(blasterShot);
					}
				}
			}
			else if (TracerManager.Instance != null)
			{
				for (int j = 0; j < 5; j++)
				{
					TracerManager.Instance.AddTracer(this.FPSCamera.WorldPosition, shotgunShotMessage.Directions[j], shotgunShotMessage.ItemID, message.Sender.Id);
				}
			}
			if (SoundManager.Instance != null)
			{
				if (@class.UseSound == null)
				{
					SoundManager.Instance.PlayInstance("GunShot3", this.SoundEmitter);
				}
				else
				{
					SoundManager.Instance.PlayInstance(@class.UseSound, this.SoundEmitter);
				}
			}
			if (this.RightHand.Children.Count > 0 && this.RightHand.Children[0] is GunEntity)
			{
				GunEntity gunEntity = (GunEntity)this.RightHand.Children[0];
				gunEntity.ShowMuzzleFlash();
			}
		}

		private void ProcessFireRocketMessage(Message message)
		{
			if (base.Scene != null)
			{
				FireRocketMessage fireRocketMessage = (FireRocketMessage)message;
				RocketEntity rocketEntity = new RocketEntity(fireRocketMessage.Position, fireRocketMessage.Direction, fireRocketMessage.WeaponType, fireRocketMessage.Guided, this.IsLocal);
				SoundManager.Instance.PlayInstance("RPGLaunch", this.SoundEmitter);
				base.Scene.Children.Add(rocketEntity);
			}
		}

		private void ProcessGrenadeMessage(Message message)
		{
			if (base.Scene != null)
			{
				GrenadeMessage grenadeMessage = (GrenadeMessage)message;
				GrenadeProjectile grenadeProjectile = GrenadeProjectile.Create(grenadeMessage.Position, grenadeMessage.Direction * 15f, grenadeMessage.SecondsLeft, grenadeMessage.GrenadeType, this.IsLocal);
				base.Scene.Children.Add(grenadeProjectile);
				this.Avatar.Animations.Play("Grenade_Reset", 3, TimeSpan.Zero);
				if (this.IsLocal && !CastleMinerZGame.Instance.InfiniteResourceMode && CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem != null && CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.ItemClass is GrenadeInventoryItemClass)
				{
					CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.PopOneItem();
				}
			}
		}

		private void ProcessChangeCarriedItemMessage(Message message)
		{
			ChangeCarriedItemMessage changeCarriedItemMessage = (ChangeCarriedItemMessage)message;
			this.PutItemInHand(changeCarriedItemMessage.ItemID);
		}

		private void ProcessDigMessage(Message message)
		{
			DigMessage digMessage = (DigMessage)message;
			if (digMessage.Placing)
			{
				SoundManager.Instance.PlayInstance("Place", this.SoundEmitter);
			}
			else
			{
				SoundManager.Instance.PlayInstance(this.GetDigSound(digMessage.BlockType), this.SoundEmitter);
			}
			if (base.Scene != null && BlockTerrain.Instance.RegionIsLoaded(digMessage.Location))
			{
				ParticleEmitter particleEmitter = Player._sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.DrawPriority = 900;
				base.Scene.Children.Add(particleEmitter);
				ParticleEmitter particleEmitter2 = Player._rocksEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter2.Reset();
				particleEmitter2.Emitting = true;
				particleEmitter2.DrawPriority = 900;
				base.Scene.Children.Add(particleEmitter2);
				Vector3 vector = Vector3.Cross(Vector3.Forward, -digMessage.Direction);
				Quaternion quaternion = Quaternion.CreateFromAxisAngle(vector, Vector3.Forward.AngleBetween(-digMessage.Direction).Radians);
				particleEmitter2.LocalPosition = (particleEmitter.LocalPosition = digMessage.Location);
				particleEmitter2.LocalRotation = (particleEmitter.LocalRotation = quaternion);
			}
		}

		private void ProcessTimeConnectedMessage(Message message)
		{
			if (!this.Gamer.IsLocal)
			{
				TimeConnectedMessage timeConnectedMessage = (TimeConnectedMessage)message;
				timeConnectedMessage.Apply(this);
			}
		}

		private void ProcessCrateFocusMessage(Message message)
		{
			CrateFocusMessage crateFocusMessage = (CrateFocusMessage)message;
			this.FocusCrate = crateFocusMessage.Location;
			this.FocusCrateItem = crateFocusMessage.ItemIndex;
		}

		public virtual void ProcessMessage(Message message)
		{
			if (message is PlayerUpdateMessage)
			{
				this.ProcessPlayerUpdateMessage(message);
				return;
			}
			if (message is GunshotMessage)
			{
				this.ProcessGunshotMessage(message);
				return;
			}
			if (message is ShotgunShotMessage)
			{
				this.ProcessShotgunShotMessage(message);
				return;
			}
			if (message is FireRocketMessage)
			{
				this.ProcessFireRocketMessage(message);
				return;
			}
			if (message is GrenadeMessage)
			{
				this.ProcessGrenadeMessage(message);
				return;
			}
			if (message is ChangeCarriedItemMessage)
			{
				this.ProcessChangeCarriedItemMessage(message);
				return;
			}
			if (message is DigMessage)
			{
				this.ProcessDigMessage(message);
				return;
			}
			if (message is TimeConnectedMessage)
			{
				this.ProcessTimeConnectedMessage(message);
				return;
			}
			if (message is CrateFocusMessage)
			{
				this.ProcessCrateFocusMessage(message);
			}
		}

		public void PlayTeleportEffect()
		{
			Vector3 worldPosition = base.WorldPosition;
			CastleMinerZGame instance = CastleMinerZGame.Instance;
			SoundManager.Instance.PlayInstance("Teleport", this.SoundEmitter);
		}

		private void SimulateTalking(bool talking, GameTime gameTime)
		{
			if (talking)
			{
				this.mouthTimer.Update(gameTime.ElapsedGameTime);
				if (this.mouthTimer.Expired)
				{
					this.mouthTimer.Reset();
					this.mouthTimer.MaxTime = TimeSpan.FromSeconds(this.rand.NextDouble() * 0.1 + 0.05);
					this.Avatar.Expression = Player.mouthLevels[this.rand.Next(Player.mouthLevels.Length)];
					return;
				}
			}
			else
			{
				this.Avatar.Expression = Player.mouthLevels[0];
			}
		}

		public void ApplyRecoil(Angle amount)
		{
			Quaternion quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this._fastRand.GetNextValue(-0.25f, 0.25f) * amount.Radians);
			Quaternion quaternion2 = Quaternion.CreateFromAxisAngle(Vector3.UnitX, this._fastRand.GetNextValue(0.5f, 1f) * amount.Radians);
			base.RecoilRotation = base.RecoilRotation * quaternion2 * quaternion;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this.FPSNode.LocalToParent = this.FPSCamera.LocalToWorld;
			Angle angle = Quaternion.Identity.AngleBetween(base.RecoilRotation);
			if (angle > Angle.Zero)
			{
				Angle angle2 = this.recoilDecay * (float)gameTime.ElapsedGameTime.TotalSeconds;
				Angle angle3 = angle - angle2;
				if (angle3 < Angle.Zero)
				{
					angle3 = Angle.Zero;
				}
				base.RecoilRotation = Quaternion.Slerp(Quaternion.Identity, base.RecoilRotation, angle3 / angle);
			}
			if (this._flyMode)
			{
				base.PlayerPhysics.WorldAcceleration = Vector3.Zero;
			}
			else
			{
				base.PlayerPhysics.WorldAcceleration = Vector3.Lerp(BasicPhysics.Gravity, new Vector3(0f, 4f, 0f), this.PercentSubmergedWater);
			}
			if (this.InWater)
			{
				float num = MathHelper.Lerp(0f, 3f, this.PercentSubmergedWater);
				Vector3 vector = -base.PlayerPhysics.WorldVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds * num;
				base.PlayerPhysics.WorldVelocity += vector;
			}
			this.UpdateAudio(gameTime);
			this.TimeConnected += gameTime.ElapsedGameTime;
			if (this.IsLocal)
			{
				BlockTerrain.Instance.CenterOn(base.WorldPosition, true);
				BlockTerrain.Instance.EyePos = this.FPSCamera.WorldPosition;
				BlockTerrain.Instance.ViewVector = this.FPSCamera.LocalToWorld.Forward;
				if (this.PlayGrenadeAnim && !this.ReadyToThrowGrenade)
				{
					this.grenadeCookTime += gameTime.ElapsedGameTime;
					if (this.grenadeCookTime >= TimeSpan.FromSeconds(4.0))
					{
						this.ReadyToThrowGrenade = true;
					}
				}
			}
			base.OnUpdate(gameTime);
			if (this.IsLocal)
			{
				Vector3 vector2 = BlockTerrain.Instance.ClipPositionToLoadedWorld(base.LocalPosition, this.MovementProbe.Radius);
				vector2.Y = Math.Min(74f, vector2.Y);
				base.LocalPosition = vector2;
			}
			this.SimulateTalking(this.Gamer.IsTalking, gameTime);
			this.shadowProbe.Init(base.WorldPosition + new Vector3(0f, 1f, 0f), base.WorldPosition + new Vector3(0f, -2.5f, 0f));
			this.shadowProbe.SkipEmbedded = true;
			BlockTerrain.Instance.Trace(this.shadowProbe);
			this._shadow.Visible = this.shadowProbe._collides;
			if (this._shadow.Visible)
			{
				Vector3 intersection = this.shadowProbe.GetIntersection();
				Vector3 vector3 = intersection - base.WorldPosition;
				float num2 = Math.Abs(vector3.Y);
				this._shadow.LocalPosition = vector3 + new Vector3(0f, 0.05f, 0f);
				int num3 = 2;
				float num4 = num2 / (float)num3;
				this._shadow.LocalScale = new Vector3(1f + 2f * num4, 1f, 1f + 2f * num4);
				this._shadow.EntityColor = new Color?(new Color(1f, 1f, 1f, Math.Max(0f, 0.5f * (1f - num4))));
			}
		}

		private bool ClipMovementToAvoidFalling(Vector3 worldPos, ref Vector3 nextPos, ref Vector3 velocity)
		{
			bool flag = false;
			BlockFace blockFace = BlockFace.NUM_FACES;
			BlockFace blockFace2 = BlockFace.NUM_FACES;
			FallLockTestResult fallLockTestResult = FallLockTestResult.EMPTY_BLOCK;
			FallLockTestResult fallLockTestResult2 = FallLockTestResult.EMPTY_BLOCK;
			float num = 0f;
			float num2 = 0f;
			if (velocity.X > 0f)
			{
				blockFace = BlockFace.POSX;
			}
			else if (velocity.X < 0f)
			{
				blockFace = BlockFace.NEGX;
			}
			else
			{
				fallLockTestResult = FallLockTestResult.SOLID_BLOCK_NO_WALL;
			}
			if (velocity.Z > 0f)
			{
				blockFace2 = BlockFace.POSZ;
			}
			else if (velocity.Z < 0f)
			{
				blockFace2 = BlockFace.NEGZ;
			}
			else
			{
				fallLockTestResult2 = FallLockTestResult.SOLID_BLOCK_NO_WALL;
			}
			IntVector3 intVector = IntVector3.FromVector3(worldPos + this.PlayerAABB.Min);
			intVector.Y--;
			if (blockFace == BlockFace.POSX && fallLockTestResult != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult = fallLockTestResult3;
					if (fallLockTestResult == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num = (float)intVector.X + 0.95f - this.PlayerAABB.Min.X;
					}
				}
			}
			if (blockFace2 == BlockFace.POSZ && fallLockTestResult2 != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace2);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult2 = fallLockTestResult3;
					if (fallLockTestResult2 == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num2 = (float)intVector.Z + 0.95f - this.PlayerAABB.Min.Z;
					}
				}
			}
			intVector.Z = (int)Math.Floor((double)(worldPos.Z + this.PlayerAABB.Max.Z));
			if (blockFace == BlockFace.POSX && fallLockTestResult != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult = fallLockTestResult3;
					if (fallLockTestResult == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num = (float)intVector.X + 0.95f - this.PlayerAABB.Min.X;
					}
				}
			}
			if (blockFace2 == BlockFace.NEGZ && fallLockTestResult2 != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace2);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult2 = fallLockTestResult3;
					if (fallLockTestResult2 == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num2 = (float)intVector.Z + 0.05f - this.PlayerAABB.Max.Z;
					}
				}
			}
			intVector.X = (int)Math.Floor((double)(worldPos.X + this.PlayerAABB.Max.X));
			intVector.Z = (int)Math.Floor((double)(worldPos.Z + this.PlayerAABB.Min.Z));
			if (blockFace == BlockFace.NEGX && fallLockTestResult != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult = fallLockTestResult3;
					if (fallLockTestResult == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num = (float)intVector.X + 0.05f - this.PlayerAABB.Max.X;
					}
				}
			}
			if (blockFace2 == BlockFace.POSZ && fallLockTestResult2 != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace2);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult2 = fallLockTestResult3;
					if (fallLockTestResult2 == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num2 = (float)intVector.Z + 0.95f - this.PlayerAABB.Min.Z;
					}
				}
			}
			intVector.Z = (int)Math.Floor((double)(worldPos.Z + this.PlayerAABB.Max.Z));
			if (blockFace == BlockFace.NEGX && fallLockTestResult != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult = fallLockTestResult3;
					if (fallLockTestResult == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num = (float)intVector.X + 0.05f - this.PlayerAABB.Max.X;
					}
				}
			}
			if (blockFace2 == BlockFace.NEGZ && fallLockTestResult2 != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult fallLockTestResult3 = BlockTerrain.Instance.FallLockFace(intVector, blockFace2);
				if (fallLockTestResult3 != FallLockTestResult.EMPTY_BLOCK)
				{
					fallLockTestResult2 = fallLockTestResult3;
					if (fallLockTestResult2 == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						num2 = (float)intVector.Z + 0.05f - this.PlayerAABB.Max.Z;
					}
				}
			}
			if (fallLockTestResult == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
			{
				if (blockFace == BlockFace.POSX)
				{
					if (nextPos.X > num)
					{
						nextPos.X = num;
						velocity.X = 0f;
						flag = true;
					}
				}
				else if (nextPos.X < num)
				{
					velocity.X = 0f;
					nextPos.X = num;
					flag = true;
				}
			}
			if (fallLockTestResult2 == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
			{
				if (blockFace2 == BlockFace.POSZ)
				{
					if (nextPos.Z > num2)
					{
						velocity.Z = 0f;
						nextPos.Z = num2;
						flag = true;
					}
				}
				else if (nextPos.Z < num2)
				{
					velocity.Z = 0f;
					nextPos.Z = num2;
					flag = true;
				}
			}
			return flag;
		}

		public override bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			base.ResolveCollsion(e, out collsionPlane, dt);
			bool flag = false;
			if (e == BlockTerrain.Instance)
			{
				float num = (float)dt.ElapsedGameTime.TotalSeconds;
				Vector3 worldPosition = base.WorldPosition;
				Vector3 vector = worldPosition;
				Vector3 vector2 = base.PlayerPhysics.WorldVelocity;
				Vector3 vector3 = vector2;
				vector3.Y = 0f;
				vector3.Normalize();
				float num2 = EnemyManager.Instance.AttentuateVelocity(this, vector3, worldPosition);
				vector2.X *= num2;
				if (vector2.Y > 0f)
				{
					vector2.Y *= num2;
				}
				vector2.Z *= num2;
				float y = vector2.Y;
				this.SetInContact(false);
				this.MovementProbe.SkipEmbedded = true;
				int num3 = 0;
				for (;;)
				{
					Vector3 vector4 = vector;
					Vector3 vector5 = Vector3.Multiply(vector2, num);
					vector += vector5;
					this.MovementProbe.Init(vector4, vector, this.PlayerAABB);
					this.MovementProbe.SimulateSlopedSides = CastleMinerZGame.Instance.PlayerStats.AutoClimb && vector2.Y < this.JumpImpulse * 0.5f;
					BlockTerrain.Instance.Trace(this.MovementProbe);
					if (this.MovementProbe._collides)
					{
						flag = true;
						if (this.MovementProbe._inFace == BlockFace.POSY)
						{
							this.SetInContact(true);
							this.GroundNormal = new Vector3(0f, 1f, 0f);
						}
						if (this.MovementProbe._startsIn)
						{
							goto IL_042D;
						}
						float num4 = Math.Max(this.MovementProbe._inT - 0.001f, 0f);
						vector = vector4 + vector5 * num4;
						if (this.MovementProbe.FoundSlopedBlock && this.MovementProbe.SlopedBlockT <= this.MovementProbe._inT)
						{
							this.SetInContact(true);
							vector2.Y = 0f;
							this.GroundNormal = new Vector3(0f, 1f, 0f);
							vector.Y += 5f * num4 * num;
							if (vector.Y > (float)this.MovementProbe.SlopedBlock.Y + 1.001f)
							{
								vector.Y = (float)this.MovementProbe.SlopedBlock.Y + 1.001f;
							}
						}
						vector2 -= Vector3.Multiply(this.MovementProbe._inNormal, Vector3.Dot(this.MovementProbe._inNormal, vector2));
						num *= 1f - num4;
						if (num <= 1E-07f)
						{
							goto IL_042D;
						}
						if (vector2.LengthSquared() <= 1E-06f || Vector3.Dot(base.PlayerPhysics.WorldVelocity, vector2) <= 1E-06f)
						{
							break;
						}
					}
					else if (this.MovementProbe.FoundSlopedBlock)
					{
						this.SetInContact(true);
						vector2.Y = 0f;
						this.GroundNormal = new Vector3(0f, 1f, 0f);
						vector.Y += 5f * num;
						if (vector.Y > (float)this.MovementProbe.SlopedBlock.Y + 1.001f)
						{
							vector.Y = (float)this.MovementProbe.SlopedBlock.Y + 1.001f;
						}
					}
					num3++;
					if (!this.MovementProbe._collides || num3 >= 4)
					{
						goto IL_042D;
					}
				}
				vector2 = Vector3.Zero;
				if (this.MovementProbe.FoundSlopedBlock && this.MovementProbe.SlopedBlockT <= this.MovementProbe._inT)
				{
					this.SetInContact(true);
					vector2.Y = 0f;
					this.GroundNormal = new Vector3(0f, 1f, 0f);
					vector.Y += 5f * num;
					if (vector.Y > (float)this.MovementProbe.SlopedBlock.Y + 1.001f)
					{
						vector.Y = (float)this.MovementProbe.SlopedBlock.Y + 1.001f;
					}
				}
				IL_042D:
				if (num3 == 4)
				{
					vector2 = Vector3.Zero;
				}
				if (this.InContact && this.LockedFromFalling && (vector2.X != 0f || vector2.Z != 0f))
				{
					flag = this.ClipMovementToAvoidFalling(worldPosition, ref vector, ref vector2) || flag;
				}
				float num5 = vector2.Y - y;
				base.LocalPosition = vector;
				base.PlayerPhysics.WorldVelocity = vector2;
				if (!this.IsLocal)
				{
					this.Avatar.Visible = BlockTerrain.Instance.RegionIsLoaded(vector);
				}
				else if (!this._flyMode && num5 > 18f && vector2.Y < 0.1f)
				{
					Vector3 localPosition = base.LocalPosition;
					localPosition.Y -= 1f;
					InGameHUD.Instance.ApplyDamage((num5 - 18f) * 0.06666667f, localPosition);
				}
				if (this.Avatar != null && this.Avatar.AvatarRenderer != null)
				{
					PlayerModelEntity playerModelEntity = (PlayerModelEntity)this.Avatar.ProxyModelEntity;
					vector.Y += 1.2f;
					BlockTerrain.Instance.GetEnemyLighting(vector, ref playerModelEntity.DirectLightDirection[0], ref playerModelEntity.DirectLightColor[0], ref playerModelEntity.DirectLightDirection[1], ref playerModelEntity.DirectLightColor[1], ref playerModelEntity.AmbientLight);
				}
			}
			return flag;
		}

		public bool UsingAnimationPlaying
		{
			get
			{
				return this.usingAnimationPlaying || this.GrenadeAnimPlaying;
			}
		}

		public bool GrenadeAnimPlaying
		{
			get
			{
				if (this.Avatar.Animations[3] == null)
				{
					return false;
				}
				string name = this.Avatar.Animations[3].Name;
				return name == "Grenade_Reset" || name == "Grenade_Throw" || name == "Grenade_Cook";
			}
		}

		public bool ShoulderedAnimState
		{
			get
			{
				return this.currentAnimState == Player.AnimationState.Shouldered;
			}
		}

		public override void ProcessInput(FPSControllerMapping controller, GameTime gameTime)
		{
			if (this.Dead)
			{
				this.UpdateAnimation(0f, 0f, this.TorsoPitch, this._playerMode, false);
				return;
			}
			this._isFlyingUp = false;
			if (this._flyMode)
			{
				this.Speed = 5f;
				if (controller.Jump.Held)
				{
					this._isFlyingUp = true;
				}
			}
			else
			{
				this.Speed = MathHelper.Lerp(5f, this.underWaterSpeed, this.PercentSubmergedWater);
			}
			if ((controller.MoveForward.Held || controller.MoveBackward.Held || controller.StrafeLeft.Held || controller.StrafeRight.Held) && controller.Sprint.Held && CastleMinerZGame.Instance.GameScreen.HUD.CheckAndUseStamina(this.GetSprintCost() * (float)gameTime.ElapsedGameTime.TotalSeconds))
			{
				this._isSprinting = true;
			}
			else
			{
				this._isSprinting = false;
			}
			base.ProcessInput(controller, gameTime);
			CastleMinerZControllerMapping castleMinerZControllerMapping = (CastleMinerZControllerMapping)controller;
			if ((double)controller.Movement.LengthSquared() < 0.1)
			{
				this.LockedFromFalling = false;
			}
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Creative && castleMinerZControllerMapping.FlyMode.Pressed)
			{
				this.FlyMode = !this.FlyMode;
			}
			this.UpdateAnimation(controller.Movement.Y, controller.Movement.X, this.TorsoPitch, this._playerMode, this.UsingTool);
		}

		public void FinishReload()
		{
			this.Reloading = false;
			if (this.IsLocal)
			{
				InventoryItem activeInventoryItem = CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem;
				if (activeInventoryItem is GunInventoryItem)
				{
					GunInventoryItem gunInventoryItem = (GunInventoryItem)activeInventoryItem;
					this.Reloading = gunInventoryItem.Reload(CastleMinerZGame.Instance.GameScreen.HUD);
				}
			}
		}

		public void UpdateAnimation(float walkAmount, float strafeAmount, Angle torsoPitch, PlayerMode playerMode, bool doAction)
		{
			float num = Math.Abs(walkAmount);
			float num2 = Math.Abs(strafeAmount);
			float num3 = Math.Max(num, num2);
			float num4 = 0f;
			float num5 = 0f;
			float num6 = MathHelper.Lerp(0.947f, this.underWaterSpeed, this.PercentSubmergedWater);
			float num7 = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
			float num8 = MathHelper.Lerp(4f * this._sprintMultiplier, this.underWaterSpeed, this.PercentSubmergedWater);
			float num9 = MathHelper.Lerp(0.947f * this._sprintMultiplier, this.underWaterSpeed, this.PercentSubmergedWater);
			this._isRunning = false;
			this._isMoveing = num3 >= 0.1f;
			string text = "GenericUse";
			string text2 = "GenericIdle";
			string text3 = "GenericWalk";
			string text4 = null;
			string text5 = null;
			switch (playerMode)
			{
			case PlayerMode.Pick:
				text2 = "PickIdle";
				text3 = "PickWalk";
				text = "PickUse";
				break;
			case PlayerMode.Block:
				text2 = "BlockIdle";
				text3 = "BlockWalk";
				text = "BlockUse";
				break;
			case PlayerMode.Fist:
				text2 = "FistIdle";
				text3 = "FistWalk";
				text = "FistUse";
				break;
			case PlayerMode.Assault:
				text4 = "GunShoulder";
				text5 = "GunReload";
				if (this.Shouldering)
				{
					text2 = "GunShoulderIdle";
					text3 = "GunShoulderWalk";
					text = "GunShoulderShoot";
				}
				else
				{
					text2 = "GunIdle";
					text3 = "GunRun";
					text = "GunShoot";
				}
				break;
			case PlayerMode.BoltRifle:
				text4 = "RifleShoulder";
				text5 = "RifleReload";
				if (this.Shouldering)
				{
					text2 = "RifleShoulderIdle";
					text3 = "RifleShoulderWalk";
					text = "RifleShoulderShoot";
				}
				else
				{
					text2 = "RifleIdle";
					text3 = "RifleWalk";
					text = "RifleShoot";
				}
				break;
			case PlayerMode.Pistol:
				text4 = "PistolShoulder";
				text5 = "PistolReload";
				if (this.Shouldering)
				{
					text2 = "PistolShoulderIdle";
					text3 = "PistolShoulderWalk";
					text = "PistolShoulderShoot";
				}
				else
				{
					text2 = "PistolIdle";
					text3 = "PistolWalk";
					text = "PistolShoot";
				}
				break;
			case PlayerMode.PumpShotgun:
				text4 = "PumpShotgunShoulder";
				text5 = "PumpShotgunReload";
				if (this.Shouldering)
				{
					text2 = "PumpShotgunShoulderIdle";
					text3 = "PumpShotgunShoulderWalk";
					text = "PumpShotgunShoulderShoot";
				}
				else
				{
					text2 = "PumpShotgunIdle";
					text3 = "PumpShotgunRun";
					text = "PumpShotgunShoot";
				}
				break;
			case PlayerMode.SMG:
				text4 = "SMGShoulder";
				text5 = "SMGReload";
				if (this.Shouldering)
				{
					text2 = "SMGShoulderIdle";
					text3 = "SMGShoulderWalk";
					text = "SMGShoulderShoot";
				}
				else
				{
					text2 = "SMGIdle";
					text3 = "SMGWalk";
					text = "SMGShoot";
				}
				break;
			case PlayerMode.LMG:
				text4 = "LMGShoulder";
				text5 = "LMGReload";
				if (this.Shouldering)
				{
					text2 = "LMGShoulderIdle";
					text3 = "LMGShoulderWalk";
					text = "LMGShoulderShoot";
				}
				else
				{
					text2 = "LMGIdle";
					text3 = "LMGWalk";
					text = "LMGShoot";
				}
				break;
			case PlayerMode.SpaceAssault:
				text4 = "LaserGunShoulder";
				text5 = "LaserGunReload";
				if (this.Shouldering)
				{
					text2 = "LaserGunShoulderIdle";
					text3 = "LaserGunShoulderWalk";
					text = "LaserGunShoulderShoot";
				}
				else
				{
					text2 = "LaserGunIdle";
					text3 = "LaserGunRun";
					text = "LaserGunShoot";
				}
				break;
			case PlayerMode.SpaceBoltRifle:
				text4 = "LaserRifleShoulder";
				text5 = "LaserRifleReload";
				if (this.Shouldering)
				{
					text2 = "LaserRifleShoulderIdle";
					text3 = "LaserRifleShoulderWalk";
					text = "LaserRifleShoulderShoot";
				}
				else
				{
					text2 = "LaserRifleIdle";
					text3 = "LaserRifleRun";
					text = "LaserRifleShoot";
				}
				break;
			case PlayerMode.SpacePistol:
				text4 = "LaserPistolShoulder";
				text5 = "LaserPistolReload";
				if (this.Shouldering)
				{
					text2 = "LaserPistolShoulderIdle";
					text3 = "LaserPistolShoulderWalk";
					text = "LaserPistolShoulderShoot";
				}
				else
				{
					text2 = "LaserPistolIdle";
					text3 = "LaserPistolRun";
					text = "LaserPistolShoot";
				}
				break;
			case PlayerMode.SpacePumpShotgun:
				text4 = "LaserGunShoulder";
				text5 = "LaserShotgunReload";
				if (this.Shouldering)
				{
					text2 = "LaserGunShoulderIdle";
					text3 = "LaserGunShoulderWalk";
					text = "LaserShotgunShoulderShoot";
				}
				else
				{
					text2 = "LaserGunIdle";
					text3 = "LaserGunRun";
					text = "LaserShotgunShoot";
				}
				break;
			case PlayerMode.SpaceSMG:
				text4 = "LaserSMGShoulder";
				text5 = "LaserSMGReload";
				if (this.Shouldering)
				{
					text2 = "LaserSMGShoulderIdle";
					text3 = "LaserSMGShoulderWalk";
					text = "LaserSMGShoulderShoot";
				}
				else
				{
					text2 = "LaserSMGIdle";
					text3 = "LaserSMGRun";
					text = "LaserSMGShoot";
				}
				break;
			case PlayerMode.Grenade:
				text2 = "GrenadeIdle";
				text3 = "GrenadeWalk";
				break;
			case PlayerMode.RPG:
				text4 = "GunShoulder";
				text5 = "PumpShotgunReload";
				if (this.Shouldering)
				{
					text2 = "GunShoulderIdle";
					text3 = "GunShoulderWalk";
					text = "PumpShotgunShoulderShoot";
				}
				else
				{
					text2 = "RPGIdle";
					text3 = "RPGWalk";
					text = "RPGShoot";
				}
				break;
			case PlayerMode.Chainsaw:
				text2 = "PickIdle";
				text3 = "PickWalk";
				text = "PickUse";
				break;
			case PlayerMode.LaserDrill:
				text4 = "LaserDrillShoulder";
				text5 = "LaserDrillReload";
				if (this.Shouldering)
				{
					text2 = "LaserDrillShoulderIdle";
					text3 = "LaserDrillShoulderWalk";
					text = "LaserDrillShoulderShoot";
				}
				else
				{
					text2 = "LaserDrillIdle";
					text3 = "LaserDrillRun";
					text = "LaserDrillShoot";
				}
				break;
			}
			if (this.Dead)
			{
				if (this.Avatar.Animations[5] == null)
				{
					this.Avatar.Animations.Play("Die", 5, TimeSpan.FromSeconds(0.25));
				}
			}
			else if (this.Avatar.Animations[5] != null)
			{
				this.Avatar.Animations.ClearAnimation(5, TimeSpan.FromSeconds(0.25));
			}
			if (this.UsingTool)
			{
				this.Avatar.Animations.Play(text, 3, TimeSpan.Zero);
				this.usingAnimationPlaying = true;
			}
			else if (this.PlayGrenadeAnim && this.Avatar.Animations[3] == null)
			{
				SoundManager.Instance.PlayInstance("GrenadeArm", this.SoundEmitter);
				this.Avatar.Animations.Play("Grenade_Cook", 3, TimeSpan.FromSeconds(0.25));
			}
			else if (this.PlayItemUsingOut && this.Avatar.Animations[3] == null)
			{
				SoundManager.Instance.PlayInstance("GrenadeArm", this.SoundEmitter);
				this.Avatar.Animations.Play("ChainsawOut", 3, TimeSpan.FromSeconds(0.25));
			}
			else if (this.Avatar.Animations[3] != null)
			{
				this.Avatar.Animations[3].Looping = false;
				if (this.Avatar.Animations[3].Finished)
				{
					if (this.PlayItemUsingOut)
					{
						if (this.Avatar.Animations[3].Name == "Grenade_Throw")
						{
							if (this.IsLocal)
							{
								Matrix localToWorld = this.FPSCamera.LocalToWorld;
								GrenadeInventoryItemClass grenadeInventoryItemClass = CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.ItemClass as GrenadeInventoryItemClass;
								if (grenadeInventoryItemClass != null)
								{
									GrenadeMessage.Send((LocalNetworkGamer)this.Gamer, localToWorld, grenadeInventoryItemClass.GrenadeType, 5f - (float)this.grenadeCookTime.TotalSeconds);
								}
							}
							this.PlayGrenadeAnim = false;
						}
						else if (this.ReadyToThrowGrenade)
						{
							this.Avatar.Animations.Play("Grenade_Throw", 3, TimeSpan.FromSeconds(0.0));
						}
					}
					if (this.PlayGrenadeAnim)
					{
						if (this.Avatar.Animations[3].Name == "Grenade_Throw")
						{
							if (this.IsLocal)
							{
								Matrix localToWorld2 = this.FPSCamera.LocalToWorld;
								GrenadeInventoryItemClass grenadeInventoryItemClass2 = CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.ItemClass as GrenadeInventoryItemClass;
								if (grenadeInventoryItemClass2 != null)
								{
									GrenadeMessage.Send((LocalNetworkGamer)this.Gamer, localToWorld2, grenadeInventoryItemClass2.GrenadeType, 5f - (float)this.grenadeCookTime.TotalSeconds);
								}
							}
							this.PlayGrenadeAnim = false;
						}
						else if (this.ReadyToThrowGrenade)
						{
							this.Avatar.Animations.Play("Grenade_Throw", 3, TimeSpan.FromSeconds(0.0));
						}
					}
					else
					{
						this.Avatar.Animations.ClearAnimation(3, TimeSpan.FromSeconds(0.25));
						this.usingAnimationPlaying = false;
					}
				}
			}
			if (this.Underwater && !this.FPSMode)
			{
				if (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Swim")
				{
					this.Avatar.Animations.Play("Swim", 0, TimeSpan.FromSeconds(0.25));
				}
				if (num3 < 0.1f)
				{
					Vector3 localVelocity = base.PlayerPhysics.LocalVelocity;
					localVelocity.X = (localVelocity.Z = 0f);
					base.PlayerPhysics.LocalVelocity = localVelocity;
					this._isMoveing = false;
				}
				else
				{
					float num10 = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
					num5 = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
					num4 = walkAmount * num10;
				}
			}
			else
			{
				if (this.currentAnimState == Player.AnimationState.Reloading && this.Avatar.Animations[2].Finished)
				{
					this.currentAnimState = Player.AnimationState.Unshouldered;
					this.FinishReload();
				}
				if (this.Reloading)
				{
					if (this.currentAnimState == Player.AnimationState.Shouldered && !this.usingAnimationPlaying)
					{
						this.currentAnimState = Player.AnimationState.UnShouldering;
						AnimationPlayer animationPlayer = this.Avatar.Animations.Play(text4, 2, TimeSpan.FromSeconds(0.25));
						animationPlayer.Reversed = true;
					}
					else if (this.currentAnimState == Player.AnimationState.Unshouldered && !this.usingAnimationPlaying)
					{
						if (text5 != null)
						{
							this.currentAnimState = Player.AnimationState.Reloading;
							AnimationPlayer animationPlayer2 = this.Avatar.Animations.Play(text5, 2, TimeSpan.FromSeconds(0.25));
							if (this.IsLocal)
							{
								GunInventoryItem gunInventoryItem = InGameHUD.Instance.ActiveInventoryItem as GunInventoryItem;
								if (gunInventoryItem != null)
								{
									animationPlayer2.Speed = (float)(animationPlayer2.Duration.TotalSeconds / gunInventoryItem.GunClass.ReloadTime.TotalSeconds);
								}
							}
							if (this.ReloadSound == null)
							{
								this._reloadCue = SoundManager.Instance.PlayInstance("Reload", this.SoundEmitter);
							}
							else
							{
								this._reloadCue = SoundManager.Instance.PlayInstance(this.ReloadSound, this.SoundEmitter);
							}
						}
						else
						{
							this.FinishReload();
						}
					}
				}
				else if (this.currentAnimState == Player.AnimationState.Reloading)
				{
					this.currentAnimState = Player.AnimationState.Unshouldered;
					if (this._reloadCue != null && this._reloadCue.IsPlaying)
					{
						this._reloadCue.Stop(AudioStopOptions.Immediate);
					}
				}
				if (this.Shouldering && text4 != null && this.currentAnimState == Player.AnimationState.Unshouldered)
				{
					this.currentAnimState = Player.AnimationState.Shouldering;
					this.Avatar.Animations.Play(text4, 2, TimeSpan.Zero);
				}
				if (!this.Shouldering && text4 != null && this.currentAnimState == Player.AnimationState.Shouldered)
				{
					this.currentAnimState = Player.AnimationState.UnShouldering;
					AnimationPlayer animationPlayer3 = this.Avatar.Animations.Play(text4, 2, TimeSpan.Zero);
					animationPlayer3.Reversed = true;
				}
				if (!this.Shouldering && text4 == null && this.currentAnimState == Player.AnimationState.Shouldered)
				{
					this.currentAnimState = Player.AnimationState.Unshouldered;
				}
				if (this.currentAnimState == Player.AnimationState.Shouldering && this.Avatar.Animations[2].Finished)
				{
					this.currentAnimState = Player.AnimationState.Shouldered;
				}
				if (this.currentAnimState == Player.AnimationState.UnShouldering && this.Avatar.Animations[2].Finished)
				{
					this.currentAnimState = Player.AnimationState.Unshouldered;
				}
				if (!this.Reloading && (this.currentAnimState == Player.AnimationState.Unshouldered || this.currentAnimState == Player.AnimationState.Shouldered))
				{
					if (this._isMoveing)
					{
						if (this.Avatar.Animations[2] == null || this.Avatar.Animations[2].Name != text3)
						{
							if (this.Shouldering)
							{
								this.Avatar.Animations.Play(text3, 2, TimeSpan.Zero);
							}
							else
							{
								this.Avatar.Animations.Play(text3, 2, TimeSpan.FromSeconds(0.25));
							}
						}
						if (this.Avatar.Animations[2] != null && this.Avatar.Animations[2].Name == text3)
						{
							this.Avatar.Animations[2].Speed = Math.Max(num, num2);
						}
					}
					else if (this.Avatar.Animations[2] == null || this.Avatar.Animations[2].Name != text2)
					{
						if (this.Shouldering)
						{
							this.Avatar.Animations.Play(text2, 2, TimeSpan.FromSeconds(0.10000000149011612));
						}
						else
						{
							this.Avatar.Animations.Play(text2, 2, TimeSpan.FromSeconds(0.25));
						}
					}
				}
				if (num3 < 0.1f)
				{
					if (this._flyMode)
					{
						base.PlayerPhysics.WorldVelocity = Vector3.Zero;
					}
					else
					{
						Vector3 localVelocity2 = base.PlayerPhysics.LocalVelocity;
						localVelocity2.X = (localVelocity2.Z = 0f);
						base.PlayerPhysics.LocalVelocity = localVelocity2;
					}
					if (!this.FPSMode && (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Stand"))
					{
						this.Avatar.Animations.Play("Stand", 0, TimeSpan.FromSeconds(0.25));
					}
					this._isMoveing = false;
				}
				else
				{
					num5 = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
					if (num < 0.8f)
					{
						float num11 = (this._isSprinting ? num9 : num6);
						float num12 = (num - 0.1f) / 0.4f;
						if (walkAmount < 0f)
						{
							num4 = num12 * num11;
						}
						else
						{
							num4 = -num12 * num11;
						}
					}
					else
					{
						float num13 = (this._isSprinting ? num8 : num7);
						float num14 = 0.8f + (num - 0.8f);
						if (walkAmount < 0f)
						{
							num4 = num14 * num13;
						}
						else
						{
							num4 = -num14 * num13;
						}
						this._isRunning = true;
					}
					if (this.FPSMode)
					{
						if (num < 0.8f)
						{
							this._walkSpeed = (num - 0.1f) / 0.4f;
						}
						else
						{
							this._walkSpeed = 0.8f + (num - 0.8f);
						}
					}
					else if (!this._flyMode)
					{
						if (num > num2)
						{
							if (num < 0.8f)
							{
								if (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Walk")
								{
									this.Avatar.Animations.Play("Walk", 0, TimeSpan.FromSeconds(0.25));
								}
								float num15 = (num - 0.1f) / 0.4f;
								this._walkSpeed = (this.Avatar.Animations[0].Speed = num15);
							}
							else
							{
								if (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Run")
								{
									this.Avatar.Animations.Play("Run", 0, TimeSpan.FromSeconds(0.25));
								}
								float num16 = 0.8f + (num - 0.8f);
								this._walkSpeed = (this.Avatar.Animations[0].Speed = num16);
							}
						}
						else
						{
							if (num2 > 0.1f)
							{
								if (num3 > 0.8f)
								{
									if (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Run")
									{
										this.Avatar.Animations.Play("Run", 0, TimeSpan.FromSeconds(0.25));
									}
								}
								else if (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Walk")
								{
									this.Avatar.Animations.Play("Walk", 0, TimeSpan.FromSeconds(0.25));
								}
							}
							this.Avatar.Animations[0].Speed = num2;
						}
						this.Avatar.Animations[0].Reversed = walkAmount < 0f;
					}
				}
				if (this.FPSMode)
				{
					GunInventoryItem gunInventoryItem2 = InGameHUD.Instance.ActiveInventoryItem as GunInventoryItem;
					float num17 = 1f;
					if (gunInventoryItem2 != null)
					{
						num17 = gunInventoryItem2.GunClass.ShoulderMagnification;
					}
					switch (this.currentAnimState)
					{
					case Player.AnimationState.Unshouldered:
						this.FPSCamera.FieldOfView = this.DefaultFOV;
						this.Avatar.EyePointCamera.FieldOfView = this.DefaultAvatarFOV;
						this.GunEyePointCamera.FieldOfView = this.DefaultAvatarFOV;
						this.ControlSensitivity = 1f;
						break;
					case Player.AnimationState.Shouldering:
						this.FPSCamera.FieldOfView = Angle.Lerp(this.DefaultFOV, this.DefaultFOV / num17, this.Avatar.Animations[2].Progress);
						this.GunEyePointCamera.FieldOfView = (this.Avatar.EyePointCamera.FieldOfView = Angle.Lerp(this.DefaultAvatarFOV, this.ShoulderedAvatarFOV, this.Avatar.Animations[2].Progress));
						this.ControlSensitivity = 0.25f;
						break;
					case Player.AnimationState.UnShouldering:
						this.FPSCamera.FieldOfView = Angle.Lerp(this.DefaultFOV / num17, this.DefaultFOV, this.Avatar.Animations[2].Progress);
						this.GunEyePointCamera.FieldOfView = (this.Avatar.EyePointCamera.FieldOfView = Angle.Lerp(this.ShoulderedAvatarFOV, this.DefaultAvatarFOV, this.Avatar.Animations[2].Progress));
						this.ControlSensitivity = 0.25f;
						break;
					case Player.AnimationState.Shouldered:
						this.FPSCamera.FieldOfView = this.DefaultFOV / num17;
						this.GunEyePointCamera.FieldOfView = (this.Avatar.EyePointCamera.FieldOfView = this.ShoulderedAvatarFOV);
						this.ControlSensitivity = 0.25f;
						break;
					}
				}
				float num18 = 6f;
				if (this._flyMode)
				{
					Matrix localToWorld3 = this.FPSCamera.LocalToWorld;
					Vector3 vector = Vector3.Multiply(localToWorld3.Right, strafeAmount * num5 * num18);
					vector += Vector3.Multiply(localToWorld3.Forward, -num4 * num18);
					base.PlayerPhysics.WorldVelocity = vector;
					if (this._isFlyingUp)
					{
						Vector3 worldVelocity = base.PlayerPhysics.WorldVelocity;
						worldVelocity.Y += this.JumpImpulse * 1.5f;
						base.PlayerPhysics.WorldVelocity = worldVelocity;
					}
				}
				else if (this.InWater && !this.InContact)
				{
					Matrix localToWorld4 = this.FPSCamera.LocalToWorld;
					Vector3 worldVelocity2 = base.PlayerPhysics.WorldVelocity;
					Vector3.Multiply(localToWorld4.Right, strafeAmount * num5);
					Vector3 vector2 = Vector3.Multiply(localToWorld4.Forward, -num4);
					vector2.Y *= this.PercentSubmergedWater;
					if (Math.Abs(vector2.Y) < Math.Abs(worldVelocity2.Y))
					{
						vector2.Y = worldVelocity2.Y;
					}
					base.PlayerPhysics.WorldVelocity = Vector3.Multiply(localToWorld4.Right, strafeAmount * num5) + vector2;
				}
				else if (this.InContact)
				{
					float num19 = Math.Abs(Vector3.Dot(this.GroundNormal, Vector3.Up));
					base.PlayerPhysics.LocalVelocity = new Vector3(strafeAmount * num5 * num19, base.PlayerPhysics.LocalVelocity.Y, num4 * num19);
				}
				else
				{
					float num20 = (this._isSprinting ? num8 : num7);
					float num21 = 0.8f + (num - 0.8f);
					if (walkAmount < 0f)
					{
						num4 = num21 * num20;
					}
					else
					{
						num4 = -num21 * num20;
					}
					float num22 = Math.Abs(Vector3.Dot(this.GroundNormal, Vector3.Up));
					base.PlayerPhysics.LocalVelocity = new Vector3(strafeAmount * num5 * num22, base.PlayerPhysics.LocalVelocity.Y, num4 * num22);
				}
			}
			this.Avatar.Animations.PlayAnimation(1, this._torsoPitchAnimation, TimeSpan.Zero);
			if (this.FPSMode)
			{
				if (this.Avatar.Animations[0].Name != "Stand")
				{
					this.Avatar.Animations.Play("Stand", 0, TimeSpan.Zero);
				}
				this.Avatar.Animations.ClearAnimation(4, TimeSpan.Zero);
				this._torsoPitchAnimation.Progress = 0.5f;
				return;
			}
			this._torsoPitchAnimation.Progress = (torsoPitch.Degrees + 90f) / 180f;
		}

		internal void AddTeleportStationObject(BlockInventoryItem tpStation)
		{
			this.PlayerInventory.TeleportStationObjects.Add(tpStation);
		}

		internal void RemoveTeleportStationObject(Vector3 _worldPosition)
		{
			BlockInventoryItem blockInventoryItem = this.PlayerInventory.TeleportStationObjects.Find((BlockInventoryItem x) => x.PointToLocation == _worldPosition);
			this.PlayerInventory.TeleportStationObjects.Remove(blockInventoryItem);
		}

		internal void SetSpawnPoint(BlockInventoryItem gpsItem)
		{
			if (this.PlayerInventory.InventorySpawnPointTeleport != null)
			{
				BlockInventoryItem inventorySpawnPointTeleport = this.PlayerInventory.InventorySpawnPointTeleport;
			}
			this.PlayerInventory.InventorySpawnPointTeleport = gpsItem;
		}

		internal Vector3 GetSpawnPoint()
		{
			Vector3 vector = WorldInfo.DefaultStartLocation;
			if (this.PlayerInventory.InventorySpawnPointTeleport != null)
			{
				vector = this.PlayerInventory.InventorySpawnPointTeleport.PointToLocation;
			}
			return vector;
		}

		private const float FALL_DAMAGE_MIN_VELOCITY = 18f;

		private const float FALL_DAMAGE_VELOCITY_MULTIPLIER = 0.06666667f;

		private static MD5HashProvider hasher = new MD5HashProvider();

		public TimeSpan TimeConnected = TimeSpan.Zero;

		public NetworkGamer Gamer;

		public Texture2D GamerPicture;

		public GamerProfile Profile;

		public Avatar Avatar;

		public AABBTraceProbe MovementProbe = new AABBTraceProbe();

		public BoundingBox PlayerAABB = new BoundingBox(new Vector3(-0.35f, 0f, -0.35f), new Vector3(0.35f, 1.7f, 0.35f));

		public ModelEntity _shadow;

		public Angle DefaultFOV = Angle.FromDegrees(73f);

		public Angle DefaultAvatarFOV = Angle.FromDegrees(90f);

		public Angle ShoulderedAvatarFOV = Angle.FromDegrees(45f);

		public PerspectiveCamera GunEyePointCamera = new PerspectiveCamera();

		public IntVector3 FocusCrate;

		public Point FocusCrateItem;

		public bool UsingTool;

		public bool Shouldering;

		public bool Reloading;

		public PlayerMode _playerMode = PlayerMode.Fist;

		public bool IsActive = true;

		private bool _isRunning;

		private bool _isMoveing;

		private bool _isSprinting;

		private bool _isFlyingUp;

		public bool LockedFromFalling;

		public bool _flyMode;

		public bool FinalSaveRegistered;

		public PlayerInventory PlayerInventory;

		public bool ReadyToThrowGrenade;

		public TimeSpan grenadeCookTime = TimeSpan.Zero;

		public bool PlayGrenadeAnim;

		public bool PlayItemUsingOut;

		public bool PlayItemUsingIn;

		public Entity FPSNode = new Entity();

		private float _sprintMultiplier = 2f;

		private float m_baseJumpCost = 0.25f;

		private float m_modJumpCost;

		private float m_multJumpCost;

		private float m_baseSprintCost = 0.6f;

		private float m_modSprintCost;

		private float m_multSprintCost;

		private AnimationPlayer _torsoPitchAnimation;

		public AudioEmitter SoundEmitter = new AudioEmitter();

		public static ParticleEffect _smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\DigSmokeEffect");

		private static ParticleEffect _sparkEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SparksEffect");

		private static ParticleEffect _rocksEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\RocksEffect");

		private static ParticleEffect _starsEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\StarRingEffect");

		private static Model _shadowModel;

		public bool Dead;

		public Entity RightHand;

		private OneShotTimer _footStepTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		public float _walkSpeed = 1f;

		private bool _prevUnderwaterState;

		public Cue UnderwaterCue;

		private static Model ProxyModel;

		private static AvatarExpression[] mouthLevels = new AvatarExpression[4];

		private OneShotTimer mouthTimer = new OneShotTimer(TimeSpan.FromSeconds(0.1));

		public Random rand = new Random();

		private FastRand _fastRand = new FastRand();

		private TraceProbe shadowProbe = new TraceProbe();

		private Angle recoilDecay = Angle.FromDegrees(30f);

		private bool usingAnimationPlaying;

		private Player.AnimationState currentAnimState;

		private float underWaterSpeed = 1.894f;

		private SoundCue3D _reloadCue;

		public string ReloadSound;

		public enum AnimChannels
		{
			Lower,
			Tilt,
			Upper,
			UpperUse,
			Head,
			Death
		}

		public class NoMovePhysics : BasicPhysics
		{
			public NoMovePhysics(Entity e)
				: base(e)
			{
			}

			public override void Move(TimeSpan dt)
			{
			}
		}

		private enum AnimationState
		{
			Unshouldered,
			Shouldering,
			UnShouldering,
			Shouldered,
			Reloading
		}
	}
}
