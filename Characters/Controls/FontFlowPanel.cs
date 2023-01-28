using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Interfaces;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
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

    public class FontFlowPanel : FlowPanel, IFontControl
    {
        private BitmapFont _font;

        public BitmapFont Font
        {
            get => _font;
            set
            {
                if (_font != value && value != null)
                {
                    _font = value;
                    OnFontChanged();
                }
            }
        }

        public string Text
        {
            get
            {
                var ctrl = (IFontControl)Children.FirstOrDefault(e => e is IFontControl);
                return ctrl?.Text;
            }
            set
            {
            }
        }

        protected virtual void OnFontChanged(object sender = null, EventArgs e = null)
        {
            foreach (IFontControl ctrl in Children)
            {
                ctrl.Font = Font;
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);

            if (Font != null && e.ChangedChild.GetType() == typeof(IFontControl))
            {
                (e.ChangedChild as IFontControl).Font = Font;
            }
        }
    }
}
