using Blish_HUD.Content;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Blish_HUD.Modules;
using Kenedia.Modules.BuildsManager.Controls.Tabs;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : TabbedWindow
    {
        private readonly TabbedRegion _tabbedRegion;

        public MainWindow(Module module, TemplatePresenter templatePresenter, TemplateTags templateTags, TagGroups tagGroups, SelectionPanel selectionPanel, AboutTab aboutTab, BuildTab buildTab, GearTab gearTab, QuickFiltersPanel quickFiltersPanel) : base(
            TexturesService.GetTextureFromRef(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, 915, 670 + 30),
                new Rectangle(40, 20, 915 - 13, 670 + 15))
        {
            TemplatePresenter = templatePresenter;
            Parent = Graphics.SpriteScreen;

            AboutTab = aboutTab;
            BuildTab = buildTab;
            GearTab = gearTab;
            QuickFiltersPanel = quickFiltersPanel;
            SelectionPanel = selectionPanel;
            AboutTab.MainWindow = SelectionPanel.MainWindow = this;

            Title = "❤";
            Subtitle = "❤";
            SavesPosition = true;
            Id = $"{module.Name} MainWindow";
            MainWindowEmblem = AsyncTexture2D.FromAssetId(156020);
            Name = module.Name;
            Version = module.Version;

            Width = 1250;
            Height = 900;

            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplatePresenter.NameChanged += TemplatePresenter_NameChanged;

            Tabs.Add(TemplateViewTab = new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(156720), () => TemplateView = new TemplateView(this, selectionPanel, aboutTab, buildTab, gearTab, quickFiltersPanel), strings.Templates));
            Tabs.Add(TagEditViewTab = new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(440021), () => TagEditView = new TagEditView(templateTags, tagGroups), strings.Tags));
            Tabs.Add(TagGroupViewTab = new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(578844), () => TagGroupView = new TagGroupView(tagGroups), strings.Group));
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            SubName = e.NewValue?.Name;
        }

        public Blish_HUD.Controls.Tab TemplateViewTab { get; }

        public TemplateView TemplateView { get; private set; }

        public Blish_HUD.Controls.Tab TagEditViewTab { get; }

        public TagEditView TagEditView { get; private set; }

        public Blish_HUD.Controls.Tab TagGroupViewTab { get; }

        public TagGroupView TagGroupView { get; private set; }

        public SelectionPanel SelectionPanel { get; }

        public AboutTab AboutTab { get; }

        public BuildTab BuildTab { get; }

        public GearTab GearTab { get; }

        public QuickFiltersPanel QuickFiltersPanel { get; }

        public TemplatePresenter TemplatePresenter { get; }

        protected override void OnTabChanged(Blish_HUD.ValueChangedEventArgs<Blish_HUD.Controls.Tab> e)
        {
            base.OnTabChanged(e);
            QuickFiltersPanel.Visible = e.NewValue == TemplateViewTab;

            SubName =
                e.NewValue == TemplateViewTab ? TemplatePresenter?.Template?.Name :
                e.NewValue == TagEditViewTab ? strings.Tags :
                e.NewValue == TagGroupViewTab ? strings.Group :
                string.Empty;
        }

        private void TemplatePresenter_NameChanged(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            SubName = e.NewValue;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (SelectionPanel is not null)
                SelectionPanel.Pointer.ZIndex = ZIndex + 1;

            if (QuickFiltersPanel is not null)
                QuickFiltersPanel.ZIndex = ZIndex + 1;
        }

        public override void Hide()
        {
            base.Hide();

            QuickFiltersPanel?.Hide();
        }

        protected override void DisposeControl()
        {
            Hide();

            base.DisposeControl();
            _tabbedRegion?.Dispose();
            BuildTab?.Dispose();
            GearTab?.Dispose();
            AboutTab?.Dispose();
            SelectionPanel?.Dispose();
            QuickFiltersPanel?.Dispose();
        }
    }
}
