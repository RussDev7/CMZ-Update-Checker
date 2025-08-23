using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class BlasterShot : Entity
	{
		public static void Init()
		{
			BlasterShot._tracerModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Projectiles\\Laser\\Tracer_Bolt");
			BlasterShot._smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeEffect");
			BlasterShot._sparkEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SparksEffect");
			BlasterShot._spashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\BlasterFlash");
		}

		public static BlasterShot Create(Vector3 position, Vector3 velocity, int enemyId, InventoryItemIDs item)
		{
			BlasterShot bullet = null;
			LaserGunInventoryItemClass gun = InventoryItem.GetClass(item) as LaserGunInventoryItemClass;
			if (gun != null)
			{
				for (int i = 0; i < BlasterShot._garbage.Count; i++)
				{
					if (BlasterShot._garbage[i].Parent == null)
					{
						bullet = BlasterShot._garbage[i];
						break;
					}
				}
				if (bullet == null)
				{
					bullet = new BlasterShot(0);
					BlasterShot._garbage.Add(bullet);
				}
				bullet._lifeTime = BlasterShot.TotalLifeTime;
				bullet._color = new Color(gun.TracerColor);
				bullet._enemyID = enemyId;
				bullet._tracer.EntityColor = new Color?(bullet._color);
				bullet.CollisionsRemaining = 3;
				bullet.ReflectedShot = false;
				bullet._velocity = velocity * 200f;
				bullet.LocalToParent = MathTools.CreateWorld(position, velocity);
				bullet._firstUpdate = true;
				bullet._noCollideFrame = false;
				bullet._weaponClassUsed = gun;
				bullet._weaponUsed = item;
				bullet._lastPosition = position;
				bullet._explosiveType = (gun.IsHarvestWeapon() ? ExplosiveTypes.Harvest : ExplosiveTypes.Laser);
			}
			return bullet;
		}

		public static BlasterShot Create(Vector3 position, Vector3 velocity, InventoryItemIDs item, byte shooterID)
		{
			BlasterShot bullet = null;
			LaserGunInventoryItemClass gun = InventoryItem.GetClass(item) as LaserGunInventoryItemClass;
			if (gun != null)
			{
				for (int i = 0; i < BlasterShot._garbage.Count; i++)
				{
					if (BlasterShot._garbage[i].Parent == null)
					{
						bullet = BlasterShot._garbage[i];
						break;
					}
				}
				if (bullet == null)
				{
					bullet = new BlasterShot(shooterID);
					BlasterShot._garbage.Add(bullet);
				}
				bullet._lifeTime = BlasterShot.TotalLifeTime;
				bullet._color = new Color(gun.TracerColor);
				bullet._shooter = shooterID;
				bullet._enemyID = -1;
				bullet._tracer.EntityColor = new Color?(bullet._color);
				bullet.CollisionsRemaining = 30;
				bullet.ReflectedShot = false;
				bullet._velocity = velocity * 200f;
				bullet.LocalToParent = MathTools.CreateWorld(position, velocity);
				bullet._firstUpdate = true;
				bullet._noCollideFrame = false;
				bullet._weaponClassUsed = gun;
				bullet._weaponUsed = item;
				bullet._lastPosition = position;
				bullet._explosiveType = (gun.IsHarvestWeapon() ? ExplosiveTypes.Harvest : ExplosiveTypes.Laser);
			}
			return bullet;
		}

		public BlasterShot()
		{
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			bool remove = false;
			this._lifeTime -= gameTime.ElapsedGameTime;
			if (this._lifeTime <= TimeSpan.Zero)
			{
				remove = true;
			}
			else
			{
				this._lastPosition = base.WorldPosition;
				base.LocalPosition = this._lastPosition + this._velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if ((CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.PVPState != CastleMinerZGame.PVPEnum.Off) || this._enemyID >= 0)
				{
					bool hitTarget = false;
					bool collision = false;
					float targetT = 2.1474836E+09f;
					Vector3 hitLocation = Vector3.Zero;
					Player playerHit = null;
					for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
					{
						NetworkGamer gamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
						if (gamer.Tag != null)
						{
							Player player = (Player)gamer.Tag;
							if (player.ValidLivingGamer)
							{
								Vector3 p = player.WorldPosition;
								BoundingBox bb = player.PlayerAABB;
								bb.Min += p;
								bb.Max += p;
								BlasterShot.tp.Reset();
								BlasterShot.tp.TestBoundBox(bb);
								if (BlasterShot.tp._collides && BlasterShot.tp._inT < targetT)
								{
									playerHit = player;
									hitLocation = BlasterShot.tp.GetIntersection();
									hitTarget = true;
									collision = true;
									targetT = BlasterShot.tp._inT;
								}
							}
						}
					}
					BlasterShot.tp.Reset();
					BlockTerrain.Instance.Trace(BlasterShot.tp);
					if (BlasterShot.tp._collides && BlasterShot.tp._inT < targetT)
					{
						hitTarget = false;
						collision = true;
						hitLocation = BlasterShot.tp.GetIntersection();
					}
					if (collision)
					{
						Vector3 Head = hitLocation;
						if (this._enemyID < 0)
						{
							remove = true;
						}
						if (hitTarget)
						{
							if (playerHit.IsLocal && (this._enemyID > 0 || this._shooter != playerHit.Gamer.Id))
							{
								LocalNetworkGamer localgamer = (LocalNetworkGamer)playerHit.Gamer;
								if (CastleMinerZGame.Instance.PVPState == CastleMinerZGame.PVPEnum.Everyone || (!localgamer.IsHost && !localgamer.SignedInGamer.IsFriend(CastleMinerZGame.Instance.CurrentNetworkSession.Host)))
								{
									InGameHUD.Instance.ApplyDamage(0.4f, Head);
								}
							}
							SoundManager.Instance.PlayInstance("BulletHitHuman", playerHit.SoundEmitter);
						}
					}
				}
				IShootableEnemy z = null;
				BlasterShot.tp.Init(this._lastPosition, base.WorldPosition);
				if (this._enemyID < 0)
				{
					z = EnemyManager.Instance.Trace(BlasterShot.tp, false);
				}
				if (BlasterShot.tp._collides)
				{
					Vector3 collisionPoint = BlasterShot.tp.GetIntersection();
					bool bounce = false;
					bool destroyBlock = false;
					IntVector3 blockToDestroy = IntVector3.Zero;
					if (z != null)
					{
						z.TakeDamage(collisionPoint, Vector3.Normalize(this._velocity), this._weaponClassUsed, this._shooter);
						if (z is BaseZombie)
						{
						}
					}
					else
					{
						BlockType bt = BlockType.GetType(BlockTerrain.Instance.GetBlockWithChanges(BlasterShot.tp._worldIndex));
						bounce = bt.BouncesLasers;
						destroyBlock = bt.CanBeDug;
						blockToDestroy = BlasterShot.tp._worldIndex;
					}
					if (z is DragonClientEntity)
					{
						ParticleEmitter flashEmitter = TracerManager._dragonFlashEffect.CreateEmitter(CastleMinerZGame.Instance);
						flashEmitter.Reset();
						flashEmitter.Emitting = true;
						TracerManager.Instance.Scene.Children.Add(flashEmitter);
						flashEmitter.LocalPosition = collisionPoint;
						flashEmitter.DrawPriority = 900;
					}
					new Plane(BlasterShot.tp._inNormal, Vector3.Dot(BlasterShot.tp._inNormal, collisionPoint));
					this.HandleCollision(BlasterShot.tp._inNormal, collisionPoint, bounce, destroyBlock, blockToDestroy);
				}
			}
			if (remove)
			{
				base.RemoveFromParent();
			}
		}

		private BlasterShot(byte shooter)
		{
			this._tracer = new ModelEntity(BlasterShot._tracerModel);
			this._tracer.DrawPriority = 900;
			this._tracer.BlendState = BlendState.Additive;
			this._tracer.DepthStencilState = DepthStencilState.DepthRead;
			this._tracer.RasterizerState = RasterizerState.CullNone;
			this._tracer.LocalScale = new Vector3(1.5f, 1.5f, 8f);
			this._tracer.LocalPosition = new Vector3(0f, 0f, -12f);
			base.Children.Add(this._tracer);
			this._shooter = shooter;
			this.Collider = false;
			this.Collidee = false;
		}

		private void HandleCollision(Vector3 collisionNormal, Vector3 collisionLocation, bool bounce, bool destroyBlock, IntVector3 blockToDestroy)
		{
			Scene scene = null;
			if (TracerManager.Instance != null)
			{
				scene = TracerManager.Instance.Scene;
			}
			Matrix newLTP = MathTools.CreateWorld(collisionLocation, -collisionNormal);
			if (scene != null && CastleMinerZGame.Instance.IsActive)
			{
				ParticleEmitter emitter = BlasterShot._spashEffect.CreateEmitter(CastleMinerZGame.Instance);
				emitter.LocalScale = new Vector3(0.01f);
				emitter.Reset();
				emitter.Emitting = true;
				emitter.LocalToParent = newLTP;
				scene.Children.Add(emitter);
				emitter.DrawPriority = 900;
				ParticleEmitter sparkEmitter = BlasterShot._sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
				sparkEmitter.Reset();
				sparkEmitter.Emitting = true;
				sparkEmitter.LocalToParent = newLTP;
				scene.Children.Add(sparkEmitter);
				sparkEmitter.DrawPriority = 900;
				ParticleEmitter smokeEmitter = BlasterShot._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				smokeEmitter.Reset();
				smokeEmitter.Emitting = true;
				smokeEmitter.LocalToParent = newLTP;
				scene.Children.Add(smokeEmitter);
				smokeEmitter.DrawPriority = 900;
				this.Emitter.Velocity = new Vector3(0f, 0f, 0f);
				this.Emitter.Position = newLTP.Translation;
				this.Emitter.Up = new Vector3(0f, 1f, 0f);
				this.Emitter.Forward = new Vector3(0f, 0f, 1f);
			}
			bool doBounce = false;
			if (this.CollisionsRemaining > 0)
			{
				this.CollisionsRemaining--;
				doBounce = bounce;
				this.ReflectedShot = bounce;
			}
			if (doBounce)
			{
				Vector3 offset = base.WorldPosition - collisionLocation;
				Vector3 newPosition = Vector3.Reflect(offset, collisionNormal) + collisionLocation;
				this._lastPosition = collisionLocation;
				this._velocity = Vector3.Reflect(this._velocity, collisionNormal);
				base.LocalToParent = MathTools.CreateWorld(newPosition, this._velocity);
				this._noCollideFrame = true;
			}
			else
			{
				this._velocity = new Vector3(0f, 0f, 0f);
				base.RemoveFromParent();
			}
			if (CastleMinerZGame.Instance.IsLocalPlayerId(this._shooter) && destroyBlock)
			{
				Explosive.FindBlocksToRemove(blockToDestroy, this._explosiveType, true);
			}
		}

		private static List<BlasterShot> _garbage = new List<BlasterShot>();

		private static ParticleEffect _spashEffect;

		private static ParticleEffect _sparkEffect;

		private static ParticleEffect _smokeEffect;

		private static Model _tracerModel;

		private static TracerManager.TracerProbe tp = new TracerManager.TracerProbe();

		private bool _headshot;

		private bool _firstUpdate = true;

		private bool _noCollideFrame;

		private Vector3 _lastPosition;

		private byte _shooter;

		private int _enemyID = -1;

		private Color _color;

		public AudioEmitter Emitter = new AudioEmitter();

		public int CollisionsRemaining;

		private bool ReflectedShot;

		private static readonly TimeSpan TotalLifeTime = TimeSpan.FromSeconds(3.0);

		private TimeSpan _lifeTime = BlasterShot.TotalLifeTime;

		private ModelEntity _tracer;

		private InventoryItemIDs _weaponUsed;

		private LaserGunInventoryItemClass _weaponClassUsed;

		private Vector3 _velocity;

		private ExplosiveTypes _explosiveType = ExplosiveTypes.Laser;
	}
}
