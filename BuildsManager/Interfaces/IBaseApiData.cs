using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;

namespace Kenedia.Modules.BuildsManager.Interfaces
{
    public interface IStatTemplateEntry
    {
        Stat Stat { get; }
    }

    public interface IRuneTemplateEntry
    {
        Rune Rune { get; }
    }

    public interface ISingleSigilTemplateEntry
    {
        Sigil Sigil1 { get; }
    }

    public interface IPvpSigilTemplateEntry
    {
        Sigil PvpSigil { get; }
    }

    public interface IDoubleSigilTemplateEntry : ISingleSigilTemplateEntry
    {
        Sigil Sigil2 { get; }
    }

    public interface ISingleInfusionTemplateEntry
    {
        Infusion Infusion1 { get; }
    }

    public interface IDoubleInfusionTemplateEntry : ISingleInfusionTemplateEntry
    {
        Infusion Infusion2 { get; }
    }

    public interface ITripleInfusionTemplateEntry : IDoubleInfusionTemplateEntry
    {
        Infusion Infusion3 { get; }
    }

    public interface IItemTemplateEntry
    {
        BaseItem Item { get; }
    }

    public interface IWeaponTemplateEntry
    {
        Weapon Weapon { get; }
    }

    public interface IEnrichmentTemplateEntry
    {
        Enrichment Enrichment { get; }
    }

    public interface IArmorTemplateEntry
    {
        Armor Armor { get; }
    }

    public interface IBaseApiData : IDataMember
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        //public AsyncTexture2D Icon { get; }
    }
}
