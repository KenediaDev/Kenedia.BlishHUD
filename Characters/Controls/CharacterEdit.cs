using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using static Kenedia.Modules.Characters.Services.TextureManager;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterEdit : AnchoredContainer
    {
        private readonly AsyncTexture2D _presentTexture = AsyncTexture2D.FromAssetId(593864);
        private readonly AsyncTexture2D _presentTextureOpen = AsyncTexture2D.FromAssetId(593865);

        private readonly List<Tag> _tags = new();

        private readonly ImageButton _closeButton;
        private readonly Panel _tagContainer;
        private readonly TextBox _tagBox;
        private readonly ImageButton _addTag;
        private readonly TagFlowPanel _tagPanel;
        private readonly ImageButton _image;
        private readonly Label _name;
        private readonly Checkbox _show;
        private readonly Checkbox _radial;
        private readonly ImageButton _birthdayButton;
        private readonly Panel _buttonContainer;
        private readonly Button _captureImages;
        private readonly Button _openFolder;
        private readonly Panel _imagePanelParent;
        private readonly FlowPanel _imagePanel;
        private readonly string _accountPath;
        private readonly TagList _allTags;
        private readonly SettingsModel _settings;
        private readonly Action _refreshCharacters;
        private Character_Model _character;

        public CharacterEdit(TextureManager tM, Action togglePotrait, string accountPath, TagList allTags, SettingsModel settings, Action refreshCharacters)
        {
            _accountPath = accountPath;
            _allTags = allTags;
            _settings = settings;
            _refreshCharacters = refreshCharacters;

            HeightSizingMode = SizingMode.AutoSize;
            WidthSizingMode = SizingMode.AutoSize;
            ContentPadding = new(5, 5, 5, 5);

            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            TextureRectangle = new Rectangle(26, 26, Math.Min(BackgroundImage.Width - 100, Width), Math.Min(BackgroundImage.Height - 100, Height));
            BorderColor = Color.Black;
            BorderWidth = new(2);

            _ = new Dummy()
            {
                Parent = this,
                Width = 355,
            };

            _closeButton = new()
            {
                Parent = this,
                Size = new(20, 20),
                Location = new(335, 5),
                Texture = AsyncTexture2D.FromAssetId(156012),
                HoveredTexture = AsyncTexture2D.FromAssetId(156011),
                TextureRectangle = new Rectangle(7, 7, 20, 20),
                SetLocalizedTooltip = () => strings.Close,
                ClickAction = (m) => Hide(),
                ZIndex = 11,
            };

            _image = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(358353),
                HoveredTexture = AsyncTexture2D.FromAssetId(358353),
                BackgroundColor = Color.Black * 0.4f,
                Size = new Point(70, 70),
                ClickAction = (m) => ShowImages(),
            };

            _name = new Label()
            {
                Text = strings.CharacterName,
                Parent = this,
                TextColor = ContentService.Colors.ColonialWhite,
                Font = GameService.Content.DefaultFont16,
                AutoSizeWidth = true,
                Location = new Point(_image.Right + 5 + 2, 0),
            };

            _show = new Checkbox()
            {
                Parent = this,
                Location = new Point(_image.Right + 5 + 2, _name.Bottom + 5 + 2),
                Size = new Point(100, 21),
                SetLocalizedText = () => strings.ShowInList,
                CheckedChangedAction = (b) =>
                {
                    if (Character != null) Character.Show = b;
                    _refreshCharacters?.Invoke();
                },
            };

            _radial = new Checkbox()
            {
                Parent = this,
                Location = new Point(_image.Right + 5 + 2, _show.Bottom),
                Size = new Point(100, 21),
                SetLocalizedText = () => strings.ShowOnRadial,
                SetLocalizedTooltip = () => strings.ShowOnRadial_Tooltip,
                CheckedChangedAction = (b) =>
                {
                    if (Character != null) Character.ShowOnRadial = b;
                    _refreshCharacters?.Invoke();
                },
            };

            int x = (355 - (_radial.Right + 5 + 2) - 48) / 2;

            _birthdayButton = new ImageButton()
            {
                Parent = this,
                Location = new Point(_radial.Right + 5 + 2 + x, _name.Bottom),
                Size = new Point(48, 48),
                Texture = _presentTexture,
                HoveredTexture = _presentTextureOpen,
                ClickAction = (m) =>
                {
                    Character.HadBirthday = false;
                    _refreshCharacters?.Invoke();
                    _birthdayButton.Hide();
                },
                SetLocalizedTooltip = () => Character != null ? string.Format(strings.Birthday_Text, Character.Name, Character.Age) + "\n" + strings.ClickBirthdayToMarkAsOpen : string.Empty,
                Visible = false,
            };

            _buttonContainer = new Panel()
            {
                Parent = this,
                Location = new Point(0, _image.Bottom + 5 + 2),
                Width = 355,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _captureImages = new Button()
            {
                Parent = _buttonContainer,
                Size = new Point(136, 25),
                Location = new(0),
                SetLocalizedText = () => strings.CaptureImages,
                SetLocalizedTooltip = () => strings.TogglePortraitCapture_Tooltip,
                Icon = tM.GetIcon(Icons.Camera),
                ResizeIcon = true,
                ClickAction = togglePotrait,
            };

            _openFolder = new Button()
            {
                Parent = _buttonContainer,
                Location = new Point(_captureImages.Right + 4, 0),
                Size = new Point(136, 25),
                SetLocalizedText = () => string.Format(strings.OpenItem, strings.Folder),
                SetLocalizedTooltip = () => strings.OpenPortraitFolder,
                Icon = tM.GetIcon(Icons.Folder),
                ResizeIcon = true,
                ClickAction = () =>
                {
                    _ = Process.Start(new ProcessStartInfo()
                    {
                        Arguments = _accountPath,
                        FileName = "explorer.exe",
                    });
                },
            };

            _tagContainer = new Panel()
            {
                Parent = this,
                Location = new Point(0, _image.Bottom + 5 + 2),
                Width = 355,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _tagBox = new TextBox()
            {
                Parent = _tagContainer,
                Size = new Point(355 - 26, 24),
                PlaceholderText = strings.Tag_Placeholder,
                EnterPressedAction = (t) =>
                {
                    if (t != null && t.Length > 0 && !_allTags.Contains(t))
                    {
                        _allTags.Add(t);
                        Character.AddTag(t);
                        _tags.Add(AddTag(t, true));
                        _refreshCharacters?.Invoke();

                        _tagBox.Text = null;
                    }
                }
            };

            _addTag = new ImageButton()
            {
                Parent = _tagContainer,
                Texture = tM.GetControlTexture(ControlTextures.Plus_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Plus_Button_Hovered),
                Location = new Point(_tagBox.Right + 2, _tagBox.Top),
                Size = new Point(24, 24),
                BasicTooltipText = string.Format(strings.AddItem, strings.Tag),
                ClickAction = (m) =>
                {
                    if (_tagBox.Text != null && _tagBox.Text.Length > 0 && !_allTags.Contains(_tagBox.Text))
                    {
                        _allTags.Add(_tagBox.Text);
                        Character.AddTag(_tagBox.Text);
                        _tags.Add(AddTag(_tagBox.Text, true));
                        _refreshCharacters?.Invoke();

                        _tagBox.Text = null;
                    }
                }
            };

            _tagPanel = new TagFlowPanel()
            {
                Parent = _tagContainer,
                Location = new Point(5, _tagBox.Bottom + 5),
                Width = 355,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
            };

            _imagePanelParent = new Panel()
            {
                Parent = this,
                BorderColor = Color.Black,
                BackgroundColor = Color.Black * 0.4f,
                Location = new(0, _buttonContainer.Bottom + 10),
                BorderWidth = new(2),
                Visible = false,
            };
            _imagePanel = new()
            {
                Parent = _imagePanelParent,
                ControlPadding = new(5),
                ContentPadding = new(5),
                OuterControlPadding = new(0),
                CanScroll = true,
                ZIndex = 11,
            };
        }

        public Character_Model Character
        {
            get => _character; set
            {
                _character = value;
                ApplyCharacter();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ShowImages(false);

            foreach (string t in _allTags)
            {
                if (_tags.Find(e => e.Text == t) == null) _tags.Add(AddTag(t, true));
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (BackgroundImage != null) TextureRectangle = new Rectangle(30, 30, Math.Min(BackgroundImage.Width - 100, Width), Math.Min(BackgroundImage.Height - 100, Height));
            if (_tagPanel != null) _tagPanel.Width = ContentRegion.Width;
            //if (_closeButton != null) _closeButton.Location = new(AbsoluteBounds.Right - _closeButton.Size.X - AutoSizePadding.X, AbsoluteBounds.Top + AutoSizePadding.Y);
        }

        public void LoadImages(object sender, EventArgs e)
        {
            string path = _accountPath;
            List<string> images = new(Directory.GetFiles(path, "*.png", SearchOption.AllDirectories));

            PanelSizes pSize = _settings.PanelSize.Value;
            int imageSize = 80;

            int maxHeight = GameService.Graphics.SpriteScreen.Height / 2;
            int width = (int)Math.Min(710, Math.Min(GameService.Graphics.SpriteScreen.Height / 2, (_imagePanel.OuterControlPadding.Y * 2) + ((images.Count + 1) * (imageSize + _imagePanel.ControlPadding.X))));
            int height = (int)Math.Min(maxHeight, (_imagePanel.OuterControlPadding.Y * 2) + ((images.Count + 1) / Math.Floor((double)width / imageSize) * (imageSize + _imagePanel.ControlPadding.Y)));

            _imagePanel.Children.Clear();

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
            {
                AsyncTexture2D noImgTexture = null;

                if (Visible && Character != null)
                {
                    noImgTexture = Character.SpecializationIcon;

                    _ = new ImageButton()
                    {
                        Parent = _imagePanel,
                        Size = new(imageSize),
                        Texture = noImgTexture,
                        ClickAction = (m) =>
                        {
                            Character.IconPath = null;
                            Character.Icon = null;
                            ApplyCharacter();
                        },
                    };

                    foreach (string p in images)
                    {
                        AsyncTexture2D texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                        ImageButton img = new()
                        {
                            Parent = _imagePanel,
                            Size = new(imageSize),
                            Texture = texture,
                            ClickAction = (m) =>
                            {
                                Character.IconPath = p.Replace(Character.ModulePath, string.Empty);
                                Character.Icon = texture;
                                ApplyCharacter();
                            }
                        };
                    }

                    _imagePanel.Width = width;
                    _imagePanel.Height = height;
                    _imagePanel.Invalidate();

                    _imagePanelParent.Width = width;
                    _imagePanelParent.Height = height;

                    _closeButton.Location = _imagePanelParent.Right > 355 ? new(_imagePanelParent.Right - _closeButton.Size.X, AutoSizePadding.Y) : new(355 - _closeButton.Size.X, AutoSizePadding.Y);
                }
            });
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _closeButton?.Dispose();
            _imagePanel?.Dispose();
            _imagePanelParent?.Dispose();
        }

        private void ShowImages(bool toggle = true)
        {
            if (_imagePanelParent.Visible || !toggle)
            {
                _closeButton.Location = new(355 - _closeButton.Size.X, AutoSizePadding.Y);
                _tagContainer.Show();
                _buttonContainer.Hide();
                _imagePanelParent.Hide();
                _imagePanelParent.Width = 0;
                _imagePanelParent.Height = 0;
                return;
            }

            _tagContainer.Hide();
            _buttonContainer.Show();
            _imagePanelParent.Show();
            LoadImages(null, null);
        }

        private void ApplyCharacter()
        {
            if (Character != null)
            {
                _image.Texture = Character.Icon;
                _name.Text = Character.Name;
                _show.Checked = Character.Show;
                _radial.Checked = Character.ShowOnRadial;
                _birthdayButton.BasicTooltipText = _birthdayButton.SetLocalizedTooltip?.Invoke();
                _birthdayButton.Visible = Character.HadBirthday;

                foreach (Tag t in _tags)
                {
                    t.SetActive(_character.Tags.Contains(t.Text));
                }
            }
        }

        private Tag AddTag(string txt, bool active = false)
        {
            Tag tag = new()
            {
                Text = txt,
                Parent = _tagPanel,
                Active = active,
                CanInteract = true,
                ShowDelete = false,
            };

            tag.ActiveChanged += Tag_ActiveChanged; ;

            return tag;
        }

        private void Tag_ActiveChanged(object sender, EventArgs e)
        {
            var tag = (Tag)sender;

            if (tag.Active && !_character.Tags.Contains(tag.Text))
            {
                _character.AddTag(tag.Text);
            }
            else
            {
                _character.RemoveTag(tag.Text);
            }
        }
    }
}
