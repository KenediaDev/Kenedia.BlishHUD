using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Extensions;
using System;
using System.Linq;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Kenedia.Modules.Core.Res;
using System.Threading.Tasks;
using System.Collections.Generic;
using Blish_HUD.Content;
using Blish_HUD.Input;

namespace Kenedia.Modules.OverflowTradingAssist.Controls.GearPage
{
    public class ItemSelection : Panel
    {
        private readonly FilterBox _filterBox;
        private readonly FlowPanel _itemPanel;

        public ItemSelection()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
            //BackgroundColor = Color.Black * 0.2F;
            BorderWidth = new(2);
            BorderColor = Color.Black;
            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            TextureRectangle = new(50, 50, BackgroundImage.Width - 150, BackgroundImage.Height - 150);
            ContentPadding = new(10);

            _filterBox = new()
            {
                Parent = this,
                Location = new(0, 0),
                Width = 300,
                FilteringOnTextChange = true,
                FilteringOnEnter = true,
                TextChangedAction = SearchForTopResults,
                FilteringDelay = 500,
                PlaceholderText = strings_common.Search,
                Focused = true,
            };

            _itemPanel = new()
            {
                Parent = this,
                Location = new(0, _filterBox.Bottom + 5),
                Width = 300,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                BackgroundColor = Color.Black * 0.5F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ControlPadding = new(4, 4),
                ContentPadding = new(4),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
            };

            Width = 320;
            Height = 500;

            Input.Mouse.LeftMouseButtonPressed += OnMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += OnMouseButtonPressed;
        }

        private void OnMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!AbsoluteBounds.Contains(e.MousePosition))
            {
                Hide();
            }
        }

        public Action<Item> OnItemSelected { get; set; }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            SearchForTopResults(_filterBox.Text ?? string.Empty);
            
            if(_filterBox is not null)
                _filterBox.Focused = true;
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
                                OnClicked = (ctrlItem) =>
                                {
                                    OnItemSelected?.Invoke(ctrlItem);
                                },
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

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_filterBox is not null)
            {
                _filterBox.Width = ContentRegion.Width;
            }

            if (_itemPanel is not null)
            {
                _itemPanel.Width = ContentRegion.Width;
            }
        }
    }
}
