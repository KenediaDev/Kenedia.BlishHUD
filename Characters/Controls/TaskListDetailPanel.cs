using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Button = Kenedia.Modules.Core.Controls.Button;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using TextBox = Blish_HUD.Controls.TextBox;

namespace Kenedia.Modules.Characters.Controls
{
    public class TaskListDetailPanel : FlowPanel
    {
        private readonly TaskListService _service;
        private readonly ObservableCollection<Character_Model> _characterModels;
        private readonly List<string> _allCharacterNames;

        private readonly Label _placeholderLabel;
        private readonly FlowPanel _contentPanel;
        private readonly TextBox _nameBox;
        private readonly Dropdown _resetDropdown;
        private readonly TextBox _characterNameBox;
        private readonly Dropdown _characterDropdown;
        private readonly TextBox _taskDescBox;
        private readonly TaskEntriesPanel _entriesPanel;

        private TaskListModel _boundList;
        private bool _syncingListMetadata;
        private bool _syncingCharacterControls;

        public TaskListDetailPanel(TaskListService service, Settings settings, ObservableCollection<Character_Model> characterModels, int width)
        {
            _service = service;
            _characterModels = characterModels;
            _allCharacterNames = [];

            Width = width;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);
            OuterControlPadding = new Vector2(5);

            _placeholderLabel = new Label()
            {
                Parent = this,
                Text = strings.TaskListPlaceholder,
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                TextColor = Color.LightGray,
            };

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
            };

