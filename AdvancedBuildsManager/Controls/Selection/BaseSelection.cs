﻿using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.Selection
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
                FilteringOnTextChange = true,
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
                ContentPadding = new(5),                
            };
            SelectionContent.Resized += OnSelectionContent_Resized;                       
        }

        protected virtual void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            if (SelectionContent == null) return;
        }

        public Rectangle SelectionBounds => new(SelectionContent.LocalBounds.Location, SelectionContent.ContentRegion.Size);

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            SelectionContent.Resized -= OnSelectionContent_Resized;

            SelectionContent?.Dispose();
            Search?.Dispose();
        }
    }
}
