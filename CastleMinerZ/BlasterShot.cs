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
			BlasterShot blasterShot = null;
			LaserGunInventoryItemClass laserGunInventoryItemClass = InventoryItem.GetClass(item) as LaserGunInventoryItemClass;
			if (laserGunInventoryItemClass != null)
			{
				for (int i = 0; i < BlasterShot._garbage.Count; i++)
				{
					if (BlasterShot._garbage[i].Parent == null)
					{
						blasterShot = BlasterShot._garbage[i];
						break;
					}
				}
				if (blasterShot == null)
				{
					blasterShot = new BlasterShot(0);
					BlasterShot._garbage.Add(blasterShot);
				}
				blasterShot._lifeTime = BlasterShot.TotalLifeTime;
				blasterShot._color = new Color(laserGunInventoryItemClass.TracerColor);
				blasterShot._enemyID = enemyId;
				blasterShot._tracer.EntityColor = new Color?(blasterShot._color);
				blasterShot.CollisionsRemaining = 3;
				blasterShot.ReflectedShot = false;
				blasterShot._velocity = velocity * 200f;
				blasterShot.LocalToParent = MathTools.CreateWorld(position, velocity);
				blasterShot._firstUpdate = true;
				blasterShot._noCollideFrame = false;
				blasterShot._weaponClassUsed = laserGunInventoryItemClass;
				blasterShot._weaponUsed = item;
				blasterShot._lastPosition = position;
				blasterShot._explosiveType = (laserGunInventoryItemClass.IsHarvestWeapon() ? ExplosiveTypes.Harvest : ExplosiveTypes.Laser);
			}
			return blasterShot;
		}

		public static BlasterShot Create(Vector3 position, Vector3 velocity, InventoryItemIDs item, byte shooterID)
		{
			BlasterShot blasterShot = null;
			LaserGunInventoryItemClass laserGunInventoryItemClass = InventoryItem.GetClass(item) as LaserGunInventoryItemClass;
			if (laserGunInventoryItemClass != null)
			{
				for (int i = 0; i < BlasterShot._garbage.Count; i++)
				{
					if (BlasterShot._garbage[i].Parent == null)
					{
						blasterShot = BlasterShot._garbage[i];
						break;
					}
				}
				if (blasterShot == null)
				{
					blasterShot = new BlasterShot(shooterID);
					BlasterShot._garbage.Add(blasterShot);
				}
				blasterShot._lifeTime = BlasterShot.TotalLifeTime;
				blasterShot._color = new Color(laserGunInventoryItemClass.TracerColor);
				blasterShot._shooter = shooterID;
				blasterShot._enemyID = -1;
				blasterShot._tracer.EntityColor = new Color?(blasterShot._color);
				blasterShot.CollisionsRemaining = 30;
				blasterShot.ReflectedShot = false;
				blasterShot._velocity = velocity * 200f;
				blasterShot.LocalToParent = MathTools.CreateWorld(position, velocity);
				blasterShot._firstUpdate = true;
				blasterShot._noCollideFrame = false;
				blasterShot._weaponClassUsed = laserGunInventoryItemClass;
				blasterShot._weaponUsed = item;
				blasterShot._lastPosition = position;
				blasterShot._explosiveType = (laserGunInventoryItemClass.IsHarvestWeapon() ? ExplosiveTypes.Harvest : ExplosiveTypes.Laser);
			}
			return blasterShot;
		}

		public BlasterShot()
		{
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			bool flag = false;
			this._lifeTime -= gameTime.ElapsedGameTime;
			if (this._lifeTime <= TimeSpan.Zero)
			{
				flag = true;
			}
			else
			{
				this._lastPosition = base.WorldPosition;
				base.LocalPosition = this._lastPosition + this._velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if ((CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.PVPState != CastleMinerZGame.PVPEnum.Off) || this._enemyID >= 0)
				{
					bool flag2 = false;
					bool flag3 = false;
					float num = 2.1474836E+09f;
					Vector3 vector = Vector3.Zero;
					Player player = null;
					for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
					{
						NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
						if (networkGamer.Tag != null)
						{
							Player player2 = (Player)networkGamer.Tag;
							if (player2.ValidLivingGamer)
							{
								Vector3 worldPosition = player2.WorldPosition;
								BoundingBox playerAABB = player2.PlayerAABB;
								playerAABB.Min += worldPosition;
								playerAABB.Max += worldPosition;
								BlasterShot.tp.Reset();
								BlasterShot.tp.TestBoundBox(playerAABB);
								if (BlasterShot.tp._collides && BlasterShot.tp._inT < num)
								{
									player = player2;
									vector = BlasterShot.tp.GetIntersection();
									flag2 = true;
									flag3 = true;
									num = BlasterShot.tp._inT;
								}
							}
						}
					}
					BlasterShot.tp.Reset();
					BlockTerrain.Instance.Trace(BlasterShot.tp);
					if (BlasterShot.tp._collides && BlasterShot.tp._inT < num)
					{
						flag2 = false;
						flag3 = true;
						vector = BlasterShot.tp.GetIntersection();
					}
					if (flag3)
					{
						Vector3 vector2 = vector;
						if (this._enemyID < 0)
						{
							flag = true;
						}
						if (flag2)
						{
							if (player.IsLocal && (this._enemyID > 0 || this._shooter != player.Gamer.Id))
							{
								LocalNetworkGamer localNetworkGamer = (LocalNetworkGamer)player.Gamer;
								if (CastleMinerZGame.Instance.PVPState == CastleMinerZGame.PVPEnum.Everyone || (!localNetworkGamer.IsHost && !localNetworkGamer.SignedInGamer.IsFriend(CastleMinerZGame.Instance.CurrentNetworkSession.Host)))
								{
									InGameHUD.Instance.ApplyDamage(0.4f, vector2);
								}
							}
							SoundManager.Instance.PlayInstance("BulletHitHuman", player.SoundEmitter);
						}
					}
				}
				IShootableEnemy shootableEnemy = null;
				BlasterShot.tp.Init(this._lastPosition, base.WorldPosition);
				if (this._enemyID < 0)
				{
					shootableEnemy = EnemyManager.Instance.Trace(BlasterShot.tp, false);
				}
				if (BlasterShot.tp._collides)
				{
					Vector3 intersection = BlasterShot.tp.GetIntersection();
					bool flag4 = false;
					bool flag5 = false;
					IntVector3 intVector = IntVector3.Zero;
					if (shootableEnemy != null)
					{
						shootableEnemy.TakeDamage(intersection, Vector3.Normalize(this._velocity), this._weaponClassUsed, this._shooter);
						if (shootableEnemy is BaseZombie)
						{
						}
					}
					else
					{
						BlockType type = BlockType.GetType(BlockTerrain.Instance.GetBlockWithChanges(BlasterShot.tp._worldIndex));
						flag4 = type.BouncesLasers;
						flag5 = type.CanBeDug;
						intVector = BlasterShot.tp._worldIndex;
					}
					if (shootableEnemy is DragonClientEntity)
					{
						ParticleEmitter particleEmitter = TracerManager._dragonFlashEffect.CreateEmitter(CastleMinerZGame.Instance);
						particleEmitter.Reset();
						particleEmitter.Emitting = true;
						TracerManager.Instance.Scene.Children.Add(particleEmitter);
						particleEmitter.LocalPosition = intersection;
						particleEmitter.DrawPriority = 900;
					}
					new Plane(BlasterShot.tp._inNormal, Vector3.Dot(BlasterShot.tp._inNormal, intersection));
					this.HandleCollision(BlasterShot.tp._inNormal, intersection, flag4, flag5, intVector);
				}
			}
			if (flag)
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
			Matrix matrix = MathTools.CreateWorld(collisionLocation, -collisionNormal);
			if (scene != null && CastleMinerZGame.Instance.IsActive)
			{
				ParticleEmitter particleEmitter = BlasterShot._spashEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter.LocalScale = new Vector3(0.01f);
				particleEmitter.Reset();
				particleEmitter.Emitting = true;
				particleEmitter.LocalToParent = matrix;
				scene.Children.Add(particleEmitter);
				particleEmitter.DrawPriority = 900;
				ParticleEmitter particleEmitter2 = BlasterShot._sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter2.Reset();
				particleEmitter2.Emitting = true;
				particleEmitter2.LocalToParent = matrix;
				scene.Children.Add(particleEmitter2);
				particleEmitter2.DrawPriority = 900;
				ParticleEmitter particleEmitter3 = BlasterShot._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
				particleEmitter3.Reset();
				particleEmitter3.Emitting = true;
				particleEmitter3.LocalToParent = matrix;
				scene.Children.Add(particleEmitter3);
				particleEmitter3.DrawPriority = 900;
				this.Emitter.Velocity = new Vector3(0f, 0f, 0f);
				this.Emitter.Position = matrix.Translation;
				this.Emitter.Up = new Vector3(0f, 1f, 0f);
				this.Emitter.Forward = new Vector3(0f, 0f, 1f);
			}
			bool flag = false;
			if (this.CollisionsRemaining > 0)
			{
				this.CollisionsRemaining--;
				flag = bounce;
				this.ReflectedShot = bounce;
			}
			if (flag)
			{
				Vector3 vector = base.WorldPosition - collisionLocation;
				Vector3 vector2 = Vector3.Reflect(vector, collisionNormal) + collisionLocation;
				this._lastPosition = collisionLocation;
				this._velocity = Vector3.Reflect(this._velocity, collisionNormal);
				base.LocalToParent = MathTools.CreateWorld(vector2, this._velocity);
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
