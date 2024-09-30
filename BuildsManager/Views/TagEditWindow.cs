using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Views;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using System.Diagnostics;

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

            MainWindowEmblem = AsyncTexture2D.FromAssetId(156020);
            SubWindowEmblem = AsyncTexture2D.FromAssetId(156027);

            Parent = GameService.Graphics.SpriteScreen;

            Tabs.Add(new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(156674), () => new TagEditView(TemplateTags, TagGroups), strings.Tags));
            Tabs.Add(new Blish_HUD.Controls.Tab(AsyncTexture2D.FromAssetId(1342334), () => new TagGroupView(TagGroups), strings.Group));
        }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        public void Show(TemplateTag e)
        {
            Show();
        }

    }
}
