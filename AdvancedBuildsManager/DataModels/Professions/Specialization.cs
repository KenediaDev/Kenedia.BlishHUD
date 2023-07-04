using Blish_HUD.Content;
using Gw2Sharp.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APISpecialization = Gw2Sharp.WebApi.V2.Models.Specialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions
{
    [DataContract]
    public class Specialization
    {
        private AsyncTexture2D _icon;
        private AsyncTexture2D _background;
        private AsyncTexture2D _profession_icon;
        private AsyncTexture2D _profession_icon_big;

        public Specialization()
        {

        }

        public Specialization(APISpecialization specialization)
        {
            if (Enum.TryParse(specialization.Profession, out ProfessionType profession))
            {
                Profession = profession;
                Id = specialization.Id;
                Elite = specialization.Elite;
                Name = specialization.Name;
                IconAssetId = specialization.Icon.GetAssetIdFromRenderUrl();
                BackgroundAssetId = specialization.Background.GetAssetIdFromRenderUrl();
                ProfessionIconAssetId = specialization.ProfessionIcon?.GetAssetIdFromRenderUrl();
                ProfessionIconBigAssetId = specialization.ProfessionIconBig?.GetAssetIdFromRenderUrl();
            }
        }

        public Specialization(APISpecialization specialization, Dictionary<int, Trait> traits) : this(specialization)
        {
            if (Enum.TryParse(specialization.Profession, out ProfessionType _))
            {
                int index = 0;
                foreach (int t in specialization.MajorTraits)
                {
                    if (traits.TryGetValue(t, out Trait trait))
                    {
                        trait.Index = index;
                        MajorTraits.Add(t, trait);
                    }

                    index++;
                }

                index = 0;
                foreach (int t in specialization.MinorTraits)
                {
                    if (traits.TryGetValue(t, out Trait trait))
                    {
                        trait.Index = index;
                        MinorTraits.Add(t, trait);
                    }
                    index++;
                }

                if (specialization.WeaponTrait != null && traits.TryGetValue((int)specialization.WeaponTrait, out Trait weaponTrait))
                {
                    WeaponTrait = weaponTrait;
                }
            }
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public bool Elite { get; set; }

        [DataMember]
        public ProfessionType Profession { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public int IconAssetId { get; set; }
        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon != null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(IconAssetId);
                return _icon;
            }
        }

        [DataMember]
        public int BackgroundAssetId { get; set; }
        public AsyncTexture2D Background
        {
            get
            {
                if (_background != null) return _background;

                _background = AsyncTexture2D.FromAssetId(BackgroundAssetId);
                return _background;
            }
        }

        [DataMember]
        public int? ProfessionIconAssetId { get; set; }
        public AsyncTexture2D ProfessionIcon
        {
            get
            {
                if (_profession_icon != null) return _profession_icon;

                if (ProfessionIconAssetId != null)
                {
                    _profession_icon = AsyncTexture2D.FromAssetId((int)ProfessionIconAssetId);
                }
                return _profession_icon;
            }
        }

        [DataMember]
        public int? ProfessionIconBigAssetId { get; set; }
        public AsyncTexture2D ProfessionIconBig
        {
            get
            {
                if (_profession_icon_big != null) return _profession_icon_big;
                if(ProfessionIconBigAssetId != null)
                {
                    _profession_icon_big = AsyncTexture2D.FromAssetId((int)ProfessionIconBigAssetId);
                }
                return _profession_icon_big;
            }
        }

        [DataMember]
        public Dictionary<int, Trait> MinorTraits { get; } = new();

        [DataMember]
        public Dictionary<int, Trait> MajorTraits { get; } = new();

        [DataMember]
        public Trait WeaponTrait { get; set; }
    }
}
