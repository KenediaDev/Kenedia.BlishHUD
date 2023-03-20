using Blish_HUD.Content;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [Flags]
    public enum TemplateTag
    {
        None = 0,
        Favorite = 1,
        Pve = 2,
        Pvp = 4,
        Wvw = 8,
        OpenWorld = 16,
        Dungeons = 32,
        Fractals = 64,
        Raids = 128,
        Power = 256,
        Condition = 512,
        Tank = 1024,
        Support = 2048,
        Heal = 4096,
        Quickness = 8192,
        Alacrity = 16384,
        WorldCompletion = 32768,
        Leveling = 65536,
        Farming = 131072,
    }

    public static class TemplateTagTextures
    {
        private static readonly Dictionary<TemplateTag, AsyncTexture2D> s_textures = new()
        {
            {TemplateTag.None, null },
            { TemplateTag.Favorite, AsyncTexture2D.FromAssetId(547827) }, // 156331
            { TemplateTag.Pve, AsyncTexture2D.FromAssetId(157085) },
            { TemplateTag.Pvp, AsyncTexture2D.FromAssetId(157119) },
            { TemplateTag.Wvw, AsyncTexture2D.FromAssetId(255428)}, //102491
            { TemplateTag.OpenWorld, AsyncTexture2D.FromAssetId(255280) }, //460029 , 156625
            { TemplateTag.Dungeons, AsyncTexture2D.FromAssetId(102478) }, //102478 , 866140
            { TemplateTag.Fractals,AsyncTexture2D.FromAssetId(514379) }, // 1441449
            { TemplateTag.Raids,AsyncTexture2D.FromAssetId(1128644) },
            { TemplateTag.Power, AsyncTexture2D.FromAssetId(66722) },
            { TemplateTag.Condition, AsyncTexture2D.FromAssetId(156600) },
            { TemplateTag.Tank, AsyncTexture2D.FromAssetId(536048) },
            { TemplateTag.Support, AsyncTexture2D.FromAssetId(156599) },
            { TemplateTag.Heal, AsyncTexture2D.FromAssetId(536052) },
            { TemplateTag.Quickness, AsyncTexture2D.FromAssetId(1012835) },
            { TemplateTag.Alacrity, AsyncTexture2D.FromAssetId(1938787) },
            { TemplateTag.WorldCompletion, AsyncTexture2D.FromAssetId(460029) },
            { TemplateTag.Leveling, AsyncTexture2D.FromAssetId(993668) },
            { TemplateTag.Farming, AsyncTexture2D.FromAssetId(784331) },
        };

        private static readonly Dictionary<TemplateTag, Rectangle> s_textureRegions = new()
        {
            {TemplateTag.None, Rectangle.Empty },
            { TemplateTag.Favorite, new(4, 4, 24, 24)},
            { TemplateTag.Pve, Rectangle.Empty },
            { TemplateTag.Pvp,  new(-2, -2, 36, 36) },
            { TemplateTag.Wvw,  new(2,  2, 28, 28) },
            { TemplateTag.OpenWorld, new(2,  2, 28, 28) },
            { TemplateTag.Dungeons, new(-2,  -2, 36, 36) },
            { TemplateTag.Fractals,  new(-4, -4, 40, 40) },
            { TemplateTag.Raids,  new(-2, -2, 36, 36) },
            { TemplateTag.Power, new(2, 2, 28, 28) },
            { TemplateTag.Condition, new(2, 2, 28, 28) },
            { TemplateTag.Tank, new(2, 2, 28, 28) },
            { TemplateTag.Support, new(2, 2, 28, 28) },
            { TemplateTag.Heal, new(2, 2, 28, 28) },
            { TemplateTag.Quickness, new(-4, -4, 40, 40)},
            { TemplateTag.Alacrity, new(-4, -4, 40, 40) },
            { TemplateTag.WorldCompletion, new(-16, -16, 160, 160)},
            { TemplateTag.Leveling,Rectangle.Empty },
            { TemplateTag.Farming, new(-4, -4, 40, 40) },
        };

        private static readonly Dictionary<TemplateTag, AsyncTexture2D> s_graytextures = new()
        {
            {TemplateTag.None, null },
            { TemplateTag.Favorite, null },
            { TemplateTag.Pve, null },
            { TemplateTag.Pvp, null },
            { TemplateTag.Wvw, null}, //102491
            { TemplateTag.OpenWorld, null }, //460029 , 156625
            { TemplateTag.Dungeons, BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\flag_textures\32.png") }, //102478
            { TemplateTag.Fractals,null }, // 1441449
            { TemplateTag.Raids, BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\flag_textures\128.png") },
            { TemplateTag.Power, null },
            { TemplateTag.Condition, null },
            { TemplateTag.Tank, null },
            { TemplateTag.Support, null },
            { TemplateTag.Heal, null },
            { TemplateTag.Quickness, BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\flag_textures\8192.png") },
            { TemplateTag.Alacrity, null },
        };

        public static TagTexture GetDetailedTexture(this TemplateTag tag)
        {
            return s_textures.TryGetValue(tag, out var texture) ? new(texture)
            {
                TextureRegion = s_textureRegions[tag] != Rectangle.Empty ? s_textureRegions[tag] : texture.Texture.Bounds,
                TemplateTag = tag,
            } : null;
        }

        public static AsyncTexture2D GetTexture(this TemplateTag tag)
        {
            return s_textures.TryGetValue(tag, out var texture) ? texture : null;
        }

        public static AsyncTexture2D GetGrayTexture(this TemplateTag tag)
        {
            if (s_graytextures[tag] == null)
            {
                if (s_textures.TryGetValue(tag, out var color_texture))
                {
                    void TextureSwapped(object sender, Blish_HUD.ValueChangedEventArgs<Microsoft.Xna.Framework.Graphics.Texture2D> e)
                    {
                        color_texture.TextureSwapped -= TextureSwapped;
                        s_graytextures[tag] = color_texture.Texture.ToGrayScaledPalettable();
                    }

                    s_graytextures[tag] = color_texture.Texture.ToGrayScaledPalettable();
                    color_texture.TextureSwapped += TextureSwapped;
                }
            }

            return s_graytextures[tag];
        }
    }

    [DataContract]
    public class Template
    {
        private BuildTemplate _buildTemplate;
        private GearTemplate _gearTemplate;
        private ProfessionType _profession;
        private string _description;
        private string _name = "Power Deadeye - Dagger/Dagger | Shortbow";
        private string _id;
        private bool _terrestrial = true;
        private AttunementType _mainAttunement = AttunementType.Fire;
        private AttunementType _altAttunement = AttunementType.Earth;
        private LegendSlot _legendSlot = LegendSlot.TerrestrialActive;
        private bool loaded = true;

        public Template()
        {
            BuildTemplate = new();
            GearTemplate = new();
        }

        public event PropertyChangedEventHandler Changed;

        //[DataMember]
        //public string Id { get => _id; set => Common.SetProperty(ref _id, value, Changed); }

        public ObservableList<string> TextTags { get; private set; } = new();

        [DataMember]
        public TemplateTag Tags { get; set; }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, Changed); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, Changed); }

        [DataMember]
        public string GearCode
        {
            get => GearTemplate?.ParseGearCode();
            set => Gearcode = value;
        }

        [DataMember]
        public string BuildCode
        {
            get => BuildTemplate?.ParseBuildCode();
            set => Buildcode = value;
        }

        public ProfessionType Profession
        {
            get => BuildTemplate.Profession;
            set
            {
                if (Common.SetProperty(ref _profession, value, Changed))
                {
                    if (BuildTemplate != null)
                    {
                        BuildTemplate.Profession = value;
                        //GearTemplate.Profession = value;
                    }
                }
            }
        }

        [DataMember]
        public Races Race = Races.None;
        private bool _pvE = true;
        private CancellationTokenSource _cancellationTokenSource;

        private string Gearcode
        {
            set
            {
                loaded = false;
                GearTemplate?.LoadFromCode(value);
                loaded = true;
            }
        }

        private string Buildcode
        {
            set
            {
                loaded = false;
                BuildTemplate?.LoadFromCode(value);
                loaded = true;
            }
        }

        public TemplateAttributes Attributes { get; private set; } = new();

        public Specialization EliteSpecialization => BuildTemplate?.Specializations[SpecializationSlot.Line_3]?.Specialization?.Elite == true ? BuildTemplate.Specializations[SpecializationSlot.Line_3].Specialization : null;

        public AttunementType MainAttunement { get => _mainAttunement; set => Common.SetProperty(ref _mainAttunement, value, Changed); }

        public AttunementType AltAttunement { get => _altAttunement; set => Common.SetProperty(ref _altAttunement, value, Changed); }

        public bool Terrestrial
        {
            get => LegendSlot is LegendSlot.TerrestrialActive or LegendSlot.TerrestrialInactive;
            set
            {
                switch (LegendSlot)
                {
                    case LegendSlot.AquaticActive:
                    case LegendSlot.AquaticInactive:
                        LegendSlot newTerrestialSlot = LegendSlot is LegendSlot.AquaticActive ? LegendSlot.TerrestrialActive : LegendSlot.TerrestrialInactive;
                        _ = Common.SetProperty(ref _legendSlot, newTerrestialSlot, Changed);

                        break;

                    case LegendSlot.TerrestrialActive:
                    case LegendSlot.TerrestrialInactive:
                        LegendSlot newAquaticSlot = LegendSlot is LegendSlot.TerrestrialActive ? LegendSlot.AquaticActive : LegendSlot.AquaticInactive;
                        _ = Common.SetProperty(ref _legendSlot, newAquaticSlot, Changed);
                        break;
                }
            }
        }

        public LegendSlot LegendSlot
        {
            get => _legendSlot;
            set => Common.SetProperty(ref _legendSlot, value, Changed);
        }

        /// <summary>
        /// Active Transform Skill which sets weapon skills to its childs and disables all others
        /// </summary>
        public Skill ActiveTransform { get; set; }

        /// <summary>
        /// Active Bundle Skill which sets weapon skills to its childs
        /// </summary>
        public Skill ActiveBundle { get; set; }

        public BuildTemplate BuildTemplate
        {
            get => _buildTemplate; set
            {
                var prev = _buildTemplate;

                if (Common.SetProperty(ref _buildTemplate, value, Changed))
                {
                    if (prev != null) prev.Changed -= TemplateChanged;

                    _buildTemplate ??= new();
                    _buildTemplate.Changed += TemplateChanged;
                }
            }
        }

        public GearTemplate GearTemplate
        {
            get => _gearTemplate; set
            {
                var prev = _gearTemplate;
                if (Common.SetProperty(ref _gearTemplate, value, Changed))
                {
                    if (prev != null) prev.PropertyChanged -= TemplateChanged;

                    _gearTemplate ??= new();
                    _gearTemplate.PropertyChanged += TemplateChanged;
                }
            }
        }

        public bool PvE { get => _pvE; internal set => Common.SetProperty(ref _pvE, value, TemplateChanged); }

        public SkillCollection GetActiveSkills()
        {
            return LegendSlot switch
            {
                LegendSlot.AquaticInactive => BuildTemplate.InactiveAquaticSkills,
                LegendSlot.TerrestrialInactive => BuildTemplate.InactiveTerrestrialSkills,
                LegendSlot.AquaticActive => BuildTemplate.AquaticSkills,
                LegendSlot.TerrestrialActive => BuildTemplate.TerrestrialSkills,
                _ => null,
            };
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (MainAttunement != AltAttunement && EliteSpecialization?.Id != (int)SpecializationType.Weaver)
            {
                AltAttunement = MainAttunement;
            }

            Changed?.Invoke(sender, e);
            _ = Save();
        }

        public async Task Save()
        {
            if (!loaded) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Delay(1000, _cancellationTokenSource.Token);
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                string path = BuildsManager.ModuleInstance.Paths.TemplatesPath;
                try
                {
                    if (!Directory.Exists(path)) _ = Directory.CreateDirectory(path);
                    string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                    File.WriteAllText($@"{path}\{Common.MakeValidFileName(Name.Trim())}.json", json);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
