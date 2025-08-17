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
			Texture[] array = null;
			if (CastleMinerZArgs.Instance.TextureFolder != null)
			{
				array = ((ProfiledContentManager)cm).LoadTerrain();
			}
			if (array == null)
			{
				array = cm.Load<Texture[]>("Terrain\\Textures");
			}
			this._diffuseAlpha = (Texture2D)array[0];
			this._normalSpec = (Texture2D)array[1];
			this._metalLight = (Texture2D)array[2];
			this._mipMapNormals = (Texture2D)array[3];
			this._mipMapDiffuse = (Texture2D)array[4];
			if (GraphicsProfileManager.Instance.IsHiDef)
			{
				Texture2D texture2D = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap0");
				Byte4[] array2 = new Byte4[texture2D.Bounds.Width * texture2D.Bounds.Height * 4];
				texture2D.GetData<Byte4>(array2, 0, texture2D.Bounds.Width * texture2D.Bounds.Height);
				texture2D = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap1");
				texture2D.GetData<Byte4>(array2, texture2D.Bounds.Width * texture2D.Bounds.Height, texture2D.Bounds.Width * texture2D.Bounds.Height);
				texture2D = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap2");
				texture2D.GetData<Byte4>(array2, texture2D.Bounds.Width * texture2D.Bounds.Height * 2, texture2D.Bounds.Width * texture2D.Bounds.Height);
				texture2D = cm.Load<Texture2D>("Textures\\EnvMaps\\envmap3");
				texture2D.GetData<Byte4>(array2, texture2D.Bounds.Width * texture2D.Bounds.Height * 3, texture2D.Bounds.Width * texture2D.Bounds.Height);
				this._envMap = new Texture3D(gd, texture2D.Bounds.Width, texture2D.Bounds.Height, 4, false, texture2D.Format);
				this._envMap.SetData<Byte4>(array2);
			}
			else
			{
				this._envMap = null;
			}
			Vector2 vector = new Vector2(0.5f / (float)this._diffuseAlpha.Width, 0.5f / (float)this._diffuseAlpha.Height);
			Vector2 vector2 = new Vector2(0.125f - vector.X, 0.125f - vector.Y);
			Vector2 vector3 = new Vector2(0.0625f - vector.X, 0.0625f - vector.Y);
			this._vertexUVs = new Vector4[]
			{
				new Vector4(vector.X, vector.Y, vector.X, vector.Y),
				new Vector4(vector2.X, vector.Y, vector3.X, vector.Y),
				new Vector4(vector.X, vector2.Y, vector.X, vector3.Y),
				new Vector4(vector2.X, vector2.Y, vector3.X, vector3.Y)
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
			int[] array3 = new int[] { 0, 1, 2, 2, 1, 3 };
			ushort[] array4 = new ushort[98304];
			int num = 0;
			int num2 = 0;
			for (int k = 0; k < 16384; k++)
			{
				for (int l = 0; l < 6; l++)
				{
					array4[num2++] = (ushort)(num + array3[l]);
					if (((long)(num + array3[l]) & (long)((ulong)(-65536))) != 0L)
					{
						break;
					}
				}
				num += 4;
			}
			bool flag = false;
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDevice())
				{
					try
					{
						this._staticIB = new IndexBuffer(gd, IndexElementSize.SixteenBits, 98304, BufferUsage.WriteOnly);
						this._staticIB.SetData<ushort>(array4);
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
			int num = 13;
			int num2 = 1;
			for (int i = 1; i <= num; i++)
			{
				for (int j = 0; j <= i; j++)
				{
					int num3 = -i;
					if (num3 >= -num)
					{
						int num4 = j;
						if (num4 < num)
						{
							this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
						}
						if (j != 0)
						{
							num4 = -j;
							if (num4 >= -num)
							{
								this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
							}
						}
					}
					num3 = i;
					if (num3 < num)
					{
						int num4 = j;
						if (num4 < num)
						{
							this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
						}
						if (j != 0)
						{
							num4 = -j;
							if (num4 >= -num)
							{
								this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
							}
						}
					}
					if (j != i)
					{
						int num4 = i;
						if (num4 < num)
						{
							num3 = j;
							if (num3 < num)
							{
								this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
							}
							if (j != 0)
							{
								num3 = -j;
								if (num3 >= -num)
								{
									this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
								}
							}
						}
						num4 = -i;
						if (num4 >= -num)
						{
							num3 = j;
							if (num3 < num)
							{
								this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
							}
							if (j != 0)
							{
								num3 = -j;
								if (num3 >= -num)
								{
									this._radiusOrderOffsets[num2++].SetValues(num4, 0, num3);
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
			IntVector3 intVector = new IntVector3((int)Math.Floor((double)(center.X / 1f)), (int)Math.Floor((double)(center.Y / 1f)), (int)Math.Floor((double)(center.Z / 1f)));
			intVector.X -= 192;
			intVector.Y = -64;
			intVector.Z -= 192;
			this.SetWorldMin(intVector);
			this._maxChunksAtOnce = 64;
			this.CenterOn(center, false);
		}

		private void InternalTeleport(Vector3 center)
		{
			this._initted = true;
			this._resetRequested = false;
			this._loadingProgress = 0;
			this._allChunksLoaded = false;
			IntVector3 intVector = new IntVector3((int)Math.Floor((double)(center.X / 1f)), (int)Math.Floor((double)(center.Y / 1f)), (int)Math.Floor((double)(center.Z / 1f)));
			intVector.X -= 192;
			intVector.Y = -64;
			intVector.Z -= 192;
			this.SetWorldMin(intVector);
			this._maxChunksAtOnce = 64;
			this.CenterOn(center, false);
		}

		public void DoThreadedReset(object context)
		{
			ChunkCache.Instance.Stop(true);
			MainThreadMessageSender.Instance.GameOver();
			this.DoThreadedCleanup(context);
			BlockTerrain.ItemBlockCommand itemBlockCommand = this.ItemBlockCommandQueue.Clear();
			BlockTerrain.ItemBlockCommand.ReleaseList(itemBlockCommand);
		}

		public void DoThreadedCleanup(object context)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (this._buildTasksRemaining || this._updateTasksRemaining)
			{
				if (stopwatch.ElapsedMilliseconds > 120000L && TaskDispatcher.Instance.IsIdle(TaskThreadEnum.NUM_THREADS))
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
				int loadingProgress = this._loadingProgress;
				if (loadingProgress >= 182)
				{
					break;
				}
				int num = Math.Min(loadingProgress + value, 182);
				if (Interlocked.CompareExchange(ref this._loadingProgress, num, loadingProgress) == loadingProgress)
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
			IntVector3 intVector = this.MakeIndexVectorFromPosition(pos);
			intVector.X /= 16;
			intVector.Y = 0;
			intVector.Z /= 16;
			return intVector;
		}

		public Vector3 MakePositionFromIndexVector(IntVector3 v)
		{
			Vector3 vector = IntVector3.ToVector3(IntVector3.Add(v, this._worldMin));
			return Vector3.Multiply(vector, 1f);
		}

		public IntVector3 MakeIndexVectorFromPosition(Vector3 a)
		{
			IntVector3 intVector = new IntVector3((int)Math.Floor((double)(a.X / 1f)), (int)Math.Floor((double)(a.Y / 1f)), (int)Math.Floor((double)(a.Z / 1f)));
			return IntVector3.Subtract(intVector, this._worldMin);
		}

		public int MakeIndexFromPosition(Vector3 a)
		{
			IntVector3 intVector = this.MakeIndexVectorFromPosition(a);
			if (this.IsIndexValid(intVector))
			{
				return this.MakeIndex(intVector);
			}
			return -1;
		}

		public int MakeIndexFromWorldIndexVector(IntVector3 a)
		{
			IntVector3 intVector = IntVector3.Subtract(a, this._worldMin);
			if (this.IsIndexValid(intVector))
			{
				return this.MakeIndex(intVector);
			}
			return -1;
		}

		public IntVector3 MakeIndexVectorFromChunkIndex(int index)
		{
			int num = index / 24;
			int num2 = index % 24;
			return new IntVector3(num2 * 16, 0, num * 16);
		}

		public IntVector3 MakeIndexVectorFromIndex(int index)
		{
			int num = index / 49152;
			index -= num * 49152;
			int num2 = index / 128;
			int num3 = index - num2 * 128;
			return new IntVector3(num2, num3, num);
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
			IntVector3 intVector = this.MakeIndexVectorFromPosition(a);
			return this.MakeIndex(intVector);
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
			IntVector3 intVector = IntVector3.Subtract(i, this._worldMin);
			if (this.IsIndexValid(intVector))
			{
				return this.MakeChunkIndexFromIndexVector(intVector);
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
			IntVector3 intVector = IntVector3.Subtract(a, this._worldMin);
			return this.GetSafeBlockAt(intVector);
		}

		public IntVector3 GetNeighborIndex(IntVector3 a, BlockFace face)
		{
			return IntVector3.Add(a, BlockTerrain._faceNeighbors[(int)face]);
		}

		public int GetNeighborBlockAtABS(IntVector3 a, BlockFace face)
		{
			IntVector3 intVector = IntVector3.Subtract(IntVector3.Add(a, BlockTerrain._faceNeighbors[(int)face]), this._worldMin);
			return this.GetSafeBlockAt(intVector);
		}

		public void FillFaceLightTable(IntVector3 local, BlockFace face, ref float[] sun, ref float[] torch)
		{
			IntVector3[] array = BlockTerrain._lightNeighbors[(int)face];
			for (int i = 0; i < 9; i++)
			{
				IntVector3 intVector = IntVector3.Add(local, array[i]);
				if (this.IsIndexValid(intVector))
				{
					int blockAt = this.GetBlockAt(intVector);
					BlockTypeEnum typeIndex = Block.GetTypeIndex(blockAt);
					if (BlockType.GetType(typeIndex).Opaque || typeIndex == BlockTypeEnum.NumberOfBlocks || Block.IsInList(blockAt))
					{
						sun[i] = -1f;
						torch[i] = -1f;
					}
					else
					{
						sun[i] = (float)Block.GetSunLightLevel(blockAt);
						torch[i] = (float)Block.GetTorchLightLevel(blockAt);
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
			IntVector3 intVector = IntVector3.Subtract(center, this._worldMin);
			IntVector3 intVector2 = new IntVector3(-1, -1, -1);
			int num = 0;
			intVector2.Z = -1;
			while (intVector2.Z <= 1)
			{
				intVector2.Y = -1;
				while (intVector2.Y <= 1)
				{
					intVector2.X = -1;
					while (intVector2.X <= 1)
					{
						IntVector3 intVector3 = IntVector3.Add(intVector, intVector2);
						if (this.IsIndexValid(intVector3))
						{
							int blockAt = this.GetBlockAt(intVector3);
							BlockTypeEnum typeIndex = Block.GetTypeIndex(blockAt);
							if (typeIndex == BlockTypeEnum.NumberOfBlocks || Block.IsInList(blockAt))
							{
								sun[num] = 1f;
								torch[num] = 0f;
							}
							else if (BlockType.GetType(typeIndex).Opaque)
							{
								sun[num] = -1f;
								torch[num] = -1f;
							}
							else
							{
								sun[num] = (float)Block.GetSunLightLevel(blockAt) / 15f;
								torch[num] = (float)Block.GetTorchLightLevel(blockAt) / 15f;
							}
						}
						else
						{
							sun[num] = 1f;
							torch[num] = 0f;
						}
						intVector2.X++;
						num++;
					}
					intVector2.Y++;
				}
				intVector2.Z++;
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
				IntVector3 intVector = IntVector3.Subtract(worldIndex, this._worldMin);
				if (this.IsIndexValid(intVector))
				{
					int num = this.MakeChunkIndexFromIndexVector(intVector);
					lock (this._chunks[num]._mods)
					{
						for (BlockTerrain.PendingMod pendingMod = this._chunks[num]._mods.Front; pendingMod != null; pendingMod = (BlockTerrain.PendingMod)pendingMod.NextNode)
						{
							if (pendingMod._worldPosition.Equals(worldIndex))
							{
								return pendingMod._blockType;
							}
						}
					}
					int num2 = this.MakeIndex(intVector);
					return Block.GetTypeIndex(this._blocks[num2]);
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
			IntVector3 intVector = IntVector3.Subtract(corner, this._worldMin);
			intVector.Y = 0;
			this.SetSkyAndEmitterLightingForRegion(intVector, new IntVector3(intVector.X + 16 - 1, intVector.Y + 128 - 1, intVector.Z + 16 - 1));
		}

		public void SetSkyAndEmitterLightingForRegion(IntVector3 regionMin, IntVector3 regionMax)
		{
			IntVector3 intVector = regionMin;
			intVector.Z = regionMin.Z;
			while (intVector.Z <= regionMax.Z)
			{
				intVector.X = regionMin.X;
				while (intVector.X <= regionMax.X)
				{
					intVector.Y = regionMax.Y;
					int num = this.MakeIndex(intVector);
					bool flag = regionMax.Y != 127 && (!Block.IsSky(this._blocks[num + 1]) || Block.GetType(this._blocks[num + 1]).LightTransmission != 16);
					while (intVector.Y >= regionMin.Y)
					{
						if (this._resetRequested)
						{
							return;
						}
						int num2 = this._blocks[num];
						BlockType type = Block.GetType(num2);
						if (Block.IsOpaque(num2))
						{
							flag = true;
							if (type.SelfIllumination != 0)
							{
								goto IL_00E9;
							}
						}
						else
						{
							if (!flag)
							{
								int num3 = type.LightTransmission - 1;
								flag = flag || num3 != 15;
								num2 |= 256 | ((num3 < 0) ? 0 : num3);
								goto IL_00E9;
							}
							goto IL_00E9;
						}
						IL_00FE:
						intVector.Y--;
						num--;
						continue;
						IL_00E9:
						this._blocks[num] = Block.SetTorchLightLevel(num2, type.SelfIllumination);
						goto IL_00FE;
					}
					intVector.X++;
				}
				intVector.Z++;
			}
		}

		public void ResetSkyAndEmitterLightingForRegion(IntVector3 regionMin, IntVector3 regionMax)
		{
			IntVector3 intVector = regionMin;
			if (!this.IsIndexValid(regionMin) || !this.IsIndexValid(regionMax))
			{
				return;
			}
			intVector.Z = regionMin.Z;
			while (intVector.Z <= regionMax.Z)
			{
				intVector.X = regionMin.X;
				while (intVector.X <= regionMax.X)
				{
					intVector.Y = regionMax.Y;
					int num = this.MakeIndex(intVector);
					bool flag = regionMax.Y != 127 && (!Block.IsSky(this._blocks[num + 1]) || Block.GetType(this._blocks[num + 1]).LightTransmission != 16);
					while (intVector.Y >= 0)
					{
						int num2 = this._blocks[num];
						BlockType type = Block.GetType(num2);
						bool flag2 = Block.IsSky(num2);
						if (Block.IsOpaque(num2))
						{
							flag = true;
							num2 &= -257;
						}
						else if (!flag)
						{
							int num3 = type.LightTransmission - 1;
							flag = flag || num3 != 15;
							num2 |= 256 | ((num3 < 0) ? 0 : num3);
						}
						else
						{
							num2 &= -257;
						}
						this._blocks[num] = Block.SetTorchLightLevel(num2, type.SelfIllumination);
						if (intVector.Y < regionMin.Y && flag2 == Block.IsSky(num2))
						{
							break;
						}
						intVector.Y--;
						num--;
					}
					intVector.X++;
				}
				intVector.Z++;
			}
		}

		protected void ApplyDelta(BlockTerrain.BuildTaskData data)
		{
			IntVector3 intVector = IntVector3.Subtract(data._intVec0, this._worldMin);
			if (this.IsIndexValid(intVector) && this._chunks[data._intData0]._delta != null)
			{
				int[] delta = this._chunks[data._intData0]._delta;
				this._chunks[data._intData0]._delta = null;
				for (int i = 0; i < delta.Length; i++)
				{
					IntVector3 intVector2 = IntVector3.Add(DeltaEntry.GetVector(delta[i]), intVector);
					BlockTypeEnum blockType = DeltaEntry.GetBlockType(delta[i]);
					int num = this.MakeIndex(intVector2);
					if (Block.GetType(this._blocks[num]).SpawnEntity)
					{
						this.RemoveItemBlockEntity(Block.GetTypeIndex(this._blocks[num]), IntVector3.Add(intVector2, this._worldMin));
					}
					if (BlockType.GetType(blockType).SpawnEntity)
					{
						this.CreateItemBlockEntity(blockType, IntVector3.Add(intVector2, this._worldMin));
					}
					this._blocks[num] = Block.SetType(0, blockType);
				}
			}
		}

		public void DoThreadedComputeBlocks(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData buildTaskData = (BlockTerrain.BuildTaskData)context;
			if (!this._resetRequested && !buildTaskData._skipProcessing)
			{
				IntVector3 intVector = IntVector3.Subtract(buildTaskData._intVec0, this._worldMin);
				if (this.IsIndexValid(intVector))
				{
					intVector.Y = 0;
					IntVector3 intVector2 = IntVector3.Add(new IntVector3(15, 127, 15), intVector);
					if (this.IsIndexValid(intVector2))
					{
						this.FillRegion(intVector, intVector2, BlockTypeEnum.NumberOfBlocks);
						this._worldBuilder.BuildWorldChunk(this, buildTaskData._intVec0);
						this.ApplyDelta(buildTaskData);
						this.ApplyModListDuringCreate(buildTaskData._intData0);
						this.ReplaceRegion(intVector, intVector2, BlockTypeEnum.NumberOfBlocks, BlockTypeEnum.Empty);
						this.SetSkyAndEmitterLightingForChunk(buildTaskData._intVec0);
					}
				}
				this.AddToLoadingProgress(1);
			}
			buildTaskData.Release();
		}

		public void DoThreadedComputeGeometry(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData buildTaskData = (BlockTerrain.BuildTaskData)context;
			bool flag = false;
			if (!this._resetRequested && !buildTaskData._skipProcessing)
			{
				if (this._chunks[buildTaskData._intData0]._action >= BlockTerrain.NextChunkAction.NEEDS_GEOMETRY)
				{
					IntVector3 intVector = IntVector3.Subtract(buildTaskData._intVec0, this._worldMin);
					if (intVector.X >= 0 && intVector.X < 384 && intVector.Z >= 0 && intVector.Z < 384)
					{
						RenderChunk renderChunk = RenderChunk.Alloc();
						renderChunk._worldMin = buildTaskData._intVec0;
						renderChunk._basePosition = IntVector3.ToVector3(renderChunk._worldMin);
						if (renderChunk.BuildFaces(this._graphicsDevice))
						{
							lock (this._vertexBuildListLockObject)
							{
								this._vertexBuildListIncoming.Add(new BlockTerrain.QueuedBufferBuild(buildTaskData._intData0, renderChunk));
							}
							flag = true;
						}
						else
						{
							renderChunk.Release();
						}
					}
					if (!flag)
					{
						this._chunks[buildTaskData._intData0]._action = BlockTerrain.NextChunkAction.NONE;
					}
				}
				if (!flag)
				{
					this._chunks[buildTaskData._intData0]._numUsers.Decrement();
					this.AddToLoadingProgress(1);
				}
			}
			buildTaskData.Release();
		}

		public void DoThreadedComputeLighting(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData buildTaskData = (BlockTerrain.BuildTaskData)context;
			if (!this._resetRequested && !buildTaskData._skipProcessing)
			{
				if (this._chunks[buildTaskData._intData0]._action >= BlockTerrain.NextChunkAction.NEEDS_LIGHTING)
				{
					IntVector3 intVector = IntVector3.Subtract(buildTaskData._intVec0, this._worldMin);
					intVector.Y = 0;
					IntVector3 intVector2 = IntVector3.Add(new IntVector3(15, 127, 15), intVector);
					this.ComputeFirstPassLightForRegion(intVector, intVector2, this._mainLightingPool);
					this._computeGeometryPool.Add(buildTaskData._intData0);
				}
				else
				{
					this._chunks[buildTaskData._intData0]._numUsers.Decrement();
					this.AddToLoadingProgress(1);
				}
			}
			buildTaskData.Release();
		}

		public void ComputeFirstPassLightForRegion(IntVector3 regionMin, IntVector3 regionMax, BlockTerrain.LightingPool lightingPool)
		{
			IntVector3 intVector = regionMin;
			if (!this.IsIndexValid(regionMin) || !this.IsIndexValid(regionMax))
			{
				return;
			}
			intVector.Z = regionMin.Z;
			while (intVector.Z <= regionMax.Z)
			{
				intVector.X = regionMin.X;
				while (intVector.X <= regionMax.X)
				{
					intVector.Y = regionMin.Y;
					int num = this.MakeIndex(intVector);
					while (intVector.Y <= regionMax.Y)
					{
						int blockAt = this.GetBlockAt(intVector);
						if (Block.IsLit(blockAt))
						{
							int num2;
							if (Block.GetTorchLightLevel(blockAt) == 0)
							{
								num2 = -2147481856;
							}
							else
							{
								num2 = -2147482112;
							}
							for (int i = 0; i < 6; i++)
							{
								IntVector3 intVector2 = IntVector3.Add(intVector, BlockTerrain._faceNeighbors[i]);
								if (this.IsIndexValid(intVector2))
								{
									int num3 = this.MakeIndex(intVector2);
									int num4 = this._blocks[num3];
									if ((num4 & num2) == 0 && Interlocked.CompareExchange(ref this._blocks[num3], Block.IsInList(num4, true), num4) == num4)
									{
										lightingPool.Add(num3);
									}
								}
							}
						}
						intVector.Y++;
						num++;
					}
					intVector.X++;
				}
				intVector.Z++;
			}
			intVector = regionMin;
			intVector.Z--;
			if (intVector.Z >= 0)
			{
				intVector.Y = 0;
				int num5 = this.MakeIndex(intVector);
				int j = regionMin.X;
				while (j <= regionMax.X)
				{
					intVector.Y = regionMin.Y;
					while (intVector.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[num5]))
						{
							int num2;
							if (Block.GetTorchLightLevel(this._blocks[num5]) == 0)
							{
								num2 = -2147481856;
							}
							else
							{
								num2 = -2147482112;
							}
							int num6 = num5 + 49152;
							int num7 = this._blocks[num6];
							if ((num7 & num2) == 0 && Interlocked.CompareExchange(ref this._blocks[num6], Block.IsInList(num7, true), num7) == num7)
							{
								lightingPool.Add(num6);
							}
						}
						intVector.Y++;
						num5++;
					}
					j++;
					intVector.X++;
				}
			}
			intVector = regionMin;
			intVector.Z += 16;
			if (intVector.Z < 384)
			{
				intVector.Y = regionMin.Y;
				int num8 = this.MakeIndex(intVector);
				int k = regionMin.X;
				while (k <= regionMax.X)
				{
					intVector.Y = regionMin.Y;
					while (intVector.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[num8]))
						{
							int num2;
							if (Block.GetTorchLightLevel(this._blocks[num8]) == 0)
							{
								num2 = -2147481856;
							}
							else
							{
								num2 = -2147482112;
							}
							int num9 = num8 - 49152;
							int num10 = this._blocks[num9];
							if ((num10 & num2) == 0 && Interlocked.CompareExchange(ref this._blocks[num9], Block.IsInList(num10, true), num10) == num10)
							{
								lightingPool.Add(num9);
							}
						}
						intVector.Y++;
						num8++;
					}
					k++;
					intVector.X++;
				}
			}
			intVector = regionMin;
			intVector.X--;
			if (intVector.X >= 0)
			{
				int l = regionMin.Z;
				while (l <= regionMax.Z)
				{
					intVector.Y = regionMin.Y;
					intVector.Z = l;
					int num11 = this.MakeIndex(intVector);
					while (intVector.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[num11]))
						{
							int num2;
							if (Block.GetTorchLightLevel(this._blocks[num11]) == 0)
							{
								num2 = -2147481856;
							}
							else
							{
								num2 = -2147482112;
							}
							int num12 = num11 + 128;
							int num13 = this._blocks[num12];
							if ((num13 & num2) == 0 && Interlocked.CompareExchange(ref this._blocks[num12], Block.IsInList(num13, true), num13) == num13)
							{
								lightingPool.Add(num12);
							}
						}
						num11++;
						intVector.Y++;
					}
					l++;
					intVector.Z++;
				}
			}
			intVector = regionMin;
			intVector.X++;
			if (intVector.X < 384)
			{
				int m = regionMin.Z;
				while (m <= regionMax.Z)
				{
					intVector.Y = regionMin.Y;
					intVector.Z = m;
					int num14 = this.MakeIndex(intVector);
					while (intVector.Y <= regionMax.Y)
					{
						if (Block.NeedToLightNewNeighbors(this._blocks[num14]))
						{
							int num2;
							if (Block.GetTorchLightLevel(this._blocks[num14]) == 0)
							{
								num2 = -2147481856;
							}
							else
							{
								num2 = -2147482112;
							}
							int num15 = num14 - 128;
							int num16 = this._blocks[num15];
							if ((num16 & num2) == 0 && Interlocked.CompareExchange(ref this._blocks[num15], Block.IsInList(num16, true), num16) == num16)
							{
								lightingPool.Add(num15);
							}
						}
						num14++;
						intVector.Y++;
					}
					m++;
					intVector.Z++;
				}
			}
		}

		public void FillLighting(BlockTerrain.LightingPool lightPool)
		{
			while (!lightPool.Empty && !this._resetRequested)
			{
				this.AddToLoadingProgress(1);
				int[] array;
				int num;
				int[] array2;
				lightPool.GetList(out array, out num, out array2);
				this.maxLightNodes = Math.Max(this.maxLightNodes, num);
				for (int i = 0; i < num; i++)
				{
					if (this._resetRequested)
					{
						return;
					}
					int num2 = array[i];
					IntVector3 intVector = this.MakeIndexVectorFromIndex(num2);
					bool flag = this.IndexVectorIsOnEdge(intVector);
					int num3 = this._blocks[num2] & -1025;
					if (!Block.IsUninitialized(num3))
					{
						BlockType type = Block.GetType(num3);
						int lighting = Block.GetLighting(num3);
						int num4 = 0;
						int num5 = 0;
						if (!flag)
						{
							for (int j = 0; j < 6; j++)
							{
								int num6 = num2 + BlockTerrain._faceIndexNeighbors[j];
								if ((this._blocks[num6] & -2147483648) == 0)
								{
									array2[num5++] = num6;
								}
							}
						}
						else
						{
							for (int k = 0; k < 6; k++)
							{
								IntVector3 intVector2 = IntVector3.Add(intVector, BlockTerrain._faceNeighbors[k]);
								if (this.IsIndexValid(intVector2))
								{
									int num7 = this.MakeIndex(intVector2);
									if ((this._blocks[num7] & -2147483648) == 0)
									{
										array2[num5++] = num7;
									}
								}
							}
						}
						if (Block.IsSky(num3))
						{
							for (int l = 0; l < num5; l++)
							{
								num4 = Math.Max(num4, Block.GetTorchLightLevel(this._blocks[array2[l]]));
							}
						}
						else
						{
							int num8 = int.MinValue;
							for (int m = 0; m < num5; m++)
							{
								int num9 = this._blocks[array2[m]];
								num8 = Math.Max(num8, Block.GetSunLightLevel(num9));
								num4 = Math.Max(num4, Block.GetTorchLightLevel(num9));
							}
							num3 = Block.SetSunLightLevel(num3, type.TransmitLight(num8));
						}
						num3 = Block.SetTorchLightLevel(num3, Math.Max(type.SelfIllumination, type.TransmitLight(num4)));
						this._blocks[num2] = num3;
						int lighting2 = Block.GetLighting(lighting ^ num3);
						if (lighting2 != 0)
						{
							lightPool.UpdateMinAABB(ref intVector);
							for (int n = 0; n < num5; n++)
							{
								int num10 = array2[n];
								int num11 = this._blocks[num10];
								if ((num11 & 1536) == 0)
								{
									this._blocks[num10] = num11 | 1024;
									lightPool.Add(num10);
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
			BlockTerrain.ShiftingTerrainData shiftingTerrainData = (BlockTerrain.ShiftingTerrainData)context;
			Buffer.BlockCopy(this._blocks, shiftingTerrainData.source, this._shiftedBlocks, shiftingTerrainData.dest, shiftingTerrainData.length);
			int fillStart = shiftingTerrainData.fillStart;
			int fillLength = shiftingTerrainData.fillLength;
			for (int i = 0; i < fillLength; i++)
			{
				this._shiftedBlocks[fillStart++] = this.initblock;
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
				int num = -dz * 16 * 128 * 384 + -dx * 16 * 128;
				int num2 = 18874368 - Math.Abs(num);
				bool flag = num > 0;
				int[] blocks = this._blocks;
				this._blocks = this._shiftedBlocks;
				this._shiftedBlocks = blocks;
				num = -dz * 24 + -dx;
				num2 = 576 - Math.Abs(num);
				int num3 = num;
				int num4;
				int num5;
				if (!flag)
				{
					num = -num;
					num4 = num;
					num5 = 1;
				}
				else
				{
					num4 = 576 - num - 1;
					num5 = -1;
				}
				if (dx > 0)
				{
					for (int i = 0; i < 24; i++)
					{
						int num6 = i * 24;
						int j = 0;
						while (j < dx)
						{
							if (this._chunks[num6]._chunk != null)
							{
								this._chunks[num6].ZeroData(true);
							}
							j++;
							num6++;
						}
					}
				}
				else if (dx < 0)
				{
					for (int k = 0; k < 24; k++)
					{
						int num7 = k * 24 + 24 - 1;
						int l = 0;
						while (l < -dx)
						{
							if (this._chunks[num7]._chunk != null)
							{
								this._chunks[num7].ZeroData(true);
							}
							l++;
							num7--;
						}
					}
				}
				for (int m = 0; m < num2; m++)
				{
					this._chunks[num4 + num3].SwapIn(ref this._chunks[num4]);
					num4 += num5;
				}
				for (int n = 0; n < 576; n++)
				{
					if (this._chunks[n]._chunk == null)
					{
						this._chunks[n].Reset();
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
				int num = -desireddz * 16 * 128 * 384 + -desireddx * 16 * 128;
				this._shiftTerrainData.fillLength = Math.Abs(num);
				int num2 = 18874368 - this._shiftTerrainData.fillLength;
				if (num <= 0)
				{
					this._shiftTerrainData.source = -num * 4;
					this._shiftTerrainData.dest = 0;
					this._shiftTerrainData.fillStart = num2;
				}
				else
				{
					this._shiftTerrainData.source = 0;
					this._shiftTerrainData.dest = num * 4;
					this._shiftTerrainData.fillStart = 0;
				}
				this._shiftTerrainData.length = num2 * 4;
				TaskDispatcher.Instance.AddRushTask(this._shiftTerrainDelegate, this._shiftTerrainData);
				this._updateTasksRemaining.Increment();
				return true;
			}
			return false;
		}

		public void FillRegion(IntVector3 min, IntVector3 max, BlockTypeEnum blockType)
		{
			int num = Block.SetType(0, blockType);
			for (int i = min.Z; i <= max.Z; i++)
			{
				for (int j = min.X; j <= max.X; j++)
				{
					int num2 = this.MakeIndex(j, min.Y, i);
					int k = min.Y;
					while (k <= max.Y)
					{
						this._blocks[num2] = num;
						k++;
						num2++;
					}
				}
			}
		}

		public void ReplaceRegion(IntVector3 min, IntVector3 max, BlockTypeEnum replaceme, BlockTypeEnum withme)
		{
			for (int i = min.Z; i <= max.Z; i++)
			{
				for (int j = min.X; j <= max.X; j++)
				{
					int num = this.MakeIndex(j, min.Y, i);
					int k = min.Y;
					while (k <= max.Y)
					{
						if (Block.GetTypeIndex(this._blocks[num]) == replaceme)
						{
							this._blocks[num] = Block.SetType(this._blocks[num], withme);
						}
						k++;
						num++;
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
			IntVector3 intVector = IntVector3.Min(IntVector3.Add(block, new IntVector3(1, 1, 1)), new IntVector3(383, 127, 383));
			IntVector3 intVector2 = IntVector3.Max(IntVector3.Subtract(block, new IntVector3(1, 1, 1)), IntVector3.Zero);
			int num = -1;
			if (intVector2.Y > 0)
			{
				num = intVector2.Y - 1;
			}
			for (int i = intVector2.Z; i <= intVector.Z; i++)
			{
				for (int j = intVector2.X; j <= intVector.X; j++)
				{
					int num2 = this.MakeIndex(j, intVector2.Y, i);
					int k = intVector2.Y;
					while (k <= intVector.Y)
					{
						if (!Block.IsUninitialized(this._blocks[num2]))
						{
							lightPool.Add(num2);
							this._blocks[num2] |= 1024;
						}
						k++;
						num2++;
					}
				}
			}
			if (num >= 0)
			{
				IntVector3 intVector3 = new IntVector3(block.X, num, block.Z);
				int num3 = this.MakeIndex(intVector3);
				while (intVector3.Y >= 0 && Block.IsSky(this._blocks[num3]))
				{
					for (int l = 0; l < 6; l++)
					{
						if (l != 5)
						{
							IntVector3 intVector4 = IntVector3.Add(intVector3, BlockTerrain._faceNeighbors[l]);
							if (this.IsIndexValid(intVector4))
							{
								int num4 = this.MakeIndex(intVector4);
								int num5 = this._blocks[num4];
								if ((num5 & -2147481856) == 0 && Interlocked.CompareExchange(ref this._blocks[num4], Block.IsInList(num5, true), num5) == num5)
								{
									lightPool.Add(num4);
								}
							}
						}
					}
					intVector3.Y--;
					num3--;
				}
			}
		}

		private void DoThreadedFinishSetBlock(BaseTask task, object context)
		{
			BlockTerrain.BuildTaskData buildTaskData = (BlockTerrain.BuildTaskData)context;
			if (!this._resetRequested)
			{
				this.FillLighting(this._updateLightingPool);
				IntVector3 intVector = IntVector3.Min(IntVector3.Add(this._updateLightingPool._max, new IntVector3(1, 1, 1)), new IntVector3(383, 127, 383));
				IntVector3 intVector2 = IntVector3.Max(IntVector3.Subtract(this._updateLightingPool._min, new IntVector3(1, 1, 1)), IntVector3.Zero);
				intVector2.X /= 16;
				intVector2.Z /= 16;
				intVector.X /= 16;
				intVector.Z /= 16;
				GatherTask gatherTask = TaskDispatcher.Instance.AddGatherTask(this._finishRegionOpDelegate, null);
				for (int i = intVector2.Z; i <= intVector.Z; i++)
				{
					for (int j = intVector2.X; j <= intVector.X; j++)
					{
						int num = i * 24 + j;
						BlockTerrain.BuildTaskData buildTaskData2 = BlockTerrain.BuildTaskData.Alloc();
						buildTaskData2._intVec0 = IntVector3.Add(this._worldMin, this.MakeIndexVectorFromChunkIndex(num));
						buildTaskData2._intData0 = num;
						this._chunks[num]._numUsers.Increment();
						gatherTask.AddTask(this._computeGeometryDelegate, buildTaskData2);
					}
				}
				this.DecrChunkInUse(buildTaskData._intData0);
				gatherTask.StartNow();
			}
			buildTaskData.Release();
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
			IntVector3 intVector = min;
			intVector.Z = min.Z;
			while (intVector.Z <= max.Z)
			{
				intVector.X = min.X;
				while (intVector.X <= max.X)
				{
					intVector.Y = test.Y + 1;
					int num = this.MakeIndex(intVector);
					if (!Block.GetType(this._blocks[num]).BlockPlayer && Block.GetType(this._blocks[num + 1]).BlockPlayer)
					{
						return true;
					}
					intVector.X++;
				}
				intVector.Z++;
			}
			return false;
		}

		private void BruteForceTrace(TraceProbe tp)
		{
			IntVector3 intVector = this.MakeIndexVectorFromPosition(tp._bounds.Min);
			IntVector3 intVector2 = this.MakeIndexVectorFromPosition(tp._bounds.Max);
			bool flag = false;
			int num = 0;
			int num2 = 0;
			if (tp.SimulateSlopedSides)
			{
				Vector3 start = tp._start;
				start.Y -= tp.HalfVector.Y;
				IntVector3 localIndex = this.GetLocalIndex(IntVector3.FromVector3(start));
				localIndex.Y++;
				if (this.IsIndexValid(localIndex))
				{
					int num3 = this.MakeIndex(localIndex);
					if (!Block.GetType(this._blocks[num3]).BlockPlayer)
					{
						num = (int)Math.Floor((double)(tp._start.Y - tp.HalfVector.Y + 0.01f)) - this._worldMin.Y;
						num2 = (int)Math.Floor((double)(tp._start.Y - tp.HalfVector.Y + 0.2f)) - this._worldMin.Y;
						flag = true;
					}
				}
			}
			tp.ShapeHasSlopedSides = false;
			intVector.SetToMax(IntVector3.Zero);
			intVector2.SetToMin(new IntVector3(383, 127, 383));
			IntVector3 intVector3 = IntVector3.Subtract(intVector, intVector2);
			if (intVector3.X * intVector3.Y * intVector3.Z < 27)
			{
				IntVector3 intVector4 = intVector;
				intVector4.Z = intVector.Z;
				while (intVector4.Z <= intVector2.Z)
				{
					intVector4.X = intVector.X;
					while (intVector4.X <= intVector2.X)
					{
						intVector4.Y = intVector.Y;
						int num4 = this.MakeIndex(intVector4);
						while (intVector4.Y <= intVector2.Y)
						{
							if (tp.TestThisType(Block.GetTypeIndex(this._blocks[num4])))
							{
								if (flag && Block.GetType(this._blocks[num4]).AllowSlopes)
								{
									if (intVector4.Y <= num2 && intVector4.Y >= num)
									{
										tp.ShapeHasSlopedSides = !this.SlopingIsBlocked(ref intVector, ref intVector2, ref intVector4);
									}
									else
									{
										tp.ShapeHasSlopedSides = false;
									}
								}
								IntVector3 intVector5 = intVector4 + this._worldMin;
								this.FillFacePlanes(intVector5);
								tp.TestShape(this._facePlanes, intVector5, Block.GetTypeIndex(this._blocks[num4]));
							}
							intVector4.Y++;
							num4++;
						}
						intVector4.X++;
					}
					intVector4.Z++;
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
			Vector3 vector = tp._end - tp._start;
			IntVector3 intVector = this.MakeIndexVectorFromPosition(tp._start);
			IntVector3 intVector2 = IntVector3.Add(this._worldMin, intVector);
			IntVector3 zero = IntVector3.Zero;
			Vector3 zero2 = Vector3.Zero;
			Vector3 zero3 = Vector3.Zero;
			float num = 0f;
			if (vector.X == 0f)
			{
				zero2.X = 2f;
				zero3.X = 0f;
			}
			else
			{
				zero3.X = Math.Abs(1f / vector.X);
				float num2 = (float)intVector2.X;
				if (vector.X > 0f)
				{
					num2 += 1f;
					zero.X = 1;
				}
				else
				{
					zero.X = -1;
				}
				zero2.X = (num2 - tp._start.X) / vector.X;
			}
			if (vector.Y == 0f)
			{
				zero2.Y = 2f;
				zero3.Y = 0f;
			}
			else
			{
				zero3.Y = Math.Abs(1f / vector.Y);
				float num2 = (float)intVector2.Y;
				if (vector.Y > 0f)
				{
					num2 += 1f;
					zero.Y = 1;
				}
				else
				{
					zero.Y = -1;
				}
				zero2.Y = (num2 - tp._start.Y) / vector.Y;
			}
			if (vector.Z == 0f)
			{
				zero2.Z = 2f;
				zero3.Z = 0f;
			}
			else
			{
				zero3.Z = Math.Abs(1f / vector.Z);
				float num2 = (float)intVector2.Z;
				if (vector.Z > 0f)
				{
					num2 += 1f;
					zero.Z = 1;
				}
				else
				{
					zero.Z = -1;
				}
				zero2.Z = (num2 - tp._start.Z) / vector.Z;
			}
			for (;;)
			{
				if (this.IsIndexValid(intVector))
				{
					int num3 = this.MakeIndex(intVector);
					BlockTypeEnum typeIndex = Block.GetTypeIndex(this._blocks[num3]);
					if (tp.TestThisType(typeIndex))
					{
						this.FillFacePlanes(intVector2);
						if (!tp.TestShape(this._facePlanes, intVector2, typeIndex))
						{
							break;
						}
					}
				}
				if (num >= 1f)
				{
					return;
				}
				if (zero2.X < zero2.Y)
				{
					if (zero2.X < zero2.Z)
					{
						num = zero2.X;
						zero2.X += zero3.X;
						intVector.X += zero.X;
						intVector2.X += zero.X;
					}
					else
					{
						num = zero2.Z;
						zero2.Z += zero3.Z;
						intVector.Z += zero.Z;
						intVector2.Z += zero.Z;
					}
				}
				else if (zero2.Y < zero2.Z)
				{
					num = zero2.Y;
					zero2.Y += zero3.Y;
					intVector.Y += zero.Y;
					intVector2.Y += zero.Y;
				}
				else
				{
					num = zero2.Z;
					zero2.Z += zero3.Z;
					intVector.Z += zero.Z;
					intVector2.Z += zero.Z;
				}
			}
		}

		public FallLockTestResult FallLockFace(Vector3 v, BlockFace f)
		{
			return this.FallLockFace(IntVector3.FromVector3(v), f);
		}

		public FallLockTestResult FallLockFace(IntVector3 v, BlockFace f)
		{
			IntVector3 intVector = IntVector3.Subtract(v, this._worldMin);
			if (this.IsIndexValid(intVector))
			{
				int num = this.MakeIndex(intVector);
				BlockType blockType = Block.GetType(this._blocks[num]);
				if (blockType.BlockPlayer)
				{
					IntVector3 neighborIndex = this.GetNeighborIndex(intVector, f);
					if (!this.IsIndexValid(neighborIndex))
					{
						return FallLockTestResult.SOLID_BLOCK_NEEDS_WALL;
					}
					int num2 = this.MakeIndex(neighborIndex);
					blockType = Block.GetType(this._blocks[num2]);
					if (!blockType.BlockPlayer)
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
			IntVector3 intVector = this.MakeIndexVectorFromPosition(location);
			if (!this.IsIndexValid(intVector))
			{
				return 0;
			}
			int num = 0;
			for (int i = intVector.Y; i < 128; i++)
			{
				intVector.Y = i;
				int num2 = this.MakeIndex(intVector);
				if (Block.GetType(this._blocks[num2]).BlockPlayer)
				{
					num++;
				}
			}
			return num;
		}

		public int DepthUnderSpaceRock(Vector3 location)
		{
			IntVector3 intVector = this.MakeIndexVectorFromPosition(location);
			if (!this.IsIndexValid(intVector))
			{
				return 0;
			}
			int num = 0;
			for (int i = intVector.Y; i < 128; i++)
			{
				intVector.Y = i;
				int num2 = this.MakeIndex(intVector);
				if (Block.GetType(this._blocks[num2])._type == BlockTypeEnum.SpaceRock)
				{
					num++;
				}
			}
			return num;
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
			IntVector3 intVector = this.MakeIndexVectorFromPosition(guess);
			if (!this.IsIndexValid(intVector))
			{
				return false;
			}
			int num = this.MakeIndex(intVector);
			if (Block.IsSky(this._blocks[num]))
			{
				return false;
			}
			int num2 = num;
			int i = intVector.Y;
			bool flag = false;
			while (i < 128)
			{
				if (Block.GetTypeIndex(this._blocks[num2]) == BlockTypeEnum.SpaceRock)
				{
					flag = true;
					break;
				}
				i++;
				num2++;
			}
			if (!flag)
			{
				return false;
			}
			num2 = num;
			i = intVector.Y;
			while (i >= 0)
			{
				if (Block.GetTypeIndex(this._blocks[num2]) == BlockTypeEnum.SpaceRock)
				{
					return true;
				}
				i--;
				num2--;
			}
			return false;
		}

		private int FindStepableHeight(ref IntVector3 v, int currentHeight)
		{
			if (!this.IsIndexValid(v))
			{
				return -1;
			}
			int num = this.MakeIndex(v);
			if (Block.GetType(this._blocks[num + currentHeight]).BlockPlayer)
			{
				int i = currentHeight;
				int num2 = Math.Min(currentHeight + 3, 128);
				bool flag = false;
				while (i < num2)
				{
					if (!Block.GetType(this._blocks[num + i]).BlockPlayer)
					{
						flag = true;
						break;
					}
					i++;
				}
				if (!flag)
				{
					return -1;
				}
				int num3 = 0;
				while (num3 < 2 && i < 128 && !Block.GetType(this._blocks[num + i]).BlockPlayer)
				{
					num3++;
					i++;
				}
				if (num3 >= 2)
				{
					return i;
				}
				return -1;
			}
			else
			{
				int num4 = currentHeight;
				int num5 = Math.Min(currentHeight + 3, 128);
				while (num4 < num5 && !Block.GetType(this._blocks[num + num4]).BlockPlayer)
				{
					num4++;
				}
				if (num4 - currentHeight < 2)
				{
					return -1;
				}
				num4 = currentHeight;
				while (num4 > 0 && !Block.GetType(this._blocks[num + num4]).BlockPlayer)
				{
					num4--;
				}
				num4++;
				if (Math.Abs(num4 - currentHeight) > 2)
				{
					return -1;
				}
				return num4;
			}
		}

		public Vector3 FindNearbySpawnPoint(Vector3 plrpos, int iterations, int range)
		{
			plrpos.X = (float)Math.Floor((double)plrpos.X) + 0.1f;
			plrpos.Y = (float)Math.Floor((double)plrpos.Y) + 0.1f;
			plrpos.Z = (float)Math.Floor((double)plrpos.Z) + 0.1f;
			IntVector3 intVector = this.MakeIndexVectorFromPosition(plrpos);
			if (!this.IsIndexValid(intVector))
			{
				return Vector3.Zero;
			}
			IntVector3 intVector2 = intVector;
			int num = intVector.Y;
			intVector2.Y = 0;
			int num2 = this.MakeIndex(intVector2);
			while (num > 0 && !Block.GetType(this._blocks[num2 + num]).BlockPlayer)
			{
				num--;
			}
			if (num < 0)
			{
				return Vector3.Zero;
			}
			num++;
			while (num < 128 && Block.GetType(this._blocks[num2 + num]).BlockPlayer)
			{
				num++;
			}
			if (num >= 128)
			{
				return Vector3.Zero;
			}
			int num3 = -1;
			for (int i = 0; i < iterations; i++)
			{
				bool flag = false;
				int num4;
				if (num3 == -1 && MathTools.RandomBool())
				{
					num4 = MathTools.RandomInt(0, 4);
				}
				else
				{
					num4 = num3 ^ 2;
				}
				int num5 = 0;
				while (!flag)
				{
					if (num4 != num3)
					{
						IntVector3 intVector3 = intVector2;
						if ((num4 & 1) != 0)
						{
							intVector3.X += (((num4 & 2) != 0) ? 1 : (-1));
						}
						else
						{
							intVector3.Z += (((num4 & 2) != 0) ? 1 : (-1));
						}
						int num6 = this.FindStepableHeight(ref intVector3, num);
						if (num6 != -1)
						{
							intVector2 = intVector3;
							num = num6;
							num3 = num4 ^ 2;
							flag = true;
						}
					}
					if (!flag)
					{
						num4 = (num4 + 1) % 4;
						num5++;
						if (num5 >= 4)
						{
							if (num3 == -1)
							{
								return Vector3.Zero;
							}
							num4 = num3;
							num3 = -1;
						}
					}
				}
			}
			intVector2.Y = num;
			intVector2 = IntVector3.Add(this._worldMin, intVector2);
			Vector3 vector = IntVector3.ToVector3(intVector2);
			vector.X += 0.5f;
			vector.Y += 0.1f;
			vector.Z += 0.5f;
			return vector;
		}

		public Vector3 FindAlienSpawnPoint(Vector3 guess, bool surfaceOkay)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 intVector = this.MakeIndexVectorFromPosition(guess);
			if (intVector.X < 0 || intVector.X >= 384)
			{
				return Vector3.Zero;
			}
			if (intVector.Z < 0 || intVector.Z >= 384)
			{
				return Vector3.Zero;
			}
			int y = intVector.Y;
			intVector.Y = 0;
			int num = this.MakeIndex(intVector);
			intVector.Y = 127;
			bool flag = false;
			int num2 = -1;
			int num3 = int.MaxValue;
			int num4 = 1;
			int num5 = 0;
			while (intVector.Y >= num4)
			{
				BlockTypeEnum typeIndex = Block.GetTypeIndex(this._blocks[num + intVector.Y]);
				if (!surfaceOkay && !flag)
				{
					if (typeIndex == BlockTypeEnum.SpaceRock)
					{
						flag = true;
					}
					num5 = 0;
				}
				else if (typeIndex == BlockTypeEnum.SpaceRock)
				{
					if (num5 > 1)
					{
						if (surfaceOkay)
						{
							num2 = intVector.Y;
							break;
						}
						int num6 = Math.Abs(intVector.Y - y);
						if (num6 <= num3)
						{
							num3 = num6;
							num2 = intVector.Y;
							num4 = y - num3;
						}
						else if (num2 != -1)
						{
							break;
						}
					}
					num5 = 0;
				}
				else if (!BlockType.GetType(typeIndex).BlockPlayer)
				{
					num5++;
				}
				else
				{
					num5 = 0;
				}
				intVector.Y--;
			}
			if (num2 != -1)
			{
				intVector.Y = num2 + 1;
				Vector3 vector = this.MakePositionFromIndexVector(intVector);
				vector.X += 0.5f;
				vector.Y += 0.1f;
				vector.Z += 0.5f;
				return vector;
			}
			return Vector3.Zero;
		}

		public Vector3 FindClosestCeiling(Vector3 guess)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 intVector = this.MakeIndexVectorFromPosition(guess);
			intVector.X = intVector.X.Clamp(0, 383);
			intVector.Y = intVector.Y.Clamp(0, 127);
			intVector.Z = intVector.Z.Clamp(0, 383);
			int num = this.MakeIndex(intVector);
			IntVector3 intVector2 = IntVector3.Zero;
			IntVector3 intVector3 = intVector;
			while (intVector3.Y < 128 && Block.GetType(this._blocks[num]).BlockPlayer)
			{
				num++;
				intVector3.Y++;
			}
			while (intVector3.Y < 128)
			{
				if (Block.GetType(this._blocks[num]).BlockPlayer)
				{
					intVector2 = intVector3;
					break;
				}
				num++;
				intVector3.Y++;
			}
			if (intVector2.Y != 0)
			{
				intVector2 = IntVector3.Add(this._worldMin, intVector2);
				Vector3 vector = IntVector3.ToVector3(intVector2);
				vector.X += 0.5f;
				vector.Y += 0.1f;
				vector.Z += 0.5f;
				return vector;
			}
			return Vector3.Zero;
		}

		public Vector3 FindSafeStartLocation(Vector3 guess)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 intVector = this.MakeIndexVectorFromPosition(guess);
			intVector.X = intVector.X.Clamp(0, 383);
			intVector.Y = intVector.Y.Clamp(0, 127);
			intVector.Z = intVector.Z.Clamp(0, 383);
			int num = -1;
			int num2 = 1;
			int num3 = (this.IsWaterWorld ? ((int)Math.Floor((double)(64f + this.WaterLevel))) : (-1));
			IntVector3 intVector2;
			for (;;)
			{
				for (int i = intVector.X; i < intVector.X + num2; i++)
				{
					if (i >= 0)
					{
						if (i >= 384)
						{
							break;
						}
						for (int j = intVector.Z; j < intVector.Z + num2; j++)
						{
							if (j >= 0)
							{
								if (j >= 384)
								{
									break;
								}
								bool flag = false;
								if (i == intVector.X || i == intVector.X + num2 - 1)
								{
									flag = true;
								}
								else if (j == intVector.Z || j == intVector.Z + num2 - 1)
								{
									flag = true;
								}
								if (flag)
								{
									intVector2 = new IntVector3(i, intVector.Y, j);
									int num4 = this.MakeIndex(intVector2);
									if (Block.GetType(this._blocks[num4]).BlockPlayer)
									{
										while (intVector2.Y < 127)
										{
											if (!Block.GetType(this._blocks[num4]).BlockPlayer)
											{
												break;
											}
											intVector2.Y++;
											num4++;
										}
									}
									else
									{
										while (intVector2.Y > 1)
										{
											if (Block.GetType(this._blocks[num4]).BlockPlayer)
											{
												intVector2.Y++;
												break;
											}
											intVector2.Y--;
											num4--;
										}
									}
									IntVector3 intVector3 = intVector2;
									num4 = this.MakeIndex(intVector3);
									while (intVector3.Y < 128)
									{
										if (!Block.GetType(this._blocks[num4]).BlockPlayer && (intVector3.Y == 127 || !Block.GetType(this._blocks[num4 + 1]).BlockPlayer))
										{
											num = intVector3.Y;
											break;
										}
										intVector3.Y++;
										num4++;
									}
									intVector3 = intVector2;
									intVector3.Y--;
									num4 = this.MakeIndex(intVector3);
									while (intVector3.Y > 0)
									{
										if (!Block.GetType(this._blocks[num4]).BlockPlayer && !Block.GetType(this._blocks[num4 + 1]).BlockPlayer)
										{
											if (intVector2.Y - intVector3.Y < num - intVector2.Y)
											{
												num = intVector3.Y;
												break;
											}
											break;
										}
										else
										{
											intVector3.Y--;
											num4--;
										}
									}
									if (num >= num3)
									{
										goto Block_18;
									}
								}
							}
						}
					}
				}
				intVector.X--;
				intVector.Z--;
				num2 += 2;
				if (num2 > 256)
				{
					goto Block_19;
				}
			}
			Block_18:
			Vector3 vector = this.MakePositionFromIndexVector(new IntVector3(intVector2.X, num, intVector2.Z));
			vector.X += 0.5f;
			vector.Y += 0.1f;
			vector.Z += 0.5f;
			return vector;
			Block_19:
			Vector3 vector2 = this.MakePositionFromIndexVector(new IntVector3(intVector.X, 128, intVector.Z));
			vector2.X += 0.5f;
			vector2.Y += 0.1f;
			vector2.Z += 0.5f;
			return vector2;
		}

		public bool ContainsBlockType(Vector3 center, int radius, BlockTypeEnum type, ref float distToClosest)
		{
			IntVector3 intVector = this.MakeIndexVectorFromPosition(center);
			IntVector3 intVector2 = new IntVector3(intVector.X - radius, 0, intVector.Z - radius);
			IntVector3 intVector3 = default(IntVector3);
			distToClosest = float.MaxValue;
			for (int i = 0; i < radius * 2; i++)
			{
				intVector3.Z = intVector2.Z + i;
				if (intVector3.Z >= 0 && intVector3.Z < 384)
				{
					for (int j = 0; j < radius * 2; j++)
					{
						intVector3.X = intVector2.X + j;
						if (intVector3.X >= 0 && intVector3.X < 384)
						{
							float num = (float)(MathTools.Square(intVector3.X - intVector.X) + MathTools.Square(intVector3.Z - intVector.Z));
							if (num < distToClosest)
							{
								int num2 = this.MakeIndex(intVector3);
								int k = 0;
								while (k < 128)
								{
									if (Block.GetTypeIndex(this._blocks[num2]) == type)
									{
										distToClosest = num;
										break;
									}
									k++;
									num2++;
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
			IntVector3 chunkVectorIndex = this.GetChunkVectorIndex(r);
			return chunkVectorIndex.X < 24 && chunkVectorIndex.X >= 0 && chunkVectorIndex.Z < 24 && chunkVectorIndex.Z >= 0;
		}

		public bool RegionIsLoaded(Vector3 r)
		{
			if (!this.IsReady)
			{
				return false;
			}
			IntVector3 chunkVectorIndex = this.GetChunkVectorIndex(r);
			if (chunkVectorIndex.X >= 24 || chunkVectorIndex.X < 0 || chunkVectorIndex.Z >= 24 || chunkVectorIndex.Z < 0)
			{
				return false;
			}
			int num = chunkVectorIndex.X + chunkVectorIndex.Z * 24;
			RenderChunk chunk = this._chunks[num].GetChunk();
			bool flag = chunk != null && chunk.HasGeometry();
			chunk.Release();
			return flag;
		}

		public Vector3 FindTopmostGroundLocation(Vector3 guess)
		{
			guess.X = (float)Math.Floor((double)guess.X) + 0.1f;
			guess.Y = (float)Math.Floor((double)guess.Y) + 0.1f;
			guess.Z = (float)Math.Floor((double)guess.Z) + 0.1f;
			IntVector3 intVector = this.MakeIndexVectorFromPosition(guess);
			intVector.X = intVector.X.Clamp(0, 383);
			intVector.Y = 127;
			intVector.Z = intVector.Z.Clamp(0, 383);
			int num = 1;
			int num2 = (this.IsWaterWorld ? ((int)Math.Floor((double)(64f + this.WaterLevel))) : (-1));
			IntVector3 intVector2;
			for (;;)
			{
				for (int i = intVector.X; i < intVector.X + num; i++)
				{
					if (i >= 0)
					{
						if (i >= 384)
						{
							break;
						}
						for (int j = intVector.Z; j < intVector.Z + num; j++)
						{
							if (j >= 0)
							{
								if (j >= 384)
								{
									break;
								}
								bool flag = false;
								if (i == intVector.X || i == intVector.X + num - 1)
								{
									flag = true;
								}
								else if (j == intVector.Z || j == intVector.Z + num - 1)
								{
									flag = true;
								}
								if (flag)
								{
									intVector2 = new IntVector3(i, 127, j);
									int num3 = this.MakeIndex(intVector2);
									while (intVector2.Y > 0 && !Block.GetType(this._blocks[num3]).BlockPlayer)
									{
										intVector2.Y--;
										num3--;
									}
									intVector2.Y++;
									if (intVector2.Y >= num2)
									{
										goto Block_10;
									}
								}
							}
						}
					}
				}
				intVector.X--;
				intVector.Z--;
				num += 2;
				if (num > 256)
				{
					goto Block_11;
				}
			}
			Block_10:
			Vector3 vector = this.MakePositionFromIndexVector(intVector2);
			vector.X += 0.5f;
			vector.Y += 0.1f;
			vector.Z += 0.5f;
			return vector;
			Block_11:
			Vector3 vector2 = this.MakePositionFromIndexVector(new IntVector3(intVector.X, 128, intVector.Z));
			vector2.X += 0.5f;
			vector2.Y += 0.1f;
			vector2.Z += 0.5f;
			return vector2;
		}

		public Vector3 ClipPositionToLoadedWorld(Vector3 pos, float radius)
		{
			if (this.IsReady)
			{
				Vector3 vector = IntVector3.ToVector3(this._worldMin);
				pos -= vector;
				pos.X = pos.X.Clamp(radius, 384f - radius);
				pos.Z = pos.Z.Clamp(radius, 384f - radius);
				pos += vector;
			}
			return pos;
		}

		public void StepInitialization()
		{
			if (this._updateTasksRemaining)
			{
				return;
			}
			IntVector3 intVector;
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
					int num = 0;
					this._allChunksLoaded = true;
					intVector = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
					for (int i = 0; i < this._radiusOrderOffsets.Length; i++)
					{
						IntVector3 intVector2 = IntVector3.Add(intVector, this._radiusOrderOffsets[i]);
						if (intVector2.X >= 0 && intVector2.X < 24 && intVector2.Z >= 0 && intVector2.Z < 24)
						{
							int num2 = intVector2.X + intVector2.Z * 24;
							if (this._chunks[num2]._action == BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
							{
								this._computeBlocksPool.Add(num2);
								this.AddSurroundingBlocksToLightList(num2);
								num++;
								if (num == this._maxChunksAtOnce)
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
			intVector = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
			for (int j = 0; j < this._radiusOrderOffsets.Length; j++)
			{
				IntVector3 intVector3 = IntVector3.Add(intVector, this._radiusOrderOffsets[j]);
				if (intVector3.X >= 0 && intVector3.X < 24 && intVector3.Z >= 0 && intVector3.Z < 24)
				{
					int num3 = intVector3.X + intVector3.Z * 24;
					if (this._chunks[num3]._action == BlockTerrain.NextChunkAction.NONE && !this._chunks[num3]._mods.Empty && !this.ChunkOrNeighborInUse(num3) && this.ApplyModList(num3))
					{
						this.IncrChunkInUse(num3);
						BlockTerrain.BuildTaskData buildTaskData = BlockTerrain.BuildTaskData.Alloc();
						buildTaskData._intData0 = num3;
						this._updateTasksRemaining.Increment();
						TaskDispatcher.Instance.AddRushTask(this._finishSetBlockDelegate, buildTaskData);
						return;
					}
				}
			}
		}

		protected void AddSurroundingBlocksToLightList(int index)
		{
			this._computeLightingPool.Add(index);
			this._chunks[index]._numUsers.Increment();
			IntVector3 intVector = this.MakeIndexVectorFromChunkIndex(index);
			for (int i = 0; i < 4; i++)
			{
				IntVector3 intVector2 = intVector;
				switch (i)
				{
				case 0:
					intVector2.X += 16;
					break;
				case 1:
					intVector2.X -= 16;
					break;
				case 2:
					intVector2.Z += 16;
					break;
				case 3:
					intVector2.Z -= 16;
					break;
				}
				if (this.IsIndexValid(intVector2))
				{
					int num = this.MakeChunkIndexFromIndexVector(intVector2);
					if (this._chunks[num]._action >= BlockTerrain.NextChunkAction.NEEDS_GEOMETRY)
					{
						this._computeLightingPool.Add(num);
						this._chunks[num]._numUsers.Increment();
					}
				}
			}
		}

		protected void ApplyModListDuringCreate(int index)
		{
			SynchronizedQueue<BlockTerrain.PendingMod> mods = this._chunks[index]._mods;
			while (!mods.Empty)
			{
				BlockTerrain.PendingMod pendingMod = mods.Dequeue();
				IntVector3 intVector = IntVector3.Subtract(pendingMod._worldPosition, this._worldMin);
				int num = this.MakeIndex(intVector);
				if (Block.GetType(this._blocks[num]).SpawnEntity)
				{
					this.RemoveItemBlockEntity(Block.GetTypeIndex(this._blocks[num]), pendingMod._worldPosition);
				}
				if (BlockType.GetType(pendingMod._blockType).SpawnEntity)
				{
					this.CreateItemBlockEntity(pendingMod._blockType, pendingMod._worldPosition);
				}
				this._blocks[num] = Block.SetType(0, pendingMod._blockType);
				pendingMod.Release();
			}
		}

		protected bool ApplyModList(int index)
		{
			bool flag = false;
			SynchronizedQueue<BlockTerrain.PendingMod> mods = this._chunks[index]._mods;
			this._updateLightingPool.Clear();
			this._updateLightingPool.ResetAABB();
			while (!mods.Empty)
			{
				BlockTerrain.PendingMod pendingMod = mods.Dequeue();
				IntVector3 intVector = IntVector3.Subtract(pendingMod._worldPosition, this._worldMin);
				int num = this.MakeIndex(intVector);
				if (Block.GetTypeIndex(this._blocks[num]) != pendingMod._blockType)
				{
					if (Block.GetType(this._blocks[num]).SpawnEntity)
					{
						this.RemoveItemBlockEntity(Block.GetTypeIndex(this._blocks[num]), pendingMod._worldPosition);
					}
					if (BlockType.GetType(pendingMod._blockType).SpawnEntity)
					{
						this.CreateItemBlockEntity(pendingMod._blockType, pendingMod._worldPosition);
					}
					this._blocks[num] = Block.SetType(0, pendingMod._blockType);
					this._updateLightingPool.UpdateMinAABB(ref intVector);
					this.ResetSkyAndEmitterLightingForRegion(intVector, intVector);
					this.AddBlockToLightList(intVector, this._updateLightingPool);
					flag = true;
				}
				pendingMod.Release();
			}
			return flag;
		}

		protected void IncrChunkInUse(int index)
		{
			IntVector3 intVector = this.MakeIndexVectorFromChunkIndex(index);
			for (int i = -2; i < 3; i++)
			{
				IntVector3 intVector2 = intVector;
				intVector2.X += i * 16;
				for (int j = -2; j < 3; j++)
				{
					intVector2.Z += j * 128;
					if (this.IsIndexValid(intVector2))
					{
						int num = this.MakeChunkIndexFromIndexVector(intVector2);
						if (this._chunks[num]._action > BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
						{
							this._chunks[num]._numUsers.Increment();
						}
					}
				}
			}
		}

		protected void DecrChunkInUse(int index)
		{
			IntVector3 intVector = this.MakeIndexVectorFromChunkIndex(index);
			for (int i = -2; i < 3; i++)
			{
				IntVector3 intVector2 = intVector;
				intVector2.X += i * 16;
				for (int j = -2; j < 3; j++)
				{
					intVector2.Z += j * 128;
					if (this.IsIndexValid(intVector2))
					{
						int num = this.MakeChunkIndexFromIndexVector(intVector2);
						if (this._chunks[num]._action > BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
						{
							this._chunks[num]._numUsers.Decrement();
						}
					}
				}
			}
		}

		protected bool ChunkOrNeighborInUse(int index)
		{
			IntVector3 intVector = this.MakeIndexVectorFromChunkIndex(index);
			for (int i = -2; i < 3; i++)
			{
				IntVector3 intVector2 = intVector;
				intVector2.X += i * 16;
				for (int j = -2; j < 3; j++)
				{
					intVector2.Z += j * 128;
					if (this.IsIndexValid(intVector2))
					{
						int num = this.MakeChunkIndexFromIndexVector(intVector2);
						if (this._chunks[num]._numUsers)
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
			IntVector3 intVector = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
			intVector.X -= 12;
			intVector.Z -= 12;
			if (intVector.X < 0)
			{
				intVector.X++;
			}
			if (intVector.Z < 0)
			{
				intVector.Z++;
			}
			return (intVector.X != 0 || intVector.Z != 0) && this.ShiftTerrain(intVector.X, intVector.Z);
		}

		public void CenterOn(Vector3 eye, bool scrollIfPossible)
		{
			if (!this.IsReady)
			{
				return;
			}
			IntVector3 chunkVectorIndex = this.GetChunkVectorIndex(eye);
			if (scrollIfPossible)
			{
				bool flag = false;
				if (chunkVectorIndex.X < 0 || chunkVectorIndex.X >= 24 || chunkVectorIndex.Z < 0 || chunkVectorIndex.Z >= 24)
				{
					flag = true;
				}
				else
				{
					IntVector3 intVector = new IntVector3(this._currentEyeChunkIndex % 24, 0, this._currentEyeChunkIndex / 24);
					IntVector3 intVector2 = IntVector3.Subtract(chunkVectorIndex, intVector);
					if (Math.Abs(intVector2.X) > 1 || Math.Abs(intVector2.Z) > 1)
					{
						int num = 0;
						for (int i = 0; i < 25; i++)
						{
							IntVector3 intVector3 = IntVector3.Add(chunkVectorIndex, this._radiusOrderOffsets[i]);
							if (intVector3.X >= 0 && intVector3.X < 24 && intVector3.Z >= 0 && intVector3.Z < 24)
							{
								int num2 = intVector3.X + intVector3.Z * 24;
								if (this._chunks[num2]._action != BlockTerrain.NextChunkAction.WAITING_TO_LOAD)
								{
									num++;
								}
							}
						}
						if (num < 25)
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					this.Teleport(eye);
					return;
				}
			}
			chunkVectorIndex.X = chunkVectorIndex.X.Clamp(0, 23);
			chunkVectorIndex.Z = chunkVectorIndex.Z.Clamp(0, 23);
			Interlocked.Exchange(ref this._currentEyeChunkIndex, chunkVectorIndex.X + chunkVectorIndex.Z * 24);
			IntVector3 intVector4 = chunkVectorIndex;
			intVector4.X = (intVector4.X - 12).Clamp(-1, 0);
			intVector4.Z = (intVector4.Z - 12).Clamp(-1, 0);
			Interlocked.Exchange(ref this._currentRenderOrder, Math.Abs(intVector4.Z * 2 + intVector4.X));
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
			Vector3 vector = new Vector3(Math.Abs(normal.X), Math.Abs(normal.Y), Math.Abs(normal.Z));
			if (vector.X > vector.Y)
			{
				if (vector.X > vector.Z)
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
			else if (vector.Y > vector.Z)
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
			Vector2 lightAtPoint = this.GetLightAtPoint(position);
			float y = lightAtPoint.Y;
			float x = lightAtPoint.X;
			direction = -this.VectorToSun;
			ambient = Vector3.Multiply(this.AmbientSunColor.ToVector3(), x) + Vector3.Multiply(this.TorchColor.ToVector3(), y * (1f - x * this.SunlightColor.ToVector3().Y));
			directional = Vector3.Multiply(this.SunlightColor.ToVector3(), (float)Math.Pow((double)x, 30.0));
		}

		public void GetEnemyLighting(Vector3 position, ref Vector3 l1d, ref Vector3 l1c, ref Vector3 l2d, ref Vector3 l2c, ref Vector3 ambient)
		{
			Vector2 lightAtPoint = this.GetLightAtPoint(position);
			float y = lightAtPoint.Y;
			float x = lightAtPoint.X;
			ambient = Vector3.Multiply(this.AmbientSunColor.ToVector3(), x) + Vector3.Multiply(this.TorchColor.ToVector3(), 0.5f * y * (1f - x * this.SunlightColor.ToVector3().Y));
			l1d = Vector3.Negate(this.VectorToSun);
			l2d = position - this.EyePos;
			float num = l2d.LengthSquared();
			if (num > 0f)
			{
				l2d *= 1f / (float)Math.Sqrt((double)num);
			}
			l1c = Vector3.Multiply(this.SunlightColor.ToVector3(), (float)Math.Pow((double)x, 30.0));
			l2c = Vector3.Multiply(this.TorchColor.ToVector3(), 0.5f * y * (1f - x * this.SunlightColor.ToVector3().Y));
		}

		public Vector2 GetLightAtPoint(Vector3 position)
		{
			IntVector3 intVector = IntVector3.FromVector3(position);
			this.FillCubeLightTable(intVector, ref this.avatarSun, ref this.avatarTorch);
			Vector3 vector = IntVector3.ToVector3(intVector);
			Vector3 zero = Vector3.Zero;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = 0;
			zero.Z = -1f;
			while (zero.Z < 1.5f)
			{
				zero.Y = -1f;
				while (zero.Y < 1.5f)
				{
					zero.X = -1f;
					while (zero.X < 1.5f)
					{
						float num6 = ((2.25f - (vector + zero - position).LengthSquared()) / 2.25f).Clamp(0f, 1f);
						if (this.avatarTorch[num5] != -1f)
						{
							num3 += num6 * this.avatarTorch[num5];
							num += num6;
						}
						if (this.avatarSun[num5] != -1f)
						{
							num4 += num6 * this.avatarSun[num5];
							num2 += num6;
						}
						zero.X += 1f;
						num5++;
					}
					zero.Y += 1f;
				}
				zero.Z += 1f;
			}
			if (num > 0f)
			{
				num3 /= num;
			}
			if (num2 > 0f)
			{
				num4 /= num2;
			}
			return new Vector2(num4, num3);
		}

		public Vector2 GetSimpleLightAtPoint(Vector3 position)
		{
			Vector2 zero = Vector2.Zero;
			IntVector3 intVector = IntVector3.FromVector3(position);
			IntVector3 intVector2 = IntVector3.Subtract(intVector, this._worldMin);
			if (this.IsIndexValid(intVector2))
			{
				int blockAt = this.GetBlockAt(intVector2);
				BlockTypeEnum typeIndex = Block.GetTypeIndex(blockAt);
				if (typeIndex == BlockTypeEnum.NumberOfBlocks || Block.IsInList(blockAt))
				{
					zero.X = 1f;
					zero.Y = 0f;
				}
				else if (BlockType.GetType(typeIndex).Opaque)
				{
					zero.X = -1f;
					zero.Y = -1f;
				}
				else
				{
					zero.X = (float)Block.GetSunLightLevel(blockAt) / 15f;
					zero.Y = (float)Block.GetTorchLightLevel(blockAt) / 15f;
				}
			}
			return zero;
		}

		public float GetSimpleSunlightAtPoint(Vector3 position)
		{
			float num = 0f;
			IntVector3 intVector = IntVector3.FromVector3(position);
			IntVector3 intVector2 = IntVector3.Subtract(intVector, this._worldMin);
			if (this.IsIndexValid(intVector2))
			{
				int blockAt = this.GetBlockAt(intVector2);
				BlockTypeEnum typeIndex = Block.GetTypeIndex(blockAt);
				if (typeIndex == BlockTypeEnum.NumberOfBlocks || Block.IsInList(blockAt))
				{
					num = 1f;
				}
				else if (BlockType.GetType(typeIndex).Opaque)
				{
					num = -1f;
				}
				else
				{
					num = (float)Block.GetSunLightLevel(blockAt) / 15f;
				}
			}
			return num;
		}

		public float GetSimpleTorchlightAtPoint(Vector3 position)
		{
			float num = 0f;
			IntVector3 intVector = IntVector3.FromVector3(position);
			IntVector3 intVector2 = IntVector3.Subtract(intVector, this._worldMin);
			if (this.IsIndexValid(intVector2))
			{
				int blockAt = this.GetBlockAt(intVector2);
				BlockTypeEnum typeIndex = Block.GetTypeIndex(blockAt);
				if (typeIndex == BlockTypeEnum.NumberOfBlocks || Block.IsInList(blockAt))
				{
					num = 1f;
				}
				else if (BlockType.GetType(typeIndex).Opaque)
				{
					num = -1f;
				}
				else
				{
					num = (float)Block.GetTorchLightLevel(blockAt) / 15f;
				}
			}
			return num;
		}

		public void CreateItemBlockEntity(BlockTypeEnum blockType, IntVector3 location)
		{
			BlockTerrain.ItemBlockCommand itemBlockCommand = BlockTerrain.ItemBlockCommand.Alloc();
			itemBlockCommand.AddItem = true;
			itemBlockCommand.BlockType = blockType;
			itemBlockCommand.WorldPosition = location;
			this.ItemBlockCommandQueue.Queue(itemBlockCommand);
		}

		public void RemoveItemBlockEntity(BlockTypeEnum blockType, IntVector3 location)
		{
			BlockTerrain.ItemBlockCommand itemBlockCommand = BlockTerrain.ItemBlockCommand.Alloc();
			itemBlockCommand.AddItem = false;
			itemBlockCommand.WorldPosition = location;
			itemBlockCommand.BlockType = blockType;
			this.ItemBlockCommandQueue.Queue(itemBlockCommand);
		}

		public bool SetBlock(IntVector3 worldIndex, BlockTypeEnum type)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.MOD;
			chunkCacheCommand._worldPosition = worldIndex;
			chunkCacheCommand._blockType = type;
			chunkCacheCommand._priority = 1;
			ChunkCache.Instance.AddCommand(chunkCacheCommand);
			if (!this.IsReady)
			{
				return false;
			}
			IntVector3 intVector = IntVector3.Subtract(worldIndex, this._worldMin);
			if (this.IsIndexValid(intVector))
			{
				int num = this.MakeChunkIndexFromIndexVector(intVector);
				lock (this._chunks[num]._mods)
				{
					for (BlockTerrain.PendingMod pendingMod = this._chunks[num]._mods.Front; pendingMod != null; pendingMod = (BlockTerrain.PendingMod)pendingMod.NextNode)
					{
						if (pendingMod._worldPosition.Equals(worldIndex))
						{
							pendingMod._blockType = type;
							return false;
						}
					}
				}
				if (this._chunks[num]._action > BlockTerrain.NextChunkAction.COMPUTING_BLOCKS && Block.GetTypeIndex(this._blocks[this.MakeIndex(intVector)]) == type)
				{
					return false;
				}
				BlockTerrain.PendingMod pendingMod2 = BlockTerrain.PendingMod.Alloc();
				pendingMod2._worldPosition = worldIndex;
				pendingMod2._blockType = type;
				this._chunks[num]._mods.Queue(pendingMod2);
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
			Vector3 vector = this.BelowWaterColor.ToVector3();
			return vector * this.SunlightColor.ToVector3();
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
			float num = this.WaterLevel - this.EyePos.Y;
			if (num < 0f)
			{
				num = 0f;
			}
			else
			{
				num = Math.Min(num / 12f, 1f - this.SunlightColor.ToVector3().X * this.SunlightColor.ToVector3().X);
			}
			color = this.GetActualWaterColor();
			return num;
		}

		public void DrawReflection(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (!this.IsReady)
			{
				return;
			}
			this._updateTasksRemaining.Increment();
			this._boundingFrustum.Matrix = view * projection;
			Matrix reflectionMatrix = this.GetReflectionMatrix();
			this._effect.Parameters["Projection"].SetValue(projection);
			this._effect.Parameters["World"].SetValue(Matrix.Identity);
			this._effect.Parameters["View"].SetValue(view);
			this._effect.Parameters["WaterLevel"].SetValue(this.WaterLevel - 0.5f);
			this._effect.Parameters["EyePosition"].SetValue(Vector3.Transform(this.EyePos, reflectionMatrix));
			this._effect.Parameters["LightDirection"].SetValue(this.VectorToSun);
			this._effect.Parameters["TorchLight"].SetValue(this.TorchColor.ToVector3());
			this._effect.Parameters["SunLight"].SetValue(this.SunlightColor.ToVector3());
			this._effect.Parameters["AmbientSun"].SetValue(this.AmbientSunColor.ToVector3());
			this._effect.Parameters["SunSpecular"].SetValue(this.SunSpecular.ToVector3());
			this._effect.Parameters["FogColor"].SetValue(this.FogColor.ToVector3());
			device.BlendState = BlendState.AlphaBlend;
			device.Indices = this._staticIB;
			int num = 0;
			IntVector3 chunkVectorIndex = this.GetChunkVectorIndex(this.EyePos);
			for (int i = 0; i < this._radiusOrderOffsets.Length; i++)
			{
				IntVector3 intVector = IntVector3.Add(chunkVectorIndex, this._radiusOrderOffsets[i]);
				if (intVector.X >= 0 && intVector.X < 24 && intVector.Z >= 0 && intVector.Z < 24)
				{
					int num2 = intVector.X + intVector.Z * 24;
					RenderChunk chunk = this._chunks[num2].GetChunk();
					if (chunk.TouchesFrustum(this._boundingFrustum))
					{
						this._renderIndexList[num++] = num2;
					}
					chunk.Release();
				}
			}
			if (num > 0)
			{
				this._effect.CurrentTechnique = this._effect.Techniques[3];
				device.BlendState = BlendState.Opaque;
				if (this.EyePos.Y >= this.WaterLevel)
				{
					device.RasterizerState = RasterizerState.CullClockwise;
				}
				for (int j = 0; j < num; j++)
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
							BlockTerrain.QueuedBufferBuild queuedBufferBuild = list[j];
							queuedBufferBuild.Chunk.SkipBuildingBuffers();
							queuedBufferBuild.Chunk.Release();
							this._chunks[queuedBufferBuild.Index]._action = BlockTerrain.NextChunkAction.NONE;
							this._chunks[queuedBufferBuild.Index]._numUsers.Decrement();
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
					List<BlockTerrain.QueuedBufferBuild> vertexBuildListIncoming = this._vertexBuildListIncoming;
					this._vertexBuildListIncoming = this._vertexBuildListOutgoing;
					this._vertexBuildListOutgoing = vertexBuildListIncoming;
				}
				for (int i = 0; i < this._vertexBuildListOutgoing.Count; i++)
				{
					BlockTerrain.QueuedBufferBuild queuedBufferBuild = this._vertexBuildListOutgoing[i];
					queuedBufferBuild.Chunk.FinishBuildingBuffers(this._graphicsDevice);
					this._chunks[queuedBufferBuild.Index].ReplaceChunk(queuedBufferBuild.Chunk);
					queuedBufferBuild.Chunk.Release();
					queuedBufferBuild.Chunk = null;
					this._chunks[queuedBufferBuild.Index]._action = BlockTerrain.NextChunkAction.NONE;
					this._chunks[queuedBufferBuild.Index]._numUsers.Decrement();
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
				float num = value.Clamp(0f, 1f);
				if (value != this._drawDistance)
				{
					int num2 = (int)Math.Floor((double)(8f * num)) + 4;
					this._farthestDrawDistanceSQ = num2 * num2;
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
					Vector3 vector = default(Vector3);
					vector.Z = this.WaterLevel - this.EyePos.Y;
					if (vector.Z >= 0f)
					{
						vector.X = vector.Z;
						vector.Y = 100000f;
					}
					else
					{
						vector.X = 0f;
						vector.Y = 1f;
						vector.Z = -vector.Z;
					}
					this._effect.Parameters["EyeWaterConstants"].SetValue(vector);
					this._effect.Parameters["TorchLight"].SetValue(this.TorchColor.ToVector3());
					this._effect.Parameters["LightDirection"].SetValue(this.VectorToSun);
					if (CastleMinerZGame.Instance.GameScreen._sky.drawLightning)
					{
						float num = (float)this.AmbientSunColor.G / 255f;
						Vector3 vector2 = Vector3.Lerp(new Vector3(0.9f, 0.95f, 1f), this.SunlightColor.ToVector3(), num);
						this._effect.Parameters["SunLight"].SetValue(vector2);
						vector2 = Vector3.Lerp(new Vector3(0.9f, 0.95f, 1f), this.AmbientSunColor.ToVector3(), num);
						this._effect.Parameters["AmbientSun"].SetValue(vector2);
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
					int num2 = 0;
					IntVector3 chunkVectorIndex = this.GetChunkVectorIndex(this.EyePos);
					for (int i = 0; i < this._radiusOrderOffsets.Length; i++)
					{
						IntVector3 intVector = this._radiusOrderOffsets[i];
						if (intVector.X * intVector.X < this._farthestDrawDistanceSQ && intVector.Y * intVector.Y < this._farthestDrawDistanceSQ && intVector.Z * intVector.Z < this._farthestDrawDistanceSQ)
						{
							IntVector3 intVector2 = IntVector3.Add(chunkVectorIndex, intVector);
							if (intVector2.X >= 0 && intVector2.X < 24 && intVector2.Z >= 0 && intVector2.Z < 24)
							{
								int num3 = intVector2.X + intVector2.Z * 24;
								RenderChunk chunk = this._chunks[num3].GetChunk();
								if (chunk.TouchesFrustum(this._boundingFrustum))
								{
									this._renderIndexList[num2++] = num3;
								}
								chunk.Release();
							}
						}
					}
					if (num2 > 0)
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
						for (int j = 0; j < 2; j++)
						{
							for (int k = 0; k < num2; k++)
							{
								RenderChunk chunk2 = this._chunks[this._renderIndexList[k]].GetChunk();
								chunk2.Draw(this._graphicsDevice, this._effect, j == 1, this._boundingFrustum);
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
				BlockTerrain.BlockReference blockReference = BlockTerrain.BlockReference.Alloc();
				blockReference.SetIndex(x, y, z);
				return blockReference;
			}

			public static BlockTerrain.BlockReference Alloc(IntVector3 i)
			{
				BlockTerrain.BlockReference blockReference = BlockTerrain.BlockReference.Alloc();
				blockReference.SetIndex(i);
				return blockReference;
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
				int num = Interlocked.Increment(ref this._nextOffset);
				if (num < 576)
				{
					this._pool[num] = index;
					BlockTerrain.Instance._chunks[index]._action = this._action;
				}
			}

			public virtual void Drain()
			{
				BlockTerrain instance = BlockTerrain.Instance;
				GatherTask gatherTask = TaskDispatcher.Instance.AddGatherTask(instance._stepUpdateDelegate, null);
				IntVector3 worldMin = instance._worldMin;
				for (int i = 0; i <= this._nextOffset; i++)
				{
					int num = this._pool[i];
					BlockTerrain.BuildTaskData buildTaskData = BlockTerrain.BuildTaskData.Alloc();
					buildTaskData._intVec0 = IntVector3.Add(worldMin, instance.MakeIndexVectorFromChunkIndex(num));
					buildTaskData._intData0 = num;
					instance._chunks[num]._action = this._nextAction;
					gatherTask.AddTask(this._work, buildTaskData);
				}
				this.Clear();
				instance.IncrementBuildTasks();
				gatherTask.Start();
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
				BlockTerrain instance = BlockTerrain.Instance;
				GatherTask gatherTask = TaskDispatcher.Instance.AddGatherTask(instance._stepUpdateDelegate, null);
				gatherTask.SetCount(this._nextOffset + 1);
				IntVector3 worldMin = instance._worldMin;
				instance.IncrementBuildTasks();
				for (int i = 0; i <= this._nextOffset; i++)
				{
					int num = this._pool[i];
					BlockTerrain.BuildTaskData buildTaskData = BlockTerrain.BuildTaskData.Alloc();
					buildTaskData._intVec0 = IntVector3.Add(worldMin, instance.MakeIndexVectorFromChunkIndex(num));
					buildTaskData._intData0 = num;
					instance._chunks[num]._action = this._nextAction;
					Task task = Task.Alloc();
					task.Init(this._work, buildTaskData, gatherTask);
					ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
					chunkCacheCommand._context = task;
					chunkCacheCommand._worldPosition = CachedChunk.MakeChunkCorner(buildTaskData._intVec0);
					if (this._loadedDelegate == null)
					{
						chunkCacheCommand._callback = new ChunkCacheCommandDelegate(this.ChunkLoaded);
					}
					else
					{
						chunkCacheCommand._callback = this._loadedDelegate;
					}
					chunkCacheCommand._command = ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN;
					chunkCacheCommand._priority = 1;
					this._chunksInFlight++;
					ChunkCache.Instance.AddCommand(chunkCacheCommand);
				}
				base.Clear();
			}

			public void ChunkLoaded(ChunkCacheCommand cmd)
			{
				this._chunksInFlight--;
				Task task = (Task)cmd._context;
				bool flag = true;
				if (cmd._command != ChunkCacheCommandEnum.RESETWAITINGCHUNKS && !BlockTerrain.Instance._resetRequested)
				{
					int num = BlockTerrain.Instance.MakeChunkIndexFromWorldIndexVector(cmd._worldPosition);
					if (num != -1)
					{
						BlockTerrain.Instance._chunks[num]._delta = cmd._delta;
						flag = false;
					}
				}
				else
				{
					int num2 = BlockTerrain.Instance.MakeChunkIndexFromWorldIndexVector(cmd._worldPosition);
					if (num2 != -1)
					{
						BlockTerrain.Instance._chunks[num2]._action = BlockTerrain.NextChunkAction.WAITING_TO_LOAD;
					}
				}
				if (flag)
				{
					BlockTerrain.BuildTaskData buildTaskData = task._context as BlockTerrain.BuildTaskData;
					if (buildTaskData != null)
					{
						buildTaskData._skipProcessing = true;
					}
				}
				TaskDispatcher.Instance.AddTask(task);
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
				int num = Interlocked.Increment(ref this._currentIndex);
				if (num < this._length)
				{
					this._currentList[num] = index;
				}
				else
				{
					BlockTerrain.LightingPool._blocks[index] &= -1025;
				}
				this._maxUsed = ((num > this._maxUsed) ? num : this._maxUsed);
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
				BlockTerrain.PendingMod pendingMod2;
				for (BlockTerrain.PendingMod pendingMod = this._mods.Clear(); pendingMod != null; pendingMod = pendingMod2)
				{
					pendingMod2 = pendingMod.NextNode as BlockTerrain.PendingMod;
					pendingMod.NextNode = null;
					pendingMod.Release();
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
				RenderChunk chunk = this._chunk;
				this._chunk = newChunk;
				this._chunkLock.Unlock();
				newChunk.AddRef();
				chunk.Release();
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
