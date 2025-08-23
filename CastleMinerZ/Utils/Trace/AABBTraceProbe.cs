using System;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.Utils.Trace
{
	public class AABBTraceProbe : TraceProbe
	{
		public override float Radius
		{
			get
			{
				return this._halfVec.X;
			}
		}

		public override Vector3 HalfVector
		{
			get
			{
				return this._halfVec;
			}
		}

		public void Init(Vector3 start, Vector3 end, BoundingBox box)
		{
			Vector3 boxCenter = Vector3.Multiply(box.Min + box.Max, 0.5f);
			this._offsetToRay = Vector3.Negate(boxCenter);
			this._start = start + boxCenter;
			this._end = end + boxCenter;
			this._halfVec = box.Max - boxCenter;
			this._halfVec.X = Math.Abs(this._halfVec.X);
			this._halfVec.Y = Math.Abs(this._halfVec.Y);
			this._halfVec.Z = Math.Abs(this._halfVec.Z);
			Vector3 min = Vector3.Min(this._start, this._end);
			min -= this._halfVec;
			Vector3 max = Vector3.Max(this._start, this._end);
			max += this._halfVec;
			this._bounds = new BoundingBox(min, max);
			this._direction = this._start - this._end;
			this.hasDirection = this._direction.LengthSquared() > 0f;
			this._direction.Normalize();
			this.Reset();
		}

		public override bool TestThisType(BlockTypeEnum e)
		{
			return BlockType.GetType(e).BlockPlayer;
		}

		public override Vector3 GetIntersection()
		{
			if (this._collides)
			{
				return base.GetIntersection() + this._offsetToRay;
			}
			return Vector3.Zero;
		}

		public override bool TestShape(Plane[] planes, IntVector3 worldIndex)
		{
			float inT = 0f;
			float outT = 1f;
			bool startsIn = true;
			bool endsIn = true;
			int inPlane = 0;
			int outPlane = 0;
			for (int i = 0; i < planes.Length; i++)
			{
				Vector3 offsetVector = Vector3.Multiply(this._halfVec, planes[i].Normal);
				float offset = -(Math.Abs(offsetVector.X) + Math.Abs(offsetVector.Y) + Math.Abs(offsetVector.Z));
				float dStart = planes[i].DotCoordinate(this._start) + offset;
				float dEnd = planes[i].DotCoordinate(this._end) + offset;
				bool startIsOut;
				if (dStart > -0.0001f)
				{
					startIsOut = true;
					dStart = Math.Max(dStart, 0f);
				}
				else
				{
					startIsOut = false;
				}
				bool endIsOut;
				if (dEnd > -0.0001f)
				{
					endIsOut = true;
					dEnd = Math.Max(dEnd, 0f);
				}
				else
				{
					endIsOut = false;
				}
				if (startIsOut && endIsOut)
				{
					return false;
				}
				if (startIsOut != endIsOut)
				{
					if (dStart > dEnd)
					{
						float t = dStart / (dStart - dEnd);
						if (t >= inT)
						{
							startsIn = false;
							inPlane = i;
							inT = t;
						}
					}
					else
					{
						float t2 = -dStart / (dEnd - dStart);
						if (t2 <= outT)
						{
							endsIn = false;
							outPlane = i;
							outT = t2;
						}
					}
				}
			}
			if (inT <= outT && (!this.hasDirection || Math.Abs(Vector3.Dot(this._direction, planes[inPlane].Normal)) > 1E-05f || (startsIn && (this.SimulateSlopedSides || !this.SkipEmbedded))))
			{
				base.SetIntersection(inT, ref planes[inPlane].Normal, startsIn, (BlockFace)inPlane, outT, ref planes[outPlane].Normal, endsIn, (BlockFace)outPlane, worldIndex);
			}
			return true;
		}

		private Vector3 _offsetToRay;

		private Vector3 _halfVec;

		private Vector3 _direction;

		private bool hasDirection;
	}
}
