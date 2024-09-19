using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Services;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Extensions;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillTooltipContentControl : Control
    {
        private readonly DetailedTexture _image = new() { TextureRegion = new(14, 14, 100, 100), };

        private readonly Label _title;
        private readonly Label _id;
        private readonly Label _description;

        public SkillTooltipContentControl(SkillSlotType skillSlot, TemplatePresenter templatePresenter)
        {
            SkillSlot = skillSlot;
            TemplatePresenter = templatePresenter;
            TemplatePresenter = templatePresenter;

            _image.Bounds = new(4, 4, 48, 48);

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            TemplatePresenter.SkillChanged += TemplatePresenter_SkillChanged;
            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            SkillSlot = skillSlot;

            Width = 300;
        }

        public SkillSlotType SkillSlot { get; }
        public TemplatePresenter TemplatePresenter { get; }
                
        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            SetSkill(TemplatePresenter.Template?[SkillSlot]);
        }

        public Color TitleColor { get; set; } = Colors.Chardonnay;

        private void TemplatePresenter_SkillChanged(object sender, SkillChangedEventArgs e)
        {
            if (e.Slot == SkillSlot)
            {
                SetSkill(e.Skill);
            }
        }

        private void SetSkill(Skill skill)
        {
            Title = skill?.Name;
            Id = $"{strings.SkillId}: {skill?.Id}";
            Description = skill?.Description.InterpretItemDescription();
            _image.Texture = skill?.Icon;

            Height = _image.Bounds.Bottom + 10 + UI.GetTextHeight(Content.DefaultFont14, Description, Width);

            RecalculateLayout();
        }

        public string? Title { get; private set; }

        public string? Id { get; private set; }

        public string? Description { get; private set; }

        public Rectangle TitleBounds { get; private set; }

        public Rectangle IdBounds { get; private set; }

        public Rectangle DescriptionBounds { get; private set; }

        private void UserLocale_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            SetSkill(TemplatePresenter.Template?[SkillSlot]);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Id))
                return;

            base.Draw(spriteBatch, drawBounds, scissor);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            TitleBounds = new Rectangle(_image.Bounds.Right + 5, _image.Bounds.Top, Width - _image.Bounds.Right - 5, Content.DefaultFont18.LineHeight);
            IdBounds = new Rectangle(_image.Bounds.Right + 5, TitleBounds.Bottom + 8, Width - _image.Bounds.Right - 5, Content.DefaultFont12.LineHeight);
            DescriptionBounds = new Rectangle(_image.Bounds.Left, _image.Bounds.Bottom + 10, Width - 10, Height - _image.Bounds.Bottom - 10);
        }

        protected override void DisposeControl()
        {
            _image.Texture = null;

            LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
            TemplatePresenter.SkillChanged -= TemplatePresenter_SkillChanged;
            TemplatePresenter.TemplateChanged -= TemplatePresenter_TemplateChanged;

            base.DisposeControl();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _image.Draw(this, spriteBatch);

            spriteBatch.DrawStringOnCtrl(this, Title, Content.DefaultFont18, TitleBounds, TitleColor);
            spriteBatch.DrawStringOnCtrl(this, Id, Content.DefaultFont12, IdBounds, Color.White);
            spriteBatch.DrawStringOnCtrl(this, Description, Content.DefaultFont14, DescriptionBounds, Color.White, true, HorizontalAlignment.Left, VerticalAlignment.Top);
        }
    }
}
