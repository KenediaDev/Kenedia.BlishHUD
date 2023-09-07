using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Services;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillTooltip : Blish_HUD.Controls.Tooltip
    {
        private readonly DetailedTexture _image = new() { TextureRegion = new(14, 14, 100, 100), };
        private readonly Label _title;
        private readonly Label _id;
        private readonly Label _description;

        private Skill _skill;

        public SkillTooltip()
        {
            WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            AutoSizePadding = new(5);

            Rectangle imageBounds;
            _image.Bounds = imageBounds = new(4, 4, 48, 48);

            _title = new()
            {
                Parent = this,
                Height = Content.DefaultFont16.LineHeight,
                AutoSizeWidth = true,
                Location = new(imageBounds.Right, imageBounds.Top),
                Font = Content.DefaultFont16,
            };

            _id = new()
            {
                Parent = this,
                Height = Content.DefaultFont12.LineHeight,
                AutoSizeWidth = true,
                Location = new(imageBounds.Right, _title.Bottom),
                Font = Content.DefaultFont12,
                TextColor = Color.White * 0.8F,
            };

            _description = new()
            {
                Parent = this,
                Width = 300,
                AutoSizeHeight = true,
                Location = new(imageBounds.Left, imageBounds.Bottom + 10),
                Font = Content.DefaultFont14,
                WrapText = true,
            };

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }

        private void ApplySkill(object sender, ValueChangedEventArgs<Skill> e)
        {
            _title.TextColor = Colors.Chardonnay;
            _title.Text = Skill?.Name;
            _id.Text = $"{strings.SkillId}: {Skill?.Id}";
            _description.Text = Skill?.Description.InterpretItemDescription();
            _image.Texture = Skill?.Icon;
        }

        private void UserLocale_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            ApplySkill(this, null);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            if (Skill == null) return;
            base.Draw(spriteBatch, drawBounds, scissor);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _image.Draw(this, spriteBatch);
        }

        protected override void DisposeControl()
        {
            Skill = null;
            _image.Texture = null;

            base.DisposeControl();
        }
    }
}
