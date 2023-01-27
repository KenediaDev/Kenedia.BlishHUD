using Blish_HUD.Controls;
using Characters.Res;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Resources;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenuBehaviors : FlowTab, ILocalizable
    {
        private List<KeyValuePair<string, DisplayCheckToggle>> _toggles = new()
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
        private Rectangle _contentRectangle;

        public SideMenuBehaviors()
        {
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

            _toggleAll = new()
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
                DisplayCheckToggle ctrl = new(Settings, t.Key)
                {
                    Parent = this,
                };
                ctrl.Changed += Toggle_Changed;

                _toggles[i] = new(t.Key, ctrl);
            }

            Characters.ModuleInstance.LanguageChanged += OnLanguageChanged;
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
            Settings.ResultMatchingBehavior.Value = e.CurrentValue.GetMatchingBehavior();
        }

        private void FilterBehaviorDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Settings.ResultFilterBehavior.Value = e.CurrentValue.GetFilterBehavior();
        }

        private ResourceManager RM => Characters.ModuleInstance.RM;

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        private MainWindow MainWindow => Characters.ModuleInstance.MainWindow;

        private Characters ModuleInstance => Characters.ModuleInstance;

        private void FlowDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Settings.SortOrder.Value = e.CurrentValue.GetSortOrder();
            MainWindow?.SortCharacters();
        }

        private void OrderDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Settings.SortType.Value = e.CurrentValue.GetSortType();
            MainWindow?.SortCharacters();
        }

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            _orderDropdown.SelectedItem = Settings.SortType.Value.GetSortType();
            _orderDropdown.Items.Clear();
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Name));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Tags));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Profession));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.LastLogin));
            _orderDropdown.Items.Add(string.Format(strings.SortBy, strings.Map));
            _orderDropdown.Items.Add(strings.Custom);

            _flowDropdown.SelectedItem = Settings.SortOrder.Value.GetSortOrder();
            _flowDropdown.Items.Clear();
            _flowDropdown.Items.Add(strings.Ascending);
            _flowDropdown.Items.Add(strings.Descending);

            _matchingDropdown.SelectedItem = Settings.ResultMatchingBehavior.Value.GetMatchingBehavior();
            _matchingDropdown.Items.Clear();
            _matchingDropdown.Items.Add(strings.MatchAnyFilter);
            _matchingDropdown.Items.Add(strings.MatchAllFilter);

            _filterBehaviorDropdown.SelectedItem = Settings.ResultFilterBehavior.Value.GetFilterBehavior();
            _filterBehaviorDropdown.Items.Clear();
            _filterBehaviorDropdown.Items.Add(strings.IncludeMatches);
            _filterBehaviorDropdown.Items.Add(strings.ExcludeMatches);

            _toggleAll.Text = strings.ToggleAll;

            foreach (KeyValuePair<string, DisplayCheckToggle> t in _toggles)
            {
                t.Value.Text = RM.GetString(t.Key);
                t.Value.DisplayTooltip = string.Format(RM.GetString("ShowItem"), RM.GetString(t.Key));
                t.Value.CheckTooltip = string.Format(RM.GetString("CheckItem"), RM.GetString(t.Key));
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            ModuleInstance.LanguageChanged -= OnLanguageChanged;

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
