using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class DoorEntity : Entity
	{
		public static DoorEntity.ModelNameEnum GetModelNameFromInventoryId(InventoryItemIDs invID)
		{
			DoorEntity.ModelNameEnum modelNameEnum = DoorEntity.ModelNameEnum.Wood;
			if (invID != InventoryItemIDs.Door)
			{
				if (invID != InventoryItemIDs.DiamondDoor)
				{
					switch (invID)
					{
					case InventoryItemIDs.IronDoor:
						modelNameEnum = DoorEntity.ModelNameEnum.Iron;
						break;
					case InventoryItemIDs.TechDoor:
						modelNameEnum = DoorEntity.ModelNameEnum.Tech;
						break;
					}
				}
				else
				{
					modelNameEnum = DoorEntity.ModelNameEnum.Diamond;
				}
			}
			else
			{
				modelNameEnum = DoorEntity.ModelNameEnum.Wood;
			}
			return modelNameEnum;
		}

		public bool DoorOpen
		{
			get
			{
				return this._doorOpen;
			}
		}

		public static Model GetDoorModel(DoorEntity.ModelNameEnum modelName)
		{
			return DoorEntity._doorModels[(int)modelName];
		}

		static DoorEntity()
		{
			if (DoorEntity._doorModels.Count == 0)
			{
				for (int i = 0; i < DoorEntity._modelPaths.Length; i++)
				{
					DoorEntity._doorModels.Add(CastleMinerZGame.Instance.Content.Load<Model>(DoorEntity._modelPaths[i]));
				}
			}
		}

		public DoorEntity(DoorEntity.ModelNameEnum modelName, BlockTypeEnum blockType)
		{
			this._modelEnt = new DoorEntity.DoorModelEntity(modelName);
			base.Children.Add(this._modelEnt);
			this.SetPosition(DoorInventoryitem.GetDoorPiece(BlockTypeEnum.NormalLowerDoorClosedX, blockType));
		}

		public void SetPosition(BlockTypeEnum doorType)
		{
			switch (doorType)
			{
			case BlockTypeEnum.NormalLowerDoorClosedZ:
				this._xAxis = false;
				this._doorOpen = false;
				break;
			case BlockTypeEnum.NormalLowerDoorClosedX:
				this._xAxis = true;
				this._doorOpen = false;
				break;
			case BlockTypeEnum.NormalLowerDoor:
			case BlockTypeEnum.NormalUpperDoorClosed:
				break;
			case BlockTypeEnum.NormalLowerDoorOpenZ:
				this._xAxis = false;
				this._doorOpen = true;
				break;
			case BlockTypeEnum.NormalLowerDoorOpenX:
				this._xAxis = true;
				this._doorOpen = true;
				break;
			default:
				switch (doorType)
				{
				case BlockTypeEnum.StrongLowerDoorClosedZ:
					this._xAxis = false;
					this._doorOpen = false;
					break;
				case BlockTypeEnum.StrongLowerDoorClosedX:
					this._xAxis = true;
					this._doorOpen = false;
					break;
				case BlockTypeEnum.StrongLowerDoorOpenZ:
					this._xAxis = false;
					this._doorOpen = true;
					break;
				case BlockTypeEnum.StrongLowerDoorOpenX:
					this._xAxis = true;
					this._doorOpen = true;
					break;
				}
				break;
			}
			if (this._xAxis)
			{
				if (this._doorOpen)
				{
					this._modelEnt.LocalPosition = new Vector3(-0.5f, -0.5f, 0f);
					this._modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.5707964f);
					return;
				}
				this._modelEnt.LocalPosition = new Vector3(-0.5f, -0.5f, 0f);
				this._modelEnt.LocalRotation = Quaternion.Identity;
				return;
			}
			else
			{
				if (this._doorOpen)
				{
					this._modelEnt.LocalPosition = new Vector3(0f, -0.5f, -0.5f);
					this._modelEnt.LocalRotation = Quaternion.Identity;
					return;
				}
				this._modelEnt.LocalPosition = new Vector3(0f, -0.5f, -0.5f);
				this._modelEnt.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -1.5707964f);
				return;
			}
		}

		public static readonly string[] _modelPaths = new string[] { "Props\\Items\\Door\\Model", "Props\\Items\\Door\\Model", "Props\\Items\\IronDoor\\Model", "Props\\Items\\DiamondDoor\\Model", "Props\\Items\\TechDoor\\Model" };

		private static List<Model> _doorModels = new List<Model>();

		public static Model _doorModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Items\\Door\\Model");

		private bool _xAxis;

		private DoorEntity.DoorModelEntity _modelEnt;

		private bool _doorOpen;

		public enum ModelNameEnum
		{
			None,
			Wood,
			Iron,
			Diamond,
			Tech
		}

		private class DoorModelEntity : ModelEntity
		{
			public DoorModelEntity(DoorEntity.ModelNameEnum modelName)
				: base(DoorEntity.GetDoorModel(modelName))
			{
			}

			public void CalculateLighting()
			{
				Vector3 worldPosition = base.WorldPosition;
				BlockTerrain.Instance.GetEnemyLighting(worldPosition, ref this.DirectLightDirection[0], ref this.DirectLightColor[0], ref this.DirectLightDirection[1], ref this.DirectLightColor[1], ref this.AmbientLight);
			}

			protected override void OnUpdate(GameTime gameTime)
			{
				this.CalculateLighting();
				base.OnUpdate(gameTime);
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dnaeffect = effect as DNAEffect;
				if (dnaeffect != null && dnaeffect.Parameters["LightDirection1"] != null)
				{
					dnaeffect.Parameters["LightDirection1"].SetValue(-this.DirectLightDirection[0]);
					dnaeffect.Parameters["LightColor1"].SetValue(this.DirectLightColor[0]);
					dnaeffect.Parameters["LightDirection2"].SetValue(-this.DirectLightDirection[1]);
					dnaeffect.Parameters["LightColor2"].SetValue(this.DirectLightColor[1]);
					dnaeffect.AmbientColor = ColorF.FromVector3(this.AmbientLight);
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}

			public Vector3[] DirectLightColor = new Vector3[2];

			public Vector3[] DirectLightDirection = new Vector3[2];

			public Vector3 AmbientLight = Color.Gray.ToVector3();
		}
	}
}
