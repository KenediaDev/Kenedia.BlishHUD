using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Blish_HUD.ContentService;

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
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }

        private void ApplySkill(object sender, ValueChangedEventArgs<Skill> e)
        {
            _title.TextColor = Colors.Chardonnay;
            _title.Text = e.NewValue?.Name;
            _id.Text = $"Skill Id: {e.NewValue?.Id}";
            _description.Text = e.NewValue?.Description.InterpretItemDescription();

            if (e.NewValue != null)
            {
                _image.Texture = e.NewValue.Icon;
            }
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
    }
}
