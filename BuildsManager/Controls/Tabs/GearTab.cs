using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Controls.Tabs
{
    public class GearTab : Blish_HUD.Controls.Container
    {
        public GearTab(TemplatePresenter templatePresenter  )
        {
            TemplatePresenter = templatePresenter;
        }

        public TemplatePresenter TemplatePresenter { get; }
    }
}
