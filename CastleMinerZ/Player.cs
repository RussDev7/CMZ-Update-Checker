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
				NetworkGamer ng = this.Gamer;
				if (ng != null && ng.AlternateAddress != 0UL)
				{
					return Player.GetHashFromGamerTag(ng.AlternateAddress.ToString());
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
			bool isDefault = false;
			PlayerInventory inv = new PlayerInventory(this, false);
			try
			{
				try
				{
					string filename = Path.Combine(path, this.PlayerHash + ".inv");
					inv.LoadFromStorage(device, filename);
				}
				catch
				{
					string filename2 = Path.Combine(path, this.OldPlayerHash + ".inv");
					inv.LoadFromStorage(device, filename2);
				}
			}
			catch
			{
				isDefault = true;
				inv = new PlayerInventory(this, true);
			}
			this.PlayerInventory = inv;
			return isDefault;
		}

		public void SaveInventory(SaveDevice device, string path)
		{
			try
			{
				string filename = Path.Combine(path, this.PlayerHash + ".inv");
				this.PlayerInventory.SaveToStorage(device, filename);
			}
			catch
			{
			}
		}

		public void DeleteInventory(SaveDevice device, string path)
		{
			try
			{
				string filename = Path.Combine(path, this.PlayerHash + ".inv");
				device.Delete(filename);
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

		public static void Init()
		{
			Player._smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeEffect");
			Player._sparkEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SparksEffect");
			Player._rocksEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\RocksEffect");
			Player._starsEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\StarRingEffect");
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
			float sprintCost = this.m_baseSprintCost * (1f + this.m_multSprintCost);
			return sprintCost + this.m_modSprintCost;
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
				float f = BlockTerrain.Instance.DepthUnderWater(base.WorldPosition);
				if (f < 0f)
				{
					return 0f;
				}
				return Math.Min(1f, f);
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
				float f = (-60f - base.WorldPosition.Y) / 2f;
				if (f < 0f)
				{
					return 0f;
				}
				return Math.Min(1f, f);
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
			Vector3 forward = new Vector3(0f, 0f, -1f);
			forward = Vector3.TransformNormal(forward, base.LocalToWorld);
			if (this.Gamer.IsLocal)
			{
				CastleMinerZGame.Instance.Listener.Position = base.WorldPosition + new Vector3(0f, 1.8f, 0f);
				CastleMinerZGame.Instance.Listener.Forward = forward;
				CastleMinerZGame.Instance.Listener.Up = new Vector3(0f, 1f, 0f);
				CastleMinerZGame.Instance.Listener.Velocity = base.PlayerPhysics.WorldVelocity;
			}
			this.SoundEmitter.Position = base.WorldPosition;
			this.SoundEmitter.Forward = forward;
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
				Vector3 vel = base.PlayerPhysics.WorldVelocity;
				vel.Y += this.JumpImpulse;
				base.PlayerPhysics.WorldVelocity = vel;
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
			InventoryItem.InventoryItemClass c = InventoryItem.GetClass(itemID);
			if (c != null)
			{
				this.RightHand.Children.Add(c.CreateEntity(ItemUse.Hand, this.IsLocal));
				this._playerMode = c.PlayerAnimationMode;
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
					Stream stream = this.Profile.GetGamerPicture();
					this.GamerPicture = Texture2D.FromStream(CastleMinerZGame.Instance.GraphicsDevice, stream);
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
			Matrix gunToWorld;
			Vector3 barrelTip;
			if (this.RightHand.Children.Count > 0 && this.RightHand.Children[0] is GunEntity)
			{
				GunEntity gent = (GunEntity)this.RightHand.Children[0];
				gunToWorld = gent.LocalToWorld;
				if (this.FPSMode)
				{
					Matrix gunToFPSWorld = this.RightHand.LocalToWorld;
					Matrix fpsWorldToCamera = this.GunEyePointCamera.WorldToLocal;
					Matrix CameraToWorld = this.FPSCamera.LocalToWorld;
					gunToWorld = gunToFPSWorld * fpsWorldToCamera * CameraToWorld;
				}
				barrelTip = gent.BarrelTipLocation;
			}
			else
			{
				gunToWorld = this.RightHand.LocalToWorld;
				barrelTip = new Vector3(0f, 0f, -0.5f);
			}
			return Vector3.Transform(barrelTip, gunToWorld);
		}

		private void ProcessPlayerUpdateMessage(Message message)
		{
			if (!this.Gamer.IsLocal)
			{
				PlayerUpdateMessage lm = (PlayerUpdateMessage)message;
				lm.Apply(this);
			}
		}

		private void ProcessGunshotMessage(Message message)
		{
			GunshotMessage gsm = (GunshotMessage)message;
			InventoryItem.InventoryItemClass invClass = InventoryItem.GetClass(gsm.ItemID);
			if (invClass is LaserGunInventoryItemClass)
			{
				Scene scene = base.Scene;
				if (scene != null)
				{
					BlasterShot shot = BlasterShot.Create(this.GetGunTipPosition(), gsm.Direction, gsm.ItemID, message.Sender.Id);
					scene.Children.Add(shot);
				}
			}
			else if (TracerManager.Instance != null)
			{
				TracerManager.Instance.AddTracer(this.FPSCamera.WorldPosition, gsm.Direction, gsm.ItemID, message.Sender.Id);
			}
			if (SoundManager.Instance != null)
			{
				if (invClass.UseSound == null)
				{
					SoundManager.Instance.PlayInstance("GunShot3", this.SoundEmitter);
				}
				else
				{
					SoundManager.Instance.PlayInstance(invClass.UseSound, this.SoundEmitter);
				}
			}
			if (this.RightHand.Children.Count > 0 && this.RightHand.Children[0] is GunEntity)
			{
				GunEntity gent = (GunEntity)this.RightHand.Children[0];
				gent.ShowMuzzleFlash();
			}
		}

		private void ProcessShotgunShotMessage(Message message)
		{
			ShotgunShotMessage gsm = (ShotgunShotMessage)message;
			InventoryItem.InventoryItemClass invClass = InventoryItem.GetClass(gsm.ItemID);
			if (invClass is LaserGunInventoryItemClass)
			{
				this.GetGunTipPosition();
				for (int i = 0; i < 5; i++)
				{
					Scene scene = base.Scene;
					if (scene != null)
					{
						BlasterShot shot = BlasterShot.Create(this.GetGunTipPosition(), gsm.Directions[i], gsm.ItemID, message.Sender.Id);
						scene.Children.Add(shot);
					}
				}
			}
			else if (TracerManager.Instance != null)
			{
				for (int j = 0; j < 5; j++)
				{
					TracerManager.Instance.AddTracer(this.FPSCamera.WorldPosition, gsm.Directions[j], gsm.ItemID, message.Sender.Id);
				}
			}
			if (SoundManager.Instance != null)
			{
				if (invClass.UseSound == null)
				{
					SoundManager.Instance.PlayInstance("GunShot3", this.SoundEmitter);
				}
				else
				{
					SoundManager.Instance.PlayInstance(invClass.UseSound, this.SoundEmitter);
				}
			}
			if (this.RightHand.Children.Count > 0 && this.RightHand.Children[0] is GunEntity)
			{
				GunEntity gent = (GunEntity)this.RightHand.Children[0];
				gent.ShowMuzzleFlash();
			}
		}

		private void ProcessFireRocketMessage(Message message)
		{
			if (base.Scene != null)
			{
				FireRocketMessage frm = (FireRocketMessage)message;
				RocketEntity re = new RocketEntity(frm.Position, frm.Direction, frm.WeaponType, frm.Guided, this.IsLocal);
				SoundManager.Instance.PlayInstance("RPGLaunch", this.SoundEmitter);
				base.Scene.Children.Add(re);
			}
		}

		private void ProcessGrenadeMessage(Message message)
		{
			if (base.Scene != null)
			{
				GrenadeMessage frm = (GrenadeMessage)message;
				GrenadeProjectile re = GrenadeProjectile.Create(frm.Position, frm.Direction * 15f, frm.SecondsLeft, frm.GrenadeType, this.IsLocal);
				base.Scene.Children.Add(re);
				this.Avatar.Animations.Play("Grenade_Reset", 3, TimeSpan.Zero);
				if (this.IsLocal && !CastleMinerZGame.Instance.InfiniteResourceMode && CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem != null && CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.ItemClass is GrenadeInventoryItemClass)
				{
					CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.PopOneItem();
				}
			}
		}

		private void ProcessChangeCarriedItemMessage(Message message)
		{
			ChangeCarriedItemMessage ccim = (ChangeCarriedItemMessage)message;
			InventoryItem.InventoryItemClass @class = InventoryItem.GetClass(ccim.ItemID);
			if (@class != null)
			{
				this.PutItemInHand(ccim.ItemID);
				return;
			}
			this.PutItemInHand(InventoryItemIDs.BareHands);
		}

		private void ProcessDigMessage(Message message)
		{
			DigMessage dm = (DigMessage)message;
			if (dm.Placing)
			{
				SoundManager.Instance.PlayInstance("Place", this.SoundEmitter);
			}
			else
			{
				SoundManager.Instance.PlayInstance(this.GetDigSound(dm.BlockType), this.SoundEmitter);
			}
			if (base.Scene != null && BlockTerrain.Instance.RegionIsLoaded(dm.Location) && CastleMinerZGame.Instance.IsActive)
			{
				ParticleEmitter sparkEmitter = Player._sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
				sparkEmitter.Reset();
				sparkEmitter.Emitting = true;
				sparkEmitter.DrawPriority = 900;
				base.Scene.Children.Add(sparkEmitter);
				ParticleEmitter rockEmitter = Player._rocksEffect.CreateEmitter(CastleMinerZGame.Instance);
				rockEmitter.Reset();
				rockEmitter.Emitting = true;
				rockEmitter.DrawPriority = 900;
				base.Scene.Children.Add(rockEmitter);
				Vector3 axis = Vector3.Cross(Vector3.Forward, -dm.Direction);
				Quaternion rot = Quaternion.CreateFromAxisAngle(axis, Vector3.Forward.AngleBetween(-dm.Direction).Radians);
				rockEmitter.LocalPosition = (sparkEmitter.LocalPosition = dm.Location);
				rockEmitter.LocalRotation = (sparkEmitter.LocalRotation = rot);
			}
		}

		private void ProcessTimeConnectedMessage(Message message)
		{
			if (!this.Gamer.IsLocal)
			{
				TimeConnectedMessage lm = (TimeConnectedMessage)message;
				lm.Apply(this);
			}
		}

		private void ProcessCrateFocusMessage(Message message)
		{
			CrateFocusMessage cfm = (CrateFocusMessage)message;
			this.FocusCrate = cfm.Location;
			this.FocusCrateItem = cfm.ItemIndex;
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
			Quaternion rotY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this._fastRand.GetNextValue(-0.25f, 0.25f) * amount.Radians);
			Quaternion rotX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, this._fastRand.GetNextValue(0.5f, 1f) * amount.Radians);
			base.RecoilRotation = base.RecoilRotation * rotX * rotY;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this.FPSNode.LocalToParent = this.FPSCamera.LocalToWorld;
			Angle recoilAngle = Quaternion.Identity.AngleBetween(base.RecoilRotation);
			if (recoilAngle > Angle.Zero)
			{
				Angle decayAmount = this.recoilDecay * (float)gameTime.ElapsedGameTime.TotalSeconds;
				Angle newRecoilAngle = recoilAngle - decayAmount;
				if (newRecoilAngle < Angle.Zero)
				{
					newRecoilAngle = Angle.Zero;
				}
				base.RecoilRotation = Quaternion.Slerp(Quaternion.Identity, base.RecoilRotation, newRecoilAngle / recoilAngle);
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
				float dragConst = MathHelper.Lerp(0f, 3f, this.PercentSubmergedWater);
				Vector3 dragVel = -base.PlayerPhysics.WorldVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds * dragConst;
				base.PlayerPhysics.WorldVelocity += dragVel;
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
				Vector3 newpos = BlockTerrain.Instance.ClipPositionToLoadedWorld(base.LocalPosition, this.MovementProbe.Radius);
				newpos.Y = Math.Min(74f, newpos.Y);
				base.LocalPosition = newpos;
			}
			this.SimulateTalking(this.Gamer.IsTalking, gameTime);
			this.shadowProbe.Init(base.WorldPosition + new Vector3(0f, 1f, 0f), base.WorldPosition + new Vector3(0f, -2.5f, 0f));
			this.shadowProbe.SkipEmbedded = true;
			BlockTerrain.Instance.Trace(this.shadowProbe);
			this._shadow.Visible = this.shadowProbe._collides;
			if (this._shadow.Visible)
			{
				Vector3 interSection = this.shadowProbe.GetIntersection();
				Vector3 toGround = interSection - base.WorldPosition;
				float height = Math.Abs(toGround.Y);
				this._shadow.LocalPosition = toGround + new Vector3(0f, 0.05f, 0f);
				int maxHeight = 2;
				float blender = height / (float)maxHeight;
				this._shadow.LocalScale = new Vector3(1f + 2f * blender, 1f, 1f + 2f * blender);
				this._shadow.EntityColor = new Color?(new Color(1f, 1f, 1f, Math.Max(0f, 0.5f * (1f - blender))));
			}
		}

		private bool ClipMovementToAvoidFalling(Vector3 worldPos, ref Vector3 nextPos, ref Vector3 velocity)
		{
			bool result = false;
			BlockFace fx = BlockFace.NUM_FACES;
			BlockFace fz = BlockFace.NUM_FACES;
			FallLockTestResult blockx = FallLockTestResult.EMPTY_BLOCK;
			FallLockTestResult blockz = FallLockTestResult.EMPTY_BLOCK;
			float xp = 0f;
			float zp = 0f;
			if (velocity.X > 0f)
			{
				fx = BlockFace.POSX;
			}
			else if (velocity.X < 0f)
			{
				fx = BlockFace.NEGX;
			}
			else
			{
				blockx = FallLockTestResult.SOLID_BLOCK_NO_WALL;
			}
			if (velocity.Z > 0f)
			{
				fz = BlockFace.POSZ;
			}
			else if (velocity.Z < 0f)
			{
				fz = BlockFace.NEGZ;
			}
			else
			{
				blockz = FallLockTestResult.SOLID_BLOCK_NO_WALL;
			}
			IntVector3 v = IntVector3.FromVector3(worldPos + this.PlayerAABB.Min);
			v.Y--;
			if (fx == BlockFace.POSX && blockx != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fx);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockx = res;
					if (blockx == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						xp = (float)v.X + 0.95f - this.PlayerAABB.Min.X;
					}
				}
			}
			if (fz == BlockFace.POSZ && blockz != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fz);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockz = res;
					if (blockz == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						zp = (float)v.Z + 0.95f - this.PlayerAABB.Min.Z;
					}
				}
			}
			v.Z = (int)Math.Floor((double)(worldPos.Z + this.PlayerAABB.Max.Z));
			if (fx == BlockFace.POSX && blockx != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fx);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockx = res;
					if (blockx == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						xp = (float)v.X + 0.95f - this.PlayerAABB.Min.X;
					}
				}
			}
			if (fz == BlockFace.NEGZ && blockz != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fz);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockz = res;
					if (blockz == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						zp = (float)v.Z + 0.05f - this.PlayerAABB.Max.Z;
					}
				}
			}
			v.X = (int)Math.Floor((double)(worldPos.X + this.PlayerAABB.Max.X));
			v.Z = (int)Math.Floor((double)(worldPos.Z + this.PlayerAABB.Min.Z));
			if (fx == BlockFace.NEGX && blockx != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fx);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockx = res;
					if (blockx == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						xp = (float)v.X + 0.05f - this.PlayerAABB.Max.X;
					}
				}
			}
			if (fz == BlockFace.POSZ && blockz != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fz);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockz = res;
					if (blockz == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						zp = (float)v.Z + 0.95f - this.PlayerAABB.Min.Z;
					}
				}
			}
			v.Z = (int)Math.Floor((double)(worldPos.Z + this.PlayerAABB.Max.Z));
			if (fx == BlockFace.NEGX && blockx != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fx);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockx = res;
					if (blockx == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						xp = (float)v.X + 0.05f - this.PlayerAABB.Max.X;
					}
				}
			}
			if (fz == BlockFace.NEGZ && blockz != FallLockTestResult.SOLID_BLOCK_NO_WALL)
			{
				FallLockTestResult res = BlockTerrain.Instance.FallLockFace(v, fz);
				if (res != FallLockTestResult.EMPTY_BLOCK)
				{
					blockz = res;
					if (blockz == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
					{
						zp = (float)v.Z + 0.05f - this.PlayerAABB.Max.Z;
					}
				}
			}
			if (blockx == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
			{
				if (fx == BlockFace.POSX)
				{
					if (nextPos.X > xp)
					{
						nextPos.X = xp;
						velocity.X = 0f;
						result = true;
					}
				}
				else if (nextPos.X < xp)
				{
					velocity.X = 0f;
					nextPos.X = xp;
					result = true;
				}
			}
			if (blockz == FallLockTestResult.SOLID_BLOCK_NEEDS_WALL)
			{
				if (fz == BlockFace.POSZ)
				{
					if (nextPos.Z > zp)
					{
						velocity.Z = 0f;
						nextPos.Z = zp;
						result = true;
					}
				}
				else if (nextPos.Z < zp)
				{
					velocity.Z = 0f;
					nextPos.Z = zp;
					result = true;
				}
			}
			return result;
		}

		public override bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			base.ResolveCollsion(e, out collsionPlane, dt);
			bool result = false;
			if (e == BlockTerrain.Instance)
			{
				float t = (float)dt.ElapsedGameTime.TotalSeconds;
				Vector3 worldPos = base.WorldPosition;
				Vector3 nextPos = worldPos;
				Vector3 velocity = base.PlayerPhysics.WorldVelocity;
				Vector3 fwd = velocity;
				fwd.Y = 0f;
				fwd.Normalize();
				float attenuation = EnemyManager.Instance.AttentuateVelocity(this, fwd, worldPos);
				velocity.X *= attenuation;
				if (velocity.Y > 0f)
				{
					velocity.Y *= attenuation;
				}
				velocity.Z *= attenuation;
				float originalYVelocity = velocity.Y;
				this.SetInContact(false);
				this.MovementProbe.SkipEmbedded = true;
				int iteration = 0;
				for (;;)
				{
					Vector3 prevPos = nextPos;
					Vector3 movement = Vector3.Multiply(velocity, t);
					nextPos += movement;
					this.MovementProbe.Init(prevPos, nextPos, this.PlayerAABB);
					this.MovementProbe.SimulateSlopedSides = CastleMinerZGame.Instance.PlayerStats.AutoClimb && velocity.Y < this.JumpImpulse * 0.5f;
					BlockTerrain.Instance.Trace(this.MovementProbe);
					if (this.MovementProbe._collides)
					{
						result = true;
						if (this.MovementProbe._inFace == BlockFace.POSY)
						{
							this.SetInContact(true);
							this.GroundNormal = new Vector3(0f, 1f, 0f);
						}
						if (this.MovementProbe._startsIn)
						{
							goto IL_042D;
						}
						float realt = Math.Max(this.MovementProbe._inT - 0.001f, 0f);
						nextPos = prevPos + movement * realt;
						if (this.MovementProbe.FoundSlopedBlock && this.MovementProbe.SlopedBlockT <= this.MovementProbe._inT)
						{
							this.SetInContact(true);
							velocity.Y = 0f;
							this.GroundNormal = new Vector3(0f, 1f, 0f);
							nextPos.Y += 5f * realt * t;
							if (nextPos.Y > (float)this.MovementProbe.SlopedBlock.Y + 1.001f)
							{
								nextPos.Y = (float)this.MovementProbe.SlopedBlock.Y + 1.001f;
							}
						}
						velocity -= Vector3.Multiply(this.MovementProbe._inNormal, Vector3.Dot(this.MovementProbe._inNormal, velocity));
						t *= 1f - realt;
						if (t <= 1E-07f)
						{
							goto IL_042D;
						}
						if (velocity.LengthSquared() <= 1E-06f || Vector3.Dot(base.PlayerPhysics.WorldVelocity, velocity) <= 1E-06f)
						{
							break;
						}
					}
					else if (this.MovementProbe.FoundSlopedBlock)
					{
						this.SetInContact(true);
						velocity.Y = 0f;
						this.GroundNormal = new Vector3(0f, 1f, 0f);
						nextPos.Y += 5f * t;
						if (nextPos.Y > (float)this.MovementProbe.SlopedBlock.Y + 1.001f)
						{
							nextPos.Y = (float)this.MovementProbe.SlopedBlock.Y + 1.001f;
						}
					}
					iteration++;
					if (!this.MovementProbe._collides || iteration >= 4)
					{
						goto IL_042D;
					}
				}
				velocity = Vector3.Zero;
				if (this.MovementProbe.FoundSlopedBlock && this.MovementProbe.SlopedBlockT <= this.MovementProbe._inT)
				{
					this.SetInContact(true);
					velocity.Y = 0f;
					this.GroundNormal = new Vector3(0f, 1f, 0f);
					nextPos.Y += 5f * t;
					if (nextPos.Y > (float)this.MovementProbe.SlopedBlock.Y + 1.001f)
					{
						nextPos.Y = (float)this.MovementProbe.SlopedBlock.Y + 1.001f;
					}
				}
				IL_042D:
				if (iteration == 4)
				{
					velocity = Vector3.Zero;
				}
				if (this.InContact && this.LockedFromFalling && (velocity.X != 0f || velocity.Z != 0f))
				{
					result = this.ClipMovementToAvoidFalling(worldPos, ref nextPos, ref velocity) || result;
				}
				float deltaVY = velocity.Y - originalYVelocity;
				base.LocalPosition = nextPos;
				base.PlayerPhysics.WorldVelocity = velocity;
				if (!this.IsLocal)
				{
					this.Avatar.Visible = BlockTerrain.Instance.RegionIsLoaded(nextPos);
				}
				else if (!this._flyMode && deltaVY > 18f && velocity.Y < 0.1f)
				{
					Vector3 hitPos = base.LocalPosition;
					hitPos.Y -= 1f;
					InGameHUD.Instance.ApplyDamage((deltaVY - 18f) * 0.06666667f, hitPos);
				}
				if (this.Avatar != null && this.Avatar.AvatarRenderer != null)
				{
					PlayerModelEntity amodel = (PlayerModelEntity)this.Avatar.ProxyModelEntity;
					nextPos.Y += 1.2f;
					BlockTerrain.Instance.GetEnemyLighting(nextPos, ref amodel.DirectLightDirection[0], ref amodel.DirectLightColor[0], ref amodel.DirectLightDirection[1], ref amodel.DirectLightColor[1], ref amodel.AmbientLight);
				}
			}
			return result;
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
				string anim = this.Avatar.Animations[3].Name;
				return anim == "Grenade_Reset" || anim == "Grenade_Throw" || anim == "Grenade_Cook";
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
			CastleMinerZControllerMapping ccontroller = (CastleMinerZControllerMapping)controller;
			if ((double)controller.Movement.LengthSquared() < 0.1)
			{
				this.LockedFromFalling = false;
			}
			if (CastleMinerZGame.Instance.GameMode == GameModeTypes.Creative && ccontroller.FlyMode.Pressed)
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
				InventoryItem item = CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem;
				if (item is GunInventoryItem)
				{
					GunInventoryItem gun = (GunInventoryItem)item;
					this.Reloading = gun.Reload(CastleMinerZGame.Instance.GameScreen.HUD);
				}
			}
		}

		public void UpdateAnimation(float walkAmount, float strafeAmount, Angle torsoPitch, PlayerMode playerMode, bool doAction)
		{
			float walkabs = Math.Abs(walkAmount);
			float strafeabs = Math.Abs(strafeAmount);
			float totalAbs = Math.Max(walkabs, strafeabs);
			float forwardSpeed = 0f;
			float strafeSpeedMult = 0f;
			float localWalkSpeed = MathHelper.Lerp(0.947f, this.underWaterSpeed, this.PercentSubmergedWater);
			float localRunSpeed = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
			float localSprintSpeed = MathHelper.Lerp(4f * this._sprintMultiplier, this.underWaterSpeed, this.PercentSubmergedWater);
			float localSprintWalkSpeed = MathHelper.Lerp(0.947f * this._sprintMultiplier, this.underWaterSpeed, this.PercentSubmergedWater);
			this._isRunning = false;
			this._isMoveing = totalAbs >= 0.1f;
			string upperUsing = "GenericUse";
			string upperIdle = "GenericIdle";
			string upperWalk = "GenericWalk";
			string shoulderAnim = null;
			string reloadAnim = null;
			switch (playerMode)
			{
			case PlayerMode.Pick:
				upperIdle = "PickIdle";
				upperWalk = "PickWalk";
				upperUsing = "PickUse";
				break;
			case PlayerMode.Block:
				upperIdle = "BlockIdle";
				upperWalk = "BlockWalk";
				upperUsing = "BlockUse";
				break;
			case PlayerMode.Fist:
				upperIdle = "FistIdle";
				upperWalk = "FistWalk";
				upperUsing = "FistUse";
				break;
			case PlayerMode.Assault:
				shoulderAnim = "GunShoulder";
				reloadAnim = "GunReload";
				if (this.Shouldering)
				{
					upperIdle = "GunShoulderIdle";
					upperWalk = "GunShoulderWalk";
					upperUsing = "GunShoulderShoot";
				}
				else
				{
					upperIdle = "GunIdle";
					upperWalk = "GunRun";
					upperUsing = "GunShoot";
				}
				break;
			case PlayerMode.BoltRifle:
				shoulderAnim = "RifleShoulder";
				reloadAnim = "RifleReload";
				if (this.Shouldering)
				{
					upperIdle = "RifleShoulderIdle";
					upperWalk = "RifleShoulderWalk";
					upperUsing = "RifleShoulderShoot";
				}
				else
				{
					upperIdle = "RifleIdle";
					upperWalk = "RifleWalk";
					upperUsing = "RifleShoot";
				}
				break;
			case PlayerMode.Pistol:
				shoulderAnim = "PistolShoulder";
				reloadAnim = "PistolReload";
				if (this.Shouldering)
				{
					upperIdle = "PistolShoulderIdle";
					upperWalk = "PistolShoulderWalk";
					upperUsing = "PistolShoulderShoot";
				}
				else
				{
					upperIdle = "PistolIdle";
					upperWalk = "PistolWalk";
					upperUsing = "PistolShoot";
				}
				break;
			case PlayerMode.PumpShotgun:
				shoulderAnim = "PumpShotgunShoulder";
				reloadAnim = "PumpShotgunReload";
				if (this.Shouldering)
				{
					upperIdle = "PumpShotgunShoulderIdle";
					upperWalk = "PumpShotgunShoulderWalk";
					upperUsing = "PumpShotgunShoulderShoot";
				}
				else
				{
					upperIdle = "PumpShotgunIdle";
					upperWalk = "PumpShotgunRun";
					upperUsing = "PumpShotgunShoot";
				}
				break;
			case PlayerMode.SMG:
				shoulderAnim = "SMGShoulder";
				reloadAnim = "SMGReload";
				if (this.Shouldering)
				{
					upperIdle = "SMGShoulderIdle";
					upperWalk = "SMGShoulderWalk";
					upperUsing = "SMGShoulderShoot";
				}
				else
				{
					upperIdle = "SMGIdle";
					upperWalk = "SMGWalk";
					upperUsing = "SMGShoot";
				}
				break;
			case PlayerMode.LMG:
				shoulderAnim = "LMGShoulder";
				reloadAnim = "LMGReload";
				if (this.Shouldering)
				{
					upperIdle = "LMGShoulderIdle";
					upperWalk = "LMGShoulderWalk";
					upperUsing = "LMGShoulderShoot";
				}
				else
				{
					upperIdle = "LMGIdle";
					upperWalk = "LMGWalk";
					upperUsing = "LMGShoot";
				}
				break;
			case PlayerMode.SpaceAssault:
				shoulderAnim = "LaserGunShoulder";
				reloadAnim = "LaserGunReload";
				if (this.Shouldering)
				{
					upperIdle = "LaserGunShoulderIdle";
					upperWalk = "LaserGunShoulderWalk";
					upperUsing = "LaserGunShoulderShoot";
				}
				else
				{
					upperIdle = "LaserGunIdle";
					upperWalk = "LaserGunRun";
					upperUsing = "LaserGunShoot";
				}
				break;
			case PlayerMode.SpaceBoltRifle:
				shoulderAnim = "LaserRifleShoulder";
				reloadAnim = "LaserRifleReload";
				if (this.Shouldering)
				{
					upperIdle = "LaserRifleShoulderIdle";
					upperWalk = "LaserRifleShoulderWalk";
					upperUsing = "LaserRifleShoulderShoot";
				}
				else
				{
					upperIdle = "LaserRifleIdle";
					upperWalk = "LaserRifleRun";
					upperUsing = "LaserRifleShoot";
				}
				break;
			case PlayerMode.SpacePistol:
				shoulderAnim = "LaserPistolShoulder";
				reloadAnim = "LaserPistolReload";
				if (this.Shouldering)
				{
					upperIdle = "LaserPistolShoulderIdle";
					upperWalk = "LaserPistolShoulderWalk";
					upperUsing = "LaserPistolShoulderShoot";
				}
				else
				{
					upperIdle = "LaserPistolIdle";
					upperWalk = "LaserPistolRun";
					upperUsing = "LaserPistolShoot";
				}
				break;
			case PlayerMode.SpacePumpShotgun:
				shoulderAnim = "LaserGunShoulder";
				reloadAnim = "LaserShotgunReload";
				if (this.Shouldering)
				{
					upperIdle = "LaserGunShoulderIdle";
					upperWalk = "LaserGunShoulderWalk";
					upperUsing = "LaserShotgunShoulderShoot";
				}
				else
				{
					upperIdle = "LaserGunIdle";
					upperWalk = "LaserGunRun";
					upperUsing = "LaserShotgunShoot";
				}
				break;
			case PlayerMode.SpaceSMG:
				shoulderAnim = "LaserSMGShoulder";
				reloadAnim = "LaserSMGReload";
				if (this.Shouldering)
				{
					upperIdle = "LaserSMGShoulderIdle";
					upperWalk = "LaserSMGShoulderWalk";
					upperUsing = "LaserSMGShoulderShoot";
				}
				else
				{
					upperIdle = "LaserSMGIdle";
					upperWalk = "LaserSMGRun";
					upperUsing = "LaserSMGShoot";
				}
				break;
			case PlayerMode.Grenade:
				upperIdle = "GrenadeIdle";
				upperWalk = "GrenadeWalk";
				break;
			case PlayerMode.RPG:
				shoulderAnim = "GunShoulder";
				reloadAnim = "PumpShotgunReload";
				if (this.Shouldering)
				{
					upperIdle = "GunShoulderIdle";
					upperWalk = "GunShoulderWalk";
					upperUsing = "PumpShotgunShoulderShoot";
				}
				else
				{
					upperIdle = "RPGIdle";
					upperWalk = "RPGWalk";
					upperUsing = "RPGShoot";
				}
				break;
			case PlayerMode.Chainsaw:
				upperIdle = "PickIdle";
				upperWalk = "PickWalk";
				upperUsing = "PickUse";
				break;
			case PlayerMode.LaserDrill:
				shoulderAnim = "LaserDrillShoulder";
				reloadAnim = "LaserDrillReload";
				if (this.Shouldering)
				{
					upperIdle = "LaserDrillShoulderIdle";
					upperWalk = "LaserDrillShoulderWalk";
					upperUsing = "LaserDrillShoulderShoot";
				}
				else
				{
					upperIdle = "LaserDrillIdle";
					upperWalk = "LaserDrillRun";
					upperUsing = "LaserDrillShoot";
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
				this.Avatar.Animations.Play(upperUsing, 3, TimeSpan.Zero);
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
								Matrix i = this.FPSCamera.LocalToWorld;
								GrenadeInventoryItemClass grenade = CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.ItemClass as GrenadeInventoryItemClass;
								if (grenade != null)
								{
									GrenadeMessage.Send((LocalNetworkGamer)this.Gamer, i, grenade.GrenadeType, 5f - (float)this.grenadeCookTime.TotalSeconds);
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
								Matrix j = this.FPSCamera.LocalToWorld;
								GrenadeInventoryItemClass grenade2 = CastleMinerZGame.Instance.GameScreen.HUD.ActiveInventoryItem.ItemClass as GrenadeInventoryItemClass;
								if (grenade2 != null)
								{
									GrenadeMessage.Send((LocalNetworkGamer)this.Gamer, j, grenade2.GrenadeType, 5f - (float)this.grenadeCookTime.TotalSeconds);
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
				if (totalAbs < 0.1f)
				{
					Vector3 localVel = base.PlayerPhysics.LocalVelocity;
					localVel.X = (localVel.Z = 0f);
					base.PlayerPhysics.LocalVelocity = localVel;
					this._isMoveing = false;
				}
				else
				{
					float waterSpeed = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
					strafeSpeedMult = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
					forwardSpeed = walkAmount * waterSpeed;
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
						AnimationPlayer player = this.Avatar.Animations.Play(shoulderAnim, 2, TimeSpan.FromSeconds(0.25));
						player.Reversed = true;
					}
					else if (this.currentAnimState == Player.AnimationState.Unshouldered && !this.usingAnimationPlaying)
					{
						if (reloadAnim != null)
						{
							this.currentAnimState = Player.AnimationState.Reloading;
							AnimationPlayer player2 = this.Avatar.Animations.Play(reloadAnim, 2, TimeSpan.FromSeconds(0.25));
							if (this.IsLocal)
							{
								GunInventoryItem item = InGameHUD.Instance.ActiveInventoryItem as GunInventoryItem;
								if (item != null)
								{
									player2.Speed = (float)(player2.Duration.TotalSeconds / item.GunClass.ReloadTime.TotalSeconds);
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
				if (this.Shouldering && shoulderAnim != null && this.currentAnimState == Player.AnimationState.Unshouldered)
				{
					this.currentAnimState = Player.AnimationState.Shouldering;
					this.Avatar.Animations.Play(shoulderAnim, 2, TimeSpan.Zero);
				}
				if (!this.Shouldering && shoulderAnim != null && this.currentAnimState == Player.AnimationState.Shouldered)
				{
					this.currentAnimState = Player.AnimationState.UnShouldering;
					AnimationPlayer player3 = this.Avatar.Animations.Play(shoulderAnim, 2, TimeSpan.Zero);
					player3.Reversed = true;
				}
				if (!this.Shouldering && shoulderAnim == null && this.currentAnimState == Player.AnimationState.Shouldered)
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
						if (this.Avatar.Animations[2] == null || this.Avatar.Animations[2].Name != upperWalk)
						{
							if (this.Shouldering)
							{
								this.Avatar.Animations.Play(upperWalk, 2, TimeSpan.Zero);
							}
							else
							{
								this.Avatar.Animations.Play(upperWalk, 2, TimeSpan.FromSeconds(0.25));
							}
						}
						if (this.Avatar.Animations[2] != null && this.Avatar.Animations[2].Name == upperWalk)
						{
							this.Avatar.Animations[2].Speed = Math.Max(walkabs, strafeabs);
						}
					}
					else if (this.Avatar.Animations[2] == null || this.Avatar.Animations[2].Name != upperIdle)
					{
						if (this.Shouldering)
						{
							this.Avatar.Animations.Play(upperIdle, 2, TimeSpan.FromSeconds(0.10000000149011612));
						}
						else
						{
							this.Avatar.Animations.Play(upperIdle, 2, TimeSpan.FromSeconds(0.25));
						}
					}
				}
				if (totalAbs < 0.1f)
				{
					if (this._flyMode)
					{
						base.PlayerPhysics.WorldVelocity = Vector3.Zero;
					}
					else
					{
						Vector3 localVel2 = base.PlayerPhysics.LocalVelocity;
						localVel2.X = (localVel2.Z = 0f);
						base.PlayerPhysics.LocalVelocity = localVel2;
					}
					if (!this.FPSMode && (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Stand"))
					{
						this.Avatar.Animations.Play("Stand", 0, TimeSpan.FromSeconds(0.25));
					}
					this._isMoveing = false;
				}
				else
				{
					strafeSpeedMult = MathHelper.Lerp(4f, this.underWaterSpeed, this.PercentSubmergedWater);
					if (walkabs < 0.8f)
					{
						float moveSpeed = (this._isSprinting ? localSprintWalkSpeed : localWalkSpeed);
						float walkBlend = (walkabs - 0.1f) / 0.4f;
						if (walkAmount < 0f)
						{
							forwardSpeed = walkBlend * moveSpeed;
						}
						else
						{
							forwardSpeed = -walkBlend * moveSpeed;
						}
					}
					else
					{
						float moveSpeed2 = (this._isSprinting ? localSprintSpeed : localRunSpeed);
						float runBlend = 0.8f + (walkabs - 0.8f);
						if (walkAmount < 0f)
						{
							forwardSpeed = runBlend * moveSpeed2;
						}
						else
						{
							forwardSpeed = -runBlend * moveSpeed2;
						}
						this._isRunning = true;
					}
					if (this.FPSMode)
					{
						if (walkabs < 0.8f)
						{
							this._walkSpeed = (walkabs - 0.1f) / 0.4f;
						}
						else
						{
							this._walkSpeed = 0.8f + (walkabs - 0.8f);
						}
					}
					else if (!this._flyMode)
					{
						if (walkabs > strafeabs)
						{
							if (walkabs < 0.8f)
							{
								if (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Walk")
								{
									this.Avatar.Animations.Play("Walk", 0, TimeSpan.FromSeconds(0.25));
								}
								float walkBlend2 = (walkabs - 0.1f) / 0.4f;
								this._walkSpeed = (this.Avatar.Animations[0].Speed = walkBlend2);
							}
							else
							{
								if (this.Avatar.Animations[0] == null || this.Avatar.Animations[0].Name != "Run")
								{
									this.Avatar.Animations.Play("Run", 0, TimeSpan.FromSeconds(0.25));
								}
								float runBlend2 = 0.8f + (walkabs - 0.8f);
								this._walkSpeed = (this.Avatar.Animations[0].Speed = runBlend2);
							}
						}
						else
						{
							if (strafeabs > 0.1f)
							{
								if (totalAbs > 0.8f)
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
							this.Avatar.Animations[0].Speed = strafeabs;
						}
						this.Avatar.Animations[0].Reversed = walkAmount < 0f;
					}
				}
				if (this.FPSMode)
				{
					GunInventoryItem gun = InGameHUD.Instance.ActiveInventoryItem as GunInventoryItem;
					float magfactor = 1f;
					if (gun != null)
					{
						magfactor = gun.GunClass.ShoulderMagnification;
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
						this.FPSCamera.FieldOfView = Angle.Lerp(this.DefaultFOV, this.DefaultFOV / magfactor, this.Avatar.Animations[2].Progress);
						this.GunEyePointCamera.FieldOfView = (this.Avatar.EyePointCamera.FieldOfView = Angle.Lerp(this.DefaultAvatarFOV, this.ShoulderedAvatarFOV, this.Avatar.Animations[2].Progress));
						this.ControlSensitivity = 0.25f;
						break;
					case Player.AnimationState.UnShouldering:
						this.FPSCamera.FieldOfView = Angle.Lerp(this.DefaultFOV / magfactor, this.DefaultFOV, this.Avatar.Animations[2].Progress);
						this.GunEyePointCamera.FieldOfView = (this.Avatar.EyePointCamera.FieldOfView = Angle.Lerp(this.ShoulderedAvatarFOV, this.DefaultAvatarFOV, this.Avatar.Animations[2].Progress));
						this.ControlSensitivity = 0.25f;
						break;
					case Player.AnimationState.Shouldered:
						this.FPSCamera.FieldOfView = this.DefaultFOV / magfactor;
						this.GunEyePointCamera.FieldOfView = (this.Avatar.EyePointCamera.FieldOfView = this.ShoulderedAvatarFOV);
						this.ControlSensitivity = 0.25f;
						break;
					}
				}
				float flyMult = 6f;
				if (this._flyMode)
				{
					Matrix k = this.FPSCamera.LocalToWorld;
					Vector3 accum = Vector3.Multiply(k.Right, strafeAmount * strafeSpeedMult * flyMult);
					accum += Vector3.Multiply(k.Forward, -forwardSpeed * flyMult);
					base.PlayerPhysics.WorldVelocity = accum;
					if (this._isFlyingUp)
					{
						Vector3 vel = base.PlayerPhysics.WorldVelocity;
						vel.Y += this.JumpImpulse * 1.5f;
						base.PlayerPhysics.WorldVelocity = vel;
					}
				}
				else if (this.InWater && !this.InContact)
				{
					Matrix l = this.FPSCamera.LocalToWorld;
					Vector3 vel2 = base.PlayerPhysics.WorldVelocity;
					Vector3.Multiply(l.Right, strafeAmount * strafeSpeedMult);
					Vector3 toAdd = Vector3.Multiply(l.Forward, -forwardSpeed);
					toAdd.Y *= this.PercentSubmergedWater;
					if (Math.Abs(toAdd.Y) < Math.Abs(vel2.Y))
					{
						toAdd.Y = vel2.Y;
					}
					base.PlayerPhysics.WorldVelocity = Vector3.Multiply(l.Right, strafeAmount * strafeSpeedMult) + toAdd;
				}
				else if (this.InContact)
				{
					float damp = Math.Abs(Vector3.Dot(this.GroundNormal, Vector3.Up));
					base.PlayerPhysics.LocalVelocity = new Vector3(strafeAmount * strafeSpeedMult * damp, base.PlayerPhysics.LocalVelocity.Y, forwardSpeed * damp);
				}
				else
				{
					float moveSpeed3 = (this._isSprinting ? localSprintSpeed : localRunSpeed);
					float runBlend3 = 0.8f + (walkabs - 0.8f);
					if (walkAmount < 0f)
					{
						forwardSpeed = runBlend3 * moveSpeed3;
					}
					else
					{
						forwardSpeed = -runBlend3 * moveSpeed3;
					}
					float damp2 = Math.Abs(Vector3.Dot(this.GroundNormal, Vector3.Up));
					base.PlayerPhysics.LocalVelocity = new Vector3(strafeAmount * strafeSpeedMult * damp2, base.PlayerPhysics.LocalVelocity.Y, forwardSpeed * damp2);
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
			BlockInventoryItem tpStation = this.PlayerInventory.TeleportStationObjects.Find((BlockInventoryItem x) => x.PointToLocation == _worldPosition);
			this.PlayerInventory.TeleportStationObjects.Remove(tpStation);
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
			Vector3 spawnPoint = WorldInfo.DefaultStartLocation;
			if (this.PlayerInventory.InventorySpawnPointTeleport != null)
			{
				spawnPoint = this.PlayerInventory.InventorySpawnPointTeleport.PointToLocation;
			}
			return spawnPoint;
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

		public static ParticleEffect _smokeEffect;

		private static ParticleEffect _sparkEffect;

		private static ParticleEffect _rocksEffect;

		private static ParticleEffect _starsEffect;

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

		public Player.AnimationState currentAnimState;

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

		public enum AnimationState
		{
			Unshouldered,
			Shouldering,
			UnShouldering,
			Shouldered,
			Reloading
		}
	}
}
