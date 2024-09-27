using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class TagEditWindowFactory
    {
        public TagEditWindow? TagEditWindow { get; private set; } = null;

        public TagEditWindowFactory(TemplateTags templateTags)
        {
            TemplateTags = templateTags;
        }

        public TemplateTags TemplateTags { get; }

        public void DisposeEditWindow()
        {
            if (TagEditWindow != null)
            {
                TagEditWindow.Dispose();
                TagEditWindow = null;
            }
        }

        public void ShowEditWindow(TemplateTag tag)
        {
            int Height = 670;
            int Width = 915;
            bool isNew = TagEditWindow == null;

            TagEditWindow ??= new TagEditWindow(
                TexturesService.GetTextureFromRef(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(40, 40, Width - 3, Height),
                TemplateTags)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{BuildsManager.ModuleInstance.Name} TagWindow",
                MainWindowEmblem = AsyncTexture2D.FromAssetId(536043),
                SubWindowEmblem = AsyncTexture2D.FromAssetId(156031),
                Name = "Tag Editing",
                Width = 580,
                Height = 800,
                CanResize = true,
            };

            TagEditWindow.Show(tag);

            if (isNew)
                TagEditWindow.Hidden += Window_Hidden;
        }

        private void Window_Hidden(object sender, EventArgs e)
        {
            if (sender is TagEditWindow window)
            {
                window.Dispose();
                TagEditWindow = null;
            }
        }
    }
}
