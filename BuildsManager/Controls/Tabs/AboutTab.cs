using Kenedia.Modules.BuildsManager.Models;

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
