using Blish_HUD.Content;
using Gw2Sharp.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;

namespace Kenedia.Modules.Characters.Models
{
    public class Specialization
    {
        public int Id { get; set; }

        public ProfessionType Profession { get; set; }

        public LocalizedString Names { get; set; } = [];

        [JsonIgnore]
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

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
        }

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
        }

        public void ApplyApiData(Gw2Sharp.WebApi.V2.Models.Specialization specialization)
        {
            Id = specialization.Id;
            Profession = Enum.TryParse(specialization.Profession, out ProfessionType professionType) ? professionType : default;

            IconAssetId = specialization.ProfessionIcon.GetAssetIdFromRenderUrl();
            IconBigAssetId = specialization.ProfessionIconBig.GetAssetIdFromRenderUrl();
            Name = specialization.Name;
        }
    }
}
