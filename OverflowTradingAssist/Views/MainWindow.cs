using Blish_HUD.Content;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Kenedia.Modules.Core.Extensions;
using System.Collections.Generic;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using System.Diagnostics;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using System.Threading.Tasks;
using Kenedia.Modules.OverflowTradingAssist.Controls;
using Kenedia.Modules.Core.Res;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly FilterBox _filterBox;
        private readonly FlowPanel _itemPanel;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            _filterBox = new()
            {
                Parent = this,
                Location = new(10, 10),
                Width = 300,
                FilteringOnTextChange = true,
                FilteringOnEnter = true,
                TextChangedAction = SearchForTopResults,
                FilteringDelay = 500,
                PlaceholderText = strings_common.Search,
            };

            _itemPanel = new()
            {
                Parent = this,
                Location = new(10, _filterBox.Bottom + 5),
                Width = 300,
                Height = windowRegion.Height + 150,
                BackgroundColor = Color.Black * 0.5F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ControlPadding = new(4, 4),
                ContentPadding = new(4),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
            };
        }

        private void SearchForTopResults(string s)
        {
            if (OverflowTradingAssist.Data.IsLoaded)
            {
                _ = Task.Run(() =>
                {
                    var topItems = GetBestMatches(OverflowTradingAssist.Data.Items.Items, s, 50);
                    _itemPanel.ClearChildren();

                    if (topItems != null && topItems.Count > 0)
                    {
                        foreach (Item item in topItems)
                        {
                            var itemControl = new ItemControl()
                            {
                                Item = item,
                                Parent = _itemPanel,
                                Height = 32,
                                Width = _itemPanel.ContentRegion.Width,
                                //WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                            };
                        }
                    }
                });
            }
        }

        private List<Item> GetBestMatches(List<Item> stringList, string searchString, int numMatches)
        {
            var bestMatches = new List<Item>();
            var matchDistances = new Dictionary<Item, int>();
            var matchDistancesX = new Dictionary<string, int>();

            foreach (Item item in stringList)
            {
                if (item?.Name?.ToLower().Contains(searchString.ToLower()) == true)
                {
                    int distance = searchString.LevenshteinDistance(item.Name);
                    matchDistances[item] = distance;
                }
            }

            var sortedMatches = matchDistances.OrderBy(kvp => kvp.Value);
            bestMatches.AddRange(sortedMatches.Take(numMatches).Select(kvp => kvp.Key));

            return bestMatches;
        }
    }
}
