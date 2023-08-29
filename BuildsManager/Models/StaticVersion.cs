using SemVer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class StaticVersion
    {
        public Version Nourishments { get; set; }
        public Version Enhancements { get; set; }
        public Version PveRunes { get; set; }
        public Version PvpRunes { get; set; }
        public Version PveSigils { get; set; }
        public Version PvpSigils { get; set; }
        public Version Infusions { get; set; }
        public Version Enrichments { get; set; }
        public Version Trinkets { get; set; }
        public Version Backs { get; set; }
        public Version Weapons { get; set; }
        public Version Armors { get; set; }
        public Version PowerCores { get; set; }
        public Version Relics { get; set; }
        public Version PvpAmulets { get; set; }
    }

    public class ItemMap : Dictionary<int, byte>
    {
        public Version Version { get; set; } = new(0, 0, 0);
    }

    public class ItemMapCollection
    {
        public ItemMap Nourishments { get; set; } = new();
        public ItemMap Enhancements { get; set; } = new();
        public ItemMap PveRunes { get; set; } = new();
        public ItemMap PvpRunes { get; set; } = new();
        public ItemMap PveSigils { get; set; } = new();
        public ItemMap PvpSigils { get; set; } = new();
        public ItemMap Infusions { get; set; } = new();
        public ItemMap Enrichments { get; set; } = new();
        public ItemMap Trinkets { get; set; } = new();
        public ItemMap Backs { get; set; } = new();
        public ItemMap Weapons { get; set; } = new();
        public ItemMap Armors { get; set; } = new();
        public ItemMap PowerCores { get; set; } = new();
        public ItemMap Relics { get; set; } = new();
        public ItemMap PvpAmulets { get; set; } = new();
    }
}
