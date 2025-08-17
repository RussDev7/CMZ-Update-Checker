using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Utils.Trace
{
	public class TraceProbe
	{
		public static Plane[] MakeBox(BoundingBox bb)
		{
			TraceProbe._boxFaces[0].D = -bb.Max.X;
			TraceProbe._boxFaces[1].D = bb.Min.Z;
			TraceProbe._boxFaces[2].D = bb.Min.X;
			TraceProbe._boxFaces[3].D = -bb.Max.Z;
			TraceProbe._boxFaces[4].D = -bb.Max.Y;
			TraceProbe._boxFaces[5].D = bb.Min.Y;
			return TraceProbe._boxFaces;
		}

		public static void MakeOrientedBox(Matrix mat, BoundingBox bb, Plane[] planes)
		{
			Plane[] array = TraceProbe.MakeBox(bb);
			for (int i = 0; i < 6; i++)
			{
				planes[i] = Plane.Transform(array[i], mat);
			}
		}

		public virtual bool AbleToBuild
		{
			get
			{
				return this._collides && !this._startsIn;
			}
		}

		public virtual float Radius
		{
			get
			{
				return 0f;
			}
		}

		public virtual Vector3 HalfVector
		{
			get
			{
				return Vector3.Zero;
			}
		}

		public virtual void Init(Vector3 start, Vector3 end)
		{
			Vector3 vector = Vector3.Min(start, end);
			Vector3 vector2 = Vector3.Max(start, end);
			this._bounds = new BoundingBox(vector, vector2);
			this._start = start;
			this._end = end;
			this.Reset();
		}

		public virtual void Reset()
		{
			this._startsIn = false;
			this._endsIn = false;
			this._inT = 1f;
			this._outT = 0f;
			this._collides = false;
			this.ShapeHasSlopedSides = false;
			this.SimulateSlopedSides = false;
			this.FoundSlopedBlock = false;
			this.SlopedBlockT = 0f;
		}

		public virtual bool TestThisType(BlockTypeEnum e)
		{
			return BlockType.GetType(e).CanBeTouched;
		}

		public virtual Vector3 GetIntersection()
		{
			if (this._collides)
			{
				return this.CalculatePoint(this._inT);
			}
			return Vector3.Zero;
		}

		public virtual Vector3 CalculatePoint(float t)
		{
			return Vector3.Lerp(this._start, this._end, t);
		}

		public virtual bool TouchesBlock(float inT, ref Vector3 inNormal, bool startsIn, BlockFace inFace, float outT, ref Vector3 outNormal, bool endsIn, BlockFace outFace, IntVector3 worldindex)
		{
			return true;
		}

		public virtual bool TestThisEnemy(IShootableEnemy enemy)
		{
			return true;
		}

		public bool SetIntersection(float inT, ref Vector3 inNormal, bool startsIn, BlockFace inFace, float outT, ref Vector3 outNormal, bool endsIn, BlockFace outFace, IntVector3 worldindex)
		{
			bool flag = false;
			if (startsIn)
			{
				if (this.SimulateSlopedSides && this.ShapeHasSlopedSides)
				{
					this.FoundSlopedBlock = true;
					this.SlopedBlock = worldindex;
					this.SlopedBlockT = 0f;
					return flag;
				}
				if (this.SkipEmbedded)
				{
					return flag;
				}
			}
			if (this.TraceCompletePath)
			{
				flag = this.TouchesBlock(inT, ref inNormal, startsIn, inFace, outT, ref outNormal, endsIn, outFace, worldindex);
				bool flag2 = false;
				bool flag3 = false;
				if (!this._collides)
				{
					flag2 = true;
					flag3 = true;
				}
				else
				{
					if (inT < this._inT)
					{
						flag2 = true;
					}
					if (outT > this._outT)
					{
						flag3 = true;
					}
				}
				if (flag2 || flag3)
				{
					this._collides = true;
					this._worldIndex = worldindex;
					if (flag2)
					{
						this._inT = inT;
						this._inNormal = inNormal;
						this._startsIn = startsIn;
						this._inFace = inFace;
					}
					if (flag3)
					{
						this._outT = outT;
						this._outNormal = outNormal;
						this._endsIn = endsIn;
						this._outFace = outFace;
					}
				}
			}
			else if (!this._collides || inT < this._inT)
			{
				if (this.SimulateSlopedSides && this.ShapeHasSlopedSides && inFace < BlockFace.POSY)
				{
					if (inT <= this.SlopedBlockT)
					{
						this.FoundSlopedBlock = true;
						this.SlopedBlock = worldindex;
						this.SlopedBlockT = inT;
					}
				}
				else
				{
					this._collides = true;
					this._inT = inT;
					this._inNormal = inNormal;
					this._startsIn = startsIn;
					this._inFace = inFace;
					this._outT = outT;
					this._outNormal = outNormal;
					this._endsIn = endsIn;
					this._outFace = outFace;
					this._worldIndex = worldindex;
				}
			}
			return flag;
		}

		public virtual bool TestBoundBox(BoundingBox bb)
		{
			return this.TestShape(TraceProbe.MakeBox(bb), IntVector3.Zero);
		}

		public virtual bool TestShape(Plane[] planes, IntVector3 worldIndex, BlockTypeEnum blockType)
		{
			this._currentTestingBlockType = blockType;
			return this.TestShape(planes, worldIndex);
		}

		public virtual bool TestShape(Plane[] planes, IntVector3 worldIndex)
		{
			float num = 0f;
			float num2 = 1f;
			bool flag = true;
			bool flag2 = true;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < planes.Length; i++)
			{
				float num5 = planes[i].DotCoordinate(this._start);
				float num6 = planes[i].DotCoordinate(this._end);
				bool flag3 = num5 > 0f;
				bool flag4 = num6 > 0f;
				if (flag3 && flag4)
				{
					return false;
				}
				if (flag3 != flag4)
				{
					if (num5 > num6)
					{
						float num7 = num5 / (num5 - num6);
						if (num7 > num)
						{
							flag = false;
							num3 = i;
							num = num7;
						}
					}
					else
					{
						float num8 = -num5 / (num6 - num5);
						if (num8 < num2)
						{
							flag2 = false;
							num4 = i;
							num2 = num8;
						}
					}
				}
			}
			return num > num2 || this.SetIntersection(num, ref planes[num3].Normal, flag, (BlockFace)num3, num2, ref planes[num4].Normal, flag2, (BlockFace)num4, worldIndex);
		}

		private static Plane[] _boxFaces = new Plane[]
		{
			new Plane(1f, 0f, 0f, 0f),
			new Plane(0f, 0f, -1f, 0f),
			new Plane(-1f, 0f, 0f, 0f),
			new Plane(0f, 0f, 1f, 0f),
			new Plane(0f, 1f, 0f, 0f),
			new Plane(0f, -1f, 0f, 0f)
		};

		public BoundingBox _bounds;

		public Vector3 _start;

		public Vector3 _end;

		public IntVector3 SlopedBlock;

		public bool FoundSlopedBlock;

		public float SlopedBlockT;

		public float _inT;

		public float _outT;

		public Vector3 _inNormal;

		public Vector3 _outNormal;

		public bool _collides;

		public bool _startsIn;

		public bool _endsIn;

		public IntVector3 _worldIndex;

		public BlockFace _inFace;

		public BlockFace _outFace;

		public bool SkipEmbedded;

		public bool TraceCompletePath;

		public bool ShapeHasSlopedSides;

		public bool SimulateSlopedSides;

		protected BlockTypeEnum _currentTestingBlockType = BlockTypeEnum.NumberOfBlocks;
	}
}
