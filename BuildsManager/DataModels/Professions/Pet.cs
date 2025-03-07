﻿using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using APIPet = Gw2Sharp.WebApi.V2.Models.Pet;

namespace Kenedia.Modules.BuildsManager.DataModels.Professions
{
    [DataContract]
    public class Pet : IDisposable, IBaseApiData
    {
        private bool _isDisposed;
        private readonly List<int> _aquaticPets = [1, 5, 6, 7, 9, 11, 12, 18, 19, 20, 21, 23, 24, 25, 26, 27, 40, 41, 42, 43, 45, 47, 63,];
        private readonly List<int> _terrestrialPets = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 44, 45, 46, 47, 48, 51, 52, 54, 55, 57, 59, 61, 63, 64, 65, 66];

        private AsyncTexture2D _icon;
        private AsyncTexture2D _selectedIcon;

        public Pet()
        {

        }

        public Pet(APIPet pet)
        {
            Apply(pet);
        }

        public Pet(APIPet pet, List<Skill> skills) : this(pet)
        {
            foreach (PetSkill petSkill in pet.Skills)
            {
                var skill = skills.Find(e => e.Id == petSkill.Id);

                if (skill is not null)
                {
                    Skills.Add(petSkill.Id, skill);
                }
            }

            ApplyLanguage(pet, skills);
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = [];

        public string Description
        {
            get => Descriptions.Text;
            set => Descriptions.Text = value;
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
        public AsyncTexture2D SelectedIcon
        {
            get
            {
                if (_selectedIcon is not null) return _selectedIcon;

                int assetId = IconAssetId == 52565 ? 1769874 : IconAssetId + 1;
                _selectedIcon = AsyncTexture2D.FromAssetId(assetId);
                return _selectedIcon;
            }
        }

        [DataMember]
        public Dictionary<int, Skill> Skills { get; set; } = [];

        [DataMember]
        public Enviroment Enviroment { get; set; }

        [DataMember]
        public int Order { get; set; }

        public static Pet FromByte(byte id)
        {
            return BuildsManager.Data.Pets?.TryGetValue((int)id, out Pet pet) == true ? pet : null;
        }

        public void ApplyLanguage(APIPet pet)
        {
            Name = pet.Name;
            Description = pet.Description;
        }

        public void ApplyLanguage(APIPet pet, List<Skill> skills)
        {
            ApplyLanguage(pet);

            foreach (var petSkill in Skills)
            {
                var skill = skills.Find(e => e.Id == petSkill.Value.Id);

                if (skill is not null)
                {
                    petSkill.Value.Name = skill.Name;
                    petSkill.Value.Description = skill.Description;
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _icon = null;
            _selectedIcon = null;

            Skills?.Values?.DisposeAll();
            Skills?.Clear();
        }

        internal void Apply(APIPet pet)
        {
            Id = pet.Id;
            IconAssetId = pet.Icon.GetAssetIdFromRenderUrl();

            bool aquatic = _aquaticPets.Contains(pet.Id);
            bool terrestrial = _terrestrialPets.Contains(pet.Id);
            Enviroment = (terrestrial ? Enviroment.Terrestrial : 0) | (aquatic ? Enviroment.Aquatic : 0);

            var petOrder = new List<int>()
            {
                13, 14, 15, 16, 17,
                5, 20, 23, 24, 25,
                1, 3, 9, 11, 47, 63,
                54,
                55,
                52,
                66,
                4, 8, 22, 28, 29,
                7, 12, 18, 19, 45,
                6, 26, 27,
                10, 30, 31, 32, 44, 65,
                57,
                33, 34, 35, 36,
                2, 37, 38, 64, 39,
                59,
                48, 51,
                46,
                61,
                21, 40,42, 41,43,
            };

            Order = petOrder.IndexOf(pet.Id);

            ApplyLanguage(pet);
        }

        internal void Apply(APIPet pet, IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Skill> skills)
        {
            Apply(pet);

            foreach (PetSkill petSkill in pet.Skills)
            {
                var skill = skills.FirstOrDefault(e => e.Id == petSkill.Id);

                if (skill is not null)
                {
                    Skills.Add(petSkill.Id, new Skill(skill));
                }
            }
        }
    }
}
