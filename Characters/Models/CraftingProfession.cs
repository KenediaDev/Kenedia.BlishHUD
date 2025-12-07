using Blish_HUD.Content;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;

namespace Kenedia.Modules.Characters.Models
{
    public class CraftingProfession
    {
        public CraftingDisciplineType Id { get; set; }

        public int MaxRating { get; set; }

        public LocalizedString Names { get; set; } = [];

        [JsonIgnore]
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        public int IconAssetId { get; set; }

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
        }
    }
}
