using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Gw2Mumble;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using static Kenedia.Modules.Characters.Services.Settings;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using MonoGame.Extended;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterCard : Panel
    {
        private readonly AsyncTexture2D _iconFrame = AsyncTexture2D.FromAssetId(1414041);
        private readonly AsyncTexture2D _loginTexture = AsyncTexture2D.FromAssetId(60968); //157092
        private readonly AsyncTexture2D _loginTextureHovered = AsyncTexture2D.FromAssetId(60968); //157094
        private readonly AsyncTexture2D _cogTexture = AsyncTexture2D.FromAssetId(157109);
        private readonly AsyncTexture2D _cogTextureHovered = AsyncTexture2D.FromAssetId(157111);
        private readonly AsyncTexture2D _presentTexture = AsyncTexture2D.FromAssetId(593864);
        private readonly AsyncTexture2D _presentTextureOpen = AsyncTexture2D.FromAssetId(593865);

        private readonly BasicTooltip _textTooltip;
        private readonly CharacterTooltip _characterTooltip;
        private readonly FlowPanel _contentPanel;
        private readonly CharacterLabels _infoLabels;
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
        private readonly TextureManager _textureManager;
        private readonly Data _data;
        private readonly MainWindow _mainWindow;
        private readonly Settings _settings;
        private double _lastUniform;
        private bool _updateCharacter;

        public CharacterCard()
        {
            HeightSizingMode = SizingMode.AutoSize;
            BackgroundColor = Color.Black * 0.5f;
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

            _infoLabels = new CharacterLabels(_contentPanel);

            _textTooltip = new BasicTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1000,

                Size = new Point(300, 50),
                Visible = false,
            };

            _textTooltip.Shown += TextTooltip_Shown;

            _created = true;
            _updateCharacter = true;
        }

        public CharacterCard(CharacterCard card) : this()
        {
            _textureManager = card._textureManager;
            _data = card._data;
            _mainWindow = card._mainWindow;
            _settings = card._settings;

            Size = card.Size;

            Character = card._character;
            _infoLabels.TextureManager = _textureManager;
            _infoLabels.Data = _data;
            _infoLabels.Settings = _settings;

            _settings.AppearanceSettingChanged += Settings_AppearanceSettingChanged;

            Settings_AppearanceSettingChanged(this, null);
        }

        private void Settings_AppearanceSettingChanged(object sender, EventArgs e)
        {
            Update();
            UniformWithAttached();
        }

        private void Update()
        {
            _infoLabels.UpdateDataControlsVisibility();
            _infoLabels.UpdateCharacterInfo();

            _ = CalculateLayout();
            AdaptNewBounds();
        }

        public CharacterCard(Func<Character_Model> currentCharacter, TextureManager textureManager, Data data, MainWindow mainWindow, Settings settings) : this()
        {
            _textureManager = textureManager;
            _data = data;
            _mainWindow = mainWindow;
            _settings = settings;

            HeightSizingMode = SizingMode.AutoSize;
            //WidthSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(0, 2);

            _infoLabels.TextureManager = _textureManager;
            _infoLabels.Data = _data;
            _infoLabels.Settings = _settings;
            _infoLabels.CurrentCharacter = currentCharacter;
            _settings.AppearanceSettingChanged += Settings_AppearanceSettingChanged;

            _characterTooltip = new CharacterTooltip(currentCharacter, textureManager, data, _settings)
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1001,

                Size = new Point(300, 50),
                Visible = false,
            };
        }

        public bool IsDraggingTarget { get; set; } = false;

        public List<CharacterCard> AttachedCards { get; set; } = [];

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont14;

        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont14;

        public int Index
        {
            get => Character is not null ? Character.Index : 0;
            set
            {
                if (Character is not null)
                {
                    _infoLabels.UpdateCharacterInfo();
                }
            }
        }

        public Character_Model Character
        {
            get => _character;
            set
            {
                var temp = _character;
                if (Common.SetProperty(ref _character, value))
                {
                    if (temp is not null)
                    {
                        temp.Deleted -= CharacterDeleted;
                        temp.Updated -= ApplyCharacter;
                    }

                    if (_character is not null)
                    {
                        _character.Deleted += CharacterDeleted;
                        _character.Updated += ApplyCharacter;
                    }
                }

                if (_characterTooltip is not null) _characterTooltip.Character = value;
                if (_infoLabels is not null) _infoLabels.Character = value;
            }
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            _updateCharacter = true;
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

        public void UniformWithAttached(bool force = false)
        {
            double now = Common.Now;
            Update();

            if (_lastUniform != now || force)
            {
                if (AttachedCards?.Count() > 0)
                {
                    int maxWidth = AttachedCards.Max(e => e.CalculateLayout().Width);
                    AttachedCards.ForEach(e => e.ControlContentBounds = new(e.ControlContentBounds.Location, new(maxWidth, e.ControlContentBounds.Height)));
                    AttachedCards.ForEach(e => e._lastUniform = now);
                    ControlContentBounds = new(ControlContentBounds.Location, new(maxWidth, ControlContentBounds.Height));
                }
                else
                {
                    _lastUniform = now;
                    ControlContentBounds = CalculateLayout();
                    AdaptNewBounds();
                }
            }
        }

        public Rectangle CalculateLayout()
        {
            if (_created && Visible)
            {
                _infoLabels.RecalculateBounds();
                _contentPanel.Visible = _settings.PanelLayout.Value != CharacterPanelLayout.OnlyIcons;

                IEnumerable<Control> controls = _infoLabels.DataControls.Where(e => e.Visible);
                Control firstControl = controls.Count() > 0 ? _infoLabels.DataControls.Where(e => e.Visible && e is IFontControl)?.FirstOrDefault() : null;
                bool anyVisible = _contentPanel.Visible && controls.Count() > 0;
                int width = anyVisible ? controls.Max(e => e.Width) + (int)(_contentPanel.OuterControlPadding.X * 2) : 0;
                int height = anyVisible ? controls.Aggregate((int)(_contentPanel.OuterControlPadding.Y * 2), (result, e) => result + e.Height + (int)_contentPanel.ControlPadding.Y) : 0;

                PanelSizes pSize = _settings.PanelSize.Value;
                _iconSize = (_settings.PanelLayout.Value == CharacterPanelLayout.OnlyText) ? 0 : pSize == PanelSizes.Small ? 64 : pSize == PanelSizes.Normal ? 80 : pSize == PanelSizes.Large ? 112 : _settings.CustomCharacterIconSize.Value;

                if (_settings.CharacterPanelFixedWidth.Value)
                {
                    width = _settings.CharacterPanelWidth.Value - _iconSize;
                }

                _iconRectangle = new Rectangle(0, 0, _iconSize, _iconSize);

                _cogSize = Math.Max(20, (firstControl is not null ? ((IFontControl)firstControl).Font.LineHeight : Font.LineHeight) - 4);
                _cogSize = !anyVisible ? _iconSize / 5 : _cogSize;

                if (firstControl is not null && width < firstControl.Width + 5 + _cogSize)
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

            if (Character is not null)
            {
                if (_settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    if (!Character.HasDefaultIcon && Character.Icon is not null)
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

                        if (texture is not null)
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
                bool loginHovered = !IsDraggingTarget && _loginRect.Contains(RelativeMousePosition);

                if (_settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        Textures.Pixel,
                        _iconRectangle,
                        Rectangle.Empty,
                        IsDraggingTarget ? Color.Transparent : Color.Black * 0.5f,
                        0f,
                        default);

                    if (!IsDraggingTarget)
                    {
                        bool anyVisible = _contentPanel.Visible && _infoLabels.DataControls.Where(e => e.Visible)?.Count() > 0;

                        _textTooltip.Text = Character.HasBirthdayPresent ? string.Format(strings.Birthday_Text, Character.Name, Character.Age) : string.Format(strings.LoginWith, Character.Name);
                        _textTooltip.Visible = loginHovered && anyVisible;

                        spriteBatch.DrawOnCtrl(
                            this,
                            Character.HasBirthdayPresent ? loginHovered ? _presentTextureOpen : _presentTexture : loginHovered ? _loginTextureHovered : _loginTexture,
                            Character.HasBirthdayPresent ? _loginRect.Add(8, 8, -16, -16) : _loginRect,
                            Character.HasBirthdayPresent ? loginHovered ? _presentTextureOpen.Bounds : _presentTexture.Bounds : loginHovered ? _loginTextureHovered.Bounds : _loginTexture.Bounds,
                            loginHovered ? Color.White : new Color(215, 215, 215),
                            0f,
                            default);
                    }
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

                    _textTooltip.Text = Character.HasBirthdayPresent ? string.Format(strings.Birthday_Text, Character.Name, Character.Age) : string.Empty;
                    _textTooltip.Visible = !string.IsNullOrEmpty(_textTooltip.Text);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Character.HasBirthdayPresent ? loginHovered ? _presentTextureOpen : _presentTexture : loginHovered ? _loginTextureHovered : _loginTexture,
                        Character.HasBirthdayPresent ? _loginRect.Add(8, 8, -16, -16) : _loginRect,
                        Character.HasBirthdayPresent ? loginHovered ? _presentTextureOpen.Bounds : _presentTexture.Bounds : loginHovered ? _loginTextureHovered.Bounds : _loginTexture.Bounds,
                        loginHovered ? Color.White : new Color(200, 200, 200),
                        0f,
                        default);
                }

                if (!IsDraggingTarget)
                {
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
                }
            }

            if (!MouseOver && Character is not null && Character.HasBirthdayPresent)
            {
                if (_settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
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
                        Character.HasBirthdayPresent ? _loginRect.Add(8, 8, -16, -16) : _loginRect,
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
                        Character.HasBirthdayPresent ? _loginRect.Add(8, 8, -16, -16) : _loginRect,
                        _presentTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }

            if (IsDraggingTarget || (_mainWindow is not null && bounds.Contains(RelativeMousePosition) && _mainWindow.IsActive) || MouseOver)
            {
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
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (!IsDraggingTarget)
            {
                if (!MouseOver && _textTooltip.Visible)
                {
                    _textTooltip.Visible = MouseOver;
                }

                if (!MouseOver && _characterTooltip.Visible)
                {
                    _characterTooltip.Visible = MouseOver;
                }
            }

            _infoLabels.Update();

            if (_updateCharacter && _created && Visible)
            {
                Settings_AppearanceSettingChanged(this, null);
                _updateCharacter = false;
            }
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            if (!IsDraggingTarget)
            {
                _mainWindow.ShowAttached(_mainWindow.CharacterEdit.Character != Character || !_mainWindow.CharacterEdit.Visible ? _mainWindow.CharacterEdit : null);
                _mainWindow.CharacterEdit.Character = Character;
            }
        }

        protected override async void OnClick(MouseEventArgs e)
        {
            if (!IsDraggingTarget)
            {
                base.OnClick(e);

                if (e.IsDoubleClick && _settings.DoubleClickToEnter.Value)
                {
                    Character.Swap();
                    return;
                }

                // Logout Icon Clicked!
                if (_loginRect.Contains(RelativeMousePosition))
                {
                    PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

                    if (player is not null && player.Name == Character.Name && Character.HasBirthdayPresent)
                    {
                        _ = await _settings.MailKey.Value.PerformPress(50, false);
                        _mainWindow.CharacterEdit.Character = Character;
                        _mainWindow.ShowAttached(_mainWindow.CharacterEdit);
                    }
                    else
                    {
                        Character.Swap();
                        _mainWindow.ShowAttached();
                    }
                }

                // Cog Icon Clicked!
                if (_cogRect.Contains(RelativeMousePosition))
                {
                    _mainWindow.CharacterEdit.Visible = !_mainWindow.CharacterEdit.Visible || _mainWindow.CharacterEdit.Character != Character;
                    _mainWindow.CharacterEdit.Character = Character;
                }
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            if (!IsDraggingTarget && Keyboard.GetState().IsKeyDown(Keys.LeftControl) && _settings.SortType.Value == SortBy.Custom)
            {
                _mainWindow.DraggingControl.StartDragging(this);

                _dragging = true;
                _characterTooltip?.Hide();
                _textTooltip?.Hide();
            }
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            if (!IsDraggingTarget && _dragging)
            {
                _mainWindow.DraggingControl.EndDragging();
                _dragging = false;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            if (!IsDraggingTarget && !_mainWindow.DraggingControl.IsActive)
            {
                if (_textTooltip == null || (!_textTooltip.Visible && _settings.ShowDetailedTooltip.Value))
                {
                    _characterTooltip?.Show();
                }
            }
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            if (!IsDraggingTarget && !_mainWindow.DraggingControl.IsActive)
            {
                if (_textTooltip == null || (!_textTooltip.Visible && _settings.ShowDetailedTooltip.Value))
                {
                    _characterTooltip?.Show();
                }
            }
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            _textTooltip?.Hide();
            _characterTooltip?.Hide();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _textTooltip.Shown -= TextTooltip_Shown;
            if (_character is not null)
            {
                _character.Deleted -= CharacterDeleted;
            }

            _infoLabels?.Dispose();
            _contentPanel?.Dispose();
            _textTooltip?.Dispose();
            _characterTooltip?.Dispose();

            Children.DisposeAll();
            _ = _mainWindow.CharacterCards.Remove(this);
        }

        private void TextTooltip_Shown(object sender, EventArgs e)
        {
            _characterTooltip?.Hide();
        }

        private void CharacterDeleted(object sender, EventArgs e)
        {
            Dispose();
        }

        public void HideTooltips()
        {
            _textTooltip.Hide();
            _characterTooltip.Hide();
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

            bool anyVisible = _contentPanel.Visible && _infoLabels.DataControls.Where(e => e.Visible)?.Count() > 0;

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
