using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Core.Views
{
    public partial class SettingsWindow : BaseTabbedWindow
    {
        private static SettingsWindow s_instance;
        private readonly AsyncTexture2D _mainWindowEmblem = AsyncTexture2D.FromAssetId(156027);
        private readonly BitmapFont _titleFont = GameService.Content.DefaultFont32;

        private Rectangle _mainEmblemRectangle;
        private Rectangle _subEmblemRectangle;
        private Rectangle _titleRectangle;

        private string _title = "Settings";

        private SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion)
        {

        }

        public static SettingsWindow Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var settingsBg = AsyncTexture2D.FromAssetId(155997);
                    Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);
                    s_instance = new(
                        settingsBg,
                        new Rectangle(20, 30, cutSettingsBg.Width + 20, cutSettingsBg.Height),
                        new Rectangle(30, 35, settingsBg.Width - 482, settingsBg.Height - 390))
                    {
                        Parent = GameService.Graphics.SpriteScreen,
                        Title = "Settings",
                        Subtitle = "General",
                        SavesPosition = true,
                        Id = $"Kenedia.Modules.Core.SettingsWindow",
                    };

                    s_instance.AddTab(new SharedSettingsView());
                    s_instance.AddTab(new SharedSettingsView());
                    s_instance.AddTab(new SharedSettingsView());
                    s_instance.AddTab(new SharedSettingsView());
                }

                return s_instance;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _subEmblemRectangle = new(-43 + 64, -58 + 64, 64, 64);
            _mainEmblemRectangle = new(-43, -58, 128, 128);

            MonoGame.Extended.RectangleF titleBounds = _titleFont.GetStringRectangle(_title);
            _titleRectangle = new(80, 5, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(
                this,
                _mainWindowEmblem,
                _mainEmblemRectangle,
                _mainWindowEmblem.Bounds,
                Color.White,
                0f,
                default);
        }

        protected override void OnTabChanged(ValueChangedEventArgs<BaseTab> tab)
        {
            base.OnTabChanged(tab);

            if (tab.NewValue != null && !string.IsNullOrEmpty(tab.NewValue.Name))
            {
                Subtitle = tab.NewValue.Name;
            }
        }
    }
}
