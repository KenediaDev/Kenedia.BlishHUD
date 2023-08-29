using Kenedia.Modules.Core.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICheckable = Blish_HUD.Controls.ICheckable;
using Control = Blish_HUD.Controls.Control;

namespace Kenedia.Modules.QoL.Controls
{
    public class ModuleHotbar : Hotbar
    {
        protected override void SortButtons()
        {
            switch (SortType)
            {
                case SortType.ActivesFirst:
                    ItemsPanel.SortChildren<ModuleButton>((a, b) => b.Checked.CompareTo(a.Checked));
                    break;

                case SortType.ByModuleName:
                    ItemsPanel.SortChildren<ModuleButton>((a, b) => a.Module.Name.CompareTo(b.Module.Name));
                    break;
            }
        }

        public override void SetButtonsExpanded()
        {
            foreach (var c in ItemsPanel.Children.OfType<ModuleButton>())
            {
                c.Visible = c.Module.ShowInHotbar.Value && (ExpandBar || c.Checked);
            }
        }
    }
}
