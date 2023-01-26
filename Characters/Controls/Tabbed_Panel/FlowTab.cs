using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class FlowTab : PanelTab
    {
        private ControlFlowDirection _flowDirection = ControlFlowDirection.LeftToRight;
        private Vector2 _outerControlPadding = Vector2.Zero;
        private Vector2 _controlPadding = Vector2.Zero;

        public ControlFlowDirection FlowDirection
        {
            get => _flowDirection;
            set => SetProperty(ref _flowDirection, value, true);
        }

        public Vector2 ControlPadding
        {
            get => _controlPadding;
            set => SetProperty(ref _controlPadding, value, true);
        }

        public Vector2 OuterControlPadding
        {
            get => _outerControlPadding;
            set => SetProperty(ref _outerControlPadding, value, true);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            ReflowChildLayout(_children.ToArray());
        }

        private void ReflowChildLayout(IEnumerable<Control> allChildren)
        {
            IEnumerable<Control> filteredChildren = allChildren.Where(c => c.GetType() != typeof(Scrollbar) && c.Visible);

            switch (_flowDirection)
            {
                case ControlFlowDirection.LeftToRight:
                    ReflowChildLayoutLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.RightToLeft:
                    ReflowChildLayoutRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.TopToBottom:
                    ReflowChildLayoutTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.BottomToTop:
                    ReflowChildLayoutBottomToTop(filteredChildren);
                    break;
                case ControlFlowDirection.SingleLeftToRight:
                    ReflowChildLayoutSingleLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.SingleRightToLeft:
                    ReflowChildLayoutSingleRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.SingleTopToBottom:
                    ReflowChildLayoutSingleTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.SingleBottomToTop:
                    ReflowChildLayoutSingleBottomToTop(filteredChildren);
                    break;
            }
        }

        private void ReflowChildLayoutLeftToRight(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastRight = outerPadX;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (child.Width >= ContentRegion.Width - lastRight)
                {
                    currentBottom = nextBottom + _controlPadding.Y;
                    lastRight = outerPadX;
                }

                child.Location = new Point((int)lastRight, (int)currentBottom);

                lastRight = child.Right + _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastLeft = ContentRegion.Width - outerPadX;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (outerPadX > lastLeft - child.Width)
                {
                    currentBottom = nextBottom + _controlPadding.Y;
                    lastLeft = ContentRegion.Width - outerPadX;
                }

                child.Location = new Point((int)(lastLeft - child.Width), (int)currentBottom);

                lastLeft = child.Left - _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastBottom = outerPadY;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (child.Height >= Height - lastBottom)
                {
                    currentRight = nextRight + _controlPadding.X;
                    lastBottom = outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)lastBottom);

                lastBottom = child.Bottom + _controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastTop = Height - outerPadY;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (outerPadY > lastTop - child.Height)
                {
                    currentRight = nextRight + _controlPadding.X;
                    lastTop = Height - outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)(lastTop - child.Height));

                lastTop = child.Top - _controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutSingleLeftToRight(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float lastLeft = outerPadX;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)lastLeft, (int)outerPadY);

                lastLeft = child.Right + _controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float lastLeft = ContentRegion.Width - outerPadX;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)(lastLeft - child.Width), (int)outerPadY);

                lastLeft = child.Left - _controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float lastBottom = outerPadY;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)lastBottom);

                lastBottom = child.Bottom + _controlPadding.Y;
            }
        }

        private void ReflowChildLayoutSingleBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float lastTop = Height - outerPadY;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)(lastTop - child.Height));

                lastTop = child.Top - _controlPadding.Y;
            }
        }
    }
}
