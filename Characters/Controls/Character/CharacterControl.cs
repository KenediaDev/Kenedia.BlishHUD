using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class Test : Control
    {
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }
    }

    public class CharacterControl : Panel
    {
        private readonly List<Control> _dataControls = new();

        private readonly AsyncTexture2D _iconFrame = AsyncTexture2D.FromAssetId(1414041);
        private readonly AsyncTexture2D _loginTexture = AsyncTexture2D.FromAssetId(157092);
        private readonly AsyncTexture2D _loginTextureHovered = AsyncTexture2D.FromAssetId(157094);
        private readonly AsyncTexture2D _cogTexture = AsyncTexture2D.FromAssetId(157109);
        private readonly AsyncTexture2D _cogTextureHovered = AsyncTexture2D.FromAssetId(157111);
        private readonly AsyncTexture2D _presentTexture = AsyncTexture2D.FromAssetId(593864);
        private readonly AsyncTexture2D _presentTextureOpen = AsyncTexture2D.FromAssetId(593865);

        private readonly IconLabel _nameLabel;
        private readonly IconLabel _levelLabel;
        private readonly IconLabel _professionLabel;
        private readonly IconLabel _raceLabel;
        private readonly IconLabel _genderLabel;
        private readonly IconLabel _mapLabel;
        private readonly IconLabel _lastLoginLabel;
        private readonly TagFlowPanel _tagPanel;

        private readonly CraftingControl _craftingControl;
        private readonly BasicTooltip _textTooltip;
        private readonly CharacterTooltip _characterTooltip;
        private readonly FlowPanel _contentPanel;
        private readonly Dummy _iconDummy;

        private Rectangle _loginRect;
        private Rectangle _cogRect;
        private Rectangle _controlBounds = Rectangle.Empty;
        private Rectangle _textBounds;
        private Rectangle _iconRectangle;

        private readonly bool _created;
        private bool _dragging;

        private int _cogSize;
        private int _iconSize;

        private Character_Model _character;
        private readonly List<Tag> _tags = new();

        public CharacterControl()
        {
            HeightSizingMode = SizingMode.AutoSize;
            //WidthSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(0, 2);

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                OuterControlPadding = new Vector2(5, 5),
            };

            _iconDummy = new()
            {
                Parent = this,
                Size = Point.Zero,
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

            _tagPanel = new()
            {
                Parent = _contentPanel,
                Font = _lastLoginLabel.Font,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(3, 2),
                Visible = false,
            };

            _dataControls = new()
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

            _textTooltip = new BasicTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1000,

                Size = new Point(300, 50),
                Visible = false,
            };
            _textTooltip.Shown += TextTooltip_Shown;

            _characterTooltip = new CharacterTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1001,

                Size = new Point(300, 50),
                Visible = false,
            };

            Characters.ModuleInstance.LanguageChanged += ApplyCharacter;

            _created = true;
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont14;

        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont14;

        public double Index
        {
            get => Character != null ? Character.Index : 0;
            set
            {
                if (Character != null)
                {
                    Character.Index = (int)value;
                }
            }
        }

        public Character_Model Character
        {
            get => _character;
            set
            {
                if (_character != null)
                {
                    _character.Updated -= ApplyCharacter;
                    _character.Deleted -= CharacterDeleted;
                }

                _character = value;
                _characterTooltip.Character = value;

                if (value != null)
                {
                    _character.Updated += ApplyCharacter;
                    _character.Deleted += CharacterDeleted;
                    ApplyCharacter(null, null);
                }
            }
        }

        public Rectangle ControlContentBounds
        {
            get => _controlBounds;
            set
            {
                _controlBounds = value;

                if (_controlBounds != null)
                {
                    AdaptNewBounds();
                }
            }
        }

        public Rectangle CalculateLayout()
        {
            if (_created && Visible)
            {
                UpdateDataControlsVisibility();
                _contentPanel.Visible = Settings.PanelLayout.Value != CharacterPanelLayout.OnlyIcons;
                _tagPanel.FitWidestTag(_dataControls.Max(e => e.Visible && e != _tagPanel ? e.Width : 0));

                IEnumerable<Control> controls = _dataControls.Where(e => e.Visible);
                Control firstControl = controls.Count() > 0 ? _dataControls.Where(e => e.Visible && e is IFontControl)?.FirstOrDefault() : null;
                bool anyVisible = _contentPanel.Visible && controls.Count() > 0;
                int width = anyVisible ? controls.Max(e => e.Width) + (int)(_contentPanel.OuterControlPadding.X * 2) : 0;
                int height = anyVisible ? controls.Aggregate((int)(_contentPanel.OuterControlPadding.Y * 2), (result, e) => result + e.Height + (int)_contentPanel.ControlPadding.Y) : 0;

                PanelSizes pSize = Settings.PanelSize.Value;
                _iconSize = (Settings.PanelLayout.Value == CharacterPanelLayout.OnlyText) ? 0 : pSize == PanelSizes.Small ? 64 : pSize == PanelSizes.Normal ? 80 : pSize == PanelSizes.Large ? 112 : Settings.CustomCharacterIconSize.Value;

                if (Settings.CharacterPanelFixedWidth.Value)
                {
                    width = Settings.CharacterPanelWidth.Value - _iconSize;
                }

                _iconRectangle = new Rectangle(0, 0, _iconSize, _iconSize);

                _cogSize = Math.Max(20, (firstControl != null ? ((IFontControl)firstControl).Font.LineHeight : Font.LineHeight) - 4);
                _cogSize = !anyVisible ? _iconSize / 5 : _cogSize;

                if (firstControl != null && width < firstControl.Width + 5 + _cogSize)
                {
                    width += anyVisible ? 5 + _cogSize : 0;
                }

                _textBounds = new Rectangle(_iconRectangle.Right + (anyVisible && _iconSize > 0 ? 5 : 0), 0, width, height);

                _contentPanel.Location = _textBounds.Location;
                _contentPanel.Size = _textBounds.Size;

                _controlBounds = new(
                    _iconRectangle.Left,
                    _iconRectangle.Top,
                    _textBounds.Right - _iconRectangle.Left,
                    Math.Max(_textBounds.Height, _iconRectangle.Height)
                    );

                _cogRect = new Rectangle(_controlBounds.Width - _cogSize - 4, 4, _cogSize, _cogSize);
                int size = _iconSize > 0 ? Math.Min(56, _iconRectangle.Width - 8) : Math.Min(56, Math.Min(_textBounds.Width, _textBounds.Height) - 8);
                int pad = (_iconRectangle.Width - size) / 2;

                _loginRect = anyVisible ?
                    _iconSize > 0 ? new Rectangle((_iconRectangle.Width - size) / 2, (_iconRectangle.Height - size) / 2, size, size) :
                    new Rectangle((_textBounds.Width - size) / 2, (_textBounds.Height - size) / 2, size, size) :
                    new Rectangle(pad, pad, size, size);
            }

            return _controlBounds;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(
                this,
                Textures.Pixel,
                _iconRectangle,
                Rectangle.Empty,
                Color.Transparent,
                0f,
                default);

            if (Character != null)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
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
                else if (MouseOver)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        Textures.Pixel,
                        _iconRectangle,
                        Rectangle.Empty,
                        Color.Transparent,
                        0f,
                        default);
                }
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (MouseOver)
            {
                _textTooltip.Visible = false;

                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        Textures.Pixel,
                        _iconRectangle,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    _textTooltip.Text = Character.HasBirthdayPresent ? string.Format(strings.Birthday_Text, Character.Name, Character.Age) : string.Format(strings.LoginWith, Character.Name);
                    _textTooltip.Visible = _loginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Character.HasBirthdayPresent ? _loginRect.Contains(RelativeMousePosition) ? _presentTextureOpen : _presentTexture : _loginRect.Contains(RelativeMousePosition) ? _loginTextureHovered : _loginTexture,
                        _loginRect,
                        _loginTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        Textures.Pixel,
                        bounds,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    _textTooltip.Text = Character.HasBirthdayPresent ? string.Format(strings.Birthday_Text, Character.Name, Character.Age) : string.Format(strings.LoginWith, Character.Name);
                    _textTooltip.Visible = _loginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Character.HasBirthdayPresent ? _loginRect.Contains(RelativeMousePosition) ? _presentTextureOpen : _presentTexture : _loginRect.Contains(RelativeMousePosition) ? _loginTextureHovered : _loginTexture,
                        _loginRect,
                        _loginTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                spriteBatch.DrawOnCtrl(
                    this,
                    _cogRect.Contains(RelativeMousePosition) ? _cogTextureHovered : _cogTexture,
                    _cogRect,
                    new Rectangle(5, 5, 22, 22),
                    Color.White,
                    0f,
                    default);
                if (_cogRect.Contains(RelativeMousePosition))
                {
                    _textTooltip.Text = string.Format(strings.AdjustSettings, Character.Name);
                    _textTooltip.Visible = true;
                }

                Color color = Colors.ColonialWhite;

                // Top
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
            }

            if (!MouseOver && Character != null && Character.HasBirthdayPresent)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        Textures.Pixel,
                        _iconRectangle,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        _presentTexture,
                        _loginRect,
                        _presentTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        Textures.Pixel,
                        bounds,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        _presentTexture,
                        _loginRect,
                        _presentTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (Character != null && _lastLoginLabel.Visible)
            {
                if (Characters.ModuleInstance.CurrentCharacterModel != Character)
                {
                    TimeSpan ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                    _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);

                    if (Character.HasBirthdayPresent)
                    {
                        // ScreenNotification.ShowNotification(String.Format("It is {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age));
                    }
                }
                else
                {
                    _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, 0, 0, 0, 0);
                }
            }

            if (!MouseOver && _textTooltip.Visible)
            {
                _textTooltip.Visible = MouseOver;
            }

            if (!MouseOver && _characterTooltip.Visible)
            {
                _characterTooltip.Visible = MouseOver;
            }
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            Views.MainWindow mainWindow = Characters.ModuleInstance.MainWindow;
            mainWindow.ShowAttachedWindow(mainWindow.CharacterEdit.Character != Character || !mainWindow.CharacterEdit.Visible ? mainWindow.CharacterEdit : null);
            Characters.ModuleInstance.MainWindow.CharacterEdit.Character = Character;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (e.IsDoubleClick && Characters.ModuleInstance.Settings.DoubleClickToEnter.Value)
            {
                Characters.ModuleInstance.SwapTo(Character);
                return;
            }

            // Logout Icon Clicked!
            if (_loginRect.Contains(RelativeMousePosition))
            {
                Characters.ModuleInstance.SwapTo(Character);
            }

            // Cog Icon Clicked!
            if (_cogRect.Contains(RelativeMousePosition))
            {
                Characters.ModuleInstance.MainWindow.CharacterEdit.Visible = !Characters.ModuleInstance.MainWindow.CharacterEdit.Visible || Characters.ModuleInstance.MainWindow.CharacterEdit.Character != Character;
                Characters.ModuleInstance.MainWindow.CharacterEdit.Character = Character;
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = this;
                _dragging = true;
            }
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            if (_dragging)
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = null;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            if (_textTooltip == null || (!_textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value))
            {
                _characterTooltip.Show();
            }
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            if (_textTooltip == null || (!_textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value))
            {
                _characterTooltip.Show();
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Characters.ModuleInstance.LanguageChanged -= ApplyCharacter;

            _textTooltip.Shown -= TextTooltip_Shown;
            if (_character != null)
            {
                _character.Updated -= ApplyCharacter;
                _character.Deleted -= CharacterDeleted;
            }

            _dataControls?.DisposeAll();
            _contentPanel?.Dispose();
            _textTooltip?.Dispose();
            _characterTooltip?.Dispose();

            _iconFrame.Dispose();
            _loginTexture.Dispose();
            _loginTextureHovered.Dispose();
            _cogTexture.Dispose();
            _cogTextureHovered.Dispose();
            _presentTexture.Dispose();
            _presentTextureOpen.Dispose();
            _tagPanel.Children.DisposeAll();
            _tagPanel.DisposeAll();

            Children.DisposeAll();
            _ = Characters.ModuleInstance.MainWindow.CharacterControls.Remove(this);
        }

        private BitmapFont GetFont(bool nameFont = false)
        {
            FontSize fontSize = FontSize.Size8;

            switch (Settings.PanelSize.Value)
            {
                case PanelSizes.Small:
                    fontSize = nameFont ? FontSize.Size16 : FontSize.Size12;
                    break;

                case PanelSizes.Normal:
                    fontSize = nameFont ? FontSize.Size18 : FontSize.Size14;
                    break;

                case PanelSizes.Large:
                    fontSize = nameFont ? FontSize.Size22 : FontSize.Size18;
                    break;

                case PanelSizes.Custom:
                    fontSize = nameFont ? (FontSize)Settings.CustomCharacterNameFontSize.Value : (FontSize)Settings.CustomCharacterFontSize.Value;
                    break;
            }

            return GameService.Content.GetFont(FontFace.Menomonia, fontSize, FontStyle.Regular);
        }

        private void TextTooltip_Shown(object sender, EventArgs e)
        {
            _characterTooltip?.Hide();
        }

        private void CharacterDeleted(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            _nameLabel.Text = Character.Name;
            _nameLabel.TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

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

            //_lastLoginLabel.Icon = AsyncTexture2D.FromAssetId(841721);
            _lastLoginLabel.Icon = AsyncTexture2D.FromAssetId(155035);
            _lastLoginLabel.TextureRectangle = new Rectangle(10, 10, 44, 44);
            _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, 0, 0, 0, 0);

            foreach (string tagText in Character.Tags)
            {
                if (_tags.Find(e => e.Text == tagText) == null)
                {
                    _tags.Add(new Tag()
                    {
                        Parent = _tagPanel,
                        Text = tagText,
                        Active = true,
                        ShowDelete = false,
                        CanInteract = false,
                    });
                }
            }

            for (int i = _tags.Count - 1; i >= 0; i--)
            {
                Tag tag = _tags[i];
                if (!Character.Tags.Contains(tag.Text))
                {
                    tag.Dispose();
                    _tags.RemoveAt(i);
                }
            }

            _craftingControl.Character = Character;

            _controlBounds = CalculateLayout();
            AdaptNewBounds();
        }

        public void HideTooltips()
        {
            _textTooltip.Hide();
            _characterTooltip.Hide();
        }

        private void UpdateDataControlsVisibility()
        {
            NameFont = GetFont(true);
            Font = GetFont();

            _contentPanel.ControlPadding = new(Font.LineHeight / 10, Font.LineHeight / 10);

            _nameLabel.Visible = Settings.DisplayToggles.Value["Name"].Show;
            _nameLabel.Font = NameFont;

            _levelLabel.Visible = Settings.DisplayToggles.Value["Level"].Show;
            _levelLabel.Font = Font;

            _genderLabel.Visible = Settings.DisplayToggles.Value["Gender"].Show;
            _genderLabel.Font = Font;

            _raceLabel.Visible = Settings.DisplayToggles.Value["Race"].Show;
            _raceLabel.Font = Font;

            _professionLabel.Visible = Settings.DisplayToggles.Value["Profession"].Show;
            _professionLabel.Font = Font;

            _lastLoginLabel.Visible = Settings.DisplayToggles.Value["LastLogin"].Show;
            _lastLoginLabel.Font = Font;

            _mapLabel.Visible = Settings.DisplayToggles.Value["Map"].Show;
            _mapLabel.Font = Font;

            _craftingControl.Visible = Settings.DisplayToggles.Value["CraftingProfession"].Show;
            _craftingControl.Font = Font;

            _tagPanel.Visible = Settings.DisplayToggles.Value["Tags"].Show && Character.Tags.Count > 0;
            _tagPanel.Font = Font;

            _craftingControl.Height = Font.LineHeight + 2;
        }

        private void AdaptNewBounds()
        {
            if (Width != _controlBounds.Width + AutoSizePadding.X)
            {
                Width = _controlBounds.Width + AutoSizePadding.X;
            }

            if (Height != _controlBounds.Height + AutoSizePadding.Y)
            {
                _iconDummy.Height = _controlBounds.Height;
                //Height = _controlBounds.Height + AutoSizePadding.Y;
            }

            bool anyVisible = _contentPanel.Visible && _dataControls.Where(e => e.Visible)?.Count() > 0;

            _cogRect = new Rectangle(_controlBounds.Width - _cogSize - 4, 4, _cogSize, _cogSize);
            int size = _iconSize > 0 ? Math.Min(56, _iconRectangle.Width - 8) : Math.Min(56, Math.Min(_textBounds.Width, _textBounds.Height) - 8);
            int pad = (_iconRectangle.Width - size) / 2;

            _loginRect = anyVisible ?
                _iconSize > 0 ? new Rectangle((_iconRectangle.Width - size) / 2, (_iconRectangle.Height - size) / 2, size, size) :
                new Rectangle((_textBounds.Width - size) / 2, (_textBounds.Height - size) / 2, size, size) :
                new Rectangle(pad, pad, size, size);
        }
    }
}
