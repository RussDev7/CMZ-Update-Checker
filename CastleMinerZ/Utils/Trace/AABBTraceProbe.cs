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
			Vector3 vector = Vector3.Multiply(box.Min + box.Max, 0.5f);
			this._offsetToRay = Vector3.Negate(vector);
			this._start = start + vector;
			this._end = end + vector;
			this._halfVec = box.Max - vector;
			this._halfVec.X = Math.Abs(this._halfVec.X);
			this._halfVec.Y = Math.Abs(this._halfVec.Y);
			this._halfVec.Z = Math.Abs(this._halfVec.Z);
			Vector3 vector2 = Vector3.Min(this._start, this._end);
			vector2 -= this._halfVec;
			Vector3 vector3 = Vector3.Max(this._start, this._end);
			vector3 += this._halfVec;
			this._bounds = new BoundingBox(vector2, vector3);
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
			float num = 0f;
			float num2 = 1f;
			bool flag = true;
			bool flag2 = true;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < planes.Length; i++)
			{
				Vector3 vector = Vector3.Multiply(this._halfVec, planes[i].Normal);
				float num5 = -(Math.Abs(vector.X) + Math.Abs(vector.Y) + Math.Abs(vector.Z));
				float num6 = planes[i].DotCoordinate(this._start) + num5;
				float num7 = planes[i].DotCoordinate(this._end) + num5;
				bool flag3;
				if (num6 > -0.0001f)
				{
					flag3 = true;
					num6 = Math.Max(num6, 0f);
				}
				else
				{
					flag3 = false;
				}
				bool flag4;
				if (num7 > -0.0001f)
				{
					flag4 = true;
					num7 = Math.Max(num7, 0f);
				}
				else
				{
					flag4 = false;
				}
				if (flag3 && flag4)
				{
					return false;
				}
				if (flag3 != flag4)
				{
					if (num6 > num7)
					{
						float num8 = num6 / (num6 - num7);
						if (num8 >= num)
						{
							flag = false;
							num3 = i;
							num = num8;
						}
					}
					else
					{
						float num9 = -num6 / (num7 - num6);
						if (num9 <= num2)
						{
							flag2 = false;
							num4 = i;
							num2 = num9;
						}
					}
				}
			}
			if (num <= num2 && (!this.hasDirection || Math.Abs(Vector3.Dot(this._direction, planes[num3].Normal)) > 1E-05f || (flag && (this.SimulateSlopedSides || !this.SkipEmbedded))))
			{
				base.SetIntersection(num, ref planes[num3].Normal, flag, (BlockFace)num3, num2, ref planes[num4].Normal, flag2, (BlockFace)num4, worldIndex);
			}
			return true;
		}

		private Vector3 _offsetToRay;

		private Vector3 _halfVec;

		private Vector3 _direction;

		private bool hasDirection;
	}
}
