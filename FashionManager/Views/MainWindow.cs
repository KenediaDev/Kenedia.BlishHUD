using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using TabbedWindow = Kenedia.Modules.Core.Views.TabbedWindow;

namespace Kenedia.Modules.FashionManager.Views
{
    public class MainWindow : TabbedWindow
    {
        public MainWindow(Module module) : base(
            TexturesService.GetTextureFromRef(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, 915, 665 + 0),
                new Rectangle(40, 20, 915 - 20, 665))
        {
            Title = "❤";
            Subtitle = "❤";
            Id = $"FashionManager_MainWindow";
            //Home House: 102339
            //Hand Mirror: 2208339
            //Name Edit: 536043
            //Mirror: 389728
            //Makeover Kit: 502053
            //Birthday Gift: 527123
            MainWindowEmblem = AsyncTexture2D.FromAssetId(2208339);
            Name = module.Name.SplitStringOnUppercase();
            Version = module.Version;

            SavesPosition = false;
            SavesSize = false;

            Width = 1250;
            Height = 900;

            Parent = Graphics.SpriteScreen;

            Tabs.Add(TemplateSlotsViewTab = new Tab(AsyncTexture2D.FromAssetId(866106), () => TemplateSlotsView = new TemplatesView(), "Template Slots"));
            Tabs.Add(HomeViewTab = new Tab(AsyncTexture2D.FromAssetId(866107), () => HomeView = new HomeView(), "Home"));

            SelectedTab = TemplateSlotsViewTab;

            Debug.WriteLine($"{SelectedTab?.Name}");
        }

        public TemplatesView TemplateSlotsView { get; private set; }

        public HomeView HomeView { get; private set; }

        public Tab HomeViewTab { get; }

        public Tab TemplateSlotsViewTab { get; }
    }
}
