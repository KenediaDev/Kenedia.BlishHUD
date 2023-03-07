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
using static Kenedia.Modules.Characters.Services.Settings;
using static Kenedia.Modules.Characters.Services.TextureManager;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;
using System.Linq;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Models;
using Blish_HUD.Input;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterEdit : AnchoredContainer
    {
        private readonly AsyncTexture2D _presentTexture = AsyncTexture2D.FromAssetId(593864);
        private readonly AsyncTexture2D _presentTextureOpen = AsyncTexture2D.FromAssetId(593865);
        private readonly ImageButton _delete;

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
        private readonly TagList _allTags;
        private readonly Settings _settings;
        private readonly Action _refreshCharacters;
        private Character_Model _character;
        private ImageButton _noImgButton;

        public CharacterEdit(TextureManager tM, Action togglePotrait, Func<string> accountPath, TagList allTags, Settings settings, Action refreshCharacters)
        {
            AccountImagePath = accountPath;
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
                ClickAction = (m) => ShowImages(!_imagePanelParent.Visible),
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

            _delete = new()
            {
                Parent = this,
                Size = new(40, 40),
                Location = new(355 - 40, 30),
                Texture = tM.GetControlTexture(ControlTextures.Delete_Button),
                HoveredTexture = tM.GetControlTexture(ControlTextures.Delete_Button_Hovered),
                SetLocalizedTooltip = () => string.Format(strings.DeleteItem, Character != null ? Character.Name : "Character"),
                ClickAction = ConfirmDelete,
                ZIndex = 11,
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
                    string p = AccountImagePath?.Invoke();
                    if (!string.IsNullOrEmpty(p))
                    {
                        _ = Process.Start(new ProcessStartInfo()
                        {
                            Arguments = p,
                            FileName = "explorer.exe",
                        });
                    }
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
                TextChangedAction = (t) =>
                {
                    LastInteraction = DateTime.Now;
                },
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
                //HeightSizingMode = SizingMode.AutoSize,
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

        public Func<string> AccountImagePath { get; set; }

        public Character_Model Character
        {
            get => _character; set
            {
                _character = value;
                ApplyCharacter();
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if(_tagBox.Focused)
            {
                LastInteraction = DateTime.Now;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ShowImages(false);
            //LoadImages(null, null);

            var tagList = _tags.Select(e => e.Text);
            var allTags = _allTags;

            var deleteTags = tagList.Except(allTags);
            var addTags = allTags.Except(tagList);

            bool tagChanged = deleteTags.Any() || addTags.Any();

            if (tagChanged)
            {
                var deleteList = new List<Tag>();
                foreach (string tag in deleteTags)
                {
                    var t = _tags.FirstOrDefault(e => e.Text == tag);
                    if (t != null) deleteList.Add(t);
                }

                foreach (var t in deleteList)
                {
                    t.Dispose();
                    _ = _tags.Remove(t);
                }

                foreach (string tag in addTags)
                {
                    _ = AddTag(tag, Character != null && Character.Tags.Contains(tag));
                }
            }

            _tagPanel.FitWidestTag(355);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (BackgroundImage != null) TextureRectangle = new Rectangle(30, 30, Math.Min(BackgroundImage.Width - 100, Width), Math.Min(BackgroundImage.Height - 100, Height));
            if (_tagPanel != null) _tagPanel.FitWidestTag(355);
            //if (_closeButton != null) _closeButton.Location = new(AbsoluteBounds.Right - _closeButton.Size.X - AutoSizePadding.X, AbsoluteBounds.Top + AutoSizePadding.Y);
        }

        public void LoadImages(object sender, EventArgs e)
        {
            string path = AccountImagePath?.Invoke();

            if (!string.IsNullOrEmpty(path))
            {
                List<string> images = new(Directory.GetFiles(path, "*.png", SearchOption.AllDirectories));

                PanelSizes pSize = _settings.PanelSize.Value;

                int imageSize = 80;

                GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                {
                    AsyncTexture2D noImgTexture = null;
                    Character ??= (Anchor as MainWindow)?.CharacterCards.FirstOrDefault()?.Character;

                    if (Character != null)
                    {
                        _imagePanel.Children.Clear();
                        noImgTexture = Character.SpecializationIcon;
                        if (Anchor != null && Anchor.Visible)
                        {
                            (Anchor as MainWindow)?.ShowAttached(this);
                            ShowImages(true, false);
                        }

                        _noImgButton = new ImageButton()
                        {
                            Parent = _imagePanel,
                            Size = new(imageSize),
                            Texture = noImgTexture,
                            SetLocalizedTooltip = () => strings.SetSpecializationIcon,
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

                        AdjustImagePanelHeight(images);
                        _closeButton.Location = _imagePanelParent.Right > 355 ? new(_imagePanelParent.Right - _closeButton.Size.X, AutoSizePadding.Y) : new(355 - _closeButton.Size.X, AutoSizePadding.Y);
                        _delete.Location = _imagePanelParent.Right > 355 ? new(_imagePanelParent.Right - _delete.Size.X, _delete.Top) : new(355 - _delete.Size.X, _delete.Top);
                    }
                });
            }
        }

        private void AdjustImagePanelHeight(List<string> images)
        {
            int imageSize = 80;
            int maxHeight = GameService.Graphics.SpriteScreen.Height / 3;

            int cols = Math.Min(images.Count + 1, Math.Min(640, GameService.Graphics.SpriteScreen.Height / 3) / 80);
            int rows = (int)Math.Ceiling((double)((images.Count + 1) / (double)cols));

            int width = (cols * imageSize) + ((cols - 1) * (int)_imagePanel.ControlPadding.X) + (int)_imagePanel.OuterControlPadding.X + 30;
            int height = (rows * imageSize) + ((rows - 1) * (int)_imagePanel.ControlPadding.Y) + (int)_imagePanel.OuterControlPadding.Y + 10;

            _imagePanelParent.Width = width + 10;
            _imagePanelParent.Height = Math.Min(maxHeight, height);
            _imagePanel.Width = width;
            _imagePanel.Height = Math.Min(maxHeight, height);
            _imagePanel.Invalidate();
            _imagePanelParent.Invalidate();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _closeButton?.Dispose();
            _imagePanel?.Dispose();
            _imagePanelParent?.Dispose();
        }

        public void ShowImages(bool toggle = true, bool loadImages = true)
        {
            if (loadImages && toggle) LoadImages(null, null);

            if (!toggle)
            {
                _closeButton.Location = new(355 - _closeButton.Size.X, AutoSizePadding.Y);
                _delete.Location = new(355 - _delete.Size.X, _delete.Top);
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
            _imagePanelParent.Invalidate();
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
                _delete.UserLocale_SettingChanged(null, null);

                foreach (Tag t in _tags)
                {
                    t.SetActive(_character.Tags.Contains(t.Text));
                }

                if (_noImgButton != null)
                {
                    _noImgButton.Texture = Character.SpecializationIcon;
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

            tag.ActiveChanged += Tag_ActiveChanged;
            _tagPanel.FitWidestTag(355);
            _tags.Add(tag);

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

        private async void ConfirmDelete(MouseEventArgs m)
        {
            var result = await new BaseDialog("Delete Character", $"Are you sure to delete {Character?.Name}?").ShowDialog();

            Debug.WriteLine($"CONFIRM DELETE: {result}");
            if (result == DialogResult.OK)
            {
                Character?.Delete();
                Hide();
            }
        }
    }
}
