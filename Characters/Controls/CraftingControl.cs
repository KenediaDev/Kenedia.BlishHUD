using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using static Kenedia.Modules.Characters.Services.Data;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class CraftingControl : Control, IFontControl
    {
        private readonly AsyncTexture2D _craftingIcon = AsyncTexture2D.FromAssetId(156711);
        private readonly Data _data;
        private readonly SettingsModel _settings;

        //private readonly AsyncTexture2D _craftingIcon = AsyncTexture2D.FromAssetId(866130);

        private BitmapFont _font = GameService.Content.DefaultFont14;

        public CraftingControl(Data data, SettingsModel settings)
        {
            _data = data;
            _settings = settings;
        }

        public Character_Model Character { get; set; }

        public BitmapFont Font
        {
            get => _font;
            set
            {
                _font = value;
                if (value != null)
                {
                    Width = Height + 4 + (int)value.MeasureString(strings.NoCraftingProfession).Width;
                }
            }
        }

        public string Text { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string toolTipText = null;
            spriteBatch.DrawOnCtrl(
                this,
                _craftingIcon,
                new Rectangle(4, 4, bounds.Height - 7, bounds.Height - 7),
                //new Rectangle(18, 18, 30, 30),
                new Rectangle(6, 6, 20, 20),
                Color.White,
                0f,
                default);

            bool craftingDisplayed = false;
            if (Character != null && Character.Crafting.Count > 0)
            {
                System.Collections.Generic.Dictionary<int, CraftingProfession> craftingDictionary = _data.CrafingProfessions;

                int i = 0;
                foreach (CharacterCrafting crafting in Character.Crafting)
                {
                    _ = craftingDictionary.TryGetValue(crafting.Id, out CraftingProfession craftingProfession);
                    if (craftingProfession != null)
                    {
                        Text = "NA";

                        bool onlyMax = _settings.DisplayToggles.Value["OnlyMaxCrafting"].Show;

                        if (craftingProfession.Icon != null && (!onlyMax || crafting.Rating == craftingProfession.MaxRating))
                        {
                            craftingDisplayed = true;
                            Rectangle craftBounds = new(bounds.Height + 6 + (i * bounds.Height), 2, bounds.Height - 4, bounds.Height - 4);

                            spriteBatch.DrawOnCtrl(
                                this,
                                craftingProfession.Icon,
                                craftBounds,
                                new Rectangle(8, 8, 16, 16),
                                Color.White,
                                0f,
                                default);

                            i++;

                            if (craftBounds.Contains(RelativeMousePosition))
                            {
                                toolTipText = craftingProfession.Name + " (" + crafting.Rating + "/" + craftingProfession.MaxRating + ")";
                            }
                        }
                    }
                }
            }

            if (!craftingDisplayed)
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    Text = strings.NoCraftingProfession,
                    Font,
                    new Rectangle(bounds.Height + 4, 0, bounds.Width - (bounds.Height + 4), bounds.Height),
                    Color.Gray,
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
            }

            BasicTooltipText = toolTipText;
        }
    }
}
