using Kenedia.Modules.Core.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiItemType = Gw2Sharp.WebApi.V2.Models.ItemType;

namespace Kenedia.Modules.Core.Extensions
{
    public static class EnumExtension
    {
        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }
    }

    public static class ItemTypeExtension
    {
        public static ItemType ToItemType(this Gw2Sharp.WebApi.V2.Models.ApiEnum<ApiItemType> apiEnum)
        {
            return apiEnum.IsUnknown ?
                apiEnum.RawValue == "Mwcc" ? ItemType.Relic :
                Enum.TryParse(apiEnum.RawValue, out ItemType itemType) ? itemType : ItemType.Unknown

                : apiEnum.Value switch
                {
                    ApiItemType.Armor => ItemType.Armor,
                    ApiItemType.Back => ItemType.Back,
                    ApiItemType.Bag => ItemType.Bag,
                    ApiItemType.Consumable => ItemType.Consumable,
                    ApiItemType.CraftingMaterial => ItemType.CraftingMaterial,
                    ApiItemType.Container => ItemType.Container,
                    ApiItemType.Gathering => ItemType.Gathering,
                    ApiItemType.Gizmo => ItemType.Gizmo,
                    ApiItemType.MiniPet => ItemType.MiniPet,
                    ApiItemType.Tool => ItemType.Tool,
                    ApiItemType.Trinket => ItemType.Trinket,
                    ApiItemType.Trophy => ItemType.Trophy,
                    ApiItemType.UpgradeComponent => ItemType.UpgradeComponent,
                    ApiItemType.Weapon => ItemType.Weapon,
                    ApiItemType.Trait => ItemType.Trait,
                    ApiItemType.Key => ItemType.Key,
                    ApiItemType.JadeTechModule => ItemType.JadeTechModule,
                    ApiItemType.PowerCore => ItemType.PowerCore,
                    _ => ItemType.Unknown,
                };
        }

        public static ItemType ToItemType(this ApiItemType apiItemType)
        {
            return apiItemType switch
            {
                ApiItemType.Armor => ItemType.Armor,
                ApiItemType.Back => ItemType.Back,
                ApiItemType.Bag => ItemType.Bag,
                ApiItemType.Consumable => ItemType.Consumable,
                ApiItemType.CraftingMaterial => ItemType.CraftingMaterial,
                ApiItemType.Container => ItemType.Container,
                ApiItemType.Gathering => ItemType.Gathering,
                ApiItemType.Gizmo => ItemType.Gizmo,
                ApiItemType.MiniPet => ItemType.MiniPet,
                ApiItemType.Tool => ItemType.Tool,
                ApiItemType.Trinket => ItemType.Trinket,
                ApiItemType.Trophy => ItemType.Trophy,
                ApiItemType.UpgradeComponent => ItemType.UpgradeComponent,
                ApiItemType.Weapon => ItemType.Weapon,
                ApiItemType.Trait => ItemType.Trait,
                ApiItemType.Key => ItemType.Key,
                ApiItemType.JadeTechModule => ItemType.JadeTechModule,
                ApiItemType.PowerCore => ItemType.PowerCore,
                ApiItemType.Unknown => ItemType.Unknown,
                _ => ItemType.Unknown,
            };
        }

        public static ApiItemType ToApiItemType(this ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Armor => ApiItemType.Armor,
                ItemType.Back => ApiItemType.Back,
                ItemType.Bag => ApiItemType.Bag,
                ItemType.Consumable => ApiItemType.Consumable,
                ItemType.CraftingMaterial => ApiItemType.CraftingMaterial,
                ItemType.Container => ApiItemType.Container,
                ItemType.Gathering => ApiItemType.Gathering,
                ItemType.Gizmo => ApiItemType.Gizmo,
                ItemType.MiniPet => ApiItemType.MiniPet,
                ItemType.Tool => ApiItemType.Tool,
                ItemType.Trinket => ApiItemType.Trinket,
                ItemType.Trophy => ApiItemType.Trophy,
                ItemType.UpgradeComponent => ApiItemType.UpgradeComponent,
                ItemType.Weapon => ApiItemType.Weapon,
                ItemType.Trait => ApiItemType.Trait,
                ItemType.Key => ApiItemType.Key,
                ItemType.JadeTechModule => ApiItemType.JadeTechModule,
                ItemType.PowerCore => ApiItemType.PowerCore,
                ItemType.Relic => ApiItemType.Unknown,
                ItemType.Unknown => ApiItemType.Unknown,
                _ => ApiItemType.Unknown,
            };
        }
    }
}
