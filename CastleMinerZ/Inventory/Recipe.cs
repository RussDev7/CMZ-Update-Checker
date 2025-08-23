using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.Inventory
{
	public class Recipe
	{
		public static List<Recipe> GetRecipes(Recipe.RecipeTypes type)
		{
			List<Recipe> recipes = new List<Recipe>();
			foreach (Recipe recipe in Recipe.CookBook)
			{
				if (recipe.Type == type)
				{
					recipes.Add(recipe);
				}
			}
			return recipes;
		}

		public List<InventoryItem> Ingredients
		{
			get
			{
				return this._ingredients;
			}
		}

		public InventoryItem Result
		{
			get
			{
				return this._result;
			}
		}

		public Recipe.RecipeTypes Type
		{
			get
			{
				return this._type;
			}
		}

		public Recipe(Recipe.RecipeTypes type, InventoryItem result, params InventoryItem[] ingredients)
		{
			this._type = type;
			this._result = result;
			this._ingredients = new List<InventoryItem>(ingredients);
		}

		static Recipe()
		{
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 4), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.Stick, 4), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.Torch, 4), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.LanternBlock, 4), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Torch, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1),
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 4),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.LanternFancyBlock, 4), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Torch, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1),
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 4),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.BrassCasing, 200), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Copper, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.IronCasing, 200), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Iron, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.GoldCasing, 200), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Gold, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.DiamondCasing, 100), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.Bullets, 100), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 1),
				InventoryItem.CreateItem(InventoryItemIDs.BrassCasing, 100),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.IronBullets, 100), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1),
				InventoryItem.CreateItem(InventoryItemIDs.BrassCasing, 100),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.GoldBullets, 100), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1),
				InventoryItem.CreateItem(InventoryItemIDs.IronCasing, 100),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.DiamondBullets, 100), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1),
				InventoryItem.CreateItem(InventoryItemIDs.GoldCasing, 100),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ammo, InventoryItem.CreateItem(InventoryItemIDs.LaserBullets, 100), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 1),
				InventoryItem.CreateItem(InventoryItemIDs.DiamondCasing, 100)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Components, InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 2),
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.Compass, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.Clock, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.GPS, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpecialTools, InventoryItem.CreateItem(InventoryItemIDs.TeleportGPS, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1),
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.Crate, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 10),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.StoneContainer, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 4),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.CopperContainer, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.IronContainer, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Iron, 4) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.GoldContainer, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.DiamondContainer, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Containers, InventoryItem.CreateItem(InventoryItemIDs.BloodstoneContainer, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 10),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.SpawnBasic, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 10)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.TeleportStation, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 40),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 20),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 20),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.TeleportStation, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 15),
				InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 999),
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SpawnPoints, InventoryItem.CreateItem(InventoryItemIDs.TeleportStation, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 100),
				InventoryItem.CreateItem(InventoryItemIDs.Snow, 100),
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 40)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.StonePickAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.CopperPickAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.IronPickAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.GoldPickAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.DiamondPickAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.BloodstonePickAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 10),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pickaxes, InventoryItem.CreateItem(InventoryItemIDs.LaserDrill, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.StoneSpade, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.CopperSpade, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.IronSpade, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.GoldSpade, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Spades, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpade, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.StoneAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.RockBlock, 4),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.CopperAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.IronAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.GoldAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Axes, InventoryItem.CreateItem(InventoryItemIDs.DiamondAxe, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Copper, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.IronOre, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.IronOre, 2),
				InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.GoldOre, 2),
				InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Copper, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.CopperOre, 2),
				InventoryItem.CreateItem(InventoryItemIDs.LogBlock, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.Knife, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.GoldKnife, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.DiamondKnife, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Knives, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneKnife, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 10),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.IronLaserSword, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2),
				InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.CopperLaserSword, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 2),
				InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.GoldLaserSword, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2),
				InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.DiamondLaserSword, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LaserSwords, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneLaserSword, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 2),
				InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.Pistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.GoldPistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.DiamondPistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.BloodStonePistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 30),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.IronSpacePistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.CopperSpacePistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.GoldSpacePistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Pistols, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpacePistol, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.LMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 6),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.GoldLMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 6),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.DiamondLMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.LMGs, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneLMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 60),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.SMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.GoldSMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.DiamondSMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneSMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 20),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.IronSpaceSMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.CopperSpaceSMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.GoldSpaceSMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.SMGs, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpaceSMGGun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.BoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.GoldBoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondBoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 20),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.IronSpaceBoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.CopperSpaceBoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.GoldSpaceBoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Rifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpaceBoltActionRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.PumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.GoldPumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.DiamondPumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.BloodStonePumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 20),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.IronSpacePumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.CopperSpacePumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.GoldSpacePumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Shotguns, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpacePumpShotgun, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 4)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.AssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 5),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.GoldAssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondAssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.BloodStoneAssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.BloodStoneBlock, 50),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.IronSpaceAssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.CopperSpaceAssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.GoldSpaceAssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 6),
				InventoryItem.CreateItem(InventoryItemIDs.Gold, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.AssaultRifles, InventoryItem.CreateItem(InventoryItemIDs.DiamondSpaceAssultRifle, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Slime, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Diamond, 7)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Advanced, InventoryItem.CreateItem(InventoryItemIDs.PrecisionLaser, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 5),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.CopperWall, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Copper, 2) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Copper, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.CopperWall, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.IronWall, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Iron, 2) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Iron, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.IronWall, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.GoldenWall, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Gold, 2) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Gold, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.GoldenWall, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Walls, InventoryItem.CreateItem(InventoryItemIDs.DiamondWall, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.Diamond, 2) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Ores, InventoryItem.CreateItem(InventoryItemIDs.Diamond, 1), new InventoryItem[] { InventoryItem.CreateItem(InventoryItemIDs.DiamondWall, 1) }));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.TNT, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1),
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.C4, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 3),
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.Grenade, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Explosives, InventoryItem.CreateItem(InventoryItemIDs.StickyGrenade, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.RPG, InventoryItem.CreateItem(InventoryItemIDs.RocketLauncher, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3),
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.RPG, InventoryItem.CreateItem(InventoryItemIDs.RocketLauncherGuided, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.ExplosivePowder, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 3),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 2),
				InventoryItem.CreateItem(InventoryItemIDs.SpaceRockInventory, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Doors, InventoryItem.CreateItem(InventoryItemIDs.Door, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Copper, 3)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.Doors, InventoryItem.CreateItem(InventoryItemIDs.IronDoor, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 5)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowWood, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.WoodBlock, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Stick, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowIron, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 5),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 1),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 1)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowGold, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 10),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
			Recipe.CookBook.Add(new Recipe(Recipe.RecipeTypes.OtherStructure, InventoryItem.CreateItem(InventoryItemIDs.GlassWindowDiamond, 1), new InventoryItem[]
			{
				InventoryItem.CreateItem(InventoryItemIDs.SandBlock, 10),
				InventoryItem.CreateItem(InventoryItemIDs.Coal, 2),
				InventoryItem.CreateItem(InventoryItemIDs.Iron, 2)
			}));
		}

		public static List<Recipe> CookBook = new List<Recipe>();

		private List<InventoryItem> _ingredients;

		private InventoryItem _result;

		private Recipe.RecipeTypes _type;

		public enum RecipeTypes
		{
			Ores,
			Components,
			Pickaxes,
			Spades,
			Axes,
			SpecialTools,
			Ammo,
			Knives,
			Pistols,
			Shotguns,
			Rifles,
			AssaultRifles,
			SMGs,
			LMGs,
			RPG,
			LaserSwords,
			Walls,
			Doors,
			OtherStructure,
			Explosives,
			Containers,
			SpawnPoints,
			Advanced
		}
	}
}
