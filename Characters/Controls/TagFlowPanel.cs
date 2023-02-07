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

        private Rectangle CalculateTagPanelSize(int? width = null, bool fitLargest = false)
        {
            var tags = Tags;
            if (tags.Count == 0) return Rectangle.Empty;

            tags = tags.OrderByDescending(e => e.Width).ThenBy(e => e.Text).ToList();
            var added = new List<Tag>();

            int widest = Tags.Count > 0 ? Tags.Max(e => e.Width) : 0;
            widest += (int)OuterControlPadding.X + AutoSizePadding.X;

            width ??= widest;
            width = Math.Max(widest, (int)width);

            int height = 0;
            int curWidth = 0;

            int index = 0;
            var last = tags.LastOrDefault();
            foreach (var t in tags)
            {
                height = height == 0 ? t.Height : height;

                if (!added.Contains(t))
                {
                    t.TagPanelIndex = index;

                    curWidth += t.Width + (int)ControlPadding.X;

                    if (curWidth + 25 < width)
                    {
                        foreach (var e in tags)
                        {
                            if (e != t && !added.Contains(e) && e.Width + (int)ControlPadding.X + curWidth <= width)
                            {
                                curWidth += e.Width + (int)ControlPadding.X;
                                index++;
                                e.TagPanelIndex = index;
                                added.Add(e);

                                if (curWidth + 25 >= width)
                                {
                                    curWidth = 0;
                                    height += t.Height + (int)ControlPadding.Y;
                                    break;
                                }
                            }
                        }
                    }
                    else if(t != last)
                    {
                        height += t.Height + (int)ControlPadding.Y;
                        curWidth = 0;
                    }

                    added.Add(t);
                    index++;
                }
            }

            return new Rectangle(Location, new((int)width, height + (int)(OuterControlPadding.Y + AutoSizePadding.Y)));
        }

        public void FitWidestTag(int? width = null)
        {
            Rectangle bounds = CalculateTagPanelSize(width);
            SortChildren<Tag>((a, b) => a.TagPanelIndex.CompareTo(b.TagPanelIndex));

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