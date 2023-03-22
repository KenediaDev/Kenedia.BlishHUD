using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;
using AttributeType = Gw2Sharp.WebApi.V2.Models.AttributeType;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class AttributeToggle : ImageToggle
    {
        private AttributeType _attribute;
        public AttributeToggle()
        {
            ImageColor = Color.Gray * 0.5F;
            ActiveColor = Color.White;
            TextureRectangle = new(4, 4, 24, 24);
        }

        public AttributeType Attribute { get => _attribute; set => Common.SetProperty(ref _attribute, value, OnAttributeChanged); }

        private void OnAttributeChanged()
        {
            Texture = _attribute switch
            {
                AttributeType.Power => AsyncTexture2D.FromAssetId(66722),
                AttributeType.Toughness => AsyncTexture2D.FromAssetId(156612),
                AttributeType.Vitality => AsyncTexture2D.FromAssetId(156613),
                AttributeType.Precision => AsyncTexture2D.FromAssetId(156609),
                AttributeType.CritDamage => AsyncTexture2D.FromAssetId(156602),
                AttributeType.ConditionDamage => AsyncTexture2D.FromAssetId(156600),
                AttributeType.ConditionDuration => AsyncTexture2D.FromAssetId(156601),
                AttributeType.BoonDuration => AsyncTexture2D.FromAssetId(156599),
                AttributeType.Healing => AsyncTexture2D.FromAssetId(156606),
                _ => AsyncTexture2D.FromAssetId(536054),
            };
        }
    }

    public class StatSelectable : Panel
    {
        private readonly AsyncTexture2D _textureVignette = AsyncTexture2D.FromAssetId(605003);
        private Rectangle _vignetteBounds;

        private readonly bool _created;
        private readonly Image _icon;
        private readonly Label _name;
        private readonly Label _statSummary;

        private double _attributeAdjustment;
        private Stat _stat;

        public StatSelectable()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            BorderWidth = new(2);
            BorderColor = Color.Black;
            BackgroundColor = Color.Black * 0.4F;
            ContentPadding = new(5);

            _name = new()
            {
                Parent = this,
                WrapText = false,
                AutoSizeHeight = true,
                //BackgroundColor = Color.Blue * 0.2F,
                Font = Content.DefaultFont16,
                TextColor = Colors.ColonialWhite,
            };

            _statSummary = new()
            {
                Parent = this,
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Top,
                //BackgroundColor = Color.White * 0.2F,
            };

            _icon = new()
            {
                Parent = this,
                Size = new(48),
                Location = new(2, 2),
            };

            _created = true;
        }

        public Action<Stat> OnStatSelected;

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public double AttributeAdjustment { get => _attributeAdjustment; set => Common.SetProperty(ref _attributeAdjustment, value, OnMultiplierChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

            _name?.SetSize(ContentRegion.Width - _icon.Width - 10, _name.Font.LineHeight);
            _name?.SetLocation(_icon.Right + 10, _icon.Top);

            _statSummary?.SetLocation(_name.Left, _name.Bottom);
            _statSummary?.SetSize(_name.Width, ContentRegion.Height - _name.Height);

            _vignetteBounds = _icon.LocalBounds.Add(0, 0, 10, 10);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, Textures.Pixel, _vignetteBounds, Rectangle.Empty, Color.Gray * 0.3F, 0F, Vector2.Zero);
            spriteBatch.DrawOnCtrl(this, _textureVignette, _vignetteBounds, _textureVignette.Bounds, Color.Black, 0F, Vector2.Zero);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnStatSelected?.Invoke(Stat);
        }

        private void OnStatChanged()
        {
            _name.Text = _stat?.Name;
            _statSummary.Text = string.Join(Environment.NewLine, _stat?.Attributes.Values.Where(e => e != null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * _attributeAdjustment))} {e.Id.GetDisplayName()}"));
            _icon.Texture = _stat?.Icon;
        }

        private void OnMultiplierChanged()
        {
            _statSummary.Text = string.Join(Environment.NewLine, _stat?.Attributes.Values.Where(e => e != null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * _attributeAdjustment))} {e.Id.GetDisplayName()}"));
        }
    }

    public class StatSelection : BaseSelection
    {
        private readonly List<AttributeToggle> _statIcons = new();
        private readonly List<StatSelectable> _stats = new();
        private readonly bool _created;
        private Template _template;
        private BaseTemplateEntry _templateSlot;

        public StatSelection()
        {
            AttributeToggle t;
            int i = 0;
            int size = 25;
            Point start = new(0, 0);

            var stats = new List<AttributeType>()
            {
                AttributeType.Power,
                AttributeType.Toughness,
                AttributeType.Vitality,
                AttributeType.Healing,
                AttributeType.Precision,
                AttributeType.CritDamage,
                AttributeType.ConditionDamage,
                AttributeType.ConditionDuration,
                AttributeType.BoonDuration,
            };

            foreach (AttributeType stat in stats)
            {
                if (stat is AttributeType.Unknown or AttributeType.None or AttributeType.AgonyResistance) continue;

                int j = 0;

                _statIcons.Add(t = new()
                {
                    Parent = this,
                    Location = new(start.X + (i * (size + 16)), start.Y + (j * size)),
                    Size = new(size, size),
                    Attribute = stat,
                    OnCheckChanged = (isChecked) => FilterStats(),
                    Checked = false,
                    BasicTooltipText = $"{stat.GetDisplayName()}",
                });

                i++;
            }

            StatSelectable selectable;
            foreach (var stat in BuildsManager.Data.Stats)
            {
                _stats.Add(selectable = new()
                {
                    Parent = SelectionContent,
                    Width = SelectionContent.Width - 35,
                    Stat = stat.Value,
                    OnStatSelected = (selectedStat) => (TemplateSlot as GearTemplateEntry).Stat = selectedStat,
                });
            }

            Search.PerformFiltering = FilterStats;
            Search.SetLocation(Search.Left, Search.Top + 30);
            SelectionContent.SetLocation(Search.Left, Search.Bottom + 5);
            FilterStats();

            _created = true;
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public BaseTemplateEntry TemplateSlot
        {
            get => _templateSlot;
            set => Common.SetProperty(ref _templateSlot, value, () =>
            {
                FilterStats(null);

                double exoticTrinkets = 0;

                foreach (var stat in _stats)
                {
                    stat.AttributeAdjustment = _templateSlot?.Item != null ? ((_templateSlot?.Item as EquipmentItem).AttributeAdjustment + exoticTrinkets) : 0;
                }
            });
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

        }

        protected override void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnSelectionContent_Resized(sender, e);

            Search?.SetSize(SelectionContent.Width - 5);

            foreach (var stat in _stats)
            {
                stat.Width = SelectionContent.Width - 35;
            }
        }

        private void FilterStats(string? txt = null)
        {
            txt ??= Search.Text;
            string searchTxt = txt.Trim().ToLower();
            bool anyName = searchTxt.IsNullOrEmpty();

            var validStats = TemplateSlot?.Item != null ? (TemplateSlot?.Item as EquipmentItem).StatChoices : new List<int>();
            bool anyStat = validStats.Count == 0;

            bool anyAttribute = !_statIcons.Any(e => e.Checked);
            var attributes = _statIcons.Where(e => e.Checked).Select(e => e.Attribute);

            foreach (var stat in _stats)
            {
                var statAttributes = stat.Stat.Attributes.Select (e => e.Value.Id);

                stat.Visible =
                    (anyAttribute || attributes.All(e => statAttributes.Contains(e))) &&
                    validStats.Contains(stat.Stat.Id) &&
                    (anyName || stat.Stat?.Name.ToLower().Trim().Contains(searchTxt) == true);
            }

            SelectionContent.SortChildren<StatSelectable>((a, b) => a.Stat.Name.CompareTo(b.Stat.Name));
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void ApplyTemplate()
        {
            FilterStats();
        }
    }
}
