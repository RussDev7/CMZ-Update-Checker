using System;
using System.Collections.Generic;
using System.Threading;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class TracerManager : Entity
	{
		public static void Initialize()
		{
			TracerManager._effect = CastleMinerZGame.Instance.Content.Load<Effect>("Shaders\\Tracer");
			TracerManager._smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SmokeEffect");
			TracerManager._sparkEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\SparksEffect");
			TracerManager._dragonFlashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("ParticleEffects\\FlashEffect");
			short[] array = new short[]
			{
				1, 0, 2, 3, 0, 1, 2, 0, 3, 1,
				4, 3, 2, 4, 1, 3, 4, 2
			};
			short[] array2 = new short[360];
			short num = 0;
			int num2 = 0;
			int i = 0;
			while (i < 20)
			{
				for (int j = 0; j < array.Length; j++)
				{
					array2[num2++] = array[j] + num;
				}
				i++;
				num += 5;
			}
			bool flag = false;
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDevice())
				{
					try
					{
						TracerManager._vb1 = new DynamicVertexBuffer(CastleMinerZGame.Instance.GraphicsDevice, typeof(TracerManager.TracerVertex), 100, BufferUsage.WriteOnly);
						TracerManager._vb2 = new DynamicVertexBuffer(CastleMinerZGame.Instance.GraphicsDevice, typeof(TracerManager.TracerVertex), 100, BufferUsage.WriteOnly);
						TracerManager._ib = new IndexBuffer(CastleMinerZGame.Instance.GraphicsDevice, IndexElementSize.SixteenBits, 360, BufferUsage.WriteOnly);
						TracerManager._ib.SetData<short>(array2);
					}
					finally
					{
						GraphicsDeviceLocker.Instance.UnlockDevice();
					}
					flag = true;
				}
				if (!flag)
				{
					Thread.Sleep(10);
				}
			}
			while (!flag);
		}

		public TracerManager()
		{
			TracerManager.Instance = this;
			this.Collidee = false;
			this.Collider = false;
			for (int i = 0; i < 100; i++)
			{
				this._vertexCache[i] = new TracerManager.TracerVertex(Vector3.Zero, (i % 5 == 4) ? Color.Red.ToVector4() : Color.Red.ToVector4());
			}
			this.DrawPriority = 700;
		}

		private TracerManager.Tracer GetTracer()
		{
			int num = this._unusedTracers.Count;
			TracerManager.Tracer tracer;
			if (num != 0)
			{
				num--;
				tracer = this._unusedTracers[num];
				this._unusedTracers.RemoveAt(num);
			}
			else
			{
				this._unusedTracers.Add(new TracerManager.Tracer());
				tracer = new TracerManager.Tracer();
			}
			return tracer;
		}

		public void AddTracer(Vector3 position, Vector3 velocity, InventoryItemIDs item, byte shooterID)
		{
			TracerManager.Tracer tracer = this.GetTracer();
			tracer.Init(position, velocity, item, shooterID);
			this._tracers.Add(tracer);
			EnemyManager.Instance.RegisterGunShot(position);
		}

		public void AddArrow(Vector3 position, Vector3 velocity, Player target)
		{
			TracerManager.Tracer tracer = this.GetTracer();
			tracer.Init(position, velocity, target, Color.Pink);
			this._tracers.Add(tracer);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			int count = this._tracers.Count;
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			for (int i = 0; i < count; i++)
			{
				this._tracers[i].Update(num);
			}
			base.OnUpdate(gameTime);
		}

		private void FlushTracers(TracerManager.TracerVertex[] vertexCache, int numtracers, bool firsttime, GraphicsDevice g, Matrix view, Matrix projection)
		{
			if (firsttime)
			{
				g.BlendState = BlendState.NonPremultiplied;
				g.DepthStencilState = DepthStencilState.DepthRead;
				TracerManager._effect.Parameters["Projection"].SetValue(projection);
				TracerManager._effect.Parameters["View"].SetValue(view);
				TracerManager._effect.Parameters["World"].SetValue(Matrix.Identity);
				g.Indices = TracerManager._ib;
				TracerManager._effect.CurrentTechnique = TracerManager._effect.Techniques[0];
				TracerManager._effect.CurrentTechnique.Passes[0].Apply();
				TracerManager._currentVb = TracerManager._vb1;
			}
			else if (TracerManager._currentVb == TracerManager._vb1)
			{
				TracerManager._currentVb = TracerManager._vb2;
			}
			else
			{
				TracerManager._currentVb = TracerManager._vb1;
			}
			TracerManager._currentVb.SetData<TracerManager.TracerVertex>(vertexCache, 0, numtracers * 5);
			g.SetVertexBuffer(TracerManager._currentVb);
			g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numtracers * 5, 0, numtracers * 6);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			int num = this._tracers.Count;
			double totalSeconds = gameTime.ElapsedGameTime.TotalSeconds;
			int num2 = 0;
			bool flag = true;
			BlendState blendState = device.BlendState;
			RasterizerState rasterizerState = device.RasterizerState;
			DepthStencilState depthStencilState = device.DepthStencilState;
			int i = 0;
			while (i < num)
			{
				num2 = this._tracers[i].AddToDrawCache(this._vertexCache, num2);
				if (num2 == 20)
				{
					this.FlushTracers(this._vertexCache, num2, flag, device, view, projection);
					flag = false;
					num2 = 0;
				}
				if (this._tracers[i].RemoveAfterDraw)
				{
					this._unusedTracers.Add(this._tracers[i]);
					num--;
					if (num != i)
					{
						this._tracers[i] = this._tracers[num];
					}
					this._tracers.RemoveAt(num);
				}
				else
				{
					i++;
				}
			}
			if (num2 != 0)
			{
				this.FlushTracers(this._vertexCache, num2, flag, device, view, projection);
				flag = false;
			}
			if (!flag)
			{
				device.RasterizerState = rasterizerState;
				device.BlendState = blendState;
			}
			base.Draw(device, gameTime, view, projection);
		}

		private const float TRACER_TIME = 2f;

		private const float TRACER_WIDTH = 0.025f;

		private const int MAX_TRACERS_PER_DRAW = 20;

		private const int VXS_PER_TRACE = 5;

		private const int POLYS_PER_TRACE = 6;

		private const int VERTEX_BUFFER_SIZE = 100;

		private const float ARROW_SOUND_CUE_DISTANCE = 5f;

		private const float ARROW_SOUND_CUE_DISTANCE_SQ = 25f;

		private static Effect _effect;

		public static ParticleEffect _smokeEffect;

		public static ParticleEffect _sparkEffect;

		public static ParticleEffect _dragonFlashEffect;

		private static DynamicVertexBuffer _currentVb;

		private static DynamicVertexBuffer _vb1;

		private static DynamicVertexBuffer _vb2;

		private static IndexBuffer _ib;

		public static TracerManager Instance;

		private List<TracerManager.Tracer> _tracers = new List<TracerManager.Tracer>();

		private List<TracerManager.Tracer> _unusedTracers = new List<TracerManager.Tracer>();

		private TracerManager.TracerVertex[] _vertexCache = new TracerManager.TracerVertex[100];

		public struct TracerVertex : IVertexType
		{
			public TracerVertex(Vector3 position, Vector4 color)
			{
				this.Position = position;
				this.Color = color;
			}

			VertexDeclaration IVertexType.VertexDeclaration
			{
				get
				{
					return TracerManager.TracerVertex.VertexDeclaration;
				}
			}

			public Vector3 Position;

			public Vector4 Color;

			public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[]
			{
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
			});
		}

		public class TracerProbe : TraceProbe
		{
			public override bool TestThisType(BlockTypeEnum e)
			{
				BlockType type = BlockType.GetType(e);
				return type.CanBeTouched && type.BlockPlayer;
			}
		}

		public class Tracer
		{
			public void Init(Vector3 pos, Vector3 dir, InventoryItemIDs item, byte shooterID)
			{
				this.ItemClass = (GunInventoryItemClass)InventoryItem.GetClass(item);
				this.Head = (this.Tail = pos + dir * 0.5f);
				float velocity = this.ItemClass.Velocity;
				this.TracerColor = this.ItemClass.TracerColor;
				this.HeadVelocity = (this.TailVelocity = dir * velocity);
				this.itemID = item;
				this.TailTime = this.ItemClass.FlightTime - 0.2f;
				this.TimeLeft = this.ItemClass.FlightTime;
				this.RemoveAfterDraw = false;
				this.Target = null;
				this.PlayedSound = false;
				this.ShooterID = shooterID;
			}

			public void Init(Vector3 pos, Vector3 vel, Player target, Color color)
			{
				Vector3 vector;
				if (vel.LengthSquared() < 0.001f)
				{
					vector = Vector3.Normalize(vel);
				}
				else
				{
					vector = Vector3.Up;
				}
				this.Head = (this.Tail = pos + vector * 0.5f);
				this.TracerColor = color.ToVector4();
				this.HeadVelocity = (this.TailVelocity = vel);
				this.itemID = InventoryItemIDs.Coal;
				this.TailTime = 1.8f;
				this.TimeLeft = 2f;
				this.RemoveAfterDraw = false;
				this.Target = target;
				this.ItemClass = null;
				this.PlayedSound = false;
				this.ShooterID = byte.MaxValue;
			}

			public void Update(float dt)
			{
				this.TimeLeft -= dt;
				if (this.TimeLeft < 0f)
				{
					this.RemoveAfterDraw = true;
				}
				this.Head += this.HeadVelocity * dt;
				this.HeadVelocity.Y = this.HeadVelocity.Y - 10f * dt;
				if (this.TimeLeft < this.TailTime - 0.2f)
				{
					this.Tail += this.TailVelocity * dt;
					this.TailVelocity.Y = this.TailVelocity.Y - 10f * dt;
				}
				TracerManager.Tracer.tp.Init(this.Tail, this.Head);
				if (this.Target != null)
				{
					bool flag = false;
					bool flag2 = false;
					float num = 2f;
					Vector3 vector = Vector3.Zero;
					if (this.Target.ValidLivingGamer)
					{
						Vector3 worldPosition = this.Target.WorldPosition;
						BoundingBox playerAABB = this.Target.PlayerAABB;
						playerAABB.Min += worldPosition;
						playerAABB.Max += worldPosition;
						TracerManager.Tracer.tp.TestBoundBox(playerAABB);
					}
					if (TracerManager.Tracer.tp._collides)
					{
						vector = TracerManager.Tracer.tp.GetIntersection();
						flag = true;
						flag2 = true;
						num = TracerManager.Tracer.tp._inT;
					}
					TracerManager.Tracer.tp.Reset();
					BlockTerrain.Instance.Trace(TracerManager.Tracer.tp);
					if (TracerManager.Tracer.tp._collides && TracerManager.Tracer.tp._inT < num)
					{
						flag = false;
						flag2 = true;
						vector = TracerManager.Tracer.tp.GetIntersection();
					}
					if (flag2)
					{
						this.Head = vector;
						this.RemoveAfterDraw = true;
						if (flag)
						{
							if (this.Target.IsLocal)
							{
								InGameHUD.Instance.ApplyDamage(0.35f, this.Head);
							}
							SoundManager.Instance.PlayInstance("BulletHitHuman", this.Target.SoundEmitter);
						}
						else
						{
							SoundManager.Instance.PlayInstance("BulletHitDirt", this.SoundEmitter);
						}
						ParticleEmitter particleEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
						particleEmitter.Reset();
						particleEmitter.Emitting = true;
						TracerManager.Instance.Scene.Children.Add(particleEmitter);
						particleEmitter.LocalPosition = vector;
						particleEmitter.DrawPriority = 900;
					}
					else if (!this.PlayedSound)
					{
						float num2 = Vector3.DistanceSquared(this.Head, this.Target.WorldPosition);
						if (num2 < 25f)
						{
							this.PlayedSound = true;
							SoundManager.Instance.PlayInstance("arrow", this.SoundEmitter);
						}
					}
				}
				else
				{
					if (CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.PVPState != CastleMinerZGame.PVPEnum.Off)
					{
						bool flag3 = false;
						bool flag4 = false;
						float num3 = 2.1474836E+09f;
						Vector3 vector2 = Vector3.Zero;
						Player player = null;
						for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
						{
							NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
							if (networkGamer.Tag != null)
							{
								Player player2 = (Player)networkGamer.Tag;
								if (player2.ValidLivingGamer)
								{
									Vector3 worldPosition2 = player2.WorldPosition;
									BoundingBox playerAABB2 = player2.PlayerAABB;
									playerAABB2.Min += worldPosition2;
									playerAABB2.Max += worldPosition2;
									TracerManager.Tracer.tp.Reset();
									TracerManager.Tracer.tp.TestBoundBox(playerAABB2);
									if (TracerManager.Tracer.tp._collides && TracerManager.Tracer.tp._inT < num3)
									{
										player = player2;
										vector2 = TracerManager.Tracer.tp.GetIntersection();
										flag3 = true;
										flag4 = true;
										num3 = TracerManager.Tracer.tp._inT;
									}
								}
							}
						}
						TracerManager.Tracer.tp.Reset();
						BlockTerrain.Instance.Trace(TracerManager.Tracer.tp);
						if (TracerManager.Tracer.tp._collides && TracerManager.Tracer.tp._inT < num3)
						{
							flag3 = false;
							flag4 = true;
							vector2 = TracerManager.Tracer.tp.GetIntersection();
						}
						if (flag4)
						{
							this.Head = vector2;
							this.RemoveAfterDraw = true;
							if (flag3)
							{
								if (player.IsLocal && this.ShooterID != player.Gamer.Id)
								{
									LocalNetworkGamer localNetworkGamer = (LocalNetworkGamer)player.Gamer;
									if (CastleMinerZGame.Instance.PVPState == CastleMinerZGame.PVPEnum.Everyone || (!localNetworkGamer.IsHost && !localNetworkGamer.SignedInGamer.IsFriend(CastleMinerZGame.Instance.CurrentNetworkSession.Host)))
									{
										InGameHUD.Instance.ApplyDamage(0.21f, this.Head);
									}
								}
								SoundManager.Instance.PlayInstance("BulletHitHuman", player.SoundEmitter);
							}
						}
					}
					IShootableEnemy shootableEnemy = EnemyManager.Instance.Trace(TracerManager.Tracer.tp, false);
					if (TracerManager.Tracer.tp._collides)
					{
						this.Head = TracerManager.Tracer.tp.GetIntersection();
						this.RemoveAfterDraw = true;
						if (shootableEnemy != null)
						{
							shootableEnemy.TakeDamage(this.Head, Vector3.Normalize(this.HeadVelocity), this.ItemClass, this.ShooterID);
							if (shootableEnemy is BaseZombie)
							{
								SoundManager.Instance.PlayInstance("BulletHitHuman", ((BaseZombie)shootableEnemy).SoundEmitter);
							}
						}
						else
						{
							SoundManager.Instance.PlayInstance("BulletHitDirt", this.SoundEmitter);
						}
						Vector3 intersection = TracerManager.Tracer.tp.GetIntersection();
						if (shootableEnemy is DragonClientEntity)
						{
							ParticleEmitter particleEmitter2 = TracerManager._dragonFlashEffect.CreateEmitter(CastleMinerZGame.Instance);
							particleEmitter2.Reset();
							particleEmitter2.Emitting = true;
							TracerManager.Instance.Scene.Children.Add(particleEmitter2);
							particleEmitter2.LocalPosition = intersection;
							particleEmitter2.DrawPriority = 900;
						}
						ParticleEmitter particleEmitter3 = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
						particleEmitter3.Reset();
						particleEmitter3.Emitting = true;
						TracerManager.Instance.Scene.Children.Add(particleEmitter3);
						particleEmitter3.LocalPosition = intersection;
						particleEmitter3.DrawPriority = 900;
						BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(TracerManager.Tracer.tp._worldIndex);
						if (blockWithChanges == BlockTypeEnum.TNT || blockWithChanges == BlockTypeEnum.C4)
						{
							DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, TracerManager.Tracer.tp._worldIndex, true, (blockWithChanges == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
						}
						if (shootableEnemy == null)
						{
							ParticleEmitter particleEmitter4 = TracerManager._sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
							particleEmitter4.Reset();
							particleEmitter4.Emitting = true;
							TracerManager.Instance.Scene.Children.Add(particleEmitter4);
							particleEmitter4.LocalPosition = intersection;
							particleEmitter4.DrawPriority = 900;
						}
					}
				}
				Vector3 vector3 = Vector3.Normalize(this.HeadVelocity);
				Vector3 vector4;
				if (vector3.Y == 1f)
				{
					vector4 = Vector3.UnitX;
				}
				else
				{
					vector4 = Vector3.UnitY;
				}
				Vector3 vector5 = Vector3.Cross(vector4, vector3);
				vector4 = Vector3.Cross(vector3, vector5);
				this.SoundEmitter.Position = this.Head;
				this.SoundEmitter.Forward = vector3;
				this.SoundEmitter.Up = Vector3.Normalize(vector4);
				this.SoundEmitter.Velocity = this.HeadVelocity;
				if (!TracerManager.Tracer.tp._collides && !BlockTerrain.Instance.IsTracerStillInWorld(this.Head))
				{
					this.RemoveAfterDraw = true;
				}
			}

			public int AddToDrawCache(TracerManager.TracerVertex[] vertexCache, int index)
			{
				Vector3 vector = this.Tail - this.Head;
				if (vector.LengthSquared() < 0.01f)
				{
					return index;
				}
				vector.Normalize();
				int num = index * 5;
				vertexCache[num].Position = this.Head;
				vertexCache[num++].Color = this.TracerColor;
				Vector3 vector2;
				if (vector.Y == 1f)
				{
					vector2 = Vector3.UnitX;
				}
				else
				{
					vector2 = Vector3.UnitY;
				}
				Vector3 vector3 = Vector3.Cross(vector2, vector);
				vector3.Normalize();
				vector2 = Vector3.Cross(vector, vector3);
				Vector3 vector4 = Vector3.Lerp(this.Head, this.Tail, 0.1f);
				vertexCache[num].Position = vector4 + vector2 * 0.025f;
				vertexCache[num++].Color = this.TracerColor;
				vertexCache[num].Position = vector4 - vector2 * 0.025f * 0.5f - vector3 * 0.025f * 0.866f;
				vertexCache[num++].Color = this.TracerColor;
				vertexCache[num].Position = vector4 - vector2 * 0.025f * 0.5f + vector3 * 0.025f * 0.866f;
				vertexCache[num++].Color = this.TracerColor;
				vertexCache[num].Position = this.Tail;
				vertexCache[num].Color = TracerManager.Tracer.TailColor;
				return ++index;
			}

			private static TracerManager.TracerProbe tp = new TracerManager.TracerProbe();

			private static Vector4 TailColor = new Vector4(1f, 1f, 1f, 0f);

			private Vector3 Head;

			private Vector3 Tail;

			private Vector3 HeadVelocity;

			private Vector3 TailVelocity;

			private Vector4 TracerColor;

			private Player Target;

			private GunInventoryItemClass ItemClass;

			private AudioEmitter SoundEmitter = new AudioEmitter();

			private float TimeLeft;

			private float TailTime;

			private byte ShooterID;

			private bool PlayedSound;

			public bool RemoveAfterDraw;

			public InventoryItemIDs itemID;
		}
	}
}
