using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class BaseSelection : Panel
    {
        protected readonly FilterBox Search;
        protected readonly FlowPanel SelectionContent;

        public BaseSelection()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;

            Search = new()
            {
                Parent = this,
                Location = new(2, 0),
                PlaceholderText = "Search . . .",
            };

            SelectionContent = new()
            {
                Parent = this,
                Location = new(0, Search.Bottom + 5),
                ShowBorder = true,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                CanScroll = true,
                ShowRightBorder = true,
                ControlPadding = new(5),
                OuterControlPadding = new(5),
            };
            SelectionContent.Resized += SelectionContent_Resized;
        }

        private void SelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            if (SelectionContent != null)
            {
                foreach (var child in SelectionContent.Children)
                {
                    //child.Width = SelectionContent.Width - 30;
                }
            }
        }

        public Rectangle SelectionBounds => new(SelectionContent.LocalBounds.Location, SelectionContent.ContentRegion.Size);

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Search != null) Search.Width = Width - Search.Left;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            SelectionContent.Resized -= SelectionContent_Resized;

            SelectionContent?.Dispose();
            Search?.Dispose();
        }
    }
}
