using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Interfaces;
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
        private enum Pets
        {
            JungleStalker = 1,
            Boar = 2,
            Lynx = 3,
            KrytanDrakehound = 4,
            BrownBear = 5,
            CarrionDevourer = 6,
            SalamanderDrake = 7,
            AlpineWolf = 8,
            SnowLeopard = 9,
            Raven = 10,
            Jaguar = 11,
            MarshDrake = 12,
            BlueMoa = 13,
            WhiteMoa = 14,
            PinkMoa = 15,
            BlackMoa = 16,
            RedMoa = 17,
            IceDrake = 18,
            RiverDrake = 19,
            Murellow = 20,
            Shark = 21,
            FernHound = 22,
            BlackBear = 23,
            PolarBear = 24,
            Arctodus = 25,
            WhiptailDevourer = 26,
            LashtailDevourer = 27,
            Hyena = 28,
            Wolf = 29,
            Owl = 30,
            Eagle = 31,
            WhiteRaven = 32,
            ForestSpider = 33,
            JungleSpider = 34,
            CaveSpider = 35,
            BlackWidowSpider = 36,
            Warthog = 37,
            Siamoth = 38,
            Pig = 39,
            ArmorFish = 40,
            BlueJellyfish = 41,
            RedJellyfish = 42,
            RainbowJellyfish = 43,
            Hawk = 44,
            ReefDrake = 45,
            Smokescale = 46,
            Tiger = 47,
            ElectricWyvern = 48,
            FireWyvern = 51,
            Bristleback = 52,
            Cheetah = 54,
            SandLion = 55,
            Jacaranda = 57,
            RockGazelle = 59,
            FangedIboga = 61,
            WhiteTiger = 63,
            Wallow = 64,
            Phoenix = 65,
            SiegeTurtle = 66,
            AetherHunter = 67,
            SkyChakStriker = 68,
            Spinegazer = 69,
            Warclaw = 70,
            JanthiriBee = 71,
            RaptorSwiftwing = 72,
        }

        private enum PetOrder
        {
            BlueMoa,
            WhiteMoa,
            PinkMoa,
            BlackMoa,
            RedMoa,
            BrownBear,
            Murellow,
            BlackBear,
            PolarBear,
            Arctodus,
            JungleStalker,
            Lynx,
            SnowLeopard,
            Jaguar,
            Tiger,
            WhiteTiger,
            Cheetah,
            SandLion,
            Warclaw,
            Bristleback,
            SiegeTurtle,
            KrytanDrakehound,
            AlpineWolf,
            FernHound,
            Hyena,
            Wolf,
            SalamanderDrake,
            MarshDrake,
            IceDrake,
            RiverDrake,
            ReefDrake,
            CarrionDevourer,
            WhiptailDevourer,
            LashtailDevourer,
            Shark,
            ArmorFish,
            RedJellyfish,
            BlueJellyfish,
            RainbowJellyfish,
            Raven,
            Owl,
            Eagle,
            WhiteRaven,
            Hawk,
            Phoenix,
            RaptorSwiftwing,
            Jacaranda,
            ForestSpider,
            JungleSpider,
            CaveSpider,
            BlackWidowSpider,
            Boar,
            Warthog,
            Siamoth,
            Wallow,
            Pig,
            RockGazelle,
            ElectricWyvern,
            FireWyvern,
            Smokescale,
            FangedIboga,
            AetherHunter,
            SkyChakStriker,
            Spinegazer,
            JanthiriBee,
        }

        private bool _isDisposed;
        private readonly List<Pets> _aquaticPets = [
            Pets.JungleStalker,
            Pets.BrownBear,
            Pets.CarrionDevourer,
            Pets.SalamanderDrake,
            Pets.SnowLeopard,
            Pets.Jaguar,
            Pets.MarshDrake,
            Pets.IceDrake,
            Pets.RiverDrake,
            Pets.Murellow,
            Pets.Shark,
            Pets.BlackBear,
            Pets.PolarBear,
            Pets.Arctodus,
            Pets.WhiptailDevourer,
            Pets.LashtailDevourer,
            Pets.ArmorFish,
            Pets.BlueJellyfish,
            Pets.RedJellyfish,
            Pets.RainbowJellyfish,
            Pets.ReefDrake,
            Pets.Tiger,
            Pets.WhiteTiger,
        ];

        private readonly List<Pets> _terrestrialPets = [
            Pets.JungleStalker,
            Pets.Boar,
            Pets.Lynx,
            Pets.KrytanDrakehound,
            Pets.BrownBear,
            Pets.CarrionDevourer,
            Pets.SalamanderDrake,
            Pets.AlpineWolf,
            Pets.SnowLeopard,
            Pets.Raven,
            Pets.Jaguar,
            Pets.MarshDrake,
            Pets.BlueMoa,
            Pets.WhiteMoa,
            Pets.PinkMoa,
            Pets.BlackMoa,
            Pets.RedMoa,
            Pets.IceDrake,
            Pets.RiverDrake,
            Pets.Murellow,
            Pets.FernHound,
            Pets.BlackBear,
            Pets.PolarBear,
            Pets.Arctodus,
            Pets.WhiptailDevourer,
            Pets.LashtailDevourer,
            Pets.Hyena,
            Pets.Wolf,
            Pets.Owl,
            Pets.Eagle,
            Pets.WhiteRaven,
            Pets.ForestSpider,
            Pets.JungleSpider,
            Pets.CaveSpider,
            Pets.BlackWidowSpider,
            Pets.Warthog,
            Pets.Siamoth,
            Pets.Pig,
            Pets.Hawk,
            Pets.ReefDrake,
            Pets.Smokescale,
            Pets.Tiger,
            Pets.ElectricWyvern,
            Pets.FireWyvern,
            Pets.Bristleback,
            Pets.Cheetah,
            Pets.SandLion,
            Pets.Jacaranda,
            Pets.RockGazelle,
            Pets.FangedIboga,
            Pets.WhiteTiger,
            Pets.Wallow,
            Pets.Phoenix,
            Pets.SiegeTurtle,
            Pets.AetherHunter,
            Pets.SkyChakStriker,
            Pets.Spinegazer,
            Pets.Warclaw,
            Pets.JanthiriBee,
            Pets.RaptorSwiftwing,
        ];

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

            var pet_enum = (Pets)pet.Id;
            bool aquatic = _aquaticPets.Contains(pet_enum);
            bool terrestrial = _terrestrialPets.Contains(pet_enum);
            Enviroment  = 
                !terrestrial && !aquatic ? (Enviroment.Terrestrial | Enviroment.Aquatic) :
                (terrestrial ? Enviroment.Terrestrial : 0) | (aquatic ? Enviroment.Aquatic : 0);

            Order = Enum.TryParse(pet_enum.ToString(), out PetOrder parsedOrder) ? (int)parsedOrder : int.MaxValue;

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
