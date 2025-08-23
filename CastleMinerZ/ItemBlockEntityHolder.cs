using System;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class ItemBlockEntityHolder : IReleaseable, ILinkedListNode
	{
		public ItemBlockEntityHolder()
		{
			this._inWorldEntity = null;
			this.TorchEntity = null;
			this._hasFlame = false;
		}

		public Entity InWorldEntity
		{
			get
			{
				return this._inWorldEntity;
			}
			set
			{
				if (this._inWorldEntity != null && this._inWorldEntity != value)
				{
					this._inWorldEntity.RemoveFromParent();
				}
				this._inWorldEntity = value;
				if (value != null && value is TorchEntity)
				{
					this.TorchEntity = (TorchEntity)value;
					this._hasFlame = this.TorchEntity.HasFlame;
					return;
				}
				this.TorchEntity = null;
				this._hasFlame = false;
			}
		}

		public bool TorchFlame
		{
			get
			{
				return this._hasFlame;
			}
			set
			{
				if (value != this._hasFlame && this.TorchEntity != null)
				{
					this.TorchEntity.HasFlame = value;
					this._hasFlame = this.TorchEntity.HasFlame;
				}
			}
		}

		public static ItemBlockEntityHolder Alloc()
		{
			ItemBlockEntityHolder result = ItemBlockEntityHolder._cache.Get();
			result.TimeUntilTorchRemovalAllowed = 1f;
			result.TimeUntilFlameRemovalAllowed = 1f;
			return result;
		}

		public void Release()
		{
			this.InWorldEntity = null;
			ItemBlockEntityHolder._cache.Put(this);
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

		public Vector3 Position;

		public BlockTypeEnum BlockType;

		public TorchEntity TorchEntity;

		private Entity _inWorldEntity;

		public float Distance;

		public float TimeUntilTorchRemovalAllowed;

		public float TimeUntilFlameRemovalAllowed;

		private bool _hasFlame;

		private static ObjectCache<ItemBlockEntityHolder> _cache = new ObjectCache<ItemBlockEntityHolder>();

		private ILinkedListNode _nextNode;
	}
}
