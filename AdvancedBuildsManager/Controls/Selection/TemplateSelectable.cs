using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD.Content;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using System.Linq;
using Blish_HUD.Input;
using Blish_HUD;
using Kenedia.Modules.Core.Utility;
using MonoGame.Extended.BitmapFonts;
using static Blish_HUD.ContentService;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Services;
using System;
using System.ComponentModel;
using ProtoBuf.WellKnownTypes;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.Selection
{
    public class TemplateSelectable : Panel
    {
        private readonly TexturesService _texturesService;
        private readonly AsyncTexture2D _lineTexture = AsyncTexture2D.FromAssetId(605025);

        private readonly AsyncTexture2D _copyTexture = AsyncTexture2D.FromAssetId(2208345);
        private readonly AsyncTexture2D _textureFillCrest = AsyncTexture2D.FromAssetId(605004);
        private readonly AsyncTexture2D _textureVignette = AsyncTexture2D.FromAssetId(605003);
        private readonly AsyncTexture2D _textureCornerButton = AsyncTexture2D.FromAssetId(605011);
        private readonly AsyncTexture2D _textureBottomSectionSeparator = AsyncTexture2D.FromAssetId(157218);

        private readonly BitmapFont _nameFont = Content.DefaultFont14;
        private readonly BitmapFont _notificationFont = UI.GetFont(FontSize.Size14, FontStyle.Regular);
        private readonly ImageButton _editButton;
        private readonly TextBox _nameEdit;
        private readonly bool _created;
        private readonly Label _name;

        private readonly List<Tag> _tags = new();
        private readonly List<TagTexture> _tagTexturess = new();

        private Rectangle _separatorBounds;
        private Rectangle _editBounds;
        private Rectangle _specBounds;
        private Rectangle _raceBounds;
        private Rectangle _raceAndSpecBounds;
        private Rectangle _leftAccentBorderBounds;
        private Rectangle _rightAccentBorderBounds;
        private Rectangle _bottomBounds;
        private Rectangle _vignetteBounds;
        private Rectangle _tagBounds;
        private Rectangle _nameBounds;

        private Template _template;
        private double _animationStart;
        private double _animationDuration = 1500;
        private float _animationOpacityStep = 1;
        private bool _animationRunning;
        private AsyncTexture2D _raceTexture;
        private AsyncTexture2D _specTexture;

        public TemplateSelectable()
        {
            Height = 85;

            BorderWidth = new(3);
            BorderColor = Color.Black;

            _name = new()
            {
                Parent = this,
                Height = _nameFont.LineHeight,
                Font = _nameFont,
                WrapText = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
            };

            _nameEdit = new()
            {
                Parent = this,
                Height = _nameFont.LineHeight,
                Font = _nameFont,
                Visible = false,
                HideBackground = true,
                EnterPressedAction = (txt) =>
                {
                    string moddedtxt = txt.Trim().ToLower();
                    var template = AdvancedBuildsManager.ModuleInstance.Templates.Where(e => e.Name.ToLower() == moddedtxt).FirstOrDefault();

                    if ((template == null || template == Template))
                    {
                        _ = (Template?.ChangeName(txt));
                        ToggleEditMode(false);
                        OnNameChangedAction?.Invoke();
                    }
                    else
                    {
                        _nameEdit.Focused = true;
                    }
                },
                TextChangedAction = (txt) =>
                {
                    txt = txt.Trim().ToLower();

                    var template = AdvancedBuildsManager.ModuleInstance.Templates.Where(e => e.Name.ToLower() == txt).FirstOrDefault();
                    _nameEdit.ForeColor = template == null || template == Template ? Color.White : Color.Red;
                },
            };

            _texturesService = AdvancedBuildsManager.ModuleInstance.Services.TexturesService;

            _editButton = new()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175779),
                DisabledTexture = AsyncTexture2D.FromAssetId(2175780),

                TextureRectangle = new(2, 2, 28, 28),
                Size = new(20),
                ClickAction = (m) => ToggleEditMode(!_nameEdit.Visible),
            };

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
            SetTooltip();

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => "Duplicate", DuplicateTemplate));
            _ = Menu.AddMenuItem(new ContextMenuItem(() => "Delete", DeleteTemplate));

            _created = true;
        }

        private async void DeleteTemplate()
        {
            var result = await new BaseDialog("Warning", $"Are you sure to delete '{Template?.Name}'?") { DesiredWidth = 300, AutoSize = true }.ShowDialog();

            if (result == DialogResult.OK)
            {
                _ = AdvancedBuildsManager.ModuleInstance.Templates.Remove(Template);
                _ = Template?.Delete();
                if (AdvancedBuildsManager.ModuleInstance.SelectedTemplate == Template) AdvancedBuildsManager.ModuleInstance.SelectedTemplate = null;
            }
        }

        private void DuplicateTemplate()
        {
            Template t;
            string name = (Template?.Name ?? "New Template") + " - Copy";
            if (AdvancedBuildsManager.ModuleInstance.Templates.Where(e => e.Name == name).Count() == 0)
            {
                AdvancedBuildsManager.ModuleInstance.Templates.Add(t = new(Template?.BuildCode, Template?.GearCode) { Name = name });
                AdvancedBuildsManager.ModuleInstance.SelectedTemplate = t;
            }
            else
            {
                ScreenNotification.ShowNotification($"There is already a template with the name '{name}'");
            }
        }

        private void SetTooltip()
        {
            string txt = "CTRL + Left Click to copy the Build Template Code";

            foreach(var c in Children)
            {
                c.BasicTooltipText = txt;
            }

            BasicTooltipText = txt;
        }

        public Action OnClickAction { get; set; }

        public Action OnNameChangedAction { get; set; }

        public Template Template
        {
            get => _template;
            set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.PropertyChanged -= TemplateChanged;
                    if (_template != null) _template.PropertyChanged += TemplateChanged;
                }
            }
        }

        public void ToggleEditMode(bool enable)
        {
            if (enable)
            {
                _nameEdit.Text = _name.Text;
                _nameEdit.Focused = true;
                _nameEdit.SelectionStart = 0;
                _nameEdit.SelectionEnd = _nameEdit.Text.Length;
            }

            _name.Text = Template?.Name ?? "No Name";

            _nameEdit.Visible = enable;
            _name.Visible = !enable;
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            RecalculateLayout();
            string? txt = null;
            spriteBatch.DrawOnCtrl(this, _textureVignette, _vignetteBounds, _textureVignette.Bounds, Color.Black, 0F, Vector2.Zero);
            //spriteBatch.DrawOnCtrl(this, _textureFillCrest, _textureFillCrest.Bounds, _textureFillCrest.Bounds, Color.Black, 0F, Vector2.Zero);

            if (Template?.Profession != null)
            {
                //var prof = MouseOver ? _copyTexture : _specTexture;

                var prof = _specTexture;
                if (prof != null) spriteBatch.DrawOnCtrl(this, prof, _specBounds, prof.Bounds, Color.White, 0F, Vector2.Zero);
                if (_raceTexture != null) spriteBatch.DrawOnCtrl(this, _raceTexture, _raceBounds, _raceTexture.Bounds, Color.White, 0F, Vector2.Zero);
            }

            int amount = 0;
            for (int i = 0; i < _tagTexturess.Count; i++)
            {
                TagTexture tagTexture = _tagTexturess[i];
                if (_tagBounds.Contains(tagTexture.Bounds))
                {
                    if (_tagTexturess.Count - amount > 1 && !_tagBounds.Contains(_tagTexturess[i + 1].Bounds))
                    {
                        spriteBatch.DrawStringOnCtrl(this, $"+{_tagTexturess.Count - amount}", Content.DefaultFont14, tagTexture.Bounds, Colors.OldLace, false, Blish_HUD.Controls.HorizontalAlignment.Center);
                        if (tagTexture.Bounds.Contains(RelativeMousePosition))
                        {
                            txt = string.Join(Environment.NewLine, _tagTexturess.Skip(amount).Take(_tagTexturess.Count - amount).Select(e => e.TemplateTag.ToString()));
                        }
                        break;
                    }
                    else
                    {
                        tagTexture.Draw(this, spriteBatch, RelativeMousePosition);
                        if (tagTexture.Hovered)
                        {
                            txt = tagTexture.TemplateTag.ToString();
                        }
                    }

                    amount++;
                }
                else
                {
                    //spriteBatch.DrawStringOnCtrl(this, $"+{_tagTexturess.Count - amount}", Content.DefaultFont14, tagTexture.Bounds, Colors.OldLace, false, Blish_HUD.Controls.HorizontalAlignment.Left);
                    break;
                }
            }

            BasicTooltipText = txt;
            //spriteBatch.DrawCenteredRotationOnCtrl(this, _lineTexture, _separatorBounds, _lineTexture.Bounds, Color.White, MathUtil.DegreesToRadians(90), false, false);
            //spriteBatch.DrawOnCtrl(this, _lineTexture, _separatorBounds, _lineTexture.Bounds, Color.Black, MathUtil.DegreesToRadians(90), new(_lineTexture.Bounds.Center.X, _lineTexture.Bounds.Center.Y));
            //spriteBatch.DrawStringOnCtrl(this, Template?.Name ?? "Unkown Name", _nameFont, _nameBounds, Color.White, true, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, ContentRegion.Add(0, 0, 0, -28), Rectangle.Empty, Template?.Profession.GetWikiColor() * 0.3F ?? Color.Transparent);
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, _vignetteBounds, Rectangle.Empty, Color.Black * 0.15F);
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new(ContentRegion.X, ContentRegion.Bottom - 28, ContentRegion.Width, 28), Rectangle.Empty, Color.Black * 0.3F);

            base.PaintBeforeChildren(spriteBatch, bounds);
            bool isActive = AdvancedBuildsManager.ModuleInstance.SelectedTemplate == _template;
            spriteBatch.DrawFrame(this, bounds, isActive ? Colors.ColonialWhite : Color.Transparent, 2);

            spriteBatch.DrawOnCtrl(this, _textureBottomSectionSeparator, _separatorBounds, _textureBottomSectionSeparator.Bounds, Color.Black, 0F, Vector2.Zero);
            spriteBatch.DrawOnCtrl(this, _lineTexture, _leftAccentBorderBounds, _lineTexture.Bounds, Color.Black * 0.6F, 0F, Vector2.Zero);
            spriteBatch.DrawOnCtrl(this, _lineTexture, _rightAccentBorderBounds, _lineTexture.Bounds, Color.Black * 0.6F, 0F, Vector2.Zero, SpriteEffects.FlipVertically);
            spriteBatch.DrawOnCtrl(this, _textureCornerButton, _raceAndSpecBounds, _textureCornerButton.Bounds, Color.Black, 0F, Vector2.Zero);

            spriteBatch.DrawOnCtrl(this, _textureCornerButton, _editBounds, _textureCornerButton.Bounds, Color.Black, 0F, Vector2.Zero);

            //spriteBatch.DrawOnCtrl(this, Textures.Pixel, _editBounds, Rectangle.Empty, Color.Red * 0.5F);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (!_created) return;

            var contentBounds = new Rectangle(ContentRegion.Left + 2, ContentRegion.Top + 2, ContentRegion.Width - 4, ContentRegion.Height - 4);

            _vignetteBounds = new(contentBounds.Left, contentBounds.Top, 55, 55);
            _name?.SetLocation(new(_vignetteBounds.Right + 5, contentBounds.Top));
            _name?.SetSize(new(contentBounds.Width - _vignetteBounds.Width - 5, _vignetteBounds.Height));

            _nameEdit?.SetLocation(new(_vignetteBounds.Right + 5, contentBounds.Top));
            _nameEdit?.SetSize(new(contentBounds.Width - _vignetteBounds.Width - 5, _vignetteBounds.Height));

            //_separatorBounds = new(ContentRegion.Width / 2, _name.Bottom + 5, 16, ContentRegion.Width - 12);
            _separatorBounds = new(2, _name.Bottom - 4, contentBounds.Width, 8);

            _bottomBounds = new(_separatorBounds.Left, _name.Bottom + 1, _separatorBounds.Width, ContentRegion.Height - 4 - _name.Bottom);
            _editBounds = new(_bottomBounds.Right - _bottomBounds.Height, _bottomBounds.Top, _bottomBounds.Height, _bottomBounds.Height);
            _specBounds = _vignetteBounds.Add(2, 2, -4, -4);

            //_specBounds = new(_bottomBounds.Location.Add(new(0, 2)), new(_bottomBounds.Height));
            //_raceBounds = new(_specBounds.Location.Add(new(_specBounds.Width + 4, 0)), new Point(_bottomBounds.Height).Add(new(-2)));

            _raceBounds = new(_bottomBounds.Location.Add(new(1)), new(_bottomBounds.Height - 2));
            _raceAndSpecBounds = new(_bottomBounds.Left, _bottomBounds.Top, _raceBounds.Right - _specBounds.Left + 5, _bottomBounds.Height);
            _leftAccentBorderBounds = new(_raceAndSpecBounds.Right - 8, _bottomBounds.Top, 16, _bottomBounds.Height + 3);
            _rightAccentBorderBounds = new(_editBounds.Left - 8, _bottomBounds.Top, 16, _bottomBounds.Height + 3);

            _editButton?.SetLocation(_editBounds.Location.Add(new(2)));
            _editButton?.SetSize(_editBounds.Size.Add(new(-4)));

            _tagBounds = new(_leftAccentBorderBounds.Right - 6, _bottomBounds.Top, _rightAccentBorderBounds.Left - (_leftAccentBorderBounds.Right - 10), _bottomBounds.Height);

            TagTexture prev;
            for (int i = 0; i < _tagTexturess.Count; i++)
            {
                TagTexture tagTexture = _tagTexturess[i];
                if (tagTexture.TemplateTag.GetType() == typeof(EncounterFlag) && tagTexture.TemplateTag is not EncounterFlag.NormalMode and not EncounterFlag.ChallengeMode)
                {
                    tagTexture.Bounds = new(_leftAccentBorderBounds.Right - 6 + (i * (_bottomBounds.Height + 3)), _bottomBounds.Top + 2, _bottomBounds.Height - 4, _bottomBounds.Height - 4);
                }
                else
                {
                    tagTexture.Bounds = new(_leftAccentBorderBounds.Right - 6 + (i * (_bottomBounds.Height + 3)), _bottomBounds.Top, _bottomBounds.Height, _bottomBounds.Height);
                }

                prev = tagTexture;
            }
        }

        protected override async void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (Input.Keyboard.KeysDown.Contains(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                try
                {
                    SetNotification("Build Code copied!", Color.LimeGreen, 350);
                    _ = await ClipboardUtil.WindowsClipboardService.SetTextAsync(Template?.BuildCode);
                }
                catch (Exception)
                {
                }

                return;
            }
            AdvancedBuildsManager.ModuleInstance.SelectedTemplate = _template;
            OnClickAction?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
            if (_template != null) _template.PropertyChanged -= TemplateChanged;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (_animationRunning)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - _animationStart < _animationDuration)
                {
                    double timepast = gameTime.TotalGameTime.TotalMilliseconds - _animationStart;

                    if (timepast > 5)
                    {
                        _name.Opacity = 1F - (float)(timepast / 5 * _animationOpacityStep);
                    }
                }
                else
                {
                    SetName();
                }
            }
        }

        private void SetNotification(string v, Color color, double duration = 1500)
        {
            _animationDuration = duration;
            _animationStart = Common.Now();
            _animationOpacityStep = (float)(1F / (_animationDuration / 5));

            _name.TextColor = color;
            _name.WrapText = false;
            _name.Font = _notificationFont;
            _name.HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left;
            _name.Text = v;

            _animationRunning = true;
        }

        private void SetName()
        {
            _name.TextColor = Color.White;
            _name.Font = _nameFont;
            _name.WrapText = true;
            _name.HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left;
            _name.Opacity = 1F;
            _name.Text = _template.Name;

            _animationRunning = false;
        }

        private void ApplyTemplate()
        {
            _name.Text = _template?.Name;
            _raceTexture = AdvancedBuildsManager.Data.Races.TryGetValue(_template?.Race ?? Races.None, out var race) ? race.Icon : null;
            _specTexture = Template?.EliteSpecialization?.ProfessionIconBig ?? (AdvancedBuildsManager.Data.Professions.TryGetValue((Gw2Sharp.Models.ProfessionType)Template?.Profession, out var profession) ? profession.IconBig : null);

            _tagTexturess.Clear();

            if (_template != null)
            {
                foreach (var flag in _template.Tags.GetFlags())
                {
                    if ((TemplateFlag)flag != TemplateFlag.None)
                    {
                        TagTexture t;
                        //_tagTexturess.Add(t = new(((TemplateTag)flag).GetTexture()));
                        _tagTexturess.Add(t = ((TemplateFlag)flag).GetDetailedTexture());
                    }
                }

                foreach (var flag in _template.Encounters.GetFlags())
                {
                    if ((EncounterFlag)flag != EncounterFlag.None)
                    {
                        TagTexture t;
                        //_tagTexturess.Add(t = new(((TemplateTag)flag).GetTexture()));
                        _tagTexturess.Add(t = ((EncounterFlag)flag).GetDetailedTexture());
                        int pad = (t.Texture.Width / 16);
                        t.TextureRegion = flag is EncounterFlag.NormalMode or EncounterFlag.ChallengeMode ? new(-pad, -pad, t.Texture.Width + (pad * 2), t.Texture.Height + (pad * 2)) : t.TextureRegion;
                    }
                }
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!_editButton.MouseOver && !_nameEdit.MouseOver && _nameEdit.Visible)
            {
                ToggleEditMode(false);
                SetName();
            }
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }
    }
}
