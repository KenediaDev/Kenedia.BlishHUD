using Blish_HUD.Content;
using Gw2Sharp.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APISpecialization = Gw2Sharp.WebApi.V2.Models.Specialization;
using Kenedia.Modules.Core.Extensions;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Specialization : IDisposable
    {
        private bool _isDisposed;
        private AsyncTexture2D _icon;
        private AsyncTexture2D _background;
        private AsyncTexture2D _profession_icon;
        private AsyncTexture2D _profession_icon_big;

        public Specialization()
        {

        }

        public Specialization(APISpecialization specialization)
        {
            Apply(specialization);
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

                if (specialization.WeaponTrait is not null && traits.TryGetValue((int)specialization.WeaponTrait, out Trait weaponTrait))
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
        public LocalizedString Names { get; protected set; } = [];

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
                if (_icon is not null) return _icon;

                if (IconAssetId is not 0)
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
                if (_background is not null) return _background;

                if (BackgroundAssetId is not 0)
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
                if (_profession_icon is not null) return _profession_icon;

                if (ProfessionIconAssetId is not null and not 0)
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
                if (_profession_icon_big is not null) return _profession_icon_big;
                if (ProfessionIconBigAssetId is not null and not 0)
                {
                    _profession_icon_big = AsyncTexture2D.FromAssetId((int)ProfessionIconBigAssetId);
                }
                return _profession_icon_big;
            }
        }

        [DataMember]
        public Dictionary<int, Trait> MinorTraits { get; } = [];

        [DataMember]
        public Dictionary<int, Trait> MajorTraits { get; } = [];

        [DataMember]
        public Trait WeaponTrait { get; set; }

        public static Specialization FromByte(byte spezializationId, ProfessionType profession)
        {
            return BuildsManager.Data.Professions?[profession]?.Specializations.TryGetValue(spezializationId, out Specialization specialization) == true ? specialization : null;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _icon = null;
            _background = null;
            _profession_icon = null;
            _profession_icon_big = null;

            WeaponTrait?.Dispose();
            WeaponTrait = null;

            MinorTraits?.Values?.DisposeAll();
            MinorTraits.Clear();

            MajorTraits?.Values?.DisposeAll();
            MajorTraits.Clear();
        }

        public void Apply(APISpecialization specialization)
        {
            if (Enum.TryParse(specialization.Profession, out ProfessionType profession))
            {
                Profession = profession;
            }

            Id = specialization.Id;
            Elite = specialization.Elite;
            Name = specialization.Name;
            IconAssetId = specialization.Icon.GetAssetIdFromRenderUrl();
            BackgroundAssetId = specialization.Background.GetAssetIdFromRenderUrl();
            ProfessionIconAssetId = specialization.ProfessionIcon?.GetAssetIdFromRenderUrl();
            ProfessionIconBigAssetId = specialization.ProfessionIconBig?.GetAssetIdFromRenderUrl();
        }

        public void Apply(APISpecialization specialization, Gw2Sharp.WebApi.V2.IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Trait> traits)
        {
            Apply(specialization);

            if (Enum.TryParse(specialization.Profession, out ProfessionType _))
            {
                int index = 0;
                foreach (int t in specialization.MajorTraits)
                {
                    bool exists = MajorTraits.TryGetValue(t, out Trait trait);
                    trait ??= new Trait();

                    var apiTrait = traits.FirstOrDefault(x => x.Id == t);
                    trait.Apply(apiTrait);
                    trait.Index = index;

                    if (!exists)
                        MajorTraits.Add(t, trait);

                    index++;
                }

                index = 0;
                foreach (int t in specialization.MinorTraits)
                {
                    bool exists = MinorTraits.TryGetValue(t, out Trait trait);
                    trait ??= new Trait();

                    var apiTrait = traits.FirstOrDefault(x => x.Id == t);
                    trait.Apply(apiTrait);
                    trait.Index = index;

                    if (!exists)
                        MinorTraits.Add(t, trait);

                    index++;
                }

                if (specialization.WeaponTrait is not null)
                {
                    if (traits.FirstOrDefault(e => e.Id == (int)specialization.WeaponTrait) is Gw2Sharp.WebApi.V2.Models.Trait trait)
                    {
                        WeaponTrait ??= new();
                        WeaponTrait?.Apply(trait);
                    }
                }
            }
        }
    }
}
