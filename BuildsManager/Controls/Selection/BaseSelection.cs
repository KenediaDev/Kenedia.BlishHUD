using Kenedia.Modules.Core.Controls;

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
            };
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Search != null) Search.Width = Width - Search.Left;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            SelectionContent?.Dispose();
            Search?.Dispose();
        }
    }
}
