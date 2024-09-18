using Kenedia.Modules.BuildsManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Controls.Tabs
{
    public class AboutTab : Blish_HUD.Controls.Container
    {
        public AboutTab(TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;
        }

        public TemplatePresenter TemplatePresenter { get; }
    }
}
