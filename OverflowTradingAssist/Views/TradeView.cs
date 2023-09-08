using Blish_HUD.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.OverflowTradingAssist.Services;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.OverflowTradingAssist.Res;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.OverflowTradingAssist.Controls.GearPage;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.Core.Models;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;

namespace Kenedia.Modules.OverflowTradingAssist.Views
{

    public class TradeView : View
    {
        private Button _mailButton;
        private FilterBox _filterBox;
        private FlowPanel _itemPanel;
        private TradePresenter _trade;
        private readonly MailingService _mailingService;

        public TradeView(MailingService mailingService, TradePresenter _tradePresenter)
        {
            _mailingService = mailingService;        
            Trade = _tradePresenter;
        }

        public TradePresenter Trade { get => _trade; private set => Common.SetProperty(ref _trade , value, ApplyTrade); }

        private void ApplyTrade(object sender, ValueChangedEventArgs<TradePresenter> e)
        {

        }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);

            _filterBox = new()
            {
                Parent = buildPanel,
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
                Parent = buildPanel,
                Location = new(10, _filterBox.Bottom + 5),
                Width = 300,
                HeightSizingMode = SizingMode.Fill,
                BackgroundColor = Color.Black * 0.5F,
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ControlPadding = new(4, 4),
                ContentPadding = new(4),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
            };

            _mailButton = new()
            {
                Parent = buildPanel,
                Location = new(_itemPanel.Right + 10, _itemPanel.Top),
                Width = 150,
                Height = 30,
                Text = $"Send {strings.Mail}",
                ClickAction = () =>
                {
                    _mailingService?.SendMail();
                },
            };

            SearchForTopResults(string.Empty);
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
