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
			short[] ibbatch = new short[]
			{
				1, 0, 2, 3, 0, 1, 2, 0, 3, 1,
				4, 3, 2, 4, 1, 3, 4, 2
			};
			short[] ibs = new short[360];
			short baseVertex = 0;
			int idx = 0;
			int i = 0;
			while (i < 20)
			{
				for (int j = 0; j < ibbatch.Length; j++)
				{
					ibs[idx++] = ibbatch[j] + baseVertex;
				}
				i++;
				baseVertex += 5;
			}
			bool created = false;
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDevice())
				{
					try
					{
						TracerManager._vb1 = new DynamicVertexBuffer(CastleMinerZGame.Instance.GraphicsDevice, typeof(TracerManager.TracerVertex), 100, BufferUsage.WriteOnly);
						TracerManager._vb2 = new DynamicVertexBuffer(CastleMinerZGame.Instance.GraphicsDevice, typeof(TracerManager.TracerVertex), 100, BufferUsage.WriteOnly);
						TracerManager._ib = new IndexBuffer(CastleMinerZGame.Instance.GraphicsDevice, IndexElementSize.SixteenBits, 360, BufferUsage.WriteOnly);
						TracerManager._ib.SetData<short>(ibs);
					}
					finally
					{
						GraphicsDeviceLocker.Instance.UnlockDevice();
					}
					created = true;
				}
				if (!created)
				{
					Thread.Sleep(10);
				}
			}
			while (!created);
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
			int c = this._unusedTracers.Count;
			TracerManager.Tracer t;
			if (c != 0)
			{
				c--;
				t = this._unusedTracers[c];
				this._unusedTracers.RemoveAt(c);
			}
			else
			{
				this._unusedTracers.Add(new TracerManager.Tracer());
				t = new TracerManager.Tracer();
			}
			return t;
		}

		public void AddTracer(Vector3 position, Vector3 velocity, InventoryItemIDs item, byte shooterID)
		{
			TracerManager.Tracer t = this.GetTracer();
			t.Init(position, velocity, item, shooterID);
			this._tracers.Add(t);
			EnemyManager.Instance.RegisterGunShot(position);
		}

		public void AddArrow(Vector3 position, Vector3 velocity, Player target)
		{
			TracerManager.Tracer t = this.GetTracer();
			t.Init(position, velocity, target, Color.Pink);
			this._tracers.Add(t);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			int tcount = this._tracers.Count;
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			for (int i = 0; i < tcount; i++)
			{
				this._tracers[i].Update(dt);
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
			int tcount = this._tracers.Count;
			double totalSeconds = gameTime.ElapsedGameTime.TotalSeconds;
			int numInCache = 0;
			bool firstDrawCall = true;
			BlendState oldState = device.BlendState;
			RasterizerState oldRs = device.RasterizerState;
			DepthStencilState depthStencilState = device.DepthStencilState;
			int i = 0;
			while (i < tcount)
			{
				numInCache = this._tracers[i].AddToDrawCache(this._vertexCache, numInCache);
				if (numInCache == 20)
				{
					this.FlushTracers(this._vertexCache, numInCache, firstDrawCall, device, view, projection);
					firstDrawCall = false;
					numInCache = 0;
				}
				if (this._tracers[i].RemoveAfterDraw)
				{
					this._unusedTracers.Add(this._tracers[i]);
					tcount--;
					if (tcount != i)
					{
						this._tracers[i] = this._tracers[tcount];
					}
					this._tracers.RemoveAt(tcount);
				}
				else
				{
					i++;
				}
			}
			if (numInCache != 0)
			{
				this.FlushTracers(this._vertexCache, numInCache, firstDrawCall, device, view, projection);
				firstDrawCall = false;
			}
			if (!firstDrawCall)
			{
				device.RasterizerState = oldRs;
				device.BlendState = oldState;
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
				BlockType bt = BlockType.GetType(e);
				return bt.CanBeTouched && bt.BlockPlayer;
			}
		}

		public class Tracer
		{
			public void Init(Vector3 pos, Vector3 dir, InventoryItemIDs item, byte shooterID)
			{
				this.ItemClass = (GunInventoryItemClass)InventoryItem.GetClass(item);
				this.Head = (this.Tail = pos + dir * 0.5f);
				float vel = this.ItemClass.Velocity;
				this.TracerColor = this.ItemClass.TracerColor;
				this.HeadVelocity = (this.TailVelocity = dir * vel);
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
				Vector3 dir;
				if (vel.LengthSquared() < 0.001f)
				{
					dir = Vector3.Normalize(vel);
				}
				else
				{
					dir = Vector3.Up;
				}
				this.Head = (this.Tail = pos + dir * 0.5f);
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
				if (this.RemoveAfterDraw)
				{
					return;
				}
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
					bool hitTarget = false;
					bool collision = false;
					float targetT = 2f;
					Vector3 hitLocation = Vector3.Zero;
					if (this.Target.ValidLivingGamer)
					{
						Vector3 p = this.Target.WorldPosition;
						BoundingBox bb = this.Target.PlayerAABB;
						bb.Min += p;
						bb.Max += p;
						TracerManager.Tracer.tp.TestBoundBox(bb);
					}
					if (TracerManager.Tracer.tp._collides)
					{
						hitLocation = TracerManager.Tracer.tp.GetIntersection();
						hitTarget = true;
						collision = true;
						targetT = TracerManager.Tracer.tp._inT;
					}
					TracerManager.Tracer.tp.Reset();
					BlockTerrain.Instance.Trace(TracerManager.Tracer.tp);
					if (TracerManager.Tracer.tp._collides && TracerManager.Tracer.tp._inT < targetT)
					{
						hitTarget = false;
						collision = true;
						hitLocation = TracerManager.Tracer.tp.GetIntersection();
					}
					if (collision)
					{
						this.Head = hitLocation;
						this.RemoveAfterDraw = true;
						if (hitTarget)
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
						if (CastleMinerZGame.Instance.IsActive)
						{
							ParticleEmitter smokeEmitter = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
							smokeEmitter.Reset();
							smokeEmitter.Emitting = true;
							TracerManager.Instance.Scene.Children.Add(smokeEmitter);
							smokeEmitter.LocalPosition = hitLocation;
							smokeEmitter.DrawPriority = 900;
						}
					}
					else if (!this.PlayedSound)
					{
						float dstsq = Vector3.DistanceSquared(this.Head, this.Target.WorldPosition);
						if (dstsq < 25f)
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
						bool hitTarget2 = false;
						bool collision2 = false;
						float targetT2 = 2.1474836E+09f;
						Vector3 hitLocation2 = Vector3.Zero;
						Player playerHit = null;
						for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers.Count; i++)
						{
							NetworkGamer gamer = CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers[i];
							if (gamer.Tag != null)
							{
								Player player = (Player)gamer.Tag;
								if (player.ValidLivingGamer)
								{
									Vector3 p2 = player.WorldPosition;
									BoundingBox bb2 = player.PlayerAABB;
									bb2.Min += p2;
									bb2.Max += p2;
									TracerManager.Tracer.tp.Reset();
									TracerManager.Tracer.tp.TestBoundBox(bb2);
									if (TracerManager.Tracer.tp._collides && TracerManager.Tracer.tp._inT < targetT2)
									{
										playerHit = player;
										hitLocation2 = TracerManager.Tracer.tp.GetIntersection();
										hitTarget2 = true;
										collision2 = true;
										targetT2 = TracerManager.Tracer.tp._inT;
									}
								}
							}
						}
						TracerManager.Tracer.tp.Reset();
						BlockTerrain.Instance.Trace(TracerManager.Tracer.tp);
						if (TracerManager.Tracer.tp._collides && TracerManager.Tracer.tp._inT < targetT2)
						{
							hitTarget2 = false;
							collision2 = true;
							hitLocation2 = TracerManager.Tracer.tp.GetIntersection();
						}
						if (collision2)
						{
							this.Head = hitLocation2;
							this.RemoveAfterDraw = true;
							if (hitTarget2)
							{
								if (playerHit.IsLocal && this.ShooterID != playerHit.Gamer.Id)
								{
									LocalNetworkGamer localgamer = (LocalNetworkGamer)playerHit.Gamer;
									if (CastleMinerZGame.Instance.PVPState == CastleMinerZGame.PVPEnum.Everyone || (!localgamer.IsHost && !localgamer.SignedInGamer.IsFriend(CastleMinerZGame.Instance.CurrentNetworkSession.Host)))
									{
										InGameHUD.Instance.ApplyDamage(0.21f, this.Head);
									}
								}
								SoundManager.Instance.PlayInstance("BulletHitHuman", playerHit.SoundEmitter);
							}
						}
					}
					IShootableEnemy z = EnemyManager.Instance.Trace(TracerManager.Tracer.tp, false);
					if (TracerManager.Tracer.tp._collides)
					{
						this.Head = TracerManager.Tracer.tp.GetIntersection();
						this.RemoveAfterDraw = true;
						if (z != null)
						{
							z.TakeDamage(this.Head, Vector3.Normalize(this.HeadVelocity), this.ItemClass, this.ShooterID);
							if (z is BaseZombie)
							{
								SoundManager.Instance.PlayInstance("BulletHitHuman", ((BaseZombie)z).SoundEmitter);
							}
						}
						else
						{
							SoundManager.Instance.PlayInstance("BulletHitDirt", this.SoundEmitter);
						}
						Vector3 hitPosition = TracerManager.Tracer.tp.GetIntersection();
						if (CastleMinerZGame.Instance.IsActive)
						{
							if (z is DragonClientEntity)
							{
								ParticleEmitter flashEmitter = TracerManager._dragonFlashEffect.CreateEmitter(CastleMinerZGame.Instance);
								flashEmitter.Reset();
								flashEmitter.Emitting = true;
								TracerManager.Instance.Scene.Children.Add(flashEmitter);
								flashEmitter.LocalPosition = hitPosition;
								flashEmitter.DrawPriority = 900;
							}
							ParticleEmitter smokeEmitter2 = TracerManager._smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
							smokeEmitter2.Reset();
							smokeEmitter2.Emitting = true;
							TracerManager.Instance.Scene.Children.Add(smokeEmitter2);
							smokeEmitter2.LocalPosition = hitPosition;
							smokeEmitter2.DrawPriority = 900;
							if (z == null)
							{
								ParticleEmitter sparkEmitter = TracerManager._sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
								sparkEmitter.Reset();
								sparkEmitter.Emitting = true;
								TracerManager.Instance.Scene.Children.Add(sparkEmitter);
								sparkEmitter.LocalPosition = hitPosition;
								sparkEmitter.DrawPriority = 900;
							}
						}
						BlockTypeEnum blockType = BlockTerrain.Instance.GetBlockWithChanges(TracerManager.Tracer.tp._worldIndex);
						if (blockType == BlockTypeEnum.TNT || blockType == BlockTypeEnum.C4)
						{
							DetonateExplosiveMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, TracerManager.Tracer.tp._worldIndex, true, (blockType == BlockTypeEnum.TNT) ? ExplosiveTypes.TNT : ExplosiveTypes.C4);
						}
					}
				}
				Vector3 nml = Vector3.Normalize(this.HeadVelocity);
				Vector3 up;
				if (nml.Y == 1f)
				{
					up = Vector3.UnitX;
				}
				else
				{
					up = Vector3.UnitY;
				}
				Vector3 over = Vector3.Cross(up, nml);
				up = Vector3.Cross(nml, over);
				this.SoundEmitter.Position = this.Head;
				this.SoundEmitter.Forward = nml;
				this.SoundEmitter.Up = Vector3.Normalize(up);
				this.SoundEmitter.Velocity = this.HeadVelocity;
				if (!TracerManager.Tracer.tp._collides && !BlockTerrain.Instance.IsTracerStillInWorld(this.Head))
				{
					this.RemoveAfterDraw = true;
				}
			}

			public int AddToDrawCache(TracerManager.TracerVertex[] vertexCache, int index)
			{
				Vector3 nml = this.Tail - this.Head;
				if (nml.LengthSquared() < 0.01f)
				{
					return index;
				}
				nml.Normalize();
				int idx = index * 5;
				vertexCache[idx].Position = this.Head;
				vertexCache[idx++].Color = this.TracerColor;
				Vector3 up;
				if (nml.Y == 1f)
				{
					up = Vector3.UnitX;
				}
				else
				{
					up = Vector3.UnitY;
				}
				Vector3 over = Vector3.Cross(up, nml);
				over.Normalize();
				up = Vector3.Cross(nml, over);
				Vector3 ptmid = Vector3.Lerp(this.Head, this.Tail, 0.1f);
				vertexCache[idx].Position = ptmid + up * 0.025f;
				vertexCache[idx++].Color = this.TracerColor;
				vertexCache[idx].Position = ptmid - up * 0.025f * 0.5f - over * 0.025f * 0.866f;
				vertexCache[idx++].Color = this.TracerColor;
				vertexCache[idx].Position = ptmid - up * 0.025f * 0.5f + over * 0.025f * 0.866f;
				vertexCache[idx++].Color = this.TracerColor;
				vertexCache[idx].Position = this.Tail;
				vertexCache[idx].Color = TracerManager.Tracer.TailColor;
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