            var namePanel = new FlowPanel()
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _ = new Label()
            {
                Parent = namePanel,
                Text = $"{strings.Name}:",
                Width = 50,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            _nameBox = new TextBox()
            {
                Parent = namePanel,
                Width = 300,
                Height = 30,
            };
            _nameBox.TextChanged += (_, _) =>
            {
                if (_syncingListMetadata) return;
                _service.UpdateSelectedListName(_nameBox.Text);
            };

            _ = new Button()
            {
                Parent = namePanel,
                Text = strings.DeleteList,
                Width = 90,
                Height = 30,
                ClickAction = () => _service.DeleteSelectedList(),
            };

            var resetPanel = new FlowPanel()
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _ = new Label()
            {
                Parent = resetPanel,
                Text = strings.AutoResetField,
                Width = 75,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            _resetDropdown = new Dropdown()
            {
                Parent = resetPanel,
                Width = 220,
                Height = 30,
            };

            _resetDropdown.Items.Add(strings.ResetNone);
            _resetDropdown.Items.Add(strings.ResetDaily);
            _resetDropdown.Items.Add(strings.ResetWeekly);
            _resetDropdown.ValueChangedAction = (selected) =>
            {
                if (_syncingListMetadata) return;

                var frequency = selected switch
                {
                    _ when selected == strings.ResetDaily => ResetFrequency.Daily,
                    _ when selected == strings.ResetWeekly => ResetFrequency.Weekly,
                    _ => ResetFrequency.None,
                };

                _service.UpdateSelectedListResetFrequency(frequency);
            };

            var row1 = new FlowPanel()
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            var row2 = new FlowPanel()
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _characterNameBox = new TextBox()
            {
                Parent = row1,
                PlaceholderText = strings.SearchCharacterName,
                Width = 190,
                Height = 30,
            };

            _characterDropdown = new Dropdown()
            {
                Parent = row2,
                Width = 190,
                Height = 30,
            };

            _characterNameBox.TextChanged += (_, _) => RefreshCharacterDropdown();
            _characterDropdown.ValueChangedAction = (selected) =>
            {
                if (_syncingCharacterControls || string.IsNullOrEmpty(selected)) return;

                _syncingCharacterControls = true;
                _characterNameBox.Text = selected;
                _syncingCharacterControls = false;

                RefreshCharacterDropdown();
            };

            _taskDescBox = new TextBox()
            {
                Parent = row1,
                PlaceholderText = strings.TaskDescriptionPlaceholder,
                Width = 400,
                Height = 30,
            };

            _ = new Button()
            {
                Parent = row1,
                Text = strings.AddTask,
                Width = 90,
                Height = 30,
                ClickAction = () =>
                {
                    string characterName = _characterNameBox.Text?.Trim();
                    if (!string.IsNullOrEmpty(characterName) && _boundList is not null)
                    {
                        _service.AddEntry(characterName, _taskDescBox.Text);
                        _characterNameBox.Text = string.Empty;
                        _taskDescBox.Text = string.Empty;
                        RefreshCharacterDropdown();
                    }
                },
            };

            _entriesPanel = new TaskEntriesPanel(_service, settings)
            {
                Parent = _contentPanel,
            };

            _characterModels.CollectionChanged += CharacterModels_CollectionChanged;
            _service.State.SelectedList.Changed += SelectedList_Changed;

            RebuildCharacterNameCache();
            RefreshCharacterDropdown();
            BindSelectedList(_service.SelectedList);
        }

        protected override void DisposeControl()
        {
            _characterModels.CollectionChanged -= CharacterModels_CollectionChanged;
            _service.State.SelectedList.Changed -= SelectedList_Changed;

            base.DisposeControl();
        }

        private void BindSelectedList(TaskListModel list)
        {
            _boundList = list;

            bool hasList = _boundList is not null;
            _placeholderLabel.Visible = !hasList;
            _contentPanel.Visible = hasList;

            _entriesPanel.BindSelectedList(_boundList);
            if (!hasList) return;

            _syncingListMetadata = true;
            _nameBox.Text = _boundList.Name ?? string.Empty;
            _resetDropdown.SelectedItem = _boundList.ResetFrequency switch
            {
                ResetFrequency.Daily => strings.ResetDaily,
                ResetFrequency.Weekly => strings.ResetWeekly,
                _ => strings.ResetNone,
            };
            _syncingListMetadata = false;
        }

        private void RefreshCharacterDropdown()
        {
            if (_syncingCharacterControls) return;

            _syncingCharacterControls = true;

            string currentText = _characterNameBox.Text ?? string.Empty;
            string selectedItem = _characterDropdown.SelectedItem;

            var filteredNames = _allCharacterNames
                .Where(name => string.IsNullOrEmpty(currentText) || name.IndexOf(currentText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            _characterDropdown.Items.Clear();
            foreach (var name in filteredNames)
            {
                _characterDropdown.Items.Add(name);
            }

            string matchingItem = filteredNames
                .FirstOrDefault(name => string.Equals(name, currentText, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(matchingItem))
            {
                _characterDropdown.SelectedItem = matchingItem;
            }
            else if (!string.IsNullOrEmpty(selectedItem) && filteredNames.Contains(selectedItem))
            {
                _characterDropdown.SelectedItem = selectedItem;
            }
            else
            {
                _characterDropdown.SelectedItem = strings.CopyCharacterName;
            }

            _syncingCharacterControls = false;
        }

        private void RebuildCharacterNameCache()
        {
            _allCharacterNames.Clear();
            _allCharacterNames.AddRange(_characterModels
                .Select(c => c.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n));
        }

        private void CharacterModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildCharacterNameCache();
            RefreshCharacterDropdown();
        }

        private bool IsBoundList(TaskListModel list)
        {
            return _boundList is not null && ReferenceEquals(_boundList, list);
        }

        private void SelectedList_Changed(object sender, StateVarChangedEventArgs<TaskListModel> e)
        {
            if (!ReferenceEquals(e.OldValue, e.NewValue))
            {
                BindSelectedList(e.NewValue);
                return;
            }

            if (!IsBoundList(e.NewValue)) return;

            _syncingListMetadata = true;
            _nameBox.Text = e.NewValue.Name ?? string.Empty;
            _resetDropdown.SelectedItem = e.NewValue.ResetFrequency switch
            {
                ResetFrequency.Daily => strings.ResetDaily,
                ResetFrequency.Weekly => strings.ResetWeekly,
                _ => strings.ResetNone,
            };
            _syncingListMetadata = false;
        }
    }
}
