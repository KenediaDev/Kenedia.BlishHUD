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
    public class GroupSelectable : Selectable<TagGroup>
    {
        protected Rectangle PriorityTextBounds;

        public TagGroup Group => Item;

        public TagGroups TagGroups { get; }

        public GroupSelectable(TagGroup tagGroup, Blish_HUD.Controls.Container parent, TagGroups tagGroups) : base(tagGroup, parent)
        {
            TagGroups = tagGroups;

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings.Delete, () => RemoveTag(Group)));
        }

        protected override void DrawItem(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //if (Group.Icon.Texture is not null)
            //{
            //    spriteBatch.DrawOnCtrl(this, Group.Icon.Texture, IconBounds);
            //}

            spriteBatch.DrawStringOnCtrl(this, string.Format("{1}", Group.Priority, Group.Name), Content.DefaultFont14, TextBounds, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
            spriteBatch.DrawStringOnCtrl(this, string.Format("{0}", Group.Priority), Content.DefaultFont12, PriorityTextBounds, Color.Gray, false, Blish_HUD.Controls.HorizontalAlignment.Right, Blish_HUD.Controls.VerticalAlignment.Middle);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = Height - (Content.DefaultFont14.LineHeight + 3 + Content.DefaultFont12.LineHeight);
            ContentBounds = new(5, padding / 2, Width - 30, Height - padding);

            PriorityTextBounds = new(Width - 5, ContentBounds.Top + 5, Content.DefaultFont12.LetterSpacing * 2, Content.DefaultFont12.LineHeight);
            TextBounds = new(ContentBounds.Left, ContentBounds.Top, ContentBounds.Width - 5 - PriorityTextBounds.Width, ContentBounds.Height);
        }

        private void RemoveTag(TagGroup group)
        {
            TagGroups.Remove(group);
        }
    }
}
