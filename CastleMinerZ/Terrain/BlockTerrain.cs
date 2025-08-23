using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.CastleMinerZ.Terrain.WorldBuilders;
using DNA.CastleMinerZ.Utils;
using DNA.CastleMinerZ.Utils.Threading;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DNA.CastleMinerZ.Terrain
{
	public class BlockTerrain : Entity
	{
		public static BlockTerrain Instance
		{
			get
			{
				return BlockTerrain._theTerrain;
			}
		}

		public BlockTerrain(GraphicsDevice gd, ContentManager cm)
		{
			BlockTerrain._theTerrain = this;
			this._graphicsDevice = gd;
			this.IsWaterWorld = false;
			this._effect = cm.Load<Effect>("Shaders\\BlockEffect");
			Texture[] textureSet = null;
			if (CastleMinerZArgs.Instance.TextureFolder != null)
			{
				textureSet = ((ProfiledContentManager)cm).LoadTerrain();
			}
			if (textureSet == null)
			{
				textureSet = cm.Load<Texture[]>("Terrain\\Textures");
			}
			this._diffuseAlpha = (Texture2D)textureSet[0];
			this._normalSpec = (Texture2D)textureSet[1];
			this._metalLight = (Texture2D)textureSet[2];
			this._mipMapNormals = (Texture2D)textureSet[3];
			this._mipMapDiffuse = (Texture2D)textureSet[4];
			if (GraphicsProfileManager.Instance.IsHiDef)
			{
				Texture2D emap = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap0");
				Byte4[] tdata = new Byte4[emap.Bounds.Width * emap.Bounds.Height * 4];
				emap.GetData<Byte4>(tdata, 0, emap.Bounds.Width * emap.Bounds.Height);
				emap = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap1");
				emap.GetData<Byte4>(tdata, emap.Bounds.Width * emap.Bounds.Height, emap.Bounds.Width * emap.Bounds.Height);
				emap = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap2");
				emap.GetData<Byte4>(tdata, emap.Bounds.Width * emap.Bounds.Height * 2, emap.Bounds.Width * emap.Bounds.Height);
				emap = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap3");
				emap.GetData<Byte4>(tdata, emap.Bounds.Width * emap.Bounds.Height * 3, emap.Bounds.Width * emap.Bounds.Height);
				this._envMap = new Texture3D(gd, emap.Bounds.Width, emap.Bounds.Height, 4, false, emap.Format);
				this._envMap.SetData<Byte4>(tdata);
			}
			else
			{
				this._envMap = null;
			}
			Vector2 halfPixel = new Vector2(0.5f / (float)this._diffuseAlpha.Width, 0.5f / (float)this._diffuseAlpha.Height);
			Vector2 omHalfPixel = new Vector2(0.125f - halfPixel.X, 0.125f - halfPixel.Y);
			Vector2 omHalfPixelAO = new Vector2(0.0625f - halfPixel.X, 0.0625f - halfPixel.Y);
			this._vertexUVs = new Vector4[]
			{
				new Vector4(halfPixel.X, halfPixel.Y, halfPixel.X, halfPixel.Y),
				new Vector4(omHalfPixel.X, halfPixel.Y, omHalfPixelAO.X, halfPixel.Y),
				new Vector4(halfPixel.X, omHalfPixel.Y, halfPixel.X, omHalfPixelAO.Y),
				new Vector4(omHalfPixel.X, omHalfPixel.Y, omHalfPixelAO.X, omHalfPixelAO.Y)
			};
			this._effect.Parameters["VertexUVs"].SetValue(this._vertexUVs);
			this._faceMatrices = new Matrix[]
			{
				new Matrix(0f, 0f, -1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f),
				new Matrix(0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, -1f, 0f, 0f, 0f, 0f, 0f, 0f, 1f),
				new Matrix(-1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, -1f, 0f, 0f, 0f, 0f, 1f),
				new Matrix(-1f, 0f, 0f, 0f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, 0f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, -1f, 0f, 0f, 0f, 0f, 0f, 1f)
			};
			this._effect.Parameters["FaceMatrices"].SetValue(this._faceMatrices);
			this._effect.Parameters["DiffuseAlphaTexture"].SetValue(this._diffuseAlpha);
			this._effect.Parameters["NormalSpecTexture"].SetValue(this._normalSpec);
			this._effect.Parameters["MetalLightTexture"].SetValue(this._metalLight);
			this._effect.Parameters["EnvMapTexture"].SetValue(this._envMap);
			this._effect.Parameters["MipMapSpecularTexture"].SetValue(this._mipMapNormals);
			this._effect.Parameters["MipMapDiffuseTexture"].SetValue(this._mipMapDiffuse);
			this._disableColorWrites = new BlendState();
			this._disableColorWrites.ColorWriteChannels = ColorWriteChannels.None;
			this._enableColorWrites = new BlendState();
			this._enableColorWrites.ColorWriteChannels = ColorWriteChannels.All;
			this._zWriteDisable = new DepthStencilState();
			this._zWriteDisable.DepthBufferWriteEnable = false;
			this._zWriteEnable = new DepthStencilState();
			this._zWriteEnable.DepthBufferWriteEnable = true;
			this._boundingFrustum = new BoundingFrustum(Matrix.Identity);
			this._blocks = new int[18874368];
			this._shiftedBlocks = new int[18874368];
			for (int i = 0; i < 18874368; i++)
			{
				this._blocks[i] = this.initblock;
				this._shiftedBlocks[i] = this.initblock;
			}
			this.SetWorldMin(new IntVector3(-192, -64, -192));
			this._renderIndexList = new int[576];
			this._chunks = new BlockTerrain.TerrainChunk[576];
			for (int j = 0; j < 576; j++)
			{
				this._chunks[j] = default(BlockTerrain.TerrainChunk);
				this._chunks[j].Init();
			}
			this._computeGeometryDelegate = new TaskDelegate(this.DoThreadedComputeGeometry);
			this._computeBlocksDelegate = new TaskDelegate(this.DoThreadedComputeBlocks);
			this._stepUpdateDelegate = new TaskDelegate(this.DoThreadedStepUpdateChunks);
			this._computeLightingDelegate = new TaskDelegate(this.DoThreadedComputeLighting);
			this._shiftTerrainDelegate = new TaskDelegate(this.DoThreadedShiftTerrain);
			this._finishSetBlockDelegate = new TaskDelegate(this.DoThreadedFinishSetBlock);
			this._finishRegionOpDelegate = new TaskDelegate(this.DoFinishThreadedRegionOperation);
			this._shiftTerrainData = new BlockTerrain.ShiftingTerrainData();
			this._buildTasksRemaining = default(CountdownLatch);
			this._updateTasksRemaining = default(CountdownLatch);
			this._currentEyeChunkIndex = -1;
			this._currentRenderOrder = 0;
			int[] offs = new int[] { 0, 1, 2, 2, 1, 3 };
			ushort[] indexes = new ushort[98304];
			int baseIndex = 0;
			int count = 0;
			for (int k = 0; k < 16384; k++)
			{
				for (int l = 0; l < 6; l++)
				{
					indexes[count++] = (ushort)(baseIndex + offs[l]);
					if (((long)(baseIndex + offs[l]) & (long)((ulong)(-65536))) != 0L)
					{
						break;
					}
				}
				baseIndex += 4;
			}
			bool created = false;
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDevice())
				{
					try
					{
						this._staticIB = new IndexBuffer(gd, IndexElementSize.SixteenBits, 98304, BufferUsage.WriteOnly);
						this._staticIB.SetData<ushort>(indexes);
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
			this._mainLightingPool = new BlockTerrain.LightingPool(262144);
			this._updateLightingPool = new BlockTerrain.LightingPool(7500);
			this._computeBlocksPool = new BlockTerrain.LoadChunkActionPool(BlockTerrain.NextChunkAction.NEEDS_BLOCKS, BlockTerrain.NextChunkAction.COMPUTING_BLOCKS, this._computeBlocksDelegate);
			this._computeLightingPool = new BlockTerrain.ChunkActionPool(BlockTerrain.NextChunkAction.NEEDS_LIGHTING, BlockTerrain.NextChunkAction.COMPUTING_LIGHTING, this._computeLightingDelegate);
			this._computeGeometryPool = new BlockTerrain.ChunkActionPool(BlockTerrain.NextChunkAction.NEEDS_GEOMETRY, BlockTerrain.NextChunkAction.COMPUTING_GEOMETRY, this._computeGeometryDelegate);
			this._allChunksLoaded = false;
			this.Collidee = true;
			this.BuildRadiusOrderOffsets();
			GC.Collect();
		}

		private void BuildRadiusOrderOffsets()
		{
			this._radiusOrderOffsets = new IntVector3[676];
			this._radiusOrderOffsets[0] = IntVector3.Zero;
			int limit = 13;
			int walkIndex = 1;
			for (int i = 1; i <= limit; i++)
			{
				for (int j = 0; j <= i; j++)
				{
					int z = -i;
					if (z >= -limit)
					{
						int x = j;
						if (x < limit)
						{
							this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
						}
						if (j != 0)
						{
							x = -j;
							if (x >= -limit)
							{
								this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
							}
						}
					}
					z = i;
					if (z < limit)
					{
						int x = j;
						if (x < limit)
						{
							this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
						}
						if (j != 0)
						{
							x = -j;
							if (x >= -limit)
							{
								this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
							}
						}
					}
					if (j != i)
					{
						int x = i;
						if (x < limit)
						{
							z = j;
							if (z < limit)
							{
								this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
							}
							if (j != 0)
							{
								z = -j;
								if (z >= -limit)
								{
									this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
								}
							}
						}
						x = -i;
						if (x >= -limit)
						{
							z = j;
							if (z < limit)
							{
								this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
							}
							if (j != 0)
							{
								z = -j;
								if (z >= -limit)
								{
									this._radiusOrderOffsets[walkIndex++].SetValues(x, 0, z);
								}
							}
						}
					}
				}
			}
		}

		public void Init(Vector3 center, WorldInfo worldInfo, bool host)
		{
			this.WorldInfo = worldInfo;
			if (this.IsReady)
			{
				if (!this._resetRequested)
				{
					this.Reset();
				}
				while (!this.ReadyForInit)
				{
				}
			}
			this._initted = true;
			this._resetRequested = false;
			ChunkCache.Instance.Start(true);
			ChunkCache.Instance.MakeHost(worldInfo, host);
			this._worldBuilder = this.WorldInfo.GetBuilder();
			this._loadingProgress = 0;
			this._allChunksLoaded = false;
			this.IsWaterWorld = false;
			IntVector3 result = new IntVector3((int)Math.Floor((double)(center.X / 1f)), (int)Math.Floor((double)(center.Y / 1f)), (int)Math.Floor((double)(center.Z / 1f)));
			result.X -= 192;
			result.Y = -64;
			result.Z -= 192;
			this.SetWorldMin(result);
			this._maxChunksAtOnce = 64;
			this.CenterOn(center, false);
		}

		private void InternalTeleport(Vector3 center)
		{
			this._initted = true;
			this._resetRequested = false;
			this._loadingProgress = 0;
			this._allChunksLoaded = false;
			IntVector3 result = new IntVector3((int)Math.Floor((double)(center.X / 1f)), (int)Math.Floor((double)(center.Y / 1f)), (int)Math.Floor((double)(center.Z / 1f)));
			result.X -= 192;
			result.Y = -64;
			result.Z -= 192;
			this.SetWorldMin(result);
			this._maxChunksAtOnce = 64;
			this.CenterOn(center, false);
		}

		public void DoThreadedReset(object context)
		{
			ChunkCache.Instance.Stop(true);
			MainThreadMessageSender.Instance.GameOver();
			this.DoThreadedCleanup(context);
			BlockTerrain.ItemBlockCommand head = this.ItemBlockCommandQueue.Clear();
			BlockTerrain.ItemBlockCommand.ReleaseList(head);
		}

		public void DoThreadedCleanup(object context)
		{
			Stopwatch timeout = Stopwatch.StartNew();
			while (this._buildTasksRemaining || this._updateTasksRemaining)
			{
				if (timeout.ElapsedMilliseconds > 120000L && TaskDispatcher.Instance.IsIdle(TaskThreadEnum.NUM_THREADS))
				{
					if (this._asyncInitData != null && this._asyncInitData.teleporting)
					{
						throw new Exception("Cleanup timed out waiting for completion during teleport");
					}
					throw new Exception("Cleanup timed out waiting for completion outside of teleport");
				}
				else
				{
					Thread.Sleep(500);
				}
			}
			this.ClearVertexBuildLists();
			this._loadingProgress = 0;
			this._buildTasksRemaining.Value = 0;
			this._updateTasksRemaining.Value = 0;
			this._shiftTerrainData.running = false;
			this._shiftTerrainData.done = false;
			for (int i = 0; i < 576; i++)
			{
				this._chunks[i].Reset();
			}
			this._computeBlocksPool.Clear();
			this._computeLightingPool.Clear();
			this._computeGeometryPool.Clear();
			this._mainLightingPool.Clear();
			this._updateLightingPool.Clear();
			for (int j = 0; j < this._blocks.Length; j++)
			{
				this._blocks[j] = this.initblock;
			}
			this._initted = false;
		}

		public void BlockingReset()
		{
			this.Reset();
			while (!this.ReadyForInit)
			{
			}
		}

		public void Reset()
		{
			this.ClearVertexBuildLists();
			if (this.IsReady)
			{
				this._resetRequested = true;
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoThreadedReset));
			}
		}

		public void Teleport(Vector3 newPosition)
		{
			this._asyncInitData = new BlockTerrain.AsynchInitData
			{
				center = newPosition,
				worldInfo = this.WorldInfo,
				teleporting = true
			};
			if (this.IsReady)
			{
				this._resetRequested = true;
				ChunkCache.Instance.ResetWaitingChunks();
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoThreadedCleanup));
			}
		}

		public void AsyncInit(WorldInfo worldInfo, bool host, AsyncCallback callback)
		{
			this._asyncInitData = new BlockTerrain.AsynchInitData
			{
				center = worldInfo.LastPosition,
				worldInfo = worldInfo,
				host = host,
				callback = callback,
				teleporting = false
			};
			this.Reset();
		}

		public bool ReadyForInit
		{
			get
			{
				return !this._initted;
			}
		}

		public bool MinimallyLoaded
		{
			get
			{
				return this.IsReady && this._loadingProgress >= 183;
			}
		}

		public int LoadingProgress
		{
			get
			{
				if (!this.IsReady)
				{
					return 0;
				}
				return Math.Min(this._loadingProgress * 134 / 183, 100);
			}
		}

		public bool Calculating
		{
			get
			{
				return this._buildTasksRemaining || this._updateTasksRemaining;
			}
		}

		public bool IsReady
		{
			get
			{
				return this._initted && !this._resetRequested;
			}
		}

		protected void AddToLoadingProgress(int value)
		{
			for (;;)
			{
				int oldL = this._loadingProgress;
				if (oldL >= 182)
				{
					break;
				}
				int i = Math.Min(oldL + value, 182);
				if (Interlocked.CompareExchange(ref this._loadingProgress, i, oldL) == oldL)
				{
					return;
				}
			}
		}

		public bool IsTracerStillInWorld(Vector3 pos)
		{
			pos -= IntVector3.ToVector3(this._worldMin);
			return pos.X >= 0f && pos.Z >= 0f && pos.Y >= 0f && pos.X < 384f && pos.Z < 384f;
		}

		protected void SetWorldMin(IntVector3 newmin)
		{
			this._worldMin.X = newmin.X / 16 * 16;
			this._worldMin.Y = -64;
			this._worldMin.Z = newmin.Z / 16 * 16;
		}

		public IntVector3 GetChunkVectorIndex(Vector3 pos)
		{
			IntVector3 result = this.MakeIndexVectorFromPosition(pos);
			result.X /= 16;
			result.Y = 0;
			result.Z /= 16;
			return result;
		}

		public Vector3 MakePositionFromIndexVector(IntVector3 v)
		{
			Vector3 result = IntVector3.ToVector3(IntVector3.Add(v, this._worldMin));
			return Vector3.Multiply(result, 1f);
		}

		public IntVector3 MakeIndexVectorFromPosition(Vector3 a)
		{
			IntVector3 result = new IntVector3((int)Math.Floor((double)(a.X / 1f)), (int)Math.Floor((double)(a.Y / 1f)), (int)Math.Floor((double)(a.Z / 1f)));
			return IntVector3.Subtract(result, this._worldMin);
		}

		public int MakeIndexFromPosition(Vector3 a)
		{
			IntVector3 i = this.MakeIndexVectorFromPosition(a);
			if (this.IsIndexValid(i))
			{
				return this.MakeIndex(i);
			}
			return -1;
		}

		public int MakeIndexFromWorldIndexVector(IntVector3 a)
		{
			IntVector3 i = IntVector3.Subtract(a, this._worldMin);
			if (this.IsIndexValid(i))
			{
				return this.MakeIndex(i);
			}
			return -1;
		}

		public IntVector3 MakeIndexVectorFromChunkIndex(int index)
		{
			int z = index / 24;
			int x = index % 24;
			return new IntVector3(x * 16, 0, z * 16);
		}

		public IntVector3 MakeIndexVectorFromIndex(int index)
		{
			int z = index / 49152;
			index -= z * 49152;
			int x = index / 128;
			int y = index - x * 128;
			return new IntVector3(x, y, z);
		}

		public bool IndexVectorIsOnEdge(IntVector3 index)
		{
			return index.X == 0 || index.X == 383 || index.Z == 0 || index.Z == 383 || index.Y == 0 || index.Y == 127;
		}

		public bool IsIndexValid(IntVector3 a)
		{
			return a.X >= 0 && a.X < 384 && a.Y >= 0 && a.Y < 128 && a.Z >= 0 && a.Z < 384;
		}

		public bool IsIndexValid(int x, int y, int z)
		{
			return x >= 0 && x < 384 && y >= 0 && y < 128 && z >= 0 && z < 384;
		}

		public int MakeIndex(Vector3 a)
		{
			IntVector3 i = this.MakeIndexVectorFromPosition(a);
			return this.MakeIndex(i);
		}

		public int MakeIndex(int x, int y, int z)
		{
			return y + x * 128 + z * 128 * 384;
		}

		public int MakeIndex(IntVector3 a)
		{
			return a.Y + a.X * 128 + a.Z * 128 * 384;
		}

		public int MakeChunkIndexFromWorldIndexVector(IntVector3 i)
		{
			IntVector3 localVector = IntVector3.Subtract(i, this._worldMin);
			if (this.IsIndexValid(localVector))
			{
				return this.MakeChunkIndexFromIndexVector(localVector);
			}
			return -1;
		}

		public int MakeChunkIndexFromIndexVector(IntVector3 a)
		{
			return a.Z / 16 * 24 + a.X / 16;
		}

		public IntVector3 MakeSafeVectorIndex(IntVector3 a)
		{
			return new IntVector3(a.X.Clamp(0, 383), a.Y.Clamp(0, 127), a.Z.Clamp(0, 383));
		}

		public int GetSafeBlockAt(IntVector3 a)
		{
			return this.GetBlockAt(this.MakeSafeVectorIndex(a));
		}

		public IntVector3 GetLocalIndex(IntVector3 a)
		{
			return IntVector3.Subtract(a, this._worldMin);
		}

		public int GetSafeBlockAtABS(IntVector3 a)
		{
			IntVector3 local = IntVector3.Subtract(a, this._worldMin);
			return this.GetSafeBlockAt(local);
		}

		public IntVector3 GetNeighborIndex(IntVector3 a, BlockFace face)
		{
			return IntVector3.Add(a, BlockTerrain._faceNeighbors[(int)face]);
		}

		public int GetNeighborBlockAtABS(IntVector3 a, BlockFace face)
		{
			IntVector3 local = IntVector3.Subtract(IntVector3.Add(a, BlockTerrain._faceNeighbors[(int)face]), this._worldMin);
			return this.GetSafeBlockAt(local);
		}

		public void FillFaceLightTable(IntVector3 local, BlockFace face, ref float[] sun, ref float[] torch)
		{
			IntVector3[] neighbors = BlockTerrain._lightNeighbors[(int)face];
			for (int i = 0; i < 9; i++)
			{
				IntVector3 p = IntVector3.Add(local, neighbors[i]);
				if (this.IsIndexValid(p))
				{
					int nblock = this.GetBlockAt(p);
					BlockTypeEnum b = Block.GetTypeIndex(nblock);
					if (BlockType.GetType(b).Opaque || b == BlockTypeEnum.NumberOfBlocks || Block.IsInList(nblock))
					{
						sun[i] = -1f;
						torch[i] = -1f;
					}
					else
					{
						sun[i] = (float)Block.GetSunLightLevel(nblock);
						torch[i] = (float)Block.GetTorchLightLevel(nblock);
					}
				}
				else
				{
					sun[i] = -1f;
					torch[i] = -1f;
				}
			}
		}

		public void FillCubeLightTable(IntVector3 center, ref float[] sun, ref float[] torch)
		{
			IntVector3 a = IntVector3.Subtract(center, this._worldMin);
			IntVector3 offset = new IntVector3(-1, -1, -1);
			int index = 0;
			offset.Z = -1;
			while (offset.Z <= 1)
			{
				offset.Y = -1;
				while (offset.Y <= 1)
				{
					offset.X = -1;
					while (offset.X <= 1)
					{
						IntVector3 p = IntVector3.Add(a, offset);
						if (this.IsIndexValid(p))
						{
							int nBlock = this.GetBlockAt(p);
							BlockTypeEnum b = Block.GetTypeIndex(nBlock);
							if (b == BlockTypeEnum.NumberOfBlocks || Block.IsInList(nBlock))
							{
								sun[index] = 1f;
								torch[index] = 0f;
							}
							else if (BlockType.GetType(b).Opaque)
							{
								sun[index] = -1f;
								torch[index] = -1f;
							}
							else
							{
								sun[index] = (float)Block.GetSunLightLevel(nBlock) / 15f;
								torch[index] = (float)Block.GetTorchLightLevel(nBlock) / 15f;
							}
						}
						else
						{
							sun[index] = 1f;
							torch[index] = 0f;
						}
						offset.X++;
						index++;
					}
					offset.Y++;
				}
				offset.Z++;
			}
		}

		public BlockTypeEnum GetBlockWithChanges(Vector3 worldLocation)
		{
			return this.GetBlockWithChanges(IntVector3.FromVector3(worldLocation));
		}

		public BlockTypeEnum GetBlockWithChanges(IntVector3 worldIndex)
		{
			if (this.IsReady)
			{
				IntVector3 localPosition = IntVector3.Subtract(worldIndex, this._worldMin);
				if (this.IsIndexValid(localPosition))
				{
					int cidx = this.MakeChunkIndexFromIndexVector(localPosition);
					lock (this._chunks[cidx]._mods)
					{
						for (BlockTerrain.PendingMod pm = this._chunks[cidx]._mods.Front; pm != null; pm = (BlockTerrain.PendingMod)pm.NextNode)
						{
							if (pm._worldPosition.Equals(worldIndex))
							{
								return pm._blockType;
							}
						}
					}
					int index = this.MakeIndex(localPosition);
					return Block.GetTypeIndex(this._blocks[index]);
				}
			}
			return BlockTypeEnum.Empty;
		}

		public int GetBlockAt(int index)
		{
			return this._blocks[index];
		}

		public int GetBlockAt(int x, int y, int z)
		{
			return this.GetBlockAt(this.MakeIndex(x, y, z));
		}

		public int GetBlockAt(IntVector3 a)
		{
			return this.GetBlockAt(this.MakeIndex(a));
		}

		public void SetBlockAt(IntVector3 a, int data)
		{
			this.SetBlockAt(this.MakeIndex(a), data);
		}

		public void SetBlockAt(int x, int y, int z, int data)
		{
			this.SetBlockAt(this.MakeIndex(x, y, z), data);
		}

		public void SetBlockAt(int index, int data)
		{
			this._blocks[index] = data;
		}

		public BlockTerrain.BlockReference GetBlockReference(IntVector3 i)
		{
			return BlockTerrain.BlockReference.Alloc(i);
		}

		public BlockTerrain.BlockReference GetBlockReference(int x, int y, int z)
		{
			return BlockTerrain.BlockReference.Alloc(x, y, z);
		}

		public BlockTerrain.BlockReference GetBlockReference(Vector3 v)
		{
			return BlockTerrain.BlockReference.Alloc(v);
		}

		public BlockTerrain.BlockReference GetBlockReference(int index)
		{
			return BlockTerrain.BlockReference.Alloc(this.MakeIndexVectorFromIndex(index));
		}

		public void DecrementBuildTasks()
		{
			this._buildTasksRemaining.Decrement();
		}

		public void IncrementBuildTasks()
		{
			this._buildTasksRemaining.Increment();
		}

		public void SetSkyAndEmitterLightingForChunk(IntVector3 corner)
		{
			IntVector3 chunkCorner = IntVector3.Subtract(corner, this._worldMin);
			chunkCorner.Y = 0;
			this.SetSkyAndEmitterLightingForRegion(chunkCorner, new IntVector3(chunkCorner.X + 16 - 1, chunkCorner.Y + 128 - 1, chunkCorner.Z + 16 - 1));
		}

		public void SetSkyAndEmitterLightingForRegion(IntVector3 regionMin, IntVector3 regionMax)
		{
			IntVector3 offset = regionMin;
			offset.Z = regionMin.Z;
			while (offset.Z <= regionMax.Z)
			{
				offset.X = regionMin.X;
				while (offset.X <= regionMax.X)
				{
					offset.Y = regionMax.Y;
					int index = this.MakeIndex(offset);
					bool foundGround = regionMax.Y != 127 && (!Block.IsSky(this._blocks[index + 1]) || Block.GetType(this._blocks[index + 1]).LightTransmission != 16);
					while (offset.Y >= regionMin.Y)
					{
						if (this._resetRequested)
						{
							return;
						}
						int block = this._blocks[index];
						BlockType t = Block.GetType(block);
						if (Block.IsOpaque(block))
						{
							foundGround = true;
							if (t.SelfIllumination != 0)
							{
								goto IL_00E9;
							}
						}
						else
						{
							if (!foundGround)
							{
								int lt = t.LightTransmission - 1;
								foundGround = foundGround || lt != 15;
								block |= 256 | ((lt < 0) ? 0 : lt);
								goto IL_00E9;
							}
							goto IL_00E9;
						}
						IL_00FE:
						offset.Y--;
						index--;
						continue;
						IL_00E9:
						this._blocks[index] = Block.SetTorchLightLevel(block, t.SelfIllumination);
						goto IL_00FE;
					}
					offset.X++;
				}
				offset.Z++;
			}
		}

		public void ResetSkyAndEmitterLightingForRegion(IntVector3 regionMin, IntVector3 regionMax)
		{
			IntVector3 offset = regionMin;
			if (!this.IsIndexValid(regionMin) || !this.IsIndexValid(regionMax))
			{
				return;
			}
			offset.Z = regionMin.Z;
			while (offset.Z <= regionMax.Z)
			{
				offset.X = regionMin.X;
				while (offset.X <= regionMax.X)
				{
					offset.Y = regionMax.Y;
					int index = this.MakeIndex(offset);
					bool foundGround = regionMax.Y != 127 && (!Block.IsSky(this._blocks[index + 1]) || Block.GetType(this._blocks[index + 1]).LightTransmission != 16);
					while (offset.Y >= 0)
					{
						int block = this._blocks[index];
						BlockType t = Block.GetType(block);
						bool wasSky = Block.IsSky(block);
						if (Block.IsOpaque(block))
						{
							foundGround = true;
							block &= -257;
						}
						else if (!foundGround)
						{
							int lt = t.LightTransmission - 1;
							foundGround = foundGround || lt != 15;
							block |= 256 | ((lt < 0) ? 0 : lt);
						}
						else
						{
							block &= -257;
						}
						this._blocks[index] = Block.SetTorchLightLevel(block, t.SelfIllumination);
						if (offset.Y < regionMin.Y && wasSky == Block.IsSky(block))
						{
							break;
						}
						offset.Y--;
						index--;
					}
					offset.X++;
				}
				offset.Z++;
			}
		}

		protected void ApplyDelta(BlockTerrain.BuildTaskData data)
		{
			IntVector3 corner = IntVector3.Subtract(data._intVec0, this._worldMin);
			if (this.IsIndexValid(corner) && this._chunks[data._intData0]._delta != null)
			{
				int[] delta = this._chunks[data._intData0]._delta;
				this._chunks[data._intData0]._delta = null;
				for (int i = 0; i < delta.Length; i++)
				{
					IntVector3 p = IntVector3.Add(DeltaEntry.GetVector(delta[i]), corner);
					BlockTypeEnum bt = DeltaEntry.GetBlockType(delta[i]);
					int id = this.MakeIndex(p);
					if (Block.GetType(this._blocks[id]).SpawnEntity)
					{
						this.RemoveItemBlockEntity(Block.GetTypeIndex(this._blocks[id]), IntVector3.Add(p, this._worldMin));
					}
					if (BlockType.GetType(bt).SpawnEntity)
					{
						this.CreateItemBlockEntity(bt, IntVector3.Add(p, this._worldMin));
					}
					this._blocks[id] = Block.SetType(0, bt);
				}
			}
		}

		public void DoThreadedComputeBlocks(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData data = (BlockTerrain.BuildTaskData)context;
			if (!this._resetRequested && !data._skipProcessing)
			{
				IntVector3 regionMin = IntVector3.Subtract(data._intVec0, this._worldMin);
				if (this.IsIndexValid(regionMin))
				{
					regionMin.Y = 0;
					IntVector3 regionMax = IntVector3.Add(new IntVector3(15, 127, 15), regionMin);
					if (this.IsIndexValid(regionMax))
					{
						this.FillRegion(regionMin, regionMax, BlockTypeEnum.NumberOfBlocks);
						this._worldBuilder.BuildWorldChunk(this, data._intVec0);
						this.ApplyDelta(data);
						this.ApplyModListDuringCreate(data._intData0);
						this.ReplaceRegion(regionMin, regionMax, BlockTypeEnum.NumberOfBlocks, BlockTypeEnum.Empty);
						this.SetSkyAndEmitterLightingForChunk(data._intVec0);
					}
				}
				this.AddToLoadingProgress(1);
			}
			data.Release();
		}

		public void DoThreadedComputeGeometry(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData data = (BlockTerrain.BuildTaskData)context;
			bool wasQueued = false;
			if (!this._resetRequested && !data._skipProcessing)
			{
				if (this._chunks[data._intData0]._action >= BlockTerrain.NextChunkAction.NEEDS_GEOMETRY)
				{
					IntVector3 offset = IntVector3.Subtract(data._intVec0, this._worldMin);
					if (offset.X >= 0 && offset.X < 384 && offset.Z >= 0 && offset.Z < 384)
					{
						RenderChunk rc = RenderChunk.Alloc();
						rc._worldMin = data._intVec0;
						rc._basePosition = IntVector3.ToVector3(rc._worldMin);
						if (rc.BuildFaces(this._graphicsDevice))
						{
							lock (this._vertexBuildListLockObject)
							{
								this._vertexBuildListIncoming.Add(new BlockTerrain.QueuedBufferBuild(data._intData0, rc));
							}
							wasQueued = true;
						}
						else
						{
							rc.Release();
						}
					}
					if (!wasQueued)
					{
						this._chunks[data._intData0]._action = BlockTerrain.NextChunkAction.NONE;
					}
				}
				if (!wasQueued)
				{
					this._chunks[data._intData0]._numUsers.Decrement();
					this.AddToLoadingProgress(1);
				}
			}
			data.Release();
		}

		public void DoThreadedComputeLighting(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData data = (BlockTerrain.BuildTaskData)context;
			if (!this._resetRequested && !data._skipProcessing)
			{
				if (this._chunks[data._intData0]._action >= BlockTerrain.NextChunkAction.NEEDS_LIGHTING)
				{
					IntVector3 regionMin = IntVector3.Subtract(data._intVec0, this._worldMin);
					regionMin.Y = 0;
					IntVector3 regionMax = IntVector3.Add(new IntVector3(15, 127, 15), regionMin);
					this.ComputeFirstPassLightForRegion(regionMin, regionMax, this._mainLightingPool);
					this._computeGeometryPool.Add(data._intData0);
				}
				else
				{
					this._chunks[data._intData0]._numUsers.Decrement();
					this.AddToLoadingProgress(1);
				}
			}
			data.Release();
		}

		public void ComputeFirstPassLightForRegion(IntVector3 regionMin, IntVector3 regionMax, BlockTerrain.LightingPool lightingPool)
		{
			IntVector3 offset = regionMin;
			if (!this.IsIndexValid(regionMin) || !this.IsIndexValid(regionMax))
			{
				return;
			}
			offset.Z = regionMin.Z;
			while (offset.Z <= regionMax.Z)
			{
				offset.X = regionMin.X;
				while (offset.X <= regionMax.X)
				{
					offset.Y = regionMin.Y;
					int index = this.MakeIndex(offset);
					while (offset.Y <= regionMax.Y)
					{
						int block = this.GetBlockAt(offset);
						if (Block.IsLit(block))
						{
							int exclusion_mask;
							if (Block.GetTorchLightLevel(block) == 0)
							{
								exclusion_mask = -2147481856;
							}
							else
							{
								exclusion_mask = -2147482112;
							}
							for (int i = 0; i < 6; i++)
							{
								IntVector3 neighbor = IntVector3.Add(offset, BlockTerrain._faceNeighbors[i]);
								if (this.IsIndexValid(neighbor))
								{
									int ni = this.MakeIndex(neighbor);
									int nb = this._blocks[ni];
									if ((nb & exclusion_mask) == 0 && Interlocked.CompareExchange(ref this._blocks[ni], Block.IsInList(nb, true), nb) == nb)
									{
										lightingPool.Add(ni);
									}
								}
							}
						}
						offset.Y++;
						index++;
					}
					offset.X++;
				}
				offset.Z++;
			}
			offset = regionMin;
			offset.Z--;
			if (offset.Z >= 0)
			{
				offset.Y = 0;
				int index2 = this.MakeIndex(offset);
				int x = regionMin.X;
				while (x <= regionMax.X)
				{
					offset.Y = regionMin.Y;
					while (offset.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[index2]))
						{
							int exclusion_mask;
							if (Block.GetTorchLightLevel(this._blocks[index2]) == 0)
							{
								exclusion_mask = -2147481856;
							}
							else
							{
								exclusion_mask = -2147482112;
							}
							int ni2 = index2 + 49152;
							int b = this._blocks[ni2];
							if ((b & exclusion_mask) == 0 && Interlocked.CompareExchange(ref this._blocks[ni2], Block.IsInList(b, true), b) == b)
							{
								lightingPool.Add(ni2);
							}
						}
						offset.Y++;
						index2++;
					}
					x++;
					offset.X++;
				}
			}
			offset = regionMin;
			offset.Z += 16;
			if (offset.Z < 384)
			{
				offset.Y = regionMin.Y;
				int index3 = this.MakeIndex(offset);
				int x2 = regionMin.X;
				while (x2 <= regionMax.X)
				{
					offset.Y = regionMin.Y;
					while (offset.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[index3]))
						{
							int exclusion_mask;
							if (Block.GetTorchLightLevel(this._blocks[index3]) == 0)
							{
								exclusion_mask = -2147481856;
							}
							else
							{
								exclusion_mask = -2147482112;
							}
							int ni3 = index3 - 49152;
							int b2 = this._blocks[ni3];
							if ((b2 & exclusion_mask) == 0 && Interlocked.CompareExchange(ref this._blocks[ni3], Block.IsInList(b2, true), b2) == b2)
							{
								lightingPool.Add(ni3);
							}
						}
						offset.Y++;
						index3++;
					}
					x2++;
					offset.X++;
				}
			}
			offset = regionMin;
			offset.X--;
			if (offset.X >= 0)
			{
				int z = regionMin.Z;
				while (z <= regionMax.Z)
				{
					offset.Y = regionMin.Y;
					offset.Z = z;
					int index4 = this.MakeIndex(offset);
					while (offset.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[index4]))
						{
							int exclusion_mask;
							if (Block.GetTorchLightLevel(this._blocks[index4]) == 0)
							{
								exclusion_mask = -2147481856;
							}
							else
							{
								exclusion_mask = -2147482112;
							}
							int ni4 = index4 + 128;
							int b3 = this._blocks[ni4];
							if ((b3 & exclusion_mask) == 0 && Interlocked.CompareExchange(ref this._blocks[ni4], Block.IsInList(b3, true), b3) == b3)
							{
								lightingPool.Add(ni4);
							}
						}
						index4++;
						offset.Y++;
					}
					z++;
					offset.Z++;
				}
			}
			offset = regionMin;
			offset.X++;
			if (offset.X < 384)
			{
				int z2 = regionMin.Z;
				while (z2 <= regionMax.Z)
				{
					offset.Y = regionMin.Y;
					offset.Z = z2;
					int index5 = this.MakeIndex(offset);
					while (offset.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[index5]))
						{
							int exclusion_mask;
							if (Block.GetTorchLightLevel(this._blocks[index5]) == 0)
							{
								exclusion_mask = -2147481856;
							}
							else
							{
								exclusion_mask = -2147482112;
							}
							int ni5 = index5 - 128;
							int b4 = this._blocks[ni5];
							if ((b4 & exclusion_mask) == 0 && Interlocked.CompareExchange(ref this._blocks[ni5], Block.IsInList(b4, true), b4) == b4)
							{
								lightingPool.Add(ni5);
							}
						}
						index5++;
						offset.Y++;
					}
					z2++;
					offset.Z++;
				}
			}
		}

		public void FillLighting(BlockTerrain.LightingPool lightPool)
		{
			while (!lightPool.Empty && !this._resetRequested)
			{
				this.AddToLoadingProgress(1);
				int[] lightBuffer;
				int numLightNodes;
				int[] neighborsForLight;
				lightPool.GetList(out lightBuffer, out numLightNodes, out neighborsForLight);
				this.maxLightNodes = Math.Max(this.maxLightNodes, numLightNodes);
				for (int lightIndex = 0; lightIndex < numLightNodes; lightIndex++)
				{
					if (this._resetRequested)
					{
						return;
					}
					int index = lightBuffer[lightIndex];
					IntVector3 c = this.MakeIndexVectorFromIndex(index);
					bool onEdge = this.IndexVectorIsOnEdge(c);
					int us = this._blocks[index] & -1025;
					if (!Block.IsUninitialized(us))
					{
						BlockType t = Block.GetType(us);
						int clight = Block.GetLighting(us);
						int torch = 0;
						int numNeighbors = 0;
						if (!onEdge)
						{
							for (int i = 0; i < 6; i++)
							{
								int idx = index + BlockTerrain._faceIndexNeighbors[i];
								if ((this._blocks[idx] & -2147483648) == 0)
								{
									neighborsForLight[numNeighbors++] = idx;
								}
							}
						}
						else
						{
							for (int j = 0; j < 6; j++)
							{
								IntVector3 neighbor = IntVector3.Add(c, BlockTerrain._faceNeighbors[j]);
								if (this.IsIndexValid(neighbor))
								{
									int idx2 = this.MakeIndex(neighbor);
									if ((this._blocks[idx2] & -2147483648) == 0)
									{
										neighborsForLight[numNeighbors++] = idx2;
									}
								}
							}
						}
						if (Block.IsSky(us))
						{
							for (int k = 0; k < numNeighbors; k++)
							{
								torch = Math.Max(torch, Block.GetTorchLightLevel(this._blocks[neighborsForLight[k]]));
							}
						}
						else
						{
							int sun = int.MinValue;
							for (int l = 0; l < numNeighbors; l++)
							{
								int m = this._blocks[neighborsForLight[l]];
								sun = Math.Max(sun, Block.GetSunLightLevel(m));
								torch = Math.Max(torch, Block.GetTorchLightLevel(m));
							}
							us = Block.SetSunLightLevel(us, t.TransmitLight(sun));
						}
						us = Block.SetTorchLightLevel(us, Math.Max(t.SelfIllumination, t.TransmitLight(torch)));
						this._blocks[index] = us;
						int diff = Block.GetLighting(clight ^ us);
						if (diff != 0)
						{
							lightPool.UpdateMinAABB(ref c);
							for (int n = 0; n < numNeighbors; n++)
							{
								int ni = neighborsForLight[n];
								int n2 = this._blocks[ni];
								if ((n2 & 1536) == 0)
								{
									this._blocks[ni] = n2 | 1024;
									lightPool.Add(ni);
								}
							}
						}
					}
				}
			}
		}

		public void DoThreadedStepUpdateChunks(BaseTask task, object context)
		{
			if (!this._resetRequested)
			{
				if (!this._mainLightingPool.Empty)
				{
					this.FillLighting(this._mainLightingPool);
				}
				if (!this._computeGeometryPool.Empty && !this._resetRequested)
				{
					this._computeGeometryPool.Drain();
				}
				else if (!this._computeLightingPool.Empty && !this._resetRequested)
				{
					this._computeLightingPool.Drain();
				}
			}
			this.DecrementBuildTasks();
		}

		public void DoThreadedShiftTerrain(BaseTask task, object context)
		{
			BlockTerrain.ShiftingTerrainData data = (BlockTerrain.ShiftingTerrainData)context;
			Buffer.BlockCopy(this._blocks, data.source, this._shiftedBlocks, data.dest, data.length);
			int fillIndex = data.fillStart;
			int fillCount = data.fillLength;
			for (int i = 0; i < fillCount; i++)
			{
				this._shiftedBlocks[fillIndex++] = this.initblock;
			}
			this._shiftTerrainData.done = true;
			this._updateTasksRemaining.Decrement();
		}

		public bool FinishShiftTerrain()
		{
			if (this._shiftTerrainData.running && this._shiftTerrainData.done)
			{
				this._shiftTerrainData.running = false;
				int dx = this._shiftTerrainData.dx;
				int dz = this._shiftTerrainData.dz;
				this._worldMin.X = this._worldMin.X + dx * 16;
				this._worldMin.Z = this._worldMin.Z + dz * 16;
				this._currentEyeChunkIndex -= dx + dz * 24;
				int bufferOffsetAmount = -dz * 16 * 128 * 384 + -dx * 16 * 128;
				int bufferMoveSize = 18874368 - Math.Abs(bufferOffsetAmount);
				bool reverse = bufferOffsetAmount > 0;
				int[] tmp = this._blocks;
				this._blocks = this._shiftedBlocks;
				this._shiftedBlocks = tmp;
				bufferOffsetAmount = -dz * 24 + -dx;
				bufferMoveSize = 576 - Math.Abs(bufferOffsetAmount);
				int offset = bufferOffsetAmount;
				int sourcei;
				int direction;
				if (!reverse)
				{
					bufferOffsetAmount = -bufferOffsetAmount;
					sourcei = bufferOffsetAmount;
					direction = 1;
				}
				else
				{
					sourcei = 576 - bufferOffsetAmount - 1;
					direction = -1;
				}
				if (dx > 0)
				{
					for (int z = 0; z < 24; z++)
					{
						int index = z * 24;
						int x = 0;
						while (x < dx)
						{
							if (this._chunks[index]._chunk != null)
							{
								this._chunks[index].ZeroData(true);
							}
							x++;
							index++;
						}
					}
				}
				else if (dx < 0)
				{
					for (int z2 = 0; z2 < 24; z2++)
					{
						int index2 = z2 * 24 + 24 - 1;
						int x2 = 0;
						while (x2 < -dx)
						{
							if (this._chunks[index2]._chunk != null)
							{
								this._chunks[index2].ZeroData(true);
							}
							x2++;
							index2--;
						}
					}
				}
				for (int i = 0; i < bufferMoveSize; i++)
				{
					this._chunks[sourcei + offset].SwapIn(ref this._chunks[sourcei]);
					sourcei += direction;
				}
				for (int j = 0; j < 576; j++)
				{
					if (this._chunks[j]._chunk == null)
					{
						this._chunks[j].Reset();
					}
				}
				this._allChunksLoaded = false;
				return false;
			}
			return true;
		}

		public bool ShiftTerrain(int desireddx, int desireddz)
		{
			if (!this._shiftTerrainData.running)
			{
				this._shiftTerrainData.dx = desireddx;
				this._shiftTerrainData.dz = desireddz;
				this._shiftTerrainData.running = true;
				this._shiftTerrainData.done = false;
				int bufferOffsetAmount = -desireddz * 16 * 128 * 384 + -desireddx * 16 * 128;
				this._shiftTerrainData.fillLength = Math.Abs(bufferOffsetAmount);
				int bufferMoveSize = 18874368 - this._shiftTerrainData.fillLength;
				if (bufferOffsetAmount <= 0)
				{
					this._shiftTerrainData.source = -bufferOffsetAmount * 4;
					this._shiftTerrainData.dest = 0;
					this._shiftTerrainData.fillStart = bufferMoveSize;
				}
				else
				{
					this._shiftTerrainData.source = 0;
					this._shiftTerrainData.dest = bufferOffsetAmount * 4;
					this._shiftTerrainData.fillStart = 0;
				}
				this._shiftTerrainData.length = bufferMoveSize * 4;
				TaskDispatcher.Instance.AddRushTask(this._shiftTerrainDelegate, this._shiftTerrainData);
				this._updateTasksRemaining.Increment();
				return true;
			}
			return false;
		}

		public void FillRegion(IntVector3 min, IntVector3 max, BlockTypeEnum blockType)
		{
			int block = Block.SetType(0, blockType);
			for (int z = min.Z; z <= max.Z; z++)
			{
				for (int x = min.X; x <= max.X; x++)
				{
					int index = this.MakeIndex(x, min.Y, z);
					int y = min.Y;
					while (y <= max.Y)
					{
						this._blocks[index] = block;
						y++;
						index++;
					}
				}
			}
		}

		public void ReplaceRegion(IntVector3 min, IntVector3 max, BlockTypeEnum replaceme, BlockTypeEnum withme)
		{
			for (int z = min.Z; z <= max.Z; z++)
			{
				for (int x = min.X; x <= max.X; x++)
				{
					int index = this.MakeIndex(x, min.Y, z);
					int y = min.Y;
					while (y <= max.Y)
					{
						if (Block.GetTypeIndex(this._blocks[index]) == replaceme)
						{
							this._blocks[index] = Block.SetType(this._blocks[index], withme);
						}
						y++;
						index++;
					}
				}
			}
		}

		public void DoFinishThreadedRegionOperation(BaseTask task, object context)
		{
			this._updateTasksRemaining.Decrement();
		}

		public void AddBlockToLightList(IntVector3 block, BlockTerrain.LightingPool lightPool)
		{
			IntVector3 max = IntVector3.Min(IntVector3.Add(block, new IntVector3(1, 1, 1)), new IntVector3(383, 127, 383));
			IntVector3 min = IntVector3.Max(IntVector3.Subtract(block, new IntVector3(1, 1, 1)), IntVector3.Zero);
			int checkdownStart = -1;
			if (min.Y > 0)
			{
				checkdownStart = min.Y - 1;
			}
			for (int z = min.Z; z <= max.Z; z++)
			{
				for (int x = min.X; x <= max.X; x++)
				{
					int index = this.MakeIndex(x, min.Y, z);
					int y = min.Y;
					while (y <= max.Y)
					{
						if (!Block.IsUninitialized(this._blocks[index]))
						{
							lightPool.Add(index);
							this._blocks[index] |= 1024;
						}
						y++;
						index++;
					}
				}
			}
			if (checkdownStart >= 0)
			{
				IntVector3 offset = new IntVector3(block.X, checkdownStart, block.Z);
				int index2 = this.MakeIndex(offset);
				while (offset.Y >= 0 && Block.IsSky(this._blocks[index2]))
				{
					for (int i = 0; i < 6; i++)
					{
						if (i != 5)
						{
							IntVector3 neighbor = IntVector3.Add(offset, BlockTerrain._faceNeighbors[i]);
							if (this.IsIndexValid(neighbor))
							{
								int ni = this.MakeIndex(neighbor);
								int nb = this._blocks[ni];
								if ((nb & -2147481856) == 0 && Interlocked.CompareExchange(ref this._blocks[ni], Block.IsInList(nb, true), nb) == nb)
								{
									lightPool.Add(ni);
								}
							}
						}
					}
					offset.Y--;
					index2--;
				}
			}
		}

		private void DoThreadedFinishSetBlock(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData data = (BlockTerrain.BuildTaskData)context;
			if (!this._resetRequested)
			{
				this.FillLighting(this._updateLightingPool);
				IntVector3 max = IntVector3.Min(IntVector3.Add(this._updateLightingPool._max, new IntVector3(1, 1, 1)), new IntVector3(383, 127, 383));
				IntVector3 min = IntVector3.Max(IntVector3.Subtract(this._updateLightingPool._min, new IntVector3(1, 1, 1)), IntVector3.Zero);
				min.X /= 16;
				min.Z /= 16;
				max.X /= 16;
				max.Z /= 16;
				GatherTask gt = TaskDispatcher.Instance.AddGatherTask(this._finishRegionOpDelegate, null);
				for (int z = min.Z; z <= max.Z; z++)
				{
					for (int x = min.X; x <= max.X; x++)
					{
						int index = z * 24 + x;
						BlockTerrain.BuildTaskData td = BlockTerrain.BuildTaskData.Alloc();
						td._intVec0 = IntVector3.Add(this._worldMin, this.MakeIndexVectorFromChunkIndex(index));
						td._intData0 = index;
						this._chunks[index]._numUsers.Increment();
						gt.AddTask(this._computeGeometryDelegate, td);
					}
				}
				this.DecrChunkInUse(data._intData0);
				gt.StartNow();
			}
			data.Release();
		}

		public bool OkayToBuildHere(IntVector3 b)
		{
			b.Y -= this._worldMin.Y;
			return b.Y > 0 && b.Y < 127;
		}

		private void FillFacePlanes(IntVector3 final)
		{
			this._facePlanes[0].D = (float)(-(float)final.X - 1);
			this._facePlanes[1].D = (float)final.Z;
			this._facePlanes[2].D = (float)final.X;
			this._facePlanes[3].D = (float)(-(float)final.Z - 1);
			this._facePlanes[4].D = (float)(-(float)final.Y - 1);
			this._facePlanes[5].D = (float)final.Y;
		}

		public bool ProbeTouchesBlock(TraceProbe probe, IntVector3 pos)
		{
			this.FillFacePlanes(pos);
			probe.TestShape(this._facePlanes, pos);
			return probe._collides;
		}

		private bool SlopingIsBlocked(ref IntVector3 min, ref IntVector3 max, ref IntVector3 test)
		{
			if (test.Y >= 126)
			{
				return false;
			}
			IntVector3 walk = min;
			walk.Z = min.Z;
			while (walk.Z <= max.Z)
			{
				walk.X = min.X;
				while (walk.X <= max.X)
				{
					walk.Y = test.Y + 1;
					int idx = this.MakeIndex(walk);
					if (!Block.GetType(this._blocks[idx]).BlockPlayer && Block.GetType(this._blocks[idx + 1]).BlockPlayer)
					{
						return true;
					}
					walk.X++;
				}
				walk.Z++;
			}
			return false;
		}

		private void BruteForceTrace(TraceProbe tp)
		{
			IntVector3 minIndex = this.MakeIndexVectorFromPosition(tp._bounds.Min);
			IntVector3 maxIndex = this.MakeIndexVectorFromPosition(tp._bounds.Max);
			bool testForSlopes = false;
			int minYForSlope = 0;
			int maxYForSlope = 0;
			if (tp.SimulateSlopedSides)
			{
				Vector3 startPoint = tp._start;
				startPoint.Y -= tp.HalfVector.Y;
				IntVector3 startIndex = this.GetLocalIndex(IntVector3.FromVector3(startPoint));
				startIndex.Y++;
				if (this.IsIndexValid(startIndex))
				{
					int index = this.MakeIndex(startIndex);
					if (!Block.GetType(this._blocks[index]).BlockPlayer)
					{
						minYForSlope = (int)Math.Floor((double)(tp._start.Y - tp.HalfVector.Y + 0.01f)) - this._worldMin.Y;
						maxYForSlope = (int)Math.Floor((double)(tp._start.Y - tp.HalfVector.Y + 0.2f)) - this._worldMin.Y;
						testForSlopes = true;
					}
				}
			}
			tp.ShapeHasSlopedSides = false;
			minIndex.SetToMax(IntVector3.Zero);
			maxIndex.SetToMin(new IntVector3(383, 127, 383));
			IntVector3 counts = IntVector3.Subtract(minIndex, maxIndex);
			if (counts.X * counts.Y * counts.Z < 27)
			{
				IntVector3 walk = minIndex;
				walk.Z = minIndex.Z;
				while (walk.Z <= maxIndex.Z)
				{
					walk.X = minIndex.X;
					while (walk.X <= maxIndex.X)
					{
						walk.Y = minIndex.Y;
						int index2 = this.MakeIndex(walk);
						while (walk.Y <= maxIndex.Y)
						{
							if (tp.TestThisType(Block.GetTypeIndex(this._blocks[index2])))
							{
								if (testForSlopes && Block.GetType(this._blocks[index2]).AllowSlopes)
								{
									if (walk.Y <= maxYForSlope && walk.Y >= minYForSlope)
									{
										tp.ShapeHasSlopedSides = !this.SlopingIsBlocked(ref minIndex, ref maxIndex, ref walk);
									}
									else
									{
										tp.ShapeHasSlopedSides = false;
									}
								}
								IntVector3 worldPosition = walk + this._worldMin;
								this.FillFacePlanes(worldPosition);
								tp.TestShape(this._facePlanes, worldPosition, Block.GetTypeIndex(this._blocks[index2]));
							}
							walk.Y++;
							index2++;
						}
						walk.X++;
					}
					walk.Z++;
				}
			}
		}

		public void Trace(TraceProbe tp)
		{
			if (tp is AABBTraceProbe)
			{
				this.BruteForceTrace(tp);
				return;
			}
			Vector3 delta = tp._end - tp._start;
			IntVector3 walker = this.MakeIndexVectorFromPosition(tp._start);
			IntVector3 worldIndex = IntVector3.Add(this._worldMin, walker);
			IntVector3 increments = IntVector3.Zero;
			Vector3 nextT = Vector3.Zero;
			Vector3 deltaT = Vector3.Zero;
			float currentT = 0f;
			if (delta.X == 0f)
			{
				nextT.X = 2f;
				deltaT.X = 0f;
			}
			else
			{
				deltaT.X = Math.Abs(1f / delta.X);
				float numerator = (float)worldIndex.X;
				if (delta.X > 0f)
				{
					numerator += 1f;
					increments.X = 1;
				}
				else
				{
					increments.X = -1;
				}
				nextT.X = (numerator - tp._start.X) / delta.X;
			}
			if (delta.Y == 0f)
			{
				nextT.Y = 2f;
				deltaT.Y = 0f;
			}
			else
			{
				deltaT.Y = Math.Abs(1f / delta.Y);
				float numerator = (float)worldIndex.Y;
				if (delta.Y > 0f)
				{
					numerator += 1f;
					increments.Y = 1;
				}
				else
				{
					increments.Y = -1;
				}
				nextT.Y = (numerator - tp._start.Y) / delta.Y;
			}
			if (delta.Z == 0f)
			{
				nextT.Z = 2f;
				deltaT.Z = 0f;
			}
			else
			{
				deltaT.Z = Math.Abs(1f / delta.Z);
				float numerator = (float)worldIndex.Z;
				if (delta.Z > 0f)
				{
					numerator += 1f;
					increments.Z = 1;
				}
				else
				{
					increments.Z = -1;
				}
				nextT.Z = (numerator - tp._start.Z) / delta.Z;
			}
			for (;;)
			{
				if (this.IsIndexValid(walker))
				{
					int index = this.MakeIndex(walker);
					BlockTypeEnum blocktype = Block.GetTypeIndex(this._blocks[index]);
					if (tp.TestThisType(blocktype))
					{
						this.FillFacePlanes(worldIndex);
						if (!tp.TestShape(this._facePlanes, worldIndex, blocktype))
						{
							break;
						}
					}
				}
				if (currentT >= 1f)
				{
					return;
				}
				if (nextT.X < nextT.Y)
				{
					if (nextT.X < nextT.Z)
					{
						currentT = nextT.X;
						nextT.X += deltaT.X;
						walker.X += increments.X;
						worldIndex.X += increments.X;
					}
					else
					{
						currentT = nextT.Z;
						nextT.Z += deltaT.Z;
						walker.Z += increments.Z;
						worldIndex.Z += increments.Z;
					}
				}
				else if (nextT.Y < nextT.Z)
				{
					currentT = nextT.Y;
					nextT.Y += deltaT.Y;
					walker.Y += increments.Y;
					worldIndex.Y += increments.Y;
				}
				else
				{
					currentT = nextT.Z;
					nextT.Z += deltaT.Z;
					walker.Z += increments.Z;
					worldIndex.Z += increments.Z;
				}
			}
		}

		public FallLockTestResult FallLockFace(Vector3 v, BlockFace f)
		{
			return this.FallLockFace(IntVector3.FromVector3(v), f);
		}

		public FallLockTestResult FallLockFace(IntVector3 v, BlockFace f)
		{
			IntVector3 off = IntVector3.Subtract(v, this._worldMin);
			if (this.IsIndexValid(off))
			{
				int index = this.MakeIndex(off);
				BlockType bt = Block.GetType(this._blocks[index]);
				if (bt.BlockPlayer)
				{
					IntVector3 i = this.GetNeighborIndex(off, f);
					if (!this.IsIndexValid(i))
					{
						return FallLockTestResult.SOLID_BLOCK_NEEDS_WALL;
					}
					int nindex = this.MakeIndex(i);
					bt = Block.GetType(this._blocks[nindex]);
					if (!bt.BlockPlayer)
					{
						return FallLockTestResult.SOLID_BLOCK_NEEDS_WALL;
					}
					return FallLockTestResult.SOLID_BLOCK_NO_WALL;
				}
			}
			return FallLockTestResult.EMPTY_BLOCK;
		}

		public int DepthUnderGround(Vector3 location)
		{
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(location);
			if (!this.IsIndexValid(startIndex))
			{
				return 0;
			}
			int count = 0;
			for (int y = startIndex.Y; y < 128; y++)
			{
				startIndex.Y = y;
				int index = this.MakeIndex(startIndex);
				if (Block.GetType(this._blocks[index]).BlockPlayer)
				{
					count++;
				}
			}
			return count;
		}

		public int DepthUnderSpaceRock(Vector3 location)
		{
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(location);
			if (!this.IsIndexValid(startIndex))
			{
				return 0;
			}
			int count = 0;
			for (int y = startIndex.Y; y < 128; y++)
			{
				startIndex.Y = y;
				int index = this.MakeIndex(startIndex);
				if (Block.GetType(this._blocks[index])._type == BlockTypeEnum.SpaceRock)
				{
					count++;
				}
			}
			return count;
		}

		public float DepthUnderWater(Vector3 location)
		{
			return Math.Max(this.WaterLevel - location.Y, 0f);
		}

		public bool PointIsInAsteroid(Vector3 guess)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(guess);
			if (!this.IsIndexValid(startIndex))
			{
				return false;
			}
			int origIndex = this.MakeIndex(startIndex);
			if (Block.IsSky(this._blocks[origIndex]))
			{
				return false;
			}
			int index = origIndex;
			int walkY = startIndex.Y;
			bool moonAbove = false;
			while (walkY < 128)
			{
				if (Block.GetTypeIndex(this._blocks[index]) == BlockTypeEnum.SpaceRock)
				{
					moonAbove = true;
					break;
				}
				walkY++;
				index++;
			}
			if (!moonAbove)
			{
				return false;
			}
			index = origIndex;
			walkY = startIndex.Y;
			while (walkY >= 0)
			{
				if (Block.GetTypeIndex(this._blocks[index]) == BlockTypeEnum.SpaceRock)
				{
					return true;
				}
				walkY--;
				index--;
			}
			return false;
		}

		private int FindStepableHeight(ref IntVector3 v, int currentHeight)
		{
			if (!this.IsIndexValid(v))
			{
				return -1;
			}
			int index = this.MakeIndex(v);
			if (Block.GetType(this._blocks[index + currentHeight]).BlockPlayer)
			{
				int newHeight = currentHeight;
				int maxHeight = Math.Min(currentHeight + 3, 128);
				bool foundAir = false;
				while (newHeight < maxHeight)
				{
					if (!Block.GetType(this._blocks[index + newHeight]).BlockPlayer)
					{
						foundAir = true;
						break;
					}
					newHeight++;
				}
				if (!foundAir)
				{
					return -1;
				}
				int aircount = 0;
				while (aircount < 2 && newHeight < 128 && !Block.GetType(this._blocks[index + newHeight]).BlockPlayer)
				{
					aircount++;
					newHeight++;
				}
				if (aircount >= 2)
				{
					return newHeight;
				}
				return -1;
			}
			else
			{
				int newHeight2 = currentHeight;
				int maxHeight2 = Math.Min(currentHeight + 3, 128);
				while (newHeight2 < maxHeight2 && !Block.GetType(this._blocks[index + newHeight2]).BlockPlayer)
				{
					newHeight2++;
				}
				if (newHeight2 - currentHeight < 2)
				{
					return -1;
				}
				newHeight2 = currentHeight;
				while (newHeight2 > 0 && !Block.GetType(this._blocks[index + newHeight2]).BlockPlayer)
				{
					newHeight2--;
				}
				newHeight2++;
				if (Math.Abs(newHeight2 - currentHeight) > 2)
				{
					return -1;
				}
				return newHeight2;
			}
		}

		public Vector3 FindNearbySpawnPoint(Vector3 plrpos, int iterations, int range)
		{
			plrpos.X = (float)Math.Floor((double)plrpos.X) + 0.1f;
			plrpos.Y = (float)Math.Floor((double)plrpos.Y) + 0.1f;
			plrpos.Z = (float)Math.Floor((double)plrpos.Z) + 0.1f;
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(plrpos);
			if (!this.IsIndexValid(startIndex))
			{
				return Vector3.Zero;
			}
			IntVector3 wanderIndex = startIndex;
			int wanderHeight = startIndex.Y;
			wanderIndex.Y = 0;
			int scalarIndex = this.MakeIndex(wanderIndex);
			while (wanderHeight > 0 && !Block.GetType(this._blocks[scalarIndex + wanderHeight]).BlockPlayer)
			{
				wanderHeight--;
			}
			if (wanderHeight < 0)
			{
				return Vector3.Zero;
			}
			wanderHeight++;
			while (wanderHeight < 128 && Block.GetType(this._blocks[scalarIndex + wanderHeight]).BlockPlayer)
			{
				wanderHeight++;
			}
			if (wanderHeight >= 128)
			{
				return Vector3.Zero;
			}
			int oldDir = -1;
			for (int i = 0; i < iterations; i++)
			{
				bool foundMove = false;
				int dir;
				if (oldDir == -1 && MathTools.RandomBool())
				{
					dir = MathTools.RandomInt(0, 4);
				}
				else
				{
					dir = oldDir ^ 2;
				}
				int trycount = 0;
				while (!foundMove)
				{
					if (dir != oldDir)
					{
						IntVector3 proposal = wanderIndex;
						if ((dir & 1) != 0)
						{
							proposal.X += (((dir & 2) != 0) ? 1 : (-1));
						}
						else
						{
							proposal.Z += (((dir & 2) != 0) ? 1 : (-1));
						}
						int newHeight = this.FindStepableHeight(ref proposal, wanderHeight);
						if (newHeight != -1)
						{
							wanderIndex = proposal;
							wanderHeight = newHeight;
							oldDir = dir ^ 2;
							foundMove = true;
						}
					}
					if (!foundMove)
					{
						dir = (dir + 1) % 4;
						trycount++;
						if (trycount >= 4)
						{
							if (oldDir == -1)
							{
								return Vector3.Zero;
							}
							dir = oldDir;
							oldDir = -1;
						}
					}
				}
			}
			wanderIndex.Y = wanderHeight;
			wanderIndex = IntVector3.Add(this._worldMin, wanderIndex);
			Vector3 r = IntVector3.ToVector3(wanderIndex);
			r.X += 0.5f;
			r.Y += 0.1f;
			r.Z += 0.5f;
			return r;
		}

		public Vector3 FindAlienSpawnPoint(Vector3 guess, bool surfaceOkay)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(guess);
			if (startIndex.X < 0 || startIndex.X >= 384)
			{
				return Vector3.Zero;
			}
			if (startIndex.Z < 0 || startIndex.Z >= 384)
			{
				return Vector3.Zero;
			}
			int originalY = startIndex.Y;
			startIndex.Y = 0;
			int index = this.MakeIndex(startIndex);
			startIndex.Y = 127;
			bool foundRock = false;
			int closestHeight = -1;
			int closestDist = int.MaxValue;
			int lowestBlock = 1;
			int emptyCount = 0;
			while (startIndex.Y >= lowestBlock)
			{
				BlockTypeEnum blockType = Block.GetTypeIndex(this._blocks[index + startIndex.Y]);
				if (!surfaceOkay && !foundRock)
				{
					if (blockType == BlockTypeEnum.SpaceRock)
					{
						foundRock = true;
					}
					emptyCount = 0;
				}
				else if (blockType == BlockTypeEnum.SpaceRock)
				{
					if (emptyCount > 1)
					{
						if (surfaceOkay)
						{
							closestHeight = startIndex.Y;
							break;
						}
						int dist = Math.Abs(startIndex.Y - originalY);
						if (dist <= closestDist)
						{
							closestDist = dist;
							closestHeight = startIndex.Y;
							lowestBlock = originalY - closestDist;
						}
						else if (closestHeight != -1)
						{
							break;
						}
					}
					emptyCount = 0;
				}
				else if (!BlockType.GetType(blockType).BlockPlayer)
				{
					emptyCount++;
				}
				else
				{
					emptyCount = 0;
				}
				startIndex.Y--;
			}
			if (closestHeight != -1)
			{
				startIndex.Y = closestHeight + 1;
				Vector3 result = this.MakePositionFromIndexVector(startIndex);
				result.X += 0.5f;
				result.Y += 0.1f;
				result.Z += 0.5f;
				return result;
			}
			return Vector3.Zero;
		}

		public Vector3 FindClosestCeiling(Vector3 guess)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(guess);
			startIndex.X = startIndex.X.Clamp(0, 383);
			startIndex.Y = startIndex.Y.Clamp(0, 127);
			startIndex.Z = startIndex.Z.Clamp(0, 383);
			int index = this.MakeIndex(startIndex);
			IntVector3 result = IntVector3.Zero;
			IntVector3 currIndex = startIndex;
			while (currIndex.Y < 128 && Block.GetType(this._blocks[index]).BlockPlayer)
			{
				index++;
				currIndex.Y++;
			}
			while (currIndex.Y < 128)
			{
				if (Block.GetType(this._blocks[index]).BlockPlayer)
				{
					result = currIndex;
					break;
				}
				index++;
				currIndex.Y++;
			}
			if (result.Y != 0)
			{
				result = IntVector3.Add(this._worldMin, result);
				Vector3 r = IntVector3.ToVector3(result);
				r.X += 0.5f;
				r.Y += 0.1f;
				r.Z += 0.5f;
				return r;
			}
			return Vector3.Zero;
		}

		public Vector3 FindSafeStartLocation(Vector3 guess)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(guess);
			startIndex.X = startIndex.X.Clamp(0, 383);
			startIndex.Y = startIndex.Y.Clamp(0, 127);
			startIndex.Z = startIndex.Z.Clamp(0, 383);
			int foundHeight = -1;
			int squareSize = 1;
			int waterBlock = (this.IsWaterWorld ? ((int)Math.Floor((double)(64f + this.WaterLevel))) : (-1));
			IntVector3 currIndex;
			for (;;)
			{
				for (int sqx = startIndex.X; sqx < startIndex.X + squareSize; sqx++)
				{
					if (sqx >= 0)
					{
						if (sqx >= 384)
						{
							break;
						}
						for (int sqz = startIndex.Z; sqz < startIndex.Z + squareSize; sqz++)
						{
							if (sqz >= 0)
							{
								if (sqz >= 384)
								{
									break;
								}
								bool dothisone = false;
								if (sqx == startIndex.X || sqx == startIndex.X + squareSize - 1)
								{
									dothisone = true;
								}
								else if (sqz == startIndex.Z || sqz == startIndex.Z + squareSize - 1)
								{
									dothisone = true;
								}
								if (dothisone)
								{
									currIndex = new IntVector3(sqx, startIndex.Y, sqz);
									int index = this.MakeIndex(currIndex);
									if (Block.GetType(this._blocks[index]).BlockPlayer)
									{
										while (currIndex.Y < 127)
										{
											if (!Block.GetType(this._blocks[index]).BlockPlayer)
											{
												break;
											}
											currIndex.Y++;
											index++;
										}
									}
									else
									{
										while (currIndex.Y > 1)
										{
											if (Block.GetType(this._blocks[index]).BlockPlayer)
											{
												currIndex.Y++;
												break;
											}
											currIndex.Y--;
											index--;
										}
									}
									IntVector3 walkIndex = currIndex;
									index = this.MakeIndex(walkIndex);
									while (walkIndex.Y < 128)
									{
										if (!Block.GetType(this._blocks[index]).BlockPlayer && (walkIndex.Y == 127 || !Block.GetType(this._blocks[index + 1]).BlockPlayer))
										{
											foundHeight = walkIndex.Y;
											break;
										}
										walkIndex.Y++;
										index++;
									}
									walkIndex = currIndex;
									walkIndex.Y--;
									index = this.MakeIndex(walkIndex);
									while (walkIndex.Y > 0)
									{
										if (!Block.GetType(this._blocks[index]).BlockPlayer && !Block.GetType(this._blocks[index + 1]).BlockPlayer)
										{
											if (currIndex.Y - walkIndex.Y < foundHeight - currIndex.Y)
											{
												foundHeight = walkIndex.Y;
												break;
											}
											break;
										}
										else
										{
											walkIndex.Y--;
											index--;
										}
									}
									if (foundHeight >= waterBlock)
									{
										goto Block_18;
									}
								}
							}
						}
					}
				}
				startIndex.X--;
				startIndex.Z--;
				squareSize += 2;
				if (squareSize > 256)
				{
					goto Block_19;
				}
			}
			Block_18:
			Vector3 result = this.MakePositionFromIndexVector(new IntVector3(currIndex.X, foundHeight, currIndex.Z));
			result.X += 0.5f;
			result.Y += 0.1f;
			result.Z += 0.5f;
			return result;
			Block_19:
			Vector3 result2 = this.MakePositionFromIndexVector(new IntVector3(startIndex.X, 128, startIndex.Z));
			result2.X += 0.5f;
			result2.Y += 0.1f;
			result2.Z += 0.5f;
			return result2;
		}

		public bool ContainsBlockType(Vector3 center, int radius, BlockTypeEnum type, ref float distToClosest)
		{
			IntVector3 centerIndex = this.MakeIndexVectorFromPosition(center);
			IntVector3 startIndex = new IntVector3(centerIndex.X - radius, 0, centerIndex.Z - radius);
			IntVector3 index = default(IntVector3);
			distToClosest = float.MaxValue;
			for (int z = 0; z < radius * 2; z++)
			{
				index.Z = startIndex.Z + z;
				if (index.Z >= 0 && index.Z < 384)
				{
					for (int x = 0; x < radius * 2; x++)
					{
						index.X = startIndex.X + x;
						if (index.X >= 0 && index.X < 384)
						{
							float dist = (float)(MathTools.Square(index.X - centerIndex.X) + MathTools.Square(index.Z - centerIndex.Z));
							if (dist < distToClosest)
							{
								int blockIndex = this.MakeIndex(index);
								int y = 0;
								while (y < 128)
								{
									if (Block.GetTypeIndex(this._blocks[blockIndex]) == type)
									{
										distToClosest = dist;
										break;
									}
									y++;
									blockIndex++;
								}
							}
						}
					}
				}
			}
			if (distToClosest != 3.4028235E+38f)
			{
				distToClosest = (float)Math.Sqrt((double)distToClosest);
				return true;
			}
			return false;
		}

		public bool IsInsideWorld(Vector3 r)
		{
			if (!this.IsReady)
			{
				return false;
			}
			IntVector3 chunkIV = this.GetChunkVectorIndex(r);
			return chunkIV.X < 24 && chunkIV.X >= 0 && chunkIV.Z < 24 && chunkIV.Z >= 0;
		}

		public bool RegionIsLoaded(Vector3 r)
		{
			if (!this.IsReady)
			{
				return false;
			}
			IntVector3 chunkIV = this.GetChunkVectorIndex(r);
			if (chunkIV.X >= 24 || chunkIV.X < 0 || chunkIV.Z >= 24 || chunkIV.Z < 0)
			{
				return false;
			}
			int ci = chunkIV.X + chunkIV.Z * 24;
			RenderChunk rc = this._chunks[ci].GetChunk();
			bool result = rc != null && rc.HasGeometry();
			rc.Release();
			return result;
		}

		public Vector3 FindTopmostGroundLocation(Vector3 guess)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 startIndex = this.MakeIndexVectorFromPosition(guess);
			startIndex.X = startIndex.X.Clamp(0, 383);
			startIndex.Y = 127;
			startIndex.Z = startIndex.Z.Clamp(0, 383);
			int squareSize = 1;
			int waterBlock = (this.IsWaterWorld ? ((int)Math.Floor((double)(64f + this.WaterLevel))) : (-1));
			IntVector3 currIndex;
			for (;;)
			{
				for (int sqx = startIndex.X; sqx < startIndex.X + squareSize; sqx++)
				{
					if (sqx >= 0)
					{
						if (sqx >= 384)
						{
							break;
						}
						for (int sqz = startIndex.Z; sqz < startIndex.Z + squareSize; sqz++)
						{
							if (sqz >= 0)
							{
								if (sqz >= 384)
								{
									break;
								}
								bool dothisone = false;
								if (sqx == startIndex.X || sqx == startIndex.X + squareSize - 1)
								{
									dothisone = true;
								}
								else if (sqz == startIndex.Z || sqz == startIndex.Z + squareSize - 1)
								{
									dothisone = true;
								}
								if (dothisone)
								{
									currIndex = new IntVector3(sqx, 127, sqz);
									int index = this.MakeIndex(currIndex);
									while (currIndex.Y > 0 && !Block.GetType(this._blocks[index]).BlockPlayer)
									{
										currIndex.Y--;
										index--;
									}
									currIndex.Y++;
									if (currIndex.Y >= waterBlock)
									{
										goto Block_10;
									}
								}
							}
						}
					}
				}
				startIndex.X--;
				startIndex.Z--;
				squareSize += 2;
				if (squareSize > 256)
				{
					goto Block_11;
				}
			}
			Block_10:
			Vector3 result = this.MakePositionFromIndexVector(currIndex);
			result.X += 0.5f;
			result.Y += 0.1f;
			result.Z += 0.5f;
			return result;
			Block_11:
			Vector3 result2 = this.MakePositionFromIndexVector(new IntVector3(startIndex.X, 128, startIndex.Z));
			result2.X += 0.5f;
			result2.Y += 0.1f;
			result2.Z += 0.5f;
			return result2;
		}

		public Vector3 ClipPositionToLoadedWorld(Vector3 pos, float radius)
		{
			if (this.IsReady)
			{
				Vector3 wm = IntVector3.ToVector3(this._worldMin);
				pos -= wm;
				pos.X = pos.X.Clamp(radius, 384f - radius);
				pos.Z = pos.Z.Clamp(radius, 384f - radius);
				pos += wm;
			}
			return pos;
		}

		public void StepInitialization()
		{
			if (this._updateTasksRemaining)
			{
				return;
			}
			IntVector3 chunkIdx;
			if (!this._buildTasksRemaining)
			{
				if (this.TryShiftTerrain())
				{
					return;
				}
				if (!this._allChunksLoaded)
				{
					if (this._loadingProgress != 0)
					{
						this._loadingProgress = 183;
					}
					int blockCount = 0;
					this._allChunksLoaded = true;
					chunkIdx = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
					for (int i = 0; i < this._radiusOrderOffsets.Length; i++)
					{
						IntVector3 newChunkIdx = IntVector3.Add(chunkIdx, this._radiusOrderOffsets[i]);
						if (newChunkIdx.X >= 0 && newChunkIdx.X < 24 && newChunkIdx.Z >= 0 && newChunkIdx.Z < 24)
						{
							int index = newChunkIdx.X + newChunkIdx.Z * 24;
							if (this._chunks[index]._action == BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
							{
								this._computeBlocksPool.Add(index);
								this.AddSurroundingBlocksToLightList(index);
								blockCount++;
								if (blockCount == this._maxChunksAtOnce)
								{
									this._maxChunksAtOnce = 8;
									this._allChunksLoaded = false;
									break;
								}
							}
						}
					}
					if (!this._computeBlocksPool.Empty)
					{
						this._computeBlocksPool.Drain();
					}
				}
			}
			chunkIdx = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
			for (int j = 0; j < this._radiusOrderOffsets.Length; j++)
			{
				IntVector3 newChunkIdx2 = IntVector3.Add(chunkIdx, this._radiusOrderOffsets[j]);
				if (newChunkIdx2.X >= 0 && newChunkIdx2.X < 24 && newChunkIdx2.Z >= 0 && newChunkIdx2.Z < 24)
				{
					int index2 = newChunkIdx2.X + newChunkIdx2.Z * 24;
					if (this._chunks[index2]._action == BlockTerrain.NextChunkAction.NONE && !this._chunks[index2]._mods.Empty && !this.ChunkOrNeighborInUse(index2) && this.ApplyModList(index2))
					{
						this.IncrChunkInUse(index2);
						BlockTerrain.BuildTaskData td = BlockTerrain.BuildTaskData.Alloc();
						td._intData0 = index2;
						this._updateTasksRemaining.Increment();
						TaskDispatcher.Instance.AddRushTask(this._finishSetBlockDelegate, td);
						return;
					}
				}
			}
		}

		protected void AddSurroundingBlocksToLightList(int index)
		{
			this._computeLightingPool.Add(index);
			this._chunks[index]._numUsers.Increment();
			IntVector3 chunkVector = this.MakeIndexVectorFromChunkIndex(index);
			for (int i = 0; i < 4; i++)
			{
				IntVector3 neighbor = chunkVector;
				switch (i)
				{
				case 0:
					neighbor.X += 16;
					break;
				case 1:
					neighbor.X -= 16;
					break;
				case 2:
					neighbor.Z += 16;
					break;
				case 3:
					neighbor.Z -= 16;
					break;
				}
				if (this.IsIndexValid(neighbor))
				{
					int ni = this.MakeChunkIndexFromIndexVector(neighbor);
					if (this._chunks[ni]._action >= BlockTerrain.NextChunkAction.NEEDS_GEOMETRY)
					{
						this._computeLightingPool.Add(ni);
						this._chunks[ni]._numUsers.Increment();
					}
				}
			}
		}

		protected void ApplyModListDuringCreate(int index)
		{
			SynchronizedQueue<BlockTerrain.PendingMod> q = this._chunks[index]._mods;
			while (!q.Empty)
			{
				BlockTerrain.PendingMod mod = q.Dequeue();
				IntVector3 localPosition = IntVector3.Subtract(mod._worldPosition, this._worldMin);
				int i = this.MakeIndex(localPosition);
				if (Block.GetType(this._blocks[i]).SpawnEntity)
				{
					this.RemoveItemBlockEntity(Block.GetTypeIndex(this._blocks[i]), mod._worldPosition);
				}
				if (BlockType.GetType(mod._blockType).SpawnEntity)
				{
					this.CreateItemBlockEntity(mod._blockType, mod._worldPosition);
				}
				this._blocks[i] = Block.SetType(0, mod._blockType);
				mod.Release();
			}
		}

		protected bool ApplyModList(int index)
		{
			bool result = false;
			SynchronizedQueue<BlockTerrain.PendingMod> q = this._chunks[index]._mods;
			this._updateLightingPool.Clear();
			this._updateLightingPool.ResetAABB();
			while (!q.Empty)
			{
				BlockTerrain.PendingMod mod = q.Dequeue();
				IntVector3 localPosition = IntVector3.Subtract(mod._worldPosition, this._worldMin);
				int i = this.MakeIndex(localPosition);
				if (Block.GetTypeIndex(this._blocks[i]) != mod._blockType)
				{
					if (Block.GetType(this._blocks[i]).SpawnEntity)
					{
						this.RemoveItemBlockEntity(Block.GetTypeIndex(this._blocks[i]), mod._worldPosition);
					}
					if (BlockType.GetType(mod._blockType).SpawnEntity)
					{
						this.CreateItemBlockEntity(mod._blockType, mod._worldPosition);
					}
					this._blocks[i] = Block.SetType(0, mod._blockType);
					this._updateLightingPool.UpdateMinAABB(ref localPosition);
					this.ResetSkyAndEmitterLightingForRegion(localPosition, localPosition);
					this.AddBlockToLightList(localPosition, this._updateLightingPool);
					result = true;
				}
				mod.Release();
			}
			return result;
		}

		protected void IncrChunkInUse(int index)
		{
			IntVector3 i = this.MakeIndexVectorFromChunkIndex(index);
			for (int x = -2; x < 3; x++)
			{
				IntVector3 j = i;
				j.X += x * 16;
				for (int z = -2; z < 3; z++)
				{
					j.Z += z * 128;
					if (this.IsIndexValid(j))
					{
						int ix = this.MakeChunkIndexFromIndexVector(j);
						if (this._chunks[ix]._action > BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
						{
							this._chunks[ix]._numUsers.Increment();
						}
					}
				}
			}
		}

		protected void DecrChunkInUse(int index)
		{
			IntVector3 i = this.MakeIndexVectorFromChunkIndex(index);
			for (int x = -2; x < 3; x++)
			{
				IntVector3 j = i;
				j.X += x * 16;
				for (int z = -2; z < 3; z++)
				{
					j.Z += z * 128;
					if (this.IsIndexValid(j))
					{
						int ix = this.MakeChunkIndexFromIndexVector(j);
						if (this._chunks[ix]._action > BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
						{
							this._chunks[ix]._numUsers.Decrement();
						}
					}
				}
			}
		}

		protected bool ChunkOrNeighborInUse(int index)
		{
			IntVector3 i = this.MakeIndexVectorFromChunkIndex(index);
			for (int x = -2; x < 3; x++)
			{
				IntVector3 j = i;
				j.X += x * 16;
				for (int z = -2; z < 3; z++)
				{
					j.Z += z * 128;
					if (this.IsIndexValid(j))
					{
						int ix = this.MakeChunkIndexFromIndexVector(j);
						if (this._chunks[ix]._numUsers)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public void GlobalUpdate(GameTime gameTime)
		{
			if (this.IsReady)
			{
				if (!CastleMinerZGame.Instance.IsActive)
				{
					this.BuildPendingVertexBuffers();
				}
				this.StepInitialization();
				ChunkCache.Instance.Update(gameTime);
				return;
			}
			if (this._asyncInitData != null && this.ReadyForInit)
			{
				if (this._asyncInitData.teleporting)
				{
					this.InternalTeleport(this._asyncInitData.center);
				}
				else
				{
					this.Init(this._asyncInitData.center, this._asyncInitData.worldInfo, this._asyncInitData.host);
					if (this._asyncInitData.callback != null)
					{
						this._asyncInitData.callback(null);
					}
				}
				this._asyncInitData = null;
			}
		}

		public bool TryShiftTerrain()
		{
			if (this._shiftTerrainData.running)
			{
				return this.FinishShiftTerrain();
			}
			IntVector3 index = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
			index.X -= 12;
			index.Z -= 12;
			if (index.X < 0)
			{
				index.X++;
			}
			if (index.Z < 0)
			{
				index.Z++;
			}
			return (index.X != 0 || index.Z != 0) && this.ShiftTerrain(index.X, index.Z);
		}

		public void CenterOn(Vector3 eye, bool scrollIfPossible)
		{
			if (!this.IsReady)
			{
				return;
			}
			IntVector3 newVector = this.GetChunkVectorIndex(eye);
			if (scrollIfPossible)
			{
				bool needFullScroll = false;
				if (newVector.X < 0 || newVector.X >= 24 || newVector.Z < 0 || newVector.Z >= 24)
				{
					needFullScroll = true;
				}
				else
				{
					IntVector3 oldVector = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
					IntVector3 diff = IntVector3.Subtract(newVector, oldVector);
					if (Math.Abs(diff.X) > 1 || Math.Abs(diff.Z) > 1)
					{
						int emptyChunkCount = 0;
						for (int i = 0; i < 25; i++)
						{
							IntVector3 newChunkIdx = IntVector3.Add(newVector, this._radiusOrderOffsets[i]);
							if (newChunkIdx.X >= 0 && newChunkIdx.X < 24 && newChunkIdx.Z >= 0 && newChunkIdx.Z < 24)
							{
								int index = newChunkIdx.X + newChunkIdx.Z * 24;
								if (this._chunks[index]._action != BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
								{
									emptyChunkCount++;
								}
							}
						}
						if (emptyChunkCount < 25)
						{
							needFullScroll = true;
						}
					}
				}
				if (needFullScroll)
				{
					this.Teleport(eye);
					return;
				}
			}
			newVector.X = newVector.X.Clamp(0, 23);
			newVector.Z = newVector.Z.Clamp(0, 23);
			Interlocked.Exchange(ref this._currentEyeChunkIndex, newVector.X + newVector.Z * 24);
			IntVector3 centerIndex = newVector;
			centerIndex.X = (centerIndex.X - 12).Clamp(-1, 0);
			centerIndex.Z = (centerIndex.Z - 12).Clamp(-1, 0);
			Interlocked.Exchange(ref this._currentRenderOrder, Math.Abs(centerIndex.Z * 2 + centerIndex.X));
		}

		public void SetCursor(bool draw, IntVector3 worldIndex, BlockFace face)
		{
			if (!this.IsReady)
			{
				return;
			}
			this._drawCursor = draw;
			if (!draw)
			{
				return;
			}
			this._cursorPosition = worldIndex;
			this._cursorFace = face;
		}

		public void SetCursor(bool draw, Vector3 position, Vector3 normal)
		{
			if (!this.IsReady)
			{
				return;
			}
			this._drawCursor = draw;
			if (!draw)
			{
				return;
			}
			this._cursorPosition = IntVector3.Add(this._worldMin, this.MakeIndexVectorFromPosition(position));
			Vector3 abs = new Vector3(Math.Abs(normal.X), Math.Abs(normal.Y), Math.Abs(normal.Z));
			if (abs.X > abs.Y)
			{
				if (abs.X > abs.Z)
				{
					if (normal.X > 0f)
					{
						this._cursorFace = BlockFace.POSX;
						return;
					}
					this._cursorFace = BlockFace.NEGX;
					return;
				}
				else
				{
					if (normal.Z > 0f)
					{
						this._cursorFace = BlockFace.POSZ;
						return;
					}
					this._cursorFace = BlockFace.NEGZ;
					return;
				}
			}
			else if (abs.Y > abs.Z)
			{
				if (normal.Y > 0f)
				{
					this._cursorFace = BlockFace.POSY;
					return;
				}
				this._cursorFace = BlockFace.NEGY;
				return;
			}
			else
			{
				if (normal.Z > 0f)
				{
					this._cursorFace = BlockFace.POSZ;
					return;
				}
				this._cursorFace = BlockFace.NEGZ;
				return;
			}
		}

		public void GetAvatarColor(Vector3 position, out Vector3 ambient, out Vector3 directional, out Vector3 direction)
		{
			Vector2 light = this.GetLightAtPoint(position);
			float totalTorch = light.Y;
			float totalSun = light.X;
			direction = -this.VectorToSun;
			ambient = Vector3.Multiply(this.AmbientSunColor.ToVector3(), totalSun) + Vector3.Multiply(this.TorchColor.ToVector3(), totalTorch * (1f - totalSun * this.SunlightColor.ToVector3().Y));
			directional = Vector3.Multiply(this.SunlightColor.ToVector3(), (float)Math.Pow((double)totalSun, 30.0));
		}

		public void GetEnemyLighting(Vector3 position, ref Vector3 l1d, ref Vector3 l1c, ref Vector3 l2d, ref Vector3 l2c, ref Vector3 ambient)
		{
			Vector2 light = this.GetLightAtPoint(position);
			float totalTorch = light.Y;
			float totalSun = light.X;
			ambient = Vector3.Multiply(this.AmbientSunColor.ToVector3(), totalSun) + Vector3.Multiply(this.TorchColor.ToVector3(), 0.5f * totalTorch * (1f - totalSun * this.SunlightColor.ToVector3().Y));
			l1d = Vector3.Negate(this.VectorToSun);
			l2d = position - this.EyePos;
			float dsq = l2d.LengthSquared();
			if (dsq > 0f)
			{
				l2d *= 1f / (float)Math.Sqrt((double)dsq);
			}
			l1c = Vector3.Multiply(this.SunlightColor.ToVector3(), (float)Math.Pow((double)totalSun, 30.0));
			l2c = Vector3.Multiply(this.TorchColor.ToVector3(), 0.5f * totalTorch * (1f - totalSun * this.SunlightColor.ToVector3().Y));
		}

		public Vector2 GetLightAtPoint(Vector3 position)
		{
			IntVector3 ip = IntVector3.FromVector3(position);
			this.FillCubeLightTable(ip, ref this.avatarSun, ref this.avatarTorch);
			Vector3 fip = IntVector3.ToVector3(ip);
			Vector3 offset = Vector3.Zero;
			float torchPower = 0f;
			float sunPower = 0f;
			float totalTorch = 0f;
			float totalSun = 0f;
			int index = 0;
			offset.Z = -1f;
			while (offset.Z < 1.5f)
			{
				offset.Y = -1f;
				while (offset.Y < 1.5f)
				{
					offset.X = -1f;
					while (offset.X < 1.5f)
					{
						float power = ((2.25f - (fip + offset - position).LengthSquared()) / 2.25f).Clamp(0f, 1f);
						if (this.avatarTorch[index] != -1f)
						{
							totalTorch += power * this.avatarTorch[index];
							torchPower += power;
						}
						if (this.avatarSun[index] != -1f)
						{
							totalSun += power * this.avatarSun[index];
							sunPower += power;
						}
						offset.X += 1f;
						index++;
					}
					offset.Y += 1f;
				}
				offset.Z += 1f;
			}
			if (torchPower > 0f)
			{
				totalTorch /= torchPower;
			}
			if (sunPower > 0f)
			{
				totalSun /= sunPower;
			}
			return new Vector2(totalSun, totalTorch);
		}

		public Vector2 GetSimpleLightAtPoint(Vector3 position)
		{
			Vector2 result = Vector2.Zero;
			IntVector3 ip = IntVector3.FromVector3(position);
			IntVector3 a = IntVector3.Subtract(ip, this._worldMin);
			if (this.IsIndexValid(a))
			{
				int nBlock = this.GetBlockAt(a);
				BlockTypeEnum b = Block.GetTypeIndex(nBlock);
				if (b == BlockTypeEnum.NumberOfBlocks || Block.IsInList(nBlock))
				{
					result.X = 1f;
					result.Y = 0f;
				}
				else if (BlockType.GetType(b).Opaque)
				{
					result.X = -1f;
					result.Y = -1f;
				}
				else
				{
					result.X = (float)Block.GetSunLightLevel(nBlock) / 15f;
					result.Y = (float)Block.GetTorchLightLevel(nBlock) / 15f;
				}
			}
			return result;
		}

		public float GetSimpleSunlightAtPoint(Vector3 position)
		{
			float result = 0f;
			IntVector3 ip = IntVector3.FromVector3(position);
			IntVector3 a = IntVector3.Subtract(ip, this._worldMin);
			if (this.IsIndexValid(a))
			{
				int nBlock = this.GetBlockAt(a);
				BlockTypeEnum b = Block.GetTypeIndex(nBlock);
				if (b == BlockTypeEnum.NumberOfBlocks || Block.IsInList(nBlock))
				{
					result = 1f;
				}
				else if (BlockType.GetType(b).Opaque)
				{
					result = -1f;
				}
				else
				{
					result = (float)Block.GetSunLightLevel(nBlock) / 15f;
				}
			}
			return result;
		}

		public float GetSimpleTorchlightAtPoint(Vector3 position)
		{
			float result = 0f;
			IntVector3 ip = IntVector3.FromVector3(position);
			IntVector3 a = IntVector3.Subtract(ip, this._worldMin);
			if (this.IsIndexValid(a))
			{
				int nBlock = this.GetBlockAt(a);
				BlockTypeEnum b = Block.GetTypeIndex(nBlock);
				if (b == BlockTypeEnum.NumberOfBlocks || Block.IsInList(nBlock))
				{
					result = 1f;
				}
				else if (BlockType.GetType(b).Opaque)
				{
					result = -1f;
				}
				else
				{
					result = (float)Block.GetTorchLightLevel(nBlock) / 15f;
				}
			}
			return result;
		}

		public void CreateItemBlockEntity(BlockTypeEnum blockType, IntVector3 location)
		{
			BlockTerrain.ItemBlockCommand ibc = BlockTerrain.ItemBlockCommand.Alloc();
			ibc.AddItem = true;
			ibc.BlockType = blockType;
			ibc.WorldPosition = location;
			this.ItemBlockCommandQueue.Queue(ibc);
		}

		public void RemoveItemBlockEntity(BlockTypeEnum blockType, IntVector3 location)
		{
			BlockTerrain.ItemBlockCommand ibc = BlockTerrain.ItemBlockCommand.Alloc();
			ibc.AddItem = false;
			ibc.WorldPosition = location;
			ibc.BlockType = blockType;
			this.ItemBlockCommandQueue.Queue(ibc);
		}

		public bool SetBlock(IntVector3 worldIndex, BlockTypeEnum type)
		{
			ChunkCacheCommand command = ChunkCacheCommand.Alloc();
			command._command = ChunkCacheCommandEnum.MOD;
			command._worldPosition = worldIndex;
			command._blockType = type;
			command._priority = 1;
			ChunkCache.Instance.AddCommand(command);
			if (!this.IsReady)
			{
				return false;
			}
			IntVector3 localPosition = IntVector3.Subtract(worldIndex, this._worldMin);
			if (this.IsIndexValid(localPosition))
			{
				int cidx = this.MakeChunkIndexFromIndexVector(localPosition);
				lock (this._chunks[cidx]._mods)
				{
					for (BlockTerrain.PendingMod pm = this._chunks[cidx]._mods.Front; pm != null; pm = (BlockTerrain.PendingMod)pm.NextNode)
					{
						if (pm._worldPosition.Equals(worldIndex))
						{
							pm._blockType = type;
							return false;
						}
					}
				}
				if (this._chunks[cidx]._action > BlockTerrain.NextChunkAction.COMPUTING_BLOCKS && Block.GetTypeIndex(this._blocks[this.MakeIndex(localPosition)]) == type)
				{
					return false;
				}
				BlockTerrain.PendingMod mod = BlockTerrain.PendingMod.Alloc();
				mod._worldPosition = worldIndex;
				mod._blockType = type;
				this._chunks[cidx]._mods.Queue(mod);
				return true;
			}
			return false;
		}

		public bool CursorVisible
		{
			get
			{
				return this._drawCursor;
			}
		}

		public Vector3 GetActualWaterColor()
		{
			Vector3 actualWaterColor = this.BelowWaterColor.ToVector3();
			return actualWaterColor * this.SunlightColor.ToVector3();
		}

		public Matrix GetReflectionMatrix()
		{
			if (this.EyePos.Y < this.WaterLevel)
			{
				return Matrix.Identity;
			}
			return Matrix.CreateReflection(new Plane(0f, 1f, 0f, -this.WaterLevel));
		}

		public float GetUnderwaterSkyTint(out Vector3 color)
		{
			if (!this.IsWaterWorld)
			{
				color = Vector3.Zero;
				return 0f;
			}
			float result = this.WaterLevel - this.EyePos.Y;
			if (result < 0f)
			{
				result = 0f;
			}
			else
			{
				result = Math.Min(result / 12f, 1f - this.SunlightColor.ToVector3().X * this.SunlightColor.ToVector3().X);
			}
			color = this.GetActualWaterColor();
			return result;
		}

		public void DrawReflection(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (!this.IsReady)
			{
				return;
			}
			this._updateTasksRemaining.Increment();
			this._boundingFrustum.Matrix = view * projection;
			Matrix reflection = this.GetReflectionMatrix();
			this._effect.Parameters["Projection"].SetValue(projection);
			this._effect.Parameters["World"].SetValue(Matrix.Identity);
			this._effect.Parameters["View"].SetValue(view);
			this._effect.Parameters["WaterLevel"].SetValue(this.WaterLevel - 0.5f);
			this._effect.Parameters["EyePosition"].SetValue(Vector3.Transform(this.EyePos, reflection));
			this._effect.Parameters["LightDirection"].SetValue(this.VectorToSun);
			this._effect.Parameters["TorchLight"].SetValue(this.TorchColor.ToVector3());
			this._effect.Parameters["SunLight"].SetValue(this.SunlightColor.ToVector3());
			this._effect.Parameters["AmbientSun"].SetValue(this.AmbientSunColor.ToVector3());
			this._effect.Parameters["SunSpecular"].SetValue(this.SunSpecular.ToVector3());
			this._effect.Parameters["FogColor"].SetValue(this.FogColor.ToVector3());
			device.BlendState = BlendState.AlphaBlend;
			device.Indices = this._staticIB;
			int chunkCount = 0;
			IntVector3 chunkIdx = this.GetChunkVectorIndex(this.EyePos);
			for (int i = 0; i < this._radiusOrderOffsets.Length; i++)
			{
				IntVector3 newChunkIdx = IntVector3.Add(chunkIdx, this._radiusOrderOffsets[i]);
				if (newChunkIdx.X >= 0 && newChunkIdx.X < 24 && newChunkIdx.Z >= 0 && newChunkIdx.Z < 24)
				{
					int index = newChunkIdx.X + newChunkIdx.Z * 24;
					RenderChunk chunk = this._chunks[index].GetChunk();
					if (chunk.TouchesFrustum(this._boundingFrustum))
					{
						this._renderIndexList[chunkCount++] = index;
					}
					chunk.Release();
				}
			}
			if (chunkCount > 0)
			{
				this._effect.CurrentTechnique = this._effect.Techniques[3];
				device.BlendState = BlendState.Opaque;
				if (this.EyePos.Y >= this.WaterLevel)
				{
					device.RasterizerState = RasterizerState.CullClockwise;
				}
				for (int j = 0; j < chunkCount; j++)
				{
					RenderChunk chunk2 = this._chunks[this._renderIndexList[j]].GetChunk();
					chunk2.DrawReflection(this._graphicsDevice, this._effect, this._boundingFrustum);
					chunk2.Release();
				}
				device.RasterizerState = RasterizerState.CullCounterClockwise;
				device.BlendState = BlendState.NonPremultiplied;
			}
			this._updateTasksRemaining.Decrement();
		}

		private void ClearVertexBuildLists()
		{
			lock (this._cleanupVertexBuildListLockObject)
			{
				lock (this._vertexBuildListLockObject)
				{
					for (int i = 0; i < 2; i++)
					{
						List<BlockTerrain.QueuedBufferBuild> list = ((i == 0) ? this._vertexBuildListIncoming : this._vertexBuildListOutgoing);
						for (int j = 0; j < list.Count; j++)
						{
							BlockTerrain.QueuedBufferBuild qbb = list[j];
							qbb.Chunk.SkipBuildingBuffers();
							qbb.Chunk.Release();
							this._chunks[qbb.Index]._action = BlockTerrain.NextChunkAction.NONE;
							this._chunks[qbb.Index]._numUsers.Decrement();
						}
						list.Clear();
					}
				}
			}
		}

		public void BuildPendingVertexBuffers()
		{
			lock (this._cleanupVertexBuildListLockObject)
			{
				lock (this._vertexBuildListLockObject)
				{
					List<BlockTerrain.QueuedBufferBuild> tmp = this._vertexBuildListIncoming;
					this._vertexBuildListIncoming = this._vertexBuildListOutgoing;
					this._vertexBuildListOutgoing = tmp;
				}
				for (int i = 0; i < this._vertexBuildListOutgoing.Count; i++)
				{
					BlockTerrain.QueuedBufferBuild qbb = this._vertexBuildListOutgoing[i];
					qbb.Chunk.FinishBuildingBuffers(this._graphicsDevice);
					this._chunks[qbb.Index].ReplaceChunk(qbb.Chunk);
					qbb.Chunk.Release();
					qbb.Chunk = null;
					this._chunks[qbb.Index]._action = BlockTerrain.NextChunkAction.NONE;
					this._chunks[qbb.Index]._numUsers.Decrement();
					this.AddToLoadingProgress(1);
				}
				this._vertexBuildListOutgoing.Clear();
			}
		}

		public float DrawDistance
		{
			get
			{
				return this._drawDistance;
			}
			set
			{
				float toset = value.Clamp(0f, 1f);
				if (value != this._drawDistance)
				{
					int i = (int)Math.Floor((double)(8f * toset)) + 4;
					this._farthestDrawDistanceSQ = i * i;
					this._drawDistance = value;
				}
			}
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (CastleMinerZGame.Instance.DrawingReflection && this.EyePos.Y >= this.WaterLevel)
			{
				this.DrawReflection(device, gameTime, view, projection);
				return;
			}
			using (Profiler.TimeSection("Drawing Terrain", ProfilerThreadEnum.MAIN))
			{
				if (this.IsReady)
				{
					this._updateTasksRemaining.Increment();
					this._boundingFrustum.Matrix = view * projection;
					this._effect.Parameters["Projection"].SetValue(projection);
					this._effect.Parameters["World"].SetValue(Matrix.Identity);
					this._effect.Parameters["View"].SetValue(view);
					this._effect.Parameters["EyePosition"].SetValue(this.EyePos);
					this._effect.Parameters["WaterDepth"].SetValue(this._worldBuilder.WaterDepth);
					this._effect.Parameters["WaterLevel"].SetValue(this.WaterLevel);
					Vector3 waterCalculations = default(Vector3);
					waterCalculations.Z = this.WaterLevel - this.EyePos.Y;
					if (waterCalculations.Z >= 0f)
					{
						waterCalculations.X = waterCalculations.Z;
						waterCalculations.Y = 100000f;
					}
					else
					{
						waterCalculations.X = 0f;
						waterCalculations.Y = 1f;
						waterCalculations.Z = -waterCalculations.Z;
					}
					this._effect.Parameters["EyeWaterConstants"].SetValue(waterCalculations);
					this._effect.Parameters["TorchLight"].SetValue(this.TorchColor.ToVector3());
					this._effect.Parameters["LightDirection"].SetValue(this.VectorToSun);
					if (CastleMinerZGame.Instance.GameScreen._sky.drawLightning)
					{
						float daylight = (float)this.AmbientSunColor.G / 255f;
						Vector3 light = Vector3.Lerp(new Vector3(0.9f, 0.95f, 1f), this.SunlightColor.ToVector3(), daylight);
						this._effect.Parameters["SunLight"].SetValue(light);
						light = Vector3.Lerp(new Vector3(0.9f, 0.95f, 1f), this.AmbientSunColor.ToVector3(), daylight);
						this._effect.Parameters["AmbientSun"].SetValue(light);
					}
					else
					{
						this._effect.Parameters["SunLight"].SetValue(this.SunlightColor.ToVector3());
						this._effect.Parameters["AmbientSun"].SetValue(this.AmbientSunColor.ToVector3());
					}
					this._effect.Parameters["SunSpecular"].SetValue(this.SunSpecular.ToVector3());
					this._effect.Parameters["FogColor"].SetValue(this.FogColor.ToVector3());
					this._effect.Parameters["BelowWaterColor"].SetValue(this.GetActualWaterColor());
					device.Indices = this._staticIB;
					int chunkCount = 0;
					IntVector3 chunkIdx = this.GetChunkVectorIndex(this.EyePos);
					for (int i = 0; i < this._radiusOrderOffsets.Length; i++)
					{
						IntVector3 offset = this._radiusOrderOffsets[i];
						if (offset.X * offset.X < this._farthestDrawDistanceSQ && offset.Y * offset.Y < this._farthestDrawDistanceSQ && offset.Z * offset.Z < this._farthestDrawDistanceSQ)
						{
							IntVector3 newChunkIdx = IntVector3.Add(chunkIdx, offset);
							if (newChunkIdx.X >= 0 && newChunkIdx.X < 24 && newChunkIdx.Z >= 0 && newChunkIdx.Z < 24)
							{
								int index = newChunkIdx.X + newChunkIdx.Z * 24;
								RenderChunk chunk = this._chunks[index].GetChunk();
								if (chunk.TouchesFrustum(this._boundingFrustum))
								{
									this._renderIndexList[chunkCount++] = index;
								}
								chunk.Release();
							}
						}
					}
					if (chunkCount > 0)
					{
						if (CastleMinerZGame.Instance.DrawingReflection)
						{
							this._effect.CurrentTechnique = this._effect.Techniques[1];
						}
						else
						{
							this._effect.CurrentTechnique = this._effect.Techniques[this.UseSimpleShader ? 3 : 2];
						}
						device.BlendState = BlendState.Opaque;
						for (int fancy = 0; fancy < 2; fancy++)
						{
							for (int j = 0; j < chunkCount; j++)
							{
								RenderChunk chunk2 = this._chunks[this._renderIndexList[j]].GetChunk();
								chunk2.Draw(this._graphicsDevice, this._effect, fancy == 1, this._boundingFrustum);
								chunk2.Release();
							}
						}
						device.BlendState = BlendState.NonPremultiplied;
					}
					this._updateTasksRemaining.Decrement();
				}
			}
		}

		public const int BADINDEX = -1;

		public const int MAXX = 384;

		public const int MAXY = 128;

		public const int MAXZ = 384;

		public const int BUFFER_SIZE = 18874368;

		public const float BLOCKSIZE = 1f;

		public const int CHUNK_WIDTH = 16;

		public const int CHUNK_DEPTH = 16;

		public const int CHUNK_HEIGHT = 128;

		public const int MAX_CHUNK_X = 24;

		public const int MAX_CHUNK_Z = 24;

		public const int NUM_CHUNKS = 576;

		public const int NUM_CHUNKS_IN_LOAD = 64;

		public const int MAX_LOADING_COUNT = 183;

		public const float TILES_PER_ROW = 8f;

		public const float AO_TILES_PER_ROW = 16f;

		private const int LIGHT_BUFFER_SIZE = 262144;

		private const int UPDATE_LIGHT_BUFFER_SIZE = 7500;

		protected static BlockTerrain _theTerrain = null;

		public static readonly IntVector3[] _faceNeighbors = new IntVector3[]
		{
			new IntVector3(1, 0, 0),
			new IntVector3(0, 0, -1),
			new IntVector3(-1, 0, 0),
			new IntVector3(0, 0, 1),
			new IntVector3(0, 1, 0),
			new IntVector3(0, -1, 0)
		};

		private static readonly int[] _faceIndexNeighbors = new int[] { 128, -49152, -128, 49152, 1, -1 };

		private static readonly IntVector3[][] _lightNeighbors = new IntVector3[][]
		{
			new IntVector3[]
			{
				new IntVector3(1, 1, 1),
				new IntVector3(1, 1, 0),
				new IntVector3(1, 1, -1),
				new IntVector3(1, 0, 1),
				new IntVector3(1, 0, 0),
				new IntVector3(1, 0, -1),
				new IntVector3(1, -1, 1),
				new IntVector3(1, -1, 0),
				new IntVector3(1, -1, -1)
			},
			new IntVector3[]
			{
				new IntVector3(1, 1, -1),
				new IntVector3(0, 1, -1),
				new IntVector3(-1, 1, -1),
				new IntVector3(1, 0, -1),
				new IntVector3(0, 0, -1),
				new IntVector3(-1, 0, -1),
				new IntVector3(1, -1, -1),
				new IntVector3(0, -1, -1),
				new IntVector3(-1, -1, -1)
			},
			new IntVector3[]
			{
				new IntVector3(-1, 1, -1),
				new IntVector3(-1, 1, 0),
				new IntVector3(-1, 1, 1),
				new IntVector3(-1, 0, -1),
				new IntVector3(-1, 0, 0),
				new IntVector3(-1, 0, 1),
				new IntVector3(-1, -1, -1),
				new IntVector3(-1, -1, 0),
				new IntVector3(-1, -1, 1)
			},
			new IntVector3[]
			{
				new IntVector3(-1, 1, 1),
				new IntVector3(0, 1, 1),
				new IntVector3(1, 1, 1),
				new IntVector3(-1, 0, 1),
				new IntVector3(0, 0, 1),
				new IntVector3(1, 0, 1),
				new IntVector3(-1, -1, 1),
				new IntVector3(0, -1, 1),
				new IntVector3(1, -1, 1)
			},
			new IntVector3[]
			{
				new IntVector3(-1, 1, -1),
				new IntVector3(0, 1, -1),
				new IntVector3(1, 1, -1),
				new IntVector3(-1, 1, 0),
				new IntVector3(0, 1, 0),
				new IntVector3(1, 1, 0),
				new IntVector3(-1, 1, 1),
				new IntVector3(0, 1, 1),
				new IntVector3(1, 1, 1)
			},
			new IntVector3[]
			{
				new IntVector3(1, -1, -1),
				new IntVector3(0, -1, -1),
				new IntVector3(-1, -1, -1),
				new IntVector3(1, -1, 0),
				new IntVector3(0, -1, 0),
				new IntVector3(-1, -1, 0),
				new IntVector3(1, -1, 1),
				new IntVector3(0, -1, 1),
				new IntVector3(-1, -1, 1)
			}
		};

		private Plane[] _facePlanes = new Plane[]
		{
			new Plane(1f, 0f, 0f, 0f),
			new Plane(0f, 0f, -1f, 0f),
			new Plane(-1f, 0f, 0f, 0f),
			new Plane(0f, 0f, 1f, 0f),
			new Plane(0f, 1f, 0f, 0f),
			new Plane(0f, -1f, 0f, 0f)
		};

		public int[] _blocks;

		private int[] _shiftedBlocks;

		public BlockTerrain.TerrainChunk[] _chunks;

		public int[] _renderIndexList;

		public int _currentEyeChunkIndex;

		public int _currentRenderOrder;

		public IntVector3[] _radiusOrderOffsets;

		public IntVector3 _worldMin;

		public GraphicsDevice _graphicsDevice;

		public Effect _effect;

		public Texture2D _diffuseAlpha;

		public Texture2D _normalSpec;

		public Texture2D _metalLight;

		public Texture3D _envMap;

		public Texture2D _mipMapNormals;

		public Texture2D _mipMapDiffuse;

		public BoundingFrustum _boundingFrustum;

		private CountdownLatch _buildTasksRemaining;

		private CountdownLatch _updateTasksRemaining;

		public TaskDelegate _computeGeometryDelegate;

		public TaskDelegate _computeBlocksDelegate;

		public TaskDelegate _computeLightingDelegate;

		public TaskDelegate _stepUpdateDelegate;

		public TaskDelegate _finishSetBlockDelegate;

		public TaskDelegate _finishRegionOpDelegate;

		public TaskDelegate _shiftTerrainDelegate;

		public BlockTerrain.ShiftingTerrainData _shiftTerrainData;

		public DepthStencilState _zWriteDisable;

		public DepthStencilState _zWriteEnable;

		public BlendState _disableColorWrites;

		public BlendState _enableColorWrites;

		public IndexBuffer _staticIB;

		private int initblock = Block.IsUninitialized(Block.SetType(0, BlockTypeEnum.NumberOfBlocks), true);

		private BlockTerrain.LoadChunkActionPool _computeBlocksPool;

		private BlockTerrain.ChunkActionPool _computeLightingPool;

		private BlockTerrain.ChunkActionPool _computeGeometryPool;

		private BlockTerrain.LightingPool _mainLightingPool;

		private BlockTerrain.LightingPool _updateLightingPool;

		public IntVector3 _cursorPosition;

		public BlockFace _cursorFace;

		public bool _drawCursor;

		private bool _allChunksLoaded;

		private bool _initted;

		public bool _resetRequested = true;

		public Matrix[] _faceMatrices;

		public Vector4[] _vertexUVs;

		private int _loadingProgress;

		private int _maxChunksAtOnce = 64;

		public bool IsWaterWorld;

		private BlockTerrain.AsynchInitData _asyncInitData;

		public SynchronizedQueue<BlockTerrain.ItemBlockCommand> ItemBlockCommandQueue = new SynchronizedQueue<BlockTerrain.ItemBlockCommand>();

		public WorldInfo WorldInfo;

		public WorldBuilder _worldBuilder;

		private int maxLightNodes;

		private float[] avatarSun = new float[27];

		private float[] avatarTorch = new float[27];

		public Vector3 EyePos = new Vector3(0f, 64f, 0f);

		public Vector3 ViewVector = new Vector3(1f, 0f, 0f);

		public Vector3 VectorToSun = new Vector3(1f, 0f, 0f);

		public Color TorchColor = new Color(255, 235, 190);

		public Color SunlightColor = new Color(1f, 1f, 1f);

		public Color SunSpecular = new Color(1, 1, 1);

		public Color FogColor = new Color(0.6f, 0.6f, 1f);

		public Color AmbientSunColor = new Color(1, 1, 1);

		public Color BelowWaterColor = new Color(0.0941f, 0.16f, 0.2235f);

		public float WaterLevel = -0.5f;

		public float PercentMidnight;

		public bool UseSimpleShader;

		private object _vertexBuildListLockObject = new object();

		private object _cleanupVertexBuildListLockObject = new object();

		private List<BlockTerrain.QueuedBufferBuild> _vertexBuildListIncoming = new List<BlockTerrain.QueuedBufferBuild>();

		private List<BlockTerrain.QueuedBufferBuild> _vertexBuildListOutgoing = new List<BlockTerrain.QueuedBufferBuild>();

		private int _farthestDrawDistanceSQ = 10000;

		private float _drawDistance = 1f;

		private class AsynchInitData
		{
			public Vector3 center;

			public WorldInfo worldInfo;

			public bool host;

			public AsyncCallback callback;

			public bool teleporting;
		}

		public class ItemBlockCommand : IReleaseable, ILinkedListNode
		{
			public static BlockTerrain.ItemBlockCommand Alloc()
			{
				return BlockTerrain.ItemBlockCommand._cache.Get();
			}

			public static void ReleaseList(BlockTerrain.ItemBlockCommand head)
			{
				BlockTerrain.ItemBlockCommand._cache.PutList(head);
			}

			public void Release()
			{
				BlockTerrain.ItemBlockCommand._cache.Put(this);
			}

			public ILinkedListNode NextNode
			{
				get
				{
					return this._nextNode;
				}
				set
				{
					this._nextNode = value;
				}
			}

			public bool AddItem;

			public IntVector3 WorldPosition = IntVector3.Zero;

			public BlockTypeEnum BlockType;

			private static ObjectCache<BlockTerrain.ItemBlockCommand> _cache = new ObjectCache<BlockTerrain.ItemBlockCommand>();

			private ILinkedListNode _nextNode;
		}

		public class BuildTaskData : IReleaseable, ILinkedListNode
		{
			public BuildTaskData()
			{
				this._intVec0 = IntVector3.Zero;
				this._intVec1 = IntVector3.Zero;
				this._intData0 = 0;
				this._intData1 = 0;
				this._skipProcessing = false;
			}

			public static BlockTerrain.BuildTaskData Alloc()
			{
				return BlockTerrain.BuildTaskData._cache.Get();
			}

			public void Release()
			{
				this._skipProcessing = false;
				BlockTerrain.BuildTaskData._cache.Put(this);
			}

			public ILinkedListNode NextNode
			{
				get
				{
					return this._nextNode;
				}
				set
				{
					this._nextNode = value;
				}
			}

			public IntVector3 _intVec0;

			public IntVector3 _intVec1;

			public int _intData0;

			public int _intData1;

			public bool _skipProcessing;

			private static ObjectCache<BlockTerrain.BuildTaskData> _cache = new ObjectCache<BlockTerrain.BuildTaskData>();

			private ILinkedListNode _nextNode;
		}

		public class ShiftingTerrainData
		{
			public int source;

			public int dest;

			public int length;

			public int fillStart;

			public int fillLength;

			public int dx;

			public int dz;

			public bool running;

			public bool done = true;
		}

		public class BlockReference : IReleaseable, ILinkedListNode
		{
			public BlockReference()
			{
				Interlocked.Increment(ref BlockTerrain.BlockReference._refcount);
				this._index = -1;
				this._vecIndex = IntVector3.Zero;
				this._nextNode = null;
			}

			public bool SetIndex(int x, int y, int z)
			{
				return this.SetIndex(new IntVector3(x, y, z));
			}

			public void SetIndex(int x, int y, int z, int index)
			{
				this._vecIndex.SetValues(x, y, z);
				this._index = index;
			}

			public void SetIndex(IntVector3 vi, int index)
			{
				this._vecIndex = vi;
				this._index = index;
			}

			public bool SetIndex(IntVector3 vi)
			{
				this._vecIndex = vi;
				if (BlockTerrain._theTerrain.IsIndexValid(this._vecIndex))
				{
					this._index = BlockTerrain._theTerrain.MakeIndex(this._vecIndex);
					return true;
				}
				this._index = -1;
				return false;
			}

			public int Get()
			{
				return BlockTerrain._theTerrain.GetBlockAt(this._index);
			}

			public void Set(int value)
			{
				BlockTerrain._theTerrain.SetBlockAt(this._index, value);
			}

			public bool IsValid
			{
				get
				{
					return this._index != -1;
				}
			}

			public static BlockTerrain.BlockReference Alloc(int x, int y, int z)
			{
				BlockTerrain.BlockReference result = BlockTerrain.BlockReference.Alloc();
				result.SetIndex(x, y, z);
				return result;
			}

			public static BlockTerrain.BlockReference Alloc(IntVector3 i)
			{
				BlockTerrain.BlockReference result = BlockTerrain.BlockReference.Alloc();
				result.SetIndex(i);
				return result;
			}

			public static BlockTerrain.BlockReference Alloc(Vector3 v)
			{
				return BlockTerrain.BlockReference.Alloc(BlockTerrain._theTerrain.MakeIndexVectorFromPosition(v));
			}

			public static BlockTerrain.BlockReference Alloc()
			{
				return BlockTerrain.BlockReference._cache.Get();
			}

			public void Release()
			{
				BlockTerrain.BlockReference._cache.Put(this);
			}

			public ILinkedListNode NextNode
			{
				get
				{
					return this._nextNode;
				}
				set
				{
					this._nextNode = value;
				}
			}

			public static int _refcount = 0;

			public IntVector3 _vecIndex;

			public int _index;

			private static ObjectCache<BlockTerrain.BlockReference> _cache = new ObjectCache<BlockTerrain.BlockReference>();

			private ILinkedListNode _nextNode;
		}

		public class ChunkActionPool
		{
			public bool Empty
			{
				get
				{
					return this._nextOffset == -1;
				}
			}

			public ChunkActionPool(BlockTerrain.NextChunkAction action, BlockTerrain.NextChunkAction nextAction, TaskDelegate work)
			{
				this._pool = new int[576];
				this._nextOffset = -1;
				this._action = action;
				this._work = work;
				this._nextAction = nextAction;
			}

			public void Add(int index)
			{
				int i = Interlocked.Increment(ref this._nextOffset);
				if (i < 576)
				{
					this._pool[i] = index;
					BlockTerrain.Instance._chunks[index]._action = this._action;
				}
			}

			public virtual void Drain()
			{
				BlockTerrain bt = BlockTerrain.Instance;
				GatherTask gt = TaskDispatcher.Instance.AddGatherTask(bt._stepUpdateDelegate, null);
				IntVector3 worldMin = bt._worldMin;
				for (int i = 0; i <= this._nextOffset; i++)
				{
					int idx = this._pool[i];
					BlockTerrain.BuildTaskData td = BlockTerrain.BuildTaskData.Alloc();
					td._intVec0 = IntVector3.Add(worldMin, bt.MakeIndexVectorFromChunkIndex(idx));
					td._intData0 = idx;
					bt._chunks[idx]._action = this._nextAction;
					gt.AddTask(this._work, td);
				}
				this.Clear();
				bt.IncrementBuildTasks();
				gt.Start();
			}

			public void Clear()
			{
				this._nextOffset = -1;
			}

			public int[] _pool;

			public int _nextOffset;

			public BlockTerrain.NextChunkAction _action;

			public BlockTerrain.NextChunkAction _nextAction;

			public TaskDelegate _work;
		}

		public class LoadChunkActionPool : BlockTerrain.ChunkActionPool
		{
			public LoadChunkActionPool(BlockTerrain.NextChunkAction action, BlockTerrain.NextChunkAction nextAction, TaskDelegate work)
				: base(action, nextAction, work)
			{
				this._chunksInFlight = 0;
				this._loadedDelegate = new ChunkCacheCommandDelegate(this.ChunkLoaded);
			}

			public override void Drain()
			{
				BlockTerrain bt = BlockTerrain.Instance;
				GatherTask gt = TaskDispatcher.Instance.AddGatherTask(bt._stepUpdateDelegate, null);
				gt.SetCount(this._nextOffset + 1);
				IntVector3 worldMin = bt._worldMin;
				bt.IncrementBuildTasks();
				for (int i = 0; i <= this._nextOffset; i++)
				{
					int idx = this._pool[i];
					BlockTerrain.BuildTaskData td = BlockTerrain.BuildTaskData.Alloc();
					td._intVec0 = IntVector3.Add(worldMin, bt.MakeIndexVectorFromChunkIndex(idx));
					td._intData0 = idx;
					bt._chunks[idx]._action = this._nextAction;
					Task task = Task.Alloc();
					task.Init(this._work, td, gt);
					ChunkCacheCommand cacheCommand = ChunkCacheCommand.Alloc();
					cacheCommand._context = task;
					cacheCommand._worldPosition = CachedChunk.MakeChunkCorner(td._intVec0);
					if (this._loadedDelegate == null)
					{
						cacheCommand._callback = new ChunkCacheCommandDelegate(this.ChunkLoaded);
					}
					else
					{
						cacheCommand._callback = this._loadedDelegate;
					}
					cacheCommand._command = ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN;
					cacheCommand._priority = 1;
					this._chunksInFlight++;
					ChunkCache.Instance.AddCommand(cacheCommand);
				}
				base.Clear();
			}

			public void ChunkLoaded(ChunkCacheCommand cmd)
			{
				this._chunksInFlight--;
				Task t = (Task)cmd._context;
				bool skipped = true;
				if (cmd._command != ChunkCacheCommandEnum.RESETWAITINGCHUNKS && !BlockTerrain.Instance._resetRequested)
				{
					int index = BlockTerrain.Instance.MakeChunkIndexFromWorldIndexVector(cmd._worldPosition);
					if (index != -1)
					{
						BlockTerrain.Instance._chunks[index]._delta = cmd._delta;
						skipped = false;
					}
				}
				else
				{
					int index2 = BlockTerrain.Instance.MakeChunkIndexFromWorldIndexVector(cmd._worldPosition);
					if (index2 != -1)
					{
						BlockTerrain.Instance._chunks[index2]._action = BlockTerrain.NextChunkAction.WAITING_TO_LOAD;
					}
				}
				if (skipped)
				{
					BlockTerrain.BuildTaskData bt = t._context as BlockTerrain.BuildTaskData;
					if (bt != null)
					{
						bt._skipProcessing = true;
					}
				}
				TaskDispatcher.Instance.AddTask(t);
				cmd.Release();
			}

			private ChunkCacheCommandDelegate _loadedDelegate;

			public int _chunksInFlight;
		}

		public class LightingPool
		{
			public LightingPool(int size)
			{
				if (BlockTerrain.LightingPool._blocks == null)
				{
					BlockTerrain.LightingPool._blocks = BlockTerrain.Instance._blocks;
				}
				this._list1 = new int[size];
				this._list2 = new int[size];
				this._neighbors = new int[6];
				this._currentList = this._list1;
				this._currentIndex = -1;
				this._length = size;
			}

			public void Add(int index)
			{
				int i = Interlocked.Increment(ref this._currentIndex);
				if (i < this._length)
				{
					this._currentList[i] = index;
				}
				else
				{
					BlockTerrain.LightingPool._blocks[index] &= -1025;
				}
				this._maxUsed = ((i > this._maxUsed) ? i : this._maxUsed);
			}

			public void GetList(out int[] list, out int count, out int[] neighbors)
			{
				list = this._currentList;
				count = this._currentIndex + 1;
				count = Math.Min(count, this._length);
				neighbors = this._neighbors;
				this._currentList = ((this._currentList == this._list1) ? this._list2 : this._list1);
				this._currentIndex = -1;
			}

			public bool Empty
			{
				get
				{
					return this._currentIndex == -1;
				}
			}

			public void Clear()
			{
				this._currentIndex = -1;
			}

			public void ResetAABB()
			{
				this._min = new IntVector3(int.MaxValue, int.MaxValue, int.MaxValue);
				this._max = new IntVector3(int.MinValue, int.MinValue, int.MinValue);
			}

			public void UpdateMinAABB(ref IntVector3 value)
			{
				this._min.SetToMin(value);
				this._max.SetToMax(value);
			}

			public static int[] _blocks;

			public int _maxUsed;

			public IntVector3 _min;

			public IntVector3 _max;

			private int _length;

			private int[] _list1;

			private int[] _list2;

			private int[] _neighbors;

			private int[] _currentList;

			private int _currentIndex;
		}

		public enum NextChunkAction
		{
			WAITING_TO_LOAD,
			NEEDS_BLOCKS,
			COMPUTING_BLOCKS,
			NEEDS_LIGHTING,
			COMPUTING_LIGHTING,
			NEEDS_GEOMETRY,
			COMPUTING_GEOMETRY,
			NONE
		}

		public class PendingMod : IReleaseable, ILinkedListNode
		{
			public static BlockTerrain.PendingMod Alloc()
			{
				return BlockTerrain.PendingMod._cache.Get();
			}

			public void Release()
			{
				BlockTerrain.PendingMod._cache.Put(this);
			}

			public ILinkedListNode NextNode
			{
				get
				{
					return this._nextNode;
				}
				set
				{
					this._nextNode = value;
				}
			}

			public IntVector3 _worldPosition;

			public BlockTypeEnum _blockType;

			private static ObjectCache<BlockTerrain.PendingMod> _cache = new ObjectCache<BlockTerrain.PendingMod>();

			private ILinkedListNode _nextNode;
		}

		public struct TerrainChunk
		{
			public void Init()
			{
				this._chunk = RenderChunk.Alloc();
				this._action = BlockTerrain.NextChunkAction.WAITING_TO_LOAD;
				this._chunkLock = default(DNA.CastleMinerZ.Utils.Threading.SpinLock);
				this._numUsers = default(CountdownLatch);
				this._mods = new SynchronizedQueue<BlockTerrain.PendingMod>();
				this._delta = null;
			}

			private void ReleaseMods()
			{
				BlockTerrain.PendingMod nextMod;
				for (BlockTerrain.PendingMod oldMods = this._mods.Clear(); oldMods != null; oldMods = nextMod)
				{
					nextMod = oldMods.NextNode as BlockTerrain.PendingMod;
					oldMods.NextNode = null;
					oldMods.Release();
				}
			}

			public void SwapIn(ref BlockTerrain.TerrainChunk newChunk)
			{
				if (this._chunk != null)
				{
					this.ZeroData(true);
				}
				if (newChunk._chunk != null)
				{
					this._chunk = newChunk._chunk;
					this._action = newChunk._action;
					this._numUsers.Value = newChunk._numUsers.Value;
					this._mods.ReplaceContentsWith(newChunk._mods);
					this._delta = newChunk._delta;
					newChunk.ZeroData(false);
				}
			}

			public void ZeroData(bool releaseChunk)
			{
				if (releaseChunk && this._chunk != null)
				{
					this._chunk.Release();
				}
				this._chunk = null;
				if (releaseChunk)
				{
					this.ReleaseMods();
				}
				else
				{
					this._mods.Clear();
				}
				this._delta = null;
				this._numUsers.Value = 0;
				this._action = BlockTerrain.NextChunkAction.WAITING_TO_LOAD;
			}

			public void Reset()
			{
				this.ZeroData(true);
				this._chunk = RenderChunk.Alloc();
			}

			public void ReplaceChunk(RenderChunk newChunk)
			{
				this._chunkLock.Lock();
				RenderChunk oldChunk = this._chunk;
				this._chunk = newChunk;
				this._chunkLock.Unlock();
				newChunk.AddRef();
				oldChunk.Release();
			}

			public RenderChunk GetChunk()
			{
				this._chunkLock.Lock();
				RenderChunk chunk = this._chunk;
				chunk.AddRef();
				this._chunkLock.Unlock();
				return chunk;
			}

			public RenderChunk _chunk;

			public DNA.CastleMinerZ.Utils.Threading.SpinLock _chunkLock;

			public CountdownLatch _numUsers;

			public SynchronizedQueue<BlockTerrain.PendingMod> _mods;

			public int[] _delta;

			public BlockTerrain.NextChunkAction _action;
		}

		public delegate void ThreadedResetDelegate();

		private struct QueuedBufferBuild
		{
			internal QueuedBufferBuild(int index, RenderChunk chunk)
			{
				this.Index = index;
				this.Chunk = chunk;
			}

			public int Index;

			public RenderChunk Chunk;
		}
	}
}
