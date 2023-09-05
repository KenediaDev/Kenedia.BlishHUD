using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.OverflowTradingAssist.Data
{
    public class HostedItems : DataEntry<int>
    {
        public override Task<bool> LoadAndUpdate(string name, SemVer.Version version, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            return base.LoadAndUpdate(name, version, path, gw2ApiManager, cancellationToken);
        }
    }
}
