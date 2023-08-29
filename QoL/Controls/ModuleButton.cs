using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.QoL.SubModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.Controls
{
    public class ModuleButton : HotbarButton
    {
        public SubModule Module { get; set; }
    }
}
