using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using System.Collections.Generic;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.Selection
{
    public class ItemIdSelection : BaseSelection
    {
        private Dictionary<GearTemplateSlot, List<Selectable>> Selectables = new();

        public ItemIdSelection()
        {

        }
    }
}
