using Blish_HUD;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagSelectable : Selectable<TemplateTag>
    {
        protected Rectangle PriorityTextBounds;
        protected Rectangle GroupTextBounds;

        public TemplateTag Tag => Item;

        public TemplateTags TemplateTags { get; }

        public TagSelectable(TemplateTag tag, Blish_HUD.Controls.Container parent, TemplateTags templateTags) : base(tag, parent)
        {
            TemplateTags = templateTags;

            Height = 40;

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings.Delete, () => RemoveTag(Tag)));
        }

        private void RemoveTag(TemplateTag tag)
        {
            TemplateTags.Remove(Tag);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = Height - (Content.DefaultFont14.LineHeight + 3 + Content.DefaultFont12.LineHeight);
            ContentBounds = new(5, padding / 2, Width - 30, Height - padding);

            IconBounds = new(ContentBounds.Left, ContentBounds.Top + ((ContentBounds.Height - 25) / 2), 25, 25);

            PriorityTextBounds = new(Width - 5, ContentBounds.Top, Content.DefaultFont12.LetterSpacing * 2, Content.DefaultFont12.LineHeight);

            TextBounds = new(IconBounds.Right + 5, ContentBounds.Top, ContentBounds.Width - IconBounds.Width - 5, Content.DefaultFont14.LineHeight);
            GroupTextBounds = new(IconBounds.Right + 5, ContentBounds.Bottom - Content.DefaultFont14.LineHeight, ContentBounds.Width - IconBounds.Width - 5, Content.DefaultFont12.LineHeight);
        }

        protected override void DrawItem(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Tag.Icon.Texture is not null)
            {
                //Tag.Icon.Draw(this, spriteBatch);
                spriteBatch.DrawOnCtrl(this, Tag.Icon.Texture, IconBounds, Tag.TextureRegion);
            }

            spriteBatch.DrawStringOnCtrl(this, string.Format("{1}", Tag.Priority, Tag.Name), Content.DefaultFont14, TextBounds, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
            spriteBatch.DrawStringOnCtrl(this, string.Format("{0}", string.IsNullOrEmpty(Tag.Group) ? TagGroup.DefaultName : Tag.Group), Content.DefaultFont12, GroupTextBounds, Color.Gray, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
            spriteBatch.DrawStringOnCtrl(this, string.Format("{0}", Tag.Priority), Content.DefaultFont12, PriorityTextBounds, Color.Gray, false, Blish_HUD.Controls.HorizontalAlignment.Right, Blish_HUD.Controls.VerticalAlignment.Middle);
        }
    }
}
