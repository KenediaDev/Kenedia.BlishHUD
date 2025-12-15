using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Stats;
using Kenedia.Modules.AdvancedBuildsManager.Extensions;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.Selection
{
    public class SkillSelectable : Panel, INotifyPropertyChanged
    {
        private readonly AsyncTexture2D _textureVignette = AsyncTexture2D.FromAssetId(605003);
        private Rectangle _vignetteBounds;
        private readonly bool _created;

        public SkillSelectable()
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
                Enabled = false,
            };

            _skillSummary = new()
            {
                Parent = this,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Top,
                AutoSizeHeight = true,
                //BackgroundColor = Color.White * 0.2F,
                Enabled = false,
            };

            _icon = new()
            {
                Parent = this,
                Size = new(48),
                Location = new(2, 2),
                Enabled = false,
            };

            _created = true;
        }

        public Template Template { get; set; }

        private readonly Image _icon;
        private readonly Label _name;
        private readonly Label _skillSummary;

        public Skill Skill { get; set => Common.SetProperty(field, value, v => field = v, OnSkillChanged); }

        public Func<RotationElement> GetRotationElement { get; set; }

        private void OnSkillChanged()
        {
            _name.Text = Skill?.Name;
            _skillSummary.Text = Skill?.Description;

            _icon.Texture = Skill?.Icon;

            OnPropertyChanged(nameof(Skill));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, Textures.Pixel, _vignetteBounds, Rectangle.Empty, Color.Gray * 0.3F, 0F, Vector2.Zero);
            spriteBatch.DrawOnCtrl(this, _textureVignette, _vignetteBounds, _textureVignette.Bounds, Color.Black, 0F, Vector2.Zero);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

            _name?.SetSize(ContentRegion.Width - _icon.Width - 10, _name.Font.LineHeight);
            _name?.SetLocation(_icon.Right + 10, _icon.Top);

            _skillSummary?.SetLocation(_name.Left, _name.Bottom);
            _skillSummary?.SetSize(_name.Width, ContentRegion.Height - _name.Height);

            _vignetteBounds = _icon.LocalBounds.Add(0, 0, 10, 10);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            var element = GetRotationElement();

            if (element is not null)
            {
                element.Skill = Skill;
            }
        }
    }

    public class StatSelectable : Panel
    {
        private readonly AsyncTexture2D _textureVignette = AsyncTexture2D.FromAssetId(605003);
        private Rectangle _vignetteBounds;
        private readonly Blish_HUD.Controls.ContextMenuStripItem _applyAll;
        private readonly Blish_HUD.Controls.ContextMenuStripItem _applyGroup;
        private readonly bool _created;
        private readonly Image _icon;
        private readonly Label _name;
        private readonly Label _statSummary;

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

            Menu = new();
            _applyAll = Menu.AddMenuItem("Apply to all");
            _applyAll.Click += ApplyAll_Click;

            _applyGroup = Menu.AddMenuItem("Apply to item group");
            _applyGroup.Click += ApplyGroup_Click;

            _created = true;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            (ActiveTemplateSlot as GearTemplateEntry).Stat = Stat;
        }

        private void ApplyGroup_Click(object sender, MouseEventArgs e)
        {
            var slots =
                (ActiveTemplateSlot as GearTemplateEntry).Slot.IsArmor() ? Template?.GearTemplate?.Armors.Values.Cast<GearTemplateEntry>() :
                (ActiveTemplateSlot as GearTemplateEntry).Slot.IsWeapon() ? Template?.GearTemplate?.Weapons.Values.Cast<GearTemplateEntry>() :
                (ActiveTemplateSlot as GearTemplateEntry).Slot.IsJewellery() ? Template?.GearTemplate?.Jewellery.Values.Cast<GearTemplateEntry>() : null;

            foreach (var slot in slots)
            {
                slot.Stat = Stat;
            }
        }

        private void ApplyAll_Click(object sender, MouseEventArgs e)
        {
            List<GearTemplateEntry> slots =
            [
                .. Template?.GearTemplate?.Armors.Values.Cast<GearTemplateEntry>(),
                .. Template?.GearTemplate?.Weapons.Values.Cast<GearTemplateEntry>(),
                .. Template?.GearTemplate?.Jewellery.Values.Cast<GearTemplateEntry>(),
            ];

            foreach (var slot in slots)
            {
                slot.Stat = Stat;
            }
        }

        public Stat Stat { get; set => Common.SetProperty(field, value, v => field = v, OnStatChanged); }

        public double AttributeAdjustment { get; set => Common.SetProperty(field, value, v => field = v, OnMultiplierChanged); }

        public BaseTemplateEntry ActiveTemplateSlot { get; set; }

        public Template Template { get; set; }

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

        private void OnStatChanged()
        {
            _name.Text = Stat?.Name;
            _statSummary.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e is not null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * AttributeAdjustment))} {e.Id.GetDisplayName()}"));
            _icon.Texture = Stat?.Icon;
        }

        private void OnMultiplierChanged()
        {
            _statSummary.Text = string.Join(Environment.NewLine, Stat?.Attributes.Values.Where(e => e is not null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * AttributeAdjustment))} {e.Id.GetDisplayName()}"));
        }
    }
}
