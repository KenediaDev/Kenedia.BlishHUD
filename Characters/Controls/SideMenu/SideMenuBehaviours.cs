using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Resources;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using Panel = Kenedia.Modules.Core.Controls.Panel;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenuBehaviors : FlowTab, ILocalizable
    {
        private readonly List<KeyValuePair<string, DisplayCheckToggle>> _toggles = new()
        {
            new( "Name", null),
            new( "Level", null),
            new( "Race", null),
            new( "Gender", null),
            new( "Profession", null),
            new( "LastLogin", null),
            new( "Map", null),
            new( "CraftingProfession", null),
            new( "OnlyMaxCrafting", null),
            new( "Tags", null),
        };
        private readonly Panel _separator;
        private readonly Dropdown _orderDropdown;
        private readonly Dropdown _flowDropdown;
        private readonly Dropdown _filterBehaviorDropdown;
        private readonly Dropdown _matchingDropdown;
        private readonly DisplayCheckToggle _toggleAll;
        private readonly ResourceManager _resourceManager;
        private readonly SettingsModel _settings;
        private readonly Action _onSortChanged;
        private Rectangle _contentRectangle;

        public SideMenuBehaviors(ResourceManager resourceManager, TextureManager textureManager, SettingsModel settings, Action onSortChanged)
        {
            _resourceManager = resourceManager;
            _settings = settings;
            _onSortChanged = onSortChanged;

            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            _orderDropdown = new() { Parent = this, };
            _orderDropdown.ValueChanged += OrderDropdown_ValueChanged;

            _flowDropdown = new() { Parent = this, };
            _flowDropdown.ValueChanged += FlowDropdown_ValueChanged;

            _filterBehaviorDropdown = new() { Parent = this, };
            _filterBehaviorDropdown.ValueChanged += FilterBehaviorDropdown_ValueChanged;

            _matchingDropdown = new() { Parent = this, };
            _matchingDropdown.ValueChanged += MatchingDropdown_ValueChanged;

            _toggleAll = new(textureManager)
            {
                Parent = this,                
            };
            _toggleAll.ShowChanged += All_ShowChanged;
            _toggleAll.CheckChanged += All_CheckChanged;

            _separator = new Panel()
            {
                BackgroundColor = Color.White * 0.6f,
                Height = 2,
                Parent = this,
            };

            for (int i = 0; i < _toggles.Count; i++)
            {
                KeyValuePair<string, DisplayCheckToggle> t = _toggles[i];
                DisplayCheckToggle ctrl = new(textureManager, _settings, t.Key)
                {
                    Parent = this,
                };
                ctrl.Changed += Toggle_Changed;

                _toggles[i] = new(t.Key, ctrl);
            }

            GameService.Overlay.UserLocale.SettingChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void All_CheckChanged(object sender, bool e)
        {
            foreach (KeyValuePair<string, DisplayCheckToggle> t in _toggles)
            {
                t.Value.CheckChecked = e;
            }
        }

        private void All_ShowChanged(object sender, bool e)
        {
            foreach (KeyValuePair<string, DisplayCheckToggle> t in _toggles)
            {
                t.Value.ShowChecked = e;
            }
        }

        private void Toggle_Changed(object sender, Tuple<bool, bool> e)
        {

        }

        private void MatchingDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            _settings.ResultMatchingBehavior.Value = e.CurrentValue.GetMatchingBehavior();
        }

        private void FilterBehaviorDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            _settings.ResultFilterBehavior.Value = e.CurrentValue.GetFilterBehavior();
        }

        private void FlowDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            _settings.SortOrder.Value = e.CurrentValue.GetSortOrder();
            _onSortChanged?.Invoke();
        }

        private void OrderDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            _settings.SortType.Value = e.CurrentValue.GetSortType();
            _onSortChanged?.Invoke();
        }

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            _orderDropdown.SelectedItem = _settings.SortType.Value.GetSortType();
            _orderDropdown.Items.Clear();
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Name));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Tags));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Profession));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.LastLogin));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Map));
            _orderDropdown.Items.Add(strings.Custom);

            _flowDropdown.SelectedItem = _settings.SortOrder.Value.GetSortOrder();
            _flowDropdown.Items.Clear();
            _flowDropdown.Items.Add(strings.Ascending);
            _flowDropdown.Items.Add(strings.Descending);

            _matchingDropdown.SelectedItem = _settings.ResultMatchingBehavior.Value.GetMatchingBehavior();
            _matchingDropdown.Items.Clear();
            _matchingDropdown.Items.Add(strings.MatchAnyFilter);
            _matchingDropdown.Items.Add(strings.MatchAllFilter);

            _filterBehaviorDropdown.SelectedItem = _settings.ResultFilterBehavior.Value.GetFilterBehavior();
            _filterBehaviorDropdown.Items.Clear();
            _filterBehaviorDropdown.Items.Add(strings.IncludeMatches);
            _filterBehaviorDropdown.Items.Add(strings.ExcludeMatches);

            _toggleAll.Text = strings.ToggleAll;

            foreach (KeyValuePair<string, DisplayCheckToggle> t in _toggles)
            {
                t.Value.Text = _resourceManager.GetString(t.Key);
                t.Value.DisplayTooltip = string.Format(_resourceManager.GetString("ShowItem"), _resourceManager.GetString(t.Key));
                t.Value.CheckTooltip = string.Format(_resourceManager.GetString("CheckItem"), _resourceManager.GetString(t.Key));
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= OnLanguageChanged;

            _orderDropdown.ValueChanged -= OrderDropdown_ValueChanged;
            _flowDropdown.ValueChanged -= FlowDropdown_ValueChanged;
            _filterBehaviorDropdown.ValueChanged -= FilterBehaviorDropdown_ValueChanged;
            _matchingDropdown.ValueChanged -= MatchingDropdown_ValueChanged;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            _contentRectangle = new Rectangle((int)OuterControlPadding.X, (int)OuterControlPadding.Y, Width - ((int)OuterControlPadding.X * 2), Height - ((int)OuterControlPadding.Y * 2));
            _orderDropdown.Width = _contentRectangle.Width;
            _flowDropdown.Width = _contentRectangle.Width;
            _filterBehaviorDropdown.Width = _contentRectangle.Width;
            _matchingDropdown.Width = _contentRectangle.Width;
            _separator.Width = _contentRectangle.Width;
        }
    }
}
