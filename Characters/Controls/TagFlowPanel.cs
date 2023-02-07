using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.Characters.Controls
{
    public class TagFlowPanel : FontFlowPanel
    {
        public TagFlowPanel()
        {
        }

        private List<Tag> Tags => Children.Cast<Tag>().ToList();

        private Rectangle CalculateTagPanelSize(int? width = null)
        {
            var tags = Tags;
            if (tags.Count == 0) return Rectangle.Empty;

            int widest = Tags.Count > 0 ? Tags.Max(e => e.Width) : 0;
            widest += (int)OuterControlPadding.X * 2;

            width ??= widest;
            width = Math.Max(widest, (int) width);

            int height = 0;

            int curWidth = 0;
            foreach (Tag tag in Tags)
            {
                if (tag.Visible)
                {
                    int newWidth = curWidth + tag.Width + (int)ControlPadding.X;

                    if (newWidth > width || height == 0)
                    {
                        height += tag.Height + (int)ControlPadding.Y;
                        curWidth = 0;                        
                    }

                    curWidth += tag.Width + (int)ControlPadding.X;
                }
            }

            return new Rectangle(Location, new((int) width, height + (int)(OuterControlPadding.Y * 2)));
        }

        public void FitWidestTag(int? width = null)
        {
            Rectangle bounds = CalculateTagPanelSize(width);
            Height = bounds.Height;
            Width = bounds.Width;
        }

        public override void Invalidate()
        {
            base.Invalidate();
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);

            Rectangle bounds = CalculateTagPanelSize();
            Height = bounds.Height;
            Width = bounds.Width;
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            base.OnChildRemoved(e);

            Rectangle bounds = CalculateTagPanelSize();
            Height = bounds.Height;
            Width = bounds.Width;
        }

        protected override void OnFontChanged(object sender = null, EventArgs e = null)
        {
            base.OnFontChanged(sender, e);

            Rectangle bounds = CalculateTagPanelSize();
            Height = bounds.Height;
            Width = bounds.Width;
        }
    }
}
