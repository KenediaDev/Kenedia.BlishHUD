using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Characters.Res;
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
        private readonly List<Tag> _tags = new();

        private readonly ImageButton _closeButton;
        private readonly Button _openFolder;
        private readonly Panel _tagContainer;
        private readonly TextBox _tagBox;
        private readonly ImageButton _addTag;
        private readonly TagFlowPanel _tagPanel;
        private readonly ImageButton _image;
        private readonly Label _name;
        private readonly Checkbox _show;
        private readonly Button _captureImages;
        private readonly Panel _imagePanelParent;
        private readonly FlowPanel _imagePanel;
        private Character_Model _character;
        private DateTime _lastInteraction;

        public CharacterEdit()
        {
            TextureManager tM = Characters.ModuleInstance.TextureManager;

            HeightSizingMode = SizingMode.AutoSize;
            WidthSizingMode = SizingMode.AutoSize;
            AutoSizePadding = new(5);

            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            TextureRectangle = new Rectangle(26, 26, Math.Min(BackgroundImage.Width - 100, Width), Math.Min(BackgroundImage.Height - 100, Height));
            BorderColor = Color.Black;

            _ = new Dummy()
            {
                Parent = this,
                Width = 350,
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
                Location = new Point(5 + 2, 5 + 2),
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
                Location = new Point(_image.Right + 5 + 2, 5),
            };

            _show = new Checkbox()
            {
                Parent = this,
                Location = new Point(_image.Right + 5 + 2, _name.Bottom + 5 + 2),
                Size = new Point(100, 21),
                Text = strings.ShowInList,
                CheckedChangedAction = (b) =>
                {
                    if (Character != null) Character.Show = !Character.Show;
                },
            };

            _captureImages = new Button()
            {
                Parent = this,
                Location = new Point(_image.Right + 4, _show.Bottom + 2),
                Size = new Point(136, 25),
                Text = strings.CaptureImages,
                BasicTooltipText = strings.TogglePortraitCapture_Tooltip,
                Icon = tM.GetIcon(Icons.Camera),
                ResizeIcon = true,
                ClickAction = () => Characters.ModuleInstance.PotraitCapture.Visible = !Characters.ModuleInstance.PotraitCapture.Visible
            };

            _openFolder = new Button()
            {
                Parent = this,
                Location = new Point(_captureImages.Right + 4, _show.Bottom + 2),
                Size = new Point(136, 25),
                Text = string.Format(strings.OpenItem, strings.Folder),
                BasicTooltipText = strings.OpenPortraitFolder,
                Icon = tM.GetIcon(Icons.Folder),
                ResizeIcon = true,
                ClickAction = () =>
                {
                    _ = Process.Start(new ProcessStartInfo()
                    {
                        Arguments = Characters.ModuleInstance.AccountImagesPath,
                        FileName = "explorer.exe",
                    });
                },
            };

            _tagContainer = new Panel()
            {
                Parent = this,
                Location = new Point(5, _image.Bottom + 5 + 2),
                Width = 350,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _tagBox = new TextBox()
            {
                Parent = _tagContainer,
                Size = new Point(350 - 24, 24),
                PlaceholderText = strings.Tag_Placeholder,
                EnterPressedAction = (t) =>
                {
                    if (t != null && t.Length > 0 && !Characters.ModuleInstance.Tags.Contains(t))
                    {
                        Characters.ModuleInstance.MainWindow.RequestUniform();
                        Characters.ModuleInstance.Tags.Add(t);
                        Character.AddTag(t);
                        _tags.Add(AddTag(t, true));

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
                    if (_tagBox.Text != null && _tagBox.Text.Length > 0 && !Characters.ModuleInstance.Tags.Contains(_tagBox.Text))
                    {
                        Characters.ModuleInstance.MainWindow.RequestUniform();
                        Characters.ModuleInstance.Tags.Add(_tagBox.Text);
                        Character.AddTag(_tagBox.Text);
                        _tags.Add(AddTag(_tagBox.Text, true));

                        _tagBox.Text = null;
                    }
                }
            };

            _tagPanel = new TagFlowPanel()
            {
                Parent = _tagContainer,
                Location = new Point(5, _tagBox.Bottom + 5),
                Width = 350,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
            };

            _imagePanelParent = new Panel()
            {
                Parent = this,
                BorderColor = Color.Black,
                BackgroundColor = Color.Black * 0.4f,
                Location = new(5, _image.Bottom + 5),
                Visible = false,
            };
            _imagePanel = new()
            {
                Location = new(0, 5),
                Parent = _imagePanelParent,
                AutoSizePadding = new Point(5, 5),
                OuterControlPadding = new Vector2(5, 0),
                ControlPadding = new(5),
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

            foreach (string t in Characters.ModuleInstance.Tags)
            {
                if (_tags.Find(e => e.Text == t) == null) _tags.Add(AddTag(t, true));
            }

            SetInteracted(null, null);
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
            string path = Characters.ModuleInstance.AccountImagesPath;
            List<string> images = new(Directory.GetFiles(path, "*.png", SearchOption.AllDirectories));

            var settings = Characters.ModuleInstance.Settings;
            PanelSizes pSize = settings.PanelSize.Value;
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
                                Character.IconPath = p.Replace(Characters.ModuleInstance.BasePath, string.Empty);
                                Character.Icon = texture;
                                ApplyCharacter();
                            }
                        };
                    }

                    _imagePanel.Width = width;
                    _imagePanel.Height = height;
                    _imagePanel.Invalidate();

                    _imagePanelParent.Width = width;
                    _imagePanelParent.Height = height + 10;

                    _closeButton.Location = new(_imagePanelParent.Right - _closeButton.Size.X - AutoSizePadding.X + 5, AutoSizePadding.Y);
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

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            SetInteracted(null, null);
        }

        private void ShowImages(bool toggle = true)
        {
            if (_imagePanelParent.Visible || !toggle)
            {
                _closeButton.Location = new(355 - _closeButton.Size.X, AutoSizePadding.Y);
                _tagContainer.Show();
                _imagePanelParent.Hide();
                _imagePanelParent.Width = 0;
                _imagePanelParent.Height = 0;
                return;
            }

            _tagContainer.Hide();
            _imagePanelParent.Show();
            LoadImages(null, null);
        }

        private void ApplyCharacter()
        {
            if (Character != null)
            {
                _image.Texture = Character.Icon;
                _name.Text = Character.Name;

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
            };

            tag.Deleted += Tag_Deleted;
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

        private void Tag_Deleted(object sender, EventArgs e)
        {
            var tag = (Tag)sender;
            _ = _tags.Remove(tag);
            _ = Characters.ModuleInstance.Tags.Remove(tag.Text);

            tag.Deleted -= Tag_Deleted;
        }

        private void SetInteracted(object sender, EventArgs e)
        {
            _lastInteraction = DateTime.Now;
            Opacity = 1f;
        }
    }
}
