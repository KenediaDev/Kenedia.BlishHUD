using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class GearSelection : BaseSelection
    {
        private Dictionary<GearTemplateSlot, List<Selectable>> Selectables = new();

        public GearSelection()
        {

        }

        public GearTemplateSlot ActiveSlot { get; set; }
    }
}
