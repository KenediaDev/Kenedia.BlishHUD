using Blish_HUD.Content;
using Gw2Sharp.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;

namespace Kenedia.Modules.Characters.Models
{
    public class Profession
    {
        public Profession()
        {
        }

        public void ApplyApiData(Gw2Sharp.WebApi.V2.Models.Profession profession)
        {
            Id = Enum.TryParse(profession.Id, out ProfessionType professionType) ? professionType : default;
            WeightClass = Id switch
            {
                ProfessionType.Guardian => ArmorWeight.Heavy,
                ProfessionType.Warrior => ArmorWeight.Heavy,
                ProfessionType.Revenant => ArmorWeight.Heavy,

                ProfessionType.Engineer => ArmorWeight.Medium,
                ProfessionType.Ranger => ArmorWeight.Medium,
                ProfessionType.Thief => ArmorWeight.Medium,

                ProfessionType.Elementalist => ArmorWeight.Light,
                ProfessionType.Mesmer => ArmorWeight.Light,
                ProfessionType.Necromancer => ArmorWeight.Light,

                _ => ArmorWeight.Heavy,
            };

            IconAssetId = profession.Icon.GetAssetIdFromRenderUrl();
            IconBigAssetId = profession.IconBig.GetAssetIdFromRenderUrl();
            Name = profession.Name;
        }

        public ProfessionType Id { get; set; }

        [JsonIgnore]
        public Color Color
        {
            get
            {
                if (field == Color.Transparent)
                {
                    field = Id.GetProfessionColor();
                }

                return field;
            }
        } = Color.Transparent;

        public ArmorWeight WeightClass { get; set; }

        public int IconAssetId { get; set; }

        public int IconBigAssetId { get; set; }

        [JsonIgnore]
        public AsyncTexture2D Icon
        {
            get
            {
                if (field == null && IconAssetId != 0)
                {
                    field = AsyncTexture2D.FromAssetId(IconAssetId);
                }

                return field;
            }
        } = null;

        [JsonIgnore]
        public AsyncTexture2D IconBig
        {
            get
            {
                if (field == null && IconAssetId != 0)
                {
                    field = AsyncTexture2D.FromAssetId(IconBigAssetId);
                }

                return field;
            }
        } = null;

        public LocalizedString Names { get; set; } = [];

        [JsonIgnore]
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }
    }
}
