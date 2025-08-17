using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherBaseInventoryItemClass : GunInventoryItemClass
	{
		public RocketLauncherBaseInventoryItemClass(InventoryItemIDs id, Model model, string name, string description, TimeSpan reloadTime, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype, string shootSound, string reloadSound)
			: base(id, model, name, description, reloadTime, ToolMaterialTypes.BloodStone, damage, durabilitydamage, ammotype, shootSound, reloadSound)
		{
			this._playerMode = PlayerMode.RPG;
			this.ReloadTime = TimeSpan.FromSeconds(0.567);
			this.Automatic = false;
			this.ClipCapacity = 1;
			this.RoundsPerReload = 1;
			this.FlightTime = 0.4f;
			this.ShoulderedMinAccuracy = Angle.FromDegrees(0.1f);
			this.ShoulderedMaxAccuracy = Angle.FromDegrees(0.25);
			this.MinInnaccuracy = Angle.FromDegrees(6.875);
			this.MaxInnaccuracy = Angle.FromDegrees(10f);
			this.Recoil = Angle.FromDegrees(12f);
			this.Velocity = 50f;
			this.EnemyDamageType = DamageType.SHOTGUN;
			this.Scoped = true;
		}

		public virtual bool IsGuided()
		{
			return false;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			GunEntity gunEntity = (GunEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (this.ID == InventoryItemIDs.RocketLauncherGuidedShotFired || this.ID == InventoryItemIDs.RocketLauncherGuided)
			{
				gunEntity.DiffuseColor = Color.Black;
			}
			else
			{
				gunEntity.DiffuseColor = Color.Gray;
			}
			switch (use)
			{
			case ItemUse.UI:
				gunEntity.LocalPosition += new Vector3(-2f, -4f, 0f);
				break;
			case ItemUse.Hand:
				if (this.ID == InventoryItemIDs.RocketLauncherGuided || this.ID == InventoryItemIDs.RocketLauncher)
				{
					ModelBone modelBone = this._model.Bones["BarrelTip"];
					Vector3 vector;
					Vector3 vector2;
					if (modelBone != null)
					{
						vector = Vector3.Transform(RocketLauncherBaseInventoryItemClass.cBarrelTipOffset, modelBone.Transform);
						vector2 = Vector3.Normalize(modelBone.Transform.Left);
					}
					else
					{
						vector = new Vector3(0f, 0f, -0.5f);
						vector2 = Vector3.Forward;
					}
					RocketLauncherBaseInventoryItemClass.RocketLauncherGrenadeModel rocketLauncherGrenadeModel;
					if (this.ID == InventoryItemIDs.RocketLauncherGuided)
					{
						rocketLauncherGrenadeModel = new RocketLauncherBaseInventoryItemClass.RocketLauncherGrenadeModel(RocketLauncherBaseInventoryItemClass.RPGGrenadeModel, true);
					}
					else
					{
						rocketLauncherGrenadeModel = new RocketLauncherBaseInventoryItemClass.RocketLauncherGrenadeModel(RocketLauncherBaseInventoryItemClass.RPGGrenadeModel, false);
					}
					gunEntity.Children.Add(rocketLauncherGrenadeModel);
					rocketLauncherGrenadeModel.LocalToParent = MathTools.CreateWorld(vector, vector2);
					rocketLauncherGrenadeModel.LocalScale = RocketLauncherBaseInventoryItemClass.cBarrelTipScale;
				}
				break;
			}
			return gunEntity;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new RocketLauncherBaseItem(this, stackCount);
		}

		private static Model RPGGrenadeModel = CastleMinerZGame.Instance.Content.Load<Model>("Props\\Weapons\\Conventional\\RPG\\RPGGrenade");

		private static readonly Vector3 cBarrelTipOffset = new Vector3(0.01f, -0.005f, 0.01f);

		private static readonly Vector3 cBarrelTipScale = new Vector3(0.65f);

		public class RocketLauncherGrenadeModel : ModelEntity
		{
			public RocketLauncherGrenadeModel(Model model, bool recolor)
				: base(model)
			{
				this._recolor = recolor;
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dnaeffect = (DNAEffect)effect;
				if (this._recolor)
				{
					dnaeffect.DiffuseColor = ColorF.FromRGB(1.5f, 0f, 0f);
				}
				else
				{
					dnaeffect.DiffuseColor = Color.Gray;
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}

			private bool _recolor;
		}

		public class RocketLauncherModel : ModelEntity
		{
			public RocketLauncherModel(Model model)
				: base(model)
			{
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dnaeffect = (DNAEffect)effect;
				if (this.Darken)
				{
					dnaeffect.DiffuseColor = Color.Gray;
				}
				else
				{
					dnaeffect.DiffuseColor = Color.White;
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}

			public bool Darken;
		}
	}
}
