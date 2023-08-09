using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class ItemIdSelection : BaseSelection
    {
        private Dictionary<TemplateSlot, List<Selectable>> Selectables = new();

        public ItemIdSelection()
        {

        }
    }
}
