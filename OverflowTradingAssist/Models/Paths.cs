using Kenedia.Modules.Core.Models;
using System.IO;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{
    public class Paths : PathCollection
    {
        public Paths()
        {
        }

        public Paths(Blish_HUD.Modules.Managers.DirectoriesManager directoriesManager, string moduleName) : base(directoriesManager, moduleName)
        {
            if (!Directory.Exists(TradeHistory)) _ = Directory.CreateDirectory(TradeHistory);
        }

        public string TradeHistory => $@"{BasePath}\{ModuleName}\history\";

        public string RepSheet => AccountName is not null ? $@"{AccountPath}{AccountName}_Overflow_Trade_Rep_Template.xlsx" : null;
    }
}
