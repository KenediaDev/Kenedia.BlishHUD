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
using Blish_HUD.Input;
using Kenedia.Modules.Core.Extensions;
using static Blish_HUD.ContentService;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using System;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : TabbedWindow
    {
        private DetailedTexture _quickFilterToggle = new(440021)
        {
            HoverDrawColor = Color.White,
            DrawColor = Color.White * 0.5F,
        };
        private readonly TabbedRegion _tabbedRegion;

        public MainWindow(Module module, TemplatePresenter templatePresenter, TemplateTags templateTags, TagGroups tagGroups, SelectionPanel selectionPanel, AboutTab aboutTab, BuildTab buildTab, GearTab gearTab, QuickFiltersPanel quickFiltersPanel, Settings settings) : base(
            TexturesService.GetTextureFromRef(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, 915, 665 + 0),
                new Rectangle(40, 20, 915 - 20, 665))
        {
            TemplatePresenter = templatePresenter;
            Parent = Graphics.SpriteScreen;

            AboutTab = aboutTab;
            BuildTab = buildTab;
            GearTab = gearTab;
            QuickFiltersPanel = quickFiltersPanel;
            Settings = settings;
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
            Tabs.Add(TagEditViewTab = new Blish_HUD.Controls.Tab(TexturesService.GetTextureFromRef(textures_common.Tag, nameof(textures_common.Tag)), () => TagEditView = new TagEditView(templateTags, tagGroups), strings.Tags));
            Tabs.Add(SettingsViewTab = new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(157109), () => SettingsView = new SettingsView(settings), strings_common.Settings));
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            SubName = e.NewValue?.Name;
        }

        public Blish_HUD.Controls.Tab TemplateViewTab { get; }

        public TemplateView TemplateView { get; private set; }

        public Blish_HUD.Controls.Tab TagEditViewTab { get; }

        public TagEditView TagEditView { get; private set; }

        public SelectionPanel SelectionPanel { get; }

        public AboutTab AboutTab { get; }

        public BuildTab BuildTab { get; }

        public GearTab GearTab { get; }

        public QuickFiltersPanel QuickFiltersPanel { get; }
        public Settings Settings { get; }

        public TemplatePresenter TemplatePresenter { get; }

        public Blish_HUD.Controls.Tab SettingsViewTab { get; private set; }

        public SettingsView SettingsView { get; private set; }

        protected override void OnTabChanged(Blish_HUD.ValueChangedEventArgs<Blish_HUD.Controls.Tab> e)
        {
            base.OnTabChanged(e);

            if (e.NewValue == TemplateViewTab)
            {
                if (Settings.ShowQuickFilterPanelOnTabOpen.Value)
                {
                    QuickFiltersPanel.Show();
                }
            }

            SubName =
                e.NewValue == TemplateViewTab ? TemplatePresenter?.Template?.Name :
                e.NewValue == SettingsViewTab ? strings_common.Settings :
                e.NewValue == TagEditViewTab ? strings.Tags :
                string.Empty;
        }

        private void TemplatePresenter_NameChanged(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            SubName = e.NewValue;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (Settings.ShowQuickFilterPanelOnWindowOpen.Value)
            {
                QuickFiltersPanel.Show();
            }

            SelectionPanel.BuildSelection.FilterTemplates();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            int index = 1;

            int TAB_VERTICALOFFSET = 40 + 40;
            int TAB_HEIGHT = 50;
            int TAB_Width = 84;

            Rectangle b = new(0, TAB_VERTICALOFFSET + (TAB_HEIGHT * index), TAB_Width, TAB_HEIGHT);

            if (_quickFilterToggle.Bounds.Contains(RelativeMousePosition))
            {
                QuickFiltersPanel.ToggleVisibility();
            }
            else
            {
                base.OnClick(e);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (SelectionPanel is not null)
                SelectionPanel.Pointer.ZIndex = ZIndex + (ActiveWindow == this ? 1 : 0);

            if (QuickFiltersPanel is not null)
                QuickFiltersPanel.ZIndex = ZIndex;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _quickFilterToggle.Bounds = new(8, 45, 32, 32);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _quickFilterToggle.Draw(this, spriteBatch, RelativeMousePosition, null, null, QuickFiltersPanel.Visible);

            if (_quickFilterToggle.Hovered)
                BasicTooltipText = strings.ToggleQuickFilters;
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
