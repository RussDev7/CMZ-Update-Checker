using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Terrain
{
	public struct BlockVertex : IVertexType
	{
		public BlockVertex(BlockFace face, int vx, int tx)
		{
			IntVector3 intVector = BlockVertex._faceVertices[(int)(face * BlockFace.POSY + vx)];
			this._blockOffsetFace = intVector.X | (intVector.Y << 8) | (intVector.Z << 16) | (tx << 24);
			this._vxSunLampFace = vx | 3840 | 983040 | 268435456;
		}

		public BlockVertex(IntVector3 iv, BlockFace face, int vx, BlockType mat, int sun, int lamp, int aoindex)
		{
			IntVector3 intVector = IntVector3.Add(iv, BlockVertex._faceVertices[(int)(face * BlockFace.POSY + vx)]);
			this._blockOffsetFace = intVector.X | (intVector.Y << 8) | (intVector.Z << 16) | (mat[face] << 24);
			if (mat.DrawFullBright)
			{
				this._vxSunLampFace = 3840 | (lamp << 16) | (int)((int)((face | (BlockFace)(vx << 4)) + 128) << 24);
				return;
			}
			this._vxSunLampFace = aoindex | (sun << 8) | (lamp << 16) | (int)((int)(face | (BlockFace)(vx << 4)) << 24);
		}

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get
			{
				return BlockVertex.VertexDeclaration;
			}
		}

		public static readonly IntVector3[] _faceVertices = new IntVector3[]
		{
			new IntVector3(1, 1, 1),
			new IntVector3(1, 1, 0),
			new IntVector3(1, 0, 1),
			new IntVector3(1, 0, 0),
			new IntVector3(1, 1, 0),
			new IntVector3(0, 1, 0),
			new IntVector3(1, 0, 0),
			new IntVector3(0, 0, 0),
			new IntVector3(0, 1, 0),
			new IntVector3(0, 1, 1),
			new IntVector3(0, 0, 0),
			new IntVector3(0, 0, 1),
			new IntVector3(0, 1, 1),
			new IntVector3(1, 1, 1),
			new IntVector3(0, 0, 1),
			new IntVector3(1, 0, 1),
			new IntVector3(0, 1, 0),
			new IntVector3(1, 1, 0),
			new IntVector3(0, 1, 1),
			new IntVector3(1, 1, 1),
			new IntVector3(1, 0, 0),
			new IntVector3(0, 0, 0),
			new IntVector3(1, 0, 1),
			new IntVector3(0, 0, 1)
		};

		private int _blockOffsetFace;

		private int _vxSunLampFace;

		public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[]
		{
			new VertexElement(0, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 0),
			new VertexElement(4, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 1)
		});
	}
}
