using System;
using DNA.CastleMinerZ.Net;
using DNA.Net.GamerServices;

namespace DNA.CastleMinerZ.Terrain
{
	public class SpawnBlockView
	{
		public IntVector3 Location
		{
			get
			{
				return this._location;
			}
		}

		public SpawnBlockView(IntVector3 location, BlockTypeEnum blockSource)
		{
			this.SetBlockProperty(blockSource);
			this._originalBlockType = blockSource;
			this._location = location;
		}

		private void SetBlockProperty(BlockTypeEnum blockSource)
		{
			foreach (SpawnBlockView.SpawnBlockProperties properties in SpawnBlockView._spawnBlockProperties)
			{
				if (properties.OffBlockTypeEnum == blockSource)
				{
					this._blockProperties = properties;
					break;
				}
			}
		}

		private BlockTypeEnum SetCurrentBlock(BlockTypeEnum blockType)
		{
			this._currentBlockType = blockType;
			return this._currentBlockType;
		}

		public void Reset()
		{
			AlterBlockMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, this.Location, this._blockProperties.OffBlockTypeEnum);
			this._spawnLightState = SpawnBlockView.SpawnLightState.Off;
		}

		public void ToggleLightState()
		{
			if (this._spawnLightState == SpawnBlockView.SpawnLightState.On)
			{
				this.SetBlockLight(false);
				return;
			}
			if (this._spawnLightState == SpawnBlockView.SpawnLightState.Off)
			{
				this.SetBlockLight(true);
			}
		}

		public void SetBlockLight(bool enable)
		{
			Player localPlayer = CastleMinerZGame.Instance.LocalPlayer;
			if (enable)
			{
				AlterBlockMessage.Send((LocalNetworkGamer)localPlayer.Gamer, this.Location, this.GetLightedBlock());
				this._spawnLightState = SpawnBlockView.SpawnLightState.On;
				this._lightTimer = 0.8f;
				return;
			}
			AlterBlockMessage.Send((LocalNetworkGamer)localPlayer.Gamer, this.Location, this.GetDimBlock());
			this._spawnLightState = SpawnBlockView.SpawnLightState.Off;
			this._lightTimer = 0.3f;
		}

		public static BlockTypeEnum GetInActiveSpawnBlockType(BlockTypeEnum blockType)
		{
			BlockTypeEnum activeBlockType = blockType;
			switch (blockType)
			{
			case BlockTypeEnum.EnemySpawnOn:
				activeBlockType = BlockTypeEnum.EnemySpawnOff;
				break;
			case BlockTypeEnum.EnemySpawnOff:
				break;
			case BlockTypeEnum.EnemySpawnRareOn:
				activeBlockType = BlockTypeEnum.EnemySpawnRareOff;
				break;
			default:
				switch (blockType)
				{
				case BlockTypeEnum.AlienSpawnOn:
					activeBlockType = BlockTypeEnum.AlienSpawnOff;
					break;
				case BlockTypeEnum.HellSpawnOn:
					activeBlockType = BlockTypeEnum.HellSpawnOff;
					break;
				case BlockTypeEnum.BossSpawnOn:
					activeBlockType = BlockTypeEnum.BossSpawnOff;
					break;
				}
				break;
			}
			return activeBlockType;
		}

		public BlockTypeEnum GetActiveSpawnBlockType()
		{
			return this._blockProperties.OnBlockTypeEnum;
		}

		private BlockTypeEnum GetAlternateBlock(BlockTypeEnum blockType)
		{
			if (blockType == this._blockProperties.OnBlockTypeEnum)
			{
				return this._blockProperties.OffBlockTypeEnum;
			}
			if (blockType == this._blockProperties.OffBlockTypeEnum)
			{
				return this._blockProperties.OnBlockTypeEnum;
			}
			if (blockType == this._blockProperties.DimBlockTypeEnum)
			{
				return this._blockProperties.OnBlockTypeEnum;
			}
			return BlockTypeEnum.Empty;
		}

		public BlockTypeEnum GetLightedBlock()
		{
			return this._blockProperties.OnBlockTypeEnum;
		}

		public BlockTypeEnum GetDimBlock()
		{
			return this._blockProperties.DimBlockTypeEnum;
		}

		public void Update(float delta)
		{
			this._lightTimer -= delta;
			if (this._lightTimer <= 0f)
			{
				this.ToggleLightState();
			}
		}

		private const float c_LightOnTime = 0.8f;

		private const float c_LightOffTime = 0.3f;

		private static readonly SpawnBlockView.SpawnBlockProperties[] _spawnBlockProperties = new SpawnBlockView.SpawnBlockProperties[]
		{
			new SpawnBlockView.SpawnBlockProperties(BlockTypeEnum.EnemySpawnOff, BlockTypeEnum.EnemySpawnOn, BlockTypeEnum.EnemySpawnDim),
			new SpawnBlockView.SpawnBlockProperties(BlockTypeEnum.EnemySpawnRareOff, BlockTypeEnum.EnemySpawnRareOn, BlockTypeEnum.EnemySpawnRareDim),
			new SpawnBlockView.SpawnBlockProperties(BlockTypeEnum.AlienSpawnOff, BlockTypeEnum.AlienSpawnOn, BlockTypeEnum.AlienSpawnDim),
			new SpawnBlockView.SpawnBlockProperties(BlockTypeEnum.AlienHordeOff, BlockTypeEnum.AlienHordeOn, BlockTypeEnum.AlienHordeDim),
			new SpawnBlockView.SpawnBlockProperties(BlockTypeEnum.HellSpawnOff, BlockTypeEnum.HellSpawnOn, BlockTypeEnum.HellSpawnDim),
			new SpawnBlockView.SpawnBlockProperties(BlockTypeEnum.BossSpawnOff, BlockTypeEnum.BossSpawnOn, BlockTypeEnum.BossSpawnDim)
		};

		public bool Destroyed;

		private IntVector3 _location;

		private float _lightTimer;

		private BlockTypeEnum _currentBlockType;

		private BlockTypeEnum _originalBlockType;

		private SpawnBlockView.SpawnLightState _spawnLightState;

		private BlockTypeEnum _dimBlockTypeEnum;

		private BlockTypeEnum _offBlockTypeEnum;

		private BlockTypeEnum _onBlockTypeEnum;

		private SpawnBlockView.SpawnBlockProperties _blockProperties;

		private class SpawnBlockProperties
		{
			public SpawnBlockProperties(BlockTypeEnum pOffBlockType, BlockTypeEnum pOnBlockType, BlockTypeEnum pDimBlockType)
			{
				this.OffBlockTypeEnum = pOffBlockType;
				this.OnBlockTypeEnum = pOnBlockType;
				this.DimBlockTypeEnum = pDimBlockType;
			}

			public BlockTypeEnum DimBlockTypeEnum;

			public BlockTypeEnum OffBlockTypeEnum;

			public BlockTypeEnum OnBlockTypeEnum;
		}

		public enum SpawnLightState
		{
			None,
			On,
			Off
		}
	}
}
