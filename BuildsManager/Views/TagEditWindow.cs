using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Views;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditWindow : TabbedWindow
    {
        public TagEditWindow(TemplateTags templateTags, TagGroups tagGroups) : base(
            TexturesService.GetTextureFromRef(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, 915, 670 + 30),
                new Rectangle(40, 40, 915 - 3, 670))
        {
            TemplateTags = templateTags;
            TagGroups = tagGroups;

            Id = $"{nameof(BuildsManager)}EditTagAndGroups";
            Title = "Edit Tags and Groups";
            MainWindowEmblem = AsyncTexture2D.FromAssetId(156020);
            SubWindowEmblem = AsyncTexture2D.FromAssetId(156027);
            Parent = Graphics.SpriteScreen;

            SavesPosition = true;
            SavesSize = true;
            CanResize = true;

            Tabs.Add(TagEditViewTab = new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(156025), () => new TagEditView(TemplateTags, TagGroups), strings.Tags));
            Tabs.Add(TagGroupViewTab = new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(578844), () => new TagGroupView(TagGroups), strings.Group));
        }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        public Blish_HUD.Controls.Tab TagEditViewTab { get; }

        public Blish_HUD.Controls.Tab TagGroupViewTab { get; }

        protected override void OnResized(Blish_HUD.Controls.ResizedEventArgs e)
        {
            var minSize = new Point(600, 300);

            if (e.CurrentSize.X >= minSize.X && e.CurrentSize.Y >= minSize.Y)
            {
                base.OnResized(e);
            }
            else
            {
                Size = new Point(Math.Max(e.CurrentSize.X, minSize.X), Math.Max(e.CurrentSize.Y, minSize.Y));
            }
        }

        public void Show(TemplateTag e)
        {
            Show();
        }

        protected override void OnTabChanged(ValueChangedEventArgs<Blish_HUD.Controls.Tab> e)
        {
            base.OnTabChanged(e);

            Subtitle = e.NewValue == TagEditViewTab ? strings.Tags : strings.Group;
        }

        protected override void DisposeControl()
        {
            Hide();

            base.DisposeControl();
        }
    }
}
