using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterTooltip : Container
    {
        private readonly AsyncTexture2D _iconFrame = AsyncTexture2D.FromAssetId(1414041);
        private readonly FlowPanel _contentPanel;
        private readonly Dummy _iconDummy;

        private readonly IconLabel _nameLabel;
        private readonly IconLabel _levelLabel;
        private readonly IconLabel _professionLabel;
        private readonly IconLabel _genderLabel;
        private readonly IconLabel _raceLabel;
        private readonly IconLabel _mapLabel;
        private readonly IconLabel _lastLoginLabel;
        private readonly FlowPanel _tagPanel;

        private readonly CraftingControl _craftingControl;
        private readonly List<Control> _dataControls;

        private Rectangle _iconRectangle;
        private Rectangle _contentRectangle;

        private Point _textureOffset = new(25, 25);
        private Character_Model _character;

        public CharacterTooltip()
        {
            HeightSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(5, 5);

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(5, 5),
            };
            _iconDummy = new Dummy()
            {
                Parent = this,
            };

            _nameLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _levelLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _genderLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _raceLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _professionLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _mapLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _craftingControl = new CraftingControl()
            {
                Parent = _contentPanel,
                Width = _contentPanel.Width,
                Height = 20,
                Character = Character,
            };

            _lastLoginLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _tagPanel = new FlowPanel()
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
                Height = Font.LineHeight + 5,
                Visible = false,
            };
            _tagPanel.Resized += Tag_Panel_Resized;

            _dataControls = new List<Control>()
            {
                _nameLabel,
                _levelLabel,
                _genderLabel,
                _raceLabel,
                _professionLabel,
                _mapLabel,
                _lastLoginLabel,
                _craftingControl,
                _tagPanel,
            };
        }

        public Rectangle TextureRectangle { get; set; } = new Rectangle(40, 25, 250, 250);

        public AsyncTexture2D Background { get; set; } = AsyncTexture2D.FromAssetId(156003);

        public Color BackgroundTint { get; set; } = Color.Honeydew * 0.95f;

        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont14;

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont18;

        public Character_Model Character
        {
            get => _character; set
            {
                _character = value;
                ApplyCharacter(null, null);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);

            if (Character != null && _lastLoginLabel.Visible && Characters.ModuleInstance.CurrentCharacterModel != Character)
            {
                TimeSpan ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);
            }
        }

        public void UpdateLayout()
        {
            if (_iconRectangle.IsEmpty)
            {
                _iconRectangle = new Rectangle(Point.Zero, new Point(Math.Min(Width, Height), Math.Min(Width, Height)));
            }

            UpdateLabelLayout();
            UpdateSize();

            _contentRectangle = new Rectangle(new Point(_iconRectangle.Right, 0), _contentPanel.Size);
            _contentPanel.Location = _contentRectangle.Location;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Background != null)
            {
                Rectangle rect = new(_textureOffset.X, _textureOffset.Y, bounds.Width, bounds.Height);

                spriteBatch.DrawOnCtrl(
                    this,
                    Background,
                    bounds,
                    rect,
                    BackgroundTint,
                    0f,
                    default);
            }

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            if (!Character.HasDefaultIcon && Character.Icon != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    Character.Icon,
                    _iconRectangle,
                    Character.Icon.Bounds,
                    Color.White,
                    0f,
                    default);
            }
            else
            {
                AsyncTexture2D texture = Character.SpecializationIcon;

                if (texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        _iconFrame,
                        new Rectangle(_iconRectangle.X, _iconRectangle.Y, _iconRectangle.Width, _iconRectangle.Height),
                        _iconFrame.Bounds,
                        Color.White,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        _iconFrame,
                        new Rectangle(_iconRectangle.Width, _iconRectangle.Height, _iconRectangle.Width, _iconRectangle.Height),
                        _iconFrame.Bounds,
                        Color.White,
                        6.28f / 2,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        texture,
                        new Rectangle(8, 8, _iconRectangle.Width - 16, _iconRectangle.Height - 16),
                        texture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }
        }

        public void UpdateLabelLayout()
        {
            _iconDummy.Visible = _iconRectangle != Rectangle.Empty;
            _iconDummy.Size = _iconRectangle.Size;
            _iconDummy.Location = _iconRectangle.Location;

            _nameLabel.Visible = true;
            _nameLabel.Font = NameFont;

            _levelLabel.Visible = true;
            _levelLabel.Font = Font;

            _professionLabel.Visible = true;
            _professionLabel.Font = Font;

            _genderLabel.Visible = true;
            _genderLabel.Font = Font;

            _raceLabel.Visible = true;
            _raceLabel.Font = Font;

            _mapLabel.Visible = true;
            _mapLabel.Font = Font;

            _lastLoginLabel.Visible = true;
            _lastLoginLabel.Font = Font;

            _craftingControl.Visible = true;
            _craftingControl.Font = Font;

            _tagPanel.Visible = Character.Tags.Count > 0;
            foreach (var tag in _tagPanel.Children.Cast<Tag>())
            {
                tag.Font = Font;
            }

            _craftingControl.Height = Font.LineHeight + 2;
        }

        public void UpdateSize()
        {
            IEnumerable<Control> visibleControls = _dataControls.Where(e => e.Visible);
            int amount = visibleControls.Count();

            int height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)_contentPanel.ControlPadding.Y) : 0;
            int width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0;

            _contentPanel.Height = height;
            _contentPanel.Width = width + (int)_contentPanel.ControlPadding.X;
            _tagPanel.Width = width;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            _nameLabel.Text = Character.Name;
            _nameLabel.TextColor = new Microsoft.Xna.Framework.Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

            _levelLabel.Text = string.Format(strings.LevelAmount, Character.Level);
            _levelLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            _levelLabel.Icon = AsyncTexture2D.FromAssetId(157085);

            _professionLabel.Icon = Character.SpecializationIcon;
            _professionLabel.Text = Character.SpecializationName;

            if (_professionLabel.Icon != null)
            {
                _professionLabel.TextureRectangle = _professionLabel.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);
            }

            _genderLabel.Text = Character.Gender.ToString();
            _genderLabel.Icon = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Gender);

            _raceLabel.Text = Characters.ModuleInstance.Data.Races[Character.Race].Name;
            _raceLabel.Icon = Characters.ModuleInstance.Data.Races[Character.Race].Icon;

            _mapLabel.Text = Characters.ModuleInstance.Data.GetMapById(Character.Map).Name;
            _mapLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            _mapLabel.Icon = AsyncTexture2D.FromAssetId(358406); // 358406 //517180 //157122;

            _lastLoginLabel.Icon = AsyncTexture2D.FromAssetId(841721);
            _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, 0, 0, 0, 0);
            _lastLoginLabel.TextureRectangle = Rectangle.Empty;

            _tagPanel.ClearChildren();
            foreach (string tagText in Character.Tags)
            {
                _ = new Tag()
                {
                    Parent = _tagPanel,
                    Text = tagText,
                    Active = true,
                    ShowDelete = false,
                };
            }

            _craftingControl.Character = Character;
            UpdateLabelLayout();
            UpdateSize();

            // UpdateLayout();
        }
    }
}
