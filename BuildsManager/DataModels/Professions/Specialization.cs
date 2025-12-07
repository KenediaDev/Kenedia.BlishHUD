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
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Specialization : IDisposable
    {
        private bool _isDisposed;

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
        private AsyncTexture2D Icon
        {
            get
            {
                if (field is not null) return field;

                if (IconAssetId is not 0)
                    field = AsyncTexture2D.FromAssetId(IconAssetId);

                return field;
            }

            set;
        }

        [DataMember]
        public int BackgroundAssetId { get; set; }

        private AsyncTexture2D Background
        {
            get
            {
                if (field is not null) return field;

                if (BackgroundAssetId is not 0)
                    field = AsyncTexture2D.FromAssetId(BackgroundAssetId);

                return field;
            }

            set;
        }

        [DataMember]
        public int? ProfessionIconAssetId { get; set; }

        private AsyncTexture2D ProfessionIcon
        {
            get
            {
                if (field is not null) return field;

                if (ProfessionIconAssetId is not null and not 0)
                {
                    field = AsyncTexture2D.FromAssetId((int)ProfessionIconAssetId);
                }
                return field;
            }

            set;
        }

        [DataMember]
        public int? ProfessionIconBigAssetId { get; set; }

        private AsyncTexture2D ProfessionIconBig
        {
            get
            {
                if (field is not null) return field;
                if (ProfessionIconBigAssetId is not null and not 0)
                {
                    field = AsyncTexture2D.FromAssetId((int)ProfessionIconBigAssetId);
                }
                return field;
            }

            set;
        }

        [DataMember]
        public Dictionary<int, Trait> MinorTraits { get; } = [];

        [DataMember]
        public Dictionary<int, Trait> MajorTraits { get; } = [];

        [DataMember]
        public Trait WeaponTrait { get; set; }

        public static Specialization FromByte(byte spezializationId, ProfessionType profession, Data data)
        {
            return data.Professions?[profession]?.Specializations.TryGetValue(spezializationId, out Specialization specialization) == true ? specialization : null;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Icon = null;
            Background = null;
            ProfessionIcon = null;
            ProfessionIconBig = null;

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
