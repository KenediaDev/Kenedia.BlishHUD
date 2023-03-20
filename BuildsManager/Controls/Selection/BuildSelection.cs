using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Control = Blish_HUD.Controls.Control;
using Blish_HUD.Input;
using Blish_HUD;
using Kenedia.Modules.Core.Utility;
using MonoGame.Extended.BitmapFonts;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using static Blish_HUD.ContentService;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Label = Kenedia.Modules.Core.Controls.Label;
using MathUtil = SharpDX.MathUtil;
using System.Xml.Linq;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Services;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class TagTexture : DetailedTexture
    {
        public TagTexture(AsyncTexture2D texture) : base(texture)
        {
        }

        public TemplateTag TemplateTag { get; set; }
    }

    public class TemplateSelectable : Panel
    {
        private readonly AsyncTexture2D _lineTexture = AsyncTexture2D.FromAssetId(605025);

        private readonly AsyncTexture2D _textureFillCrest = AsyncTexture2D.FromAssetId(605004);
        private readonly AsyncTexture2D _textureVignette = AsyncTexture2D.FromAssetId(605003);
        private readonly AsyncTexture2D _textureCornerButton = AsyncTexture2D.FromAssetId(605011);
        private readonly AsyncTexture2D _textureBottomSectionSeparator = AsyncTexture2D.FromAssetId(157218);

        private readonly TexturesService _texturesService;
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

        private Template _template;
        private BitmapFont _nameFont = Content.DefaultFont14;
        private ImageButton _editButton;
        private bool _created;
        private Label _name;

        private Rectangle _nameBounds;

        public TemplateSelectable()
        {
            Height = 85;

            BorderWidth = new(3);
            BorderColor = Color.Black;

            _name = new()
            {
                Parent = this,
                Height = _nameFont.LineHeight,
                WrapText = true,
                Font = _nameFont,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
            };

            _texturesService = BuildsManager.ModuleInstance.Services.TexturesService;

            //_editButton = new()
            //{
            //    Parent = this,
            //    Texture = AsyncTexture2D.FromAssetId(155941),
            //    HoveredTexture = AsyncTexture2D.FromAssetId(155940),
            //    DisabledTexture = AsyncTexture2D.FromAssetId(155939),
            //    ClickedTexture = AsyncTexture2D.FromAssetId(155942),
            //    TextureRectangle = new(16, 16, 32, 32),
            //    Size = new(32),
            //};

            _editButton = new()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175779),
                DisabledTexture = AsyncTexture2D.FromAssetId(2175780),

                TextureRectangle = new(2, 2, 28, 28),
                Size = new(20),
            };

            ClipsBounds = true;
            _created = true;
        }

        public Template Template
        {
            get => _template;
            set => Common.SetProperty(ref _template, value, ApplyTemplate);
        }

        public Action OnClickAction { get; set; }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            RecalculateLayout();
            string? txt = null;
            spriteBatch.DrawOnCtrl(this, _textureVignette, _vignetteBounds, _textureVignette.Bounds, Color.Black, 0F, Vector2.Zero);
            //spriteBatch.DrawOnCtrl(this, _textureFillCrest, _textureFillCrest.Bounds, _textureFillCrest.Bounds, Color.Black, 0F, Vector2.Zero);

            if (Template?.Profession != null)
            {
                var prof =
                    Template.EliteSpecialization != null ?
                    Template.EliteSpecialization.ProfessionIconBig :
                    BuildsManager.Data.Professions[Template.Profession].IconBig;
                var race = GetRaceTexture(Template.Race);
                spriteBatch.DrawOnCtrl(this, prof, _specBounds, prof.Bounds, Color.White, 0F, Vector2.Zero);
                spriteBatch.DrawOnCtrl(this, race, _raceBounds, race.Bounds, Color.White, 0F, Vector2.Zero);
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
            bool isActive = BuildsManager.ModuleInstance.SelectedTemplate == _template;
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

            DetailedTexture prev;
            for (int i = 0; i < _tagTexturess.Count; i++)
            {
                DetailedTexture tagTexture = _tagTexturess[i];
                //tagTexture.Bounds = new((tagTexture?.Bounds.Right ?? _leftAccentBorderBounds.Right - 6) + 4, _bottomBounds.Top, _bottomBounds.Height, _bottomBounds.Height);
                tagTexture.Bounds = new(_leftAccentBorderBounds.Right - 6 + (i * (_bottomBounds.Height + 3)), _bottomBounds.Top, _bottomBounds.Height, _bottomBounds.Height);
                prev = tagTexture;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            BuildsManager.ModuleInstance.SelectedTemplate = _template;
            OnClickAction?.Invoke();
        }

        private void ApplyTemplate()
        {
            _name.Text = _template?.Name;
            _tagTexturess.Clear();

            if (_template != null)
            {
                foreach (var flag in _template.Tags.GetFlags())
                {
                    if ((TemplateTag)flag != TemplateTag.None)
                    {
                        TagTexture t;
                        //_tagTexturess.Add(t = new(((TemplateTag)flag).GetTexture()));
                        _tagTexturess.Add(t = ((TemplateTag)flag).GetDetailedTexture());
                    }
                }
            }
        }

        private Texture2D GetRaceTexture(Races race)
        {
            return race switch
            {
                Races.None => _texturesService.GetTexture(@"textures\races\pact.png", "pact"),
                Races.Asura => _texturesService.GetTexture(@"textures\races\asura.png", "asura"),
                Races.Charr => _texturesService.GetTexture(@"textures\races\charr.png", "charr"),
                Races.Human => _texturesService.GetTexture(@"textures\races\human.png", "human"),
                Races.Norn => _texturesService.GetTexture(@"textures\races\norn.png", "norn"),
                Races.Sylvari => _texturesService.GetTexture(@"textures\races\sylvari.png", "sylvari"),
                _ => _texturesService.GetTexture(@"textures\races\pact.png", "pact"),
            };
        }
    }

    public class BuildSelection : BaseSelection
    {
        private readonly List<DetailedTexture> _specIcons = new();
        private readonly ImageButton _addBuildsButton;
        private readonly List<TemplateSelectable> _templates = new();

        private readonly Dropdown _sortBehavior;
        private double _lastShown;

        public BuildSelection()
        {
            _sortBehavior = new()
            {
                Parent = this,
                Location = new(0, 30),
            };
            _sortBehavior.Items.Add("Sort by Profession");
            _sortBehavior.Items.Add("Sort by Name");
            _sortBehavior.Items.Add("Sort by Game Mode");

            Search.Location = new(2, 60);
            SelectionContent.Location = new(0, Search.Bottom + 5);

            int i = 0;
            int size = 25;
            Point start = new(0, 0);
            foreach (var prof in BuildsManager.Data.Professions.Values)
            {
                int j = 0;
                _specIcons.Add(new DetailedTexture(prof.Icon)
                {
                    Bounds = new(start.X + (i * (size + 10)), start.Y + (j * size), size, size),
                    DrawColor = Color.White * 0.7F,
                    //HoverDrawColor = ColorExtension.GetProfessionColor(prof.Id),
                    HoverDrawColor = Color.White,
                });

                //foreach (var spec in prof.Specializations.Values)
                //{                    
                //    if (spec.Elite)
                //    {
                //        j++;
                //        _specIcons.Add(new DetailedTexture(spec.ProfessionIconBig.Texture.ToGrayScaledPalettable())
                //        {
                //            Bounds = new(i * 25, j * 25, 25, 25),
                //            DrawColor = ColorExtension.Guardian * 0.7F,
                //            HoverDrawColor = ColorExtension.Guardian,
                //        });
                //    }
                //}

                i++;
            }

            _addBuildsButton = new()
            {
                Parent = this,
                Location = new(0, 30),
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                TextureRectangle = new(2, 2, 28, 28),
                ClickAction = (m) =>
                {
                    Template t;
                    string name = string.IsNullOrEmpty(Search.Text) ? "New Template" : Search.Text;
                    if (BuildsManager.ModuleInstance.Templates.Where(e => e.Name == name).Count() == 0)
                    {
                        BuildsManager.ModuleInstance.Templates.Add(t = new() { Name = name });
                        BuildsManager.ModuleInstance.SelectedTemplate = t;
                        Search.Text = null;
                    }
                },
            };

            BuildsManager.ModuleInstance.SelectedTemplateChanged += ModuleInstance_SelectedTemplateChanged;

            Search.TextChangedAction = (txt) =>
            {
                _addBuildsButton.BasicTooltipText = string.IsNullOrEmpty(txt) ? $"Create a new Template" : $"Create new Template '{txt}'";
            };

            BuildsManager.ModuleInstance.Templates.CollectionChanged += Templates_CollectionChanged;
            Templates_CollectionChanged(this, null);
        }

        private void ModuleInstance_SelectedTemplateChanged(object sender, EventArgs e)
        {
            var activeTemplate = _templates.Find(e => e.Template == BuildsManager.ModuleInstance.SelectedTemplate);
            if (activeTemplate != null) SelectionPanel.SetTemplateAnchor(activeTemplate);
        }

        public SelectionPanel SelectionPanel { get; set; }

        private void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var templates = _templates.Select(e => e.Template);
            var removedTemplates = templates.Except(BuildsManager.ModuleInstance.Templates);
            var addedTemplates = BuildsManager.ModuleInstance.Templates.Except(templates);

            foreach (var template in addedTemplates)
            {
                TemplateSelectable t = new()
                {
                    Parent = SelectionContent,
                    Template = template,
                    Width = SelectionContent.Width - 35,
                };

                t.OnClickAction = () =>
                {
                    SelectionPanel?.SetTemplateAnchor(t);
                };

                _templates.Add(t);
            }

            foreach (var template in removedTemplates)
            {
                _ = _templates.RemoveAll(e => e.Template == template);
            }

            SelectionContent?.Invalidate();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            foreach (var specIcon in _specIcons)
            {
                specIcon.Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Search?.SetSize(Width - Search.Left - Search.Height - 2);

            _addBuildsButton?.SetLocation(Search.Right, Search.Top);
            _addBuildsButton?.SetSize(Search.Height, Search.Height);

            _sortBehavior?.SetLocation(Search.Left);
            _sortBehavior?.SetSize((_addBuildsButton?.Right ?? 0) - Search.Left);
        }

        protected override void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnSelectionContent_Resized(sender, e);

            foreach (var template in _templates)
            {
                template.Width = SelectionContent.Width - 35;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (Common.Now() - _lastShown >= 250)
            {
                base.OnClick(e);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _lastShown = Common.Now();
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            _sortBehavior.Enabled = false;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            _sortBehavior.Enabled = _sortBehavior.Enabled || Common.Now() - _lastShown >= 5;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _sortBehavior?.Dispose();

            BuildsManager.ModuleInstance.SelectedTemplateChanged -= ModuleInstance_SelectedTemplateChanged;
        }
    }
}
