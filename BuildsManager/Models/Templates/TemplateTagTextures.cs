using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public static class TemplateTagTextures
    {
        private static readonly Dictionary<EncounterFlag, AsyncTexture2D> s_encounterTextures = new()
        {
            {EncounterFlag.None, null },
            {EncounterFlag.NormalMode, AsyncTexture2D.FromAssetId(741055)},
            {EncounterFlag.ChallengeMode, AsyncTexture2D.FromAssetId(741057)},
            {EncounterFlag.ValeGuardian, AsyncTexture2D.FromAssetId(1301792)},
            {EncounterFlag.Gorseval, AsyncTexture2D.FromAssetId(1301787)},
            {EncounterFlag.Sabetha, AsyncTexture2D.FromAssetId(1301795)},
            {EncounterFlag.Slothasor, AsyncTexture2D.FromAssetId(1377392)},
            {EncounterFlag.BanditTrio, AsyncTexture2D.FromAssetId(1377389)},
            {EncounterFlag.Matthias, AsyncTexture2D.FromAssetId(1377391)},
            {EncounterFlag.Escort, AsyncTexture2D.FromAssetId(1451172)},
            {EncounterFlag.KeepConstruct, AsyncTexture2D.FromAssetId(1451173)},
            {EncounterFlag.Xera, AsyncTexture2D.FromAssetId(1451174)},
            {EncounterFlag.Cairn, AsyncTexture2D.FromAssetId(1633961)},
            {EncounterFlag.MursaatOverseer, AsyncTexture2D.FromAssetId(1633963)},
            {EncounterFlag.Samarog, AsyncTexture2D.FromAssetId(1633967)},
            {EncounterFlag.Deimos, AsyncTexture2D.FromAssetId(1633966)},
            {EncounterFlag.SoullessHorror, AsyncTexture2D.FromAssetId(1894936)},
            {EncounterFlag.Statues, AsyncTexture2D.FromAssetId(1894799)},
            {EncounterFlag.River, AsyncTexture2D.FromAssetId(1894803)},
            {EncounterFlag.Dhuum, AsyncTexture2D.FromAssetId(1894937)},
            {EncounterFlag.ConjuredAmalgamate, AsyncTexture2D.FromAssetId(2038799)},
            {EncounterFlag.TwinLargos, AsyncTexture2D.FromAssetId(2038615)},
            {EncounterFlag.Qadim1, AsyncTexture2D.FromAssetId(2038618)},
            {EncounterFlag.Sabir, AsyncTexture2D.FromAssetId(1766790)},
            {EncounterFlag.Adina, AsyncTexture2D.FromAssetId(1766806)},
            {EncounterFlag.Qadim2, AsyncTexture2D.FromAssetId(2155914)},
            {EncounterFlag.Shiverpeaks, AsyncTexture2D.FromAssetId(2221486)},
            {EncounterFlag.KodanTwins, AsyncTexture2D.FromAssetId(771054)},
            {EncounterFlag.Fraenir, AsyncTexture2D.FromAssetId(2200036)},
            {EncounterFlag.Boneskinner, AsyncTexture2D.FromAssetId(2221487)},
            {EncounterFlag.WhisperOfJormag, AsyncTexture2D.FromAssetId(2247615)},
            {EncounterFlag.ForgingSteel, AsyncTexture2D.FromAssetId(2270861)},
            {EncounterFlag.ColdWar, AsyncTexture2D.FromAssetId(2293648)},
            {EncounterFlag.Aetherblade, AsyncTexture2D.FromAssetId(740290)},
            {EncounterFlag.Junkyard, AsyncTexture2D.FromAssetId(638233)},
            {EncounterFlag.KainengOverlook, AsyncTexture2D.FromAssetId(2752298)},
            {EncounterFlag.HarvestTemple, AsyncTexture2D.FromAssetId(2595195)},
            {EncounterFlag.OldLionsCourt, AsyncTexture2D.FromAssetId(2759435)},
        };

        private static readonly Dictionary<TemplateFlag, AsyncTexture2D> s_tagTextures = new()
        {
            {TemplateFlag.None, null },
            { TemplateFlag.Favorite, AsyncTexture2D.FromAssetId(547827) }, // 156331
            { TemplateFlag.Pve, AsyncTexture2D.FromAssetId(157085) },
            { TemplateFlag.Pvp, AsyncTexture2D.FromAssetId(157119) },
            { TemplateFlag.Wvw, AsyncTexture2D.FromAssetId(255428)}, //102491
            { TemplateFlag.OpenWorld, AsyncTexture2D.FromAssetId(255280) }, //460029 , 156625
            { TemplateFlag.Dungeons, AsyncTexture2D.FromAssetId(102478) }, //102478 , 866140
            { TemplateFlag.Fractals,AsyncTexture2D.FromAssetId(514379) }, // 1441449
            { TemplateFlag.Raids,AsyncTexture2D.FromAssetId(1128644) },
            { TemplateFlag.Power, AsyncTexture2D.FromAssetId(66722) },
            { TemplateFlag.Condition, AsyncTexture2D.FromAssetId(156600) },
            { TemplateFlag.Tank, AsyncTexture2D.FromAssetId(536048) },
            { TemplateFlag.Support, AsyncTexture2D.FromAssetId(156599) },
            { TemplateFlag.Heal, AsyncTexture2D.FromAssetId(536052) },
            { TemplateFlag.Quickness, AsyncTexture2D.FromAssetId(1012835) },
            { TemplateFlag.Alacrity, AsyncTexture2D.FromAssetId(1938787) },
            { TemplateFlag.WorldCompletion, AsyncTexture2D.FromAssetId(460029) },
            { TemplateFlag.Leveling, AsyncTexture2D.FromAssetId(993668) },
            { TemplateFlag.Farming, AsyncTexture2D.FromAssetId(784331) },
        };

        private static readonly Dictionary<EncounterFlag, Rectangle> s_encounterTexturesRegions = new()
        {

        };

        private static readonly Dictionary<TemplateFlag, Rectangle> s_tagTextureRegions = new()
        {
            {TemplateFlag.None, Rectangle.Empty },
            { TemplateFlag.Favorite, new(4, 4, 24, 24)},
            { TemplateFlag.Pve, Rectangle.Empty },
            { TemplateFlag.Pvp,  new(-2, -2, 36, 36) },
            { TemplateFlag.Wvw,  new(2,  2, 28, 28) },
            { TemplateFlag.OpenWorld, new(2,  2, 28, 28) },
            { TemplateFlag.Dungeons, new(-2,  -2, 36, 36) },
            { TemplateFlag.Fractals,  new(-4, -4, 40, 40) },
            { TemplateFlag.Raids,  new(-2, -2, 36, 36) },
            { TemplateFlag.Power, new(2, 2, 28, 28) },
            { TemplateFlag.Condition, new(2, 2, 28, 28) },
            { TemplateFlag.Tank, new(2, 2, 28, 28) },
            { TemplateFlag.Support, new(2, 2, 28, 28) },
            { TemplateFlag.Heal, new(2, 2, 28, 28) },
            { TemplateFlag.Quickness, new(-4, -4, 40, 40)},
            { TemplateFlag.Alacrity, new(-4, -4, 40, 40) },
            { TemplateFlag.WorldCompletion, new(-16, -16, 160, 160)},
            { TemplateFlag.Leveling,Rectangle.Empty },
            { TemplateFlag.Farming, new(-4, -4, 40, 40) },
        };

        public static TagTexture GetDetailedTexture(this TemplateFlag tag)
        {
            return s_tagTextures.TryGetValue(tag, out var texture) ? new(texture)
            {
                TextureRegion = s_tagTextureRegions.ContainsKey(tag) && s_tagTextureRegions[tag] != Rectangle.Empty ? s_tagTextureRegions[tag] : texture.Texture.Bounds,
                TemplateTag = tag,
            } : null;
        }

        public static TagTexture GetDetailedTexture(this EncounterFlag tag)
        {
            return s_encounterTextures.TryGetValue(tag, out var texture) ? new(texture)
            {
                TextureRegion = s_encounterTexturesRegions.ContainsKey(tag) && s_encounterTexturesRegions[tag] != Rectangle.Empty ? s_encounterTexturesRegions[tag] : texture.Texture.Bounds,
                TemplateTag = tag,
            } : null;
        }

        public static AsyncTexture2D GetTexture(this TemplateFlag tag)
        {
            return s_tagTextures.TryGetValue(tag, out var texture) ? texture : null;
        }

        public static AsyncTexture2D GetTexture(this EncounterFlag tag)
        {
            return s_encounterTextures.TryGetValue(tag, out var texture) ? texture : null;
        }
    }
}
