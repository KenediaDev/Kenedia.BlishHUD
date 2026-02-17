using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.ObjectModel;
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

        public TaskListDetailPanel(TaskListService service, ObservableCollection<Character_Model> characterModels, int width)
        {
            _service = service;
            _characterModels = characterModels;

            Width = width;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);
            OuterControlPadding = new Vector2(5);

            _service.DetailPanelChanged += Populate;

            Populate();
        }

        public void Populate()
        {
            ClearChildren();

            if (_service.SelectedList is null)
            {
                BuildPlaceholder();
                return;
            }

            BuildListNameEditor();
            BuildResetFrequencySelector();
            BuildAddEntryForm();
            BuildEntriesPanel();
        }

        private void BuildPlaceholder()
        {
            _ = new Label()
            {
                Parent = this,
                Text = "Select or create a task list to get started.",
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                TextColor = Color.LightGray,
            };
        }

        private void BuildListNameEditor()
        {
            var namePanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _ = new Label()
            {
                Parent = namePanel,
                Text = "Name:",
                Width = 50,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            var nameBox = new TextBox()
            {
                Parent = namePanel,
                Text = _service.SelectedList.Name,
                Width = 300,
                Height = 30,
            };
            nameBox.TextChanged += (s, e) => _service.UpdateSelectedListName(nameBox.Text);

            _ = new Button()
            {
                Parent = namePanel,
                Text = "Delete List",
                Width = 90,
                Height = 30,
                ClickAction = () => _service.DeleteSelectedList(),
            };
        }

        private void BuildResetFrequencySelector()
        {
            var resetPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _ = new Label()
            {
                Parent = resetPanel,
                Text = "Auto Reset:",
                Width = 75,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            var resetDropdown = new Dropdown()
            {
                Parent = resetPanel,
                Width = 220,
                Height = 30,
            };

            resetDropdown.Items.Add("None");
            resetDropdown.Items.Add("Daily (00:00 UTC)");
            resetDropdown.Items.Add("Weekly (Mon 07:30 UTC)");

            resetDropdown.SelectedItem = _service.SelectedList.ResetFrequency switch
            {
                ResetFrequency.Daily => "Daily (00:00 UTC)",
                ResetFrequency.Weekly => "Weekly (Mon 07:30 UTC)",
                _ => "None",
            };

            resetDropdown.ValueChangedAction = (selected) =>
            {
                var frequency = selected switch
                {
                    "Daily (00:00 UTC)" => ResetFrequency.Daily,
                    "Weekly (Mon 07:30 UTC)" => ResetFrequency.Weekly,
                    _ => ResetFrequency.None,
                };

                _service.UpdateSelectedListResetFrequency(frequency);
            };
        }

        private void BuildAddEntryForm()
        {
            var row1 = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            var row2 = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            var allCharacterNames = _characterModels
                .Select(c => c.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToList();

            var characterNameBox = new TextBox()
            {
                Parent = row1,
                PlaceholderText = "Search String",
                Width = 190,
                Height = 30,
            };

            var characterDropdown = new Dropdown()
            {
                Parent = row2,
                Width = 190,
                Height = 30,
            };

            bool syncingCharacterControls = false;
            Action refreshCharacterDropdown = () =>
            {
                if (syncingCharacterControls) return;

                syncingCharacterControls = true;

                string currentText = characterNameBox.Text ?? string.Empty;
                string selectedItem = characterDropdown.SelectedItem;

                var filteredNames = allCharacterNames
                    .Where(name => string.IsNullOrEmpty(currentText) || name.IndexOf(currentText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                characterDropdown.Items.Clear();
                foreach (var name in filteredNames)
                {
                    characterDropdown.Items.Add(name);
                }

                string matchingItem = filteredNames
                    .FirstOrDefault(name => string.Equals(name, currentText, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(matchingItem))
                {
                    characterDropdown.SelectedItem = matchingItem;
                }
                else if (!string.IsNullOrEmpty(selectedItem) && filteredNames.Contains(selectedItem))
                {
                    characterDropdown.SelectedItem = selectedItem;
                }
                else
                {
                    characterDropdown.SelectedItem = string.Empty;
                }

                syncingCharacterControls = false;
            };

            characterNameBox.TextChanged += (s, e) => refreshCharacterDropdown();
            characterDropdown.ValueChangedAction = (selected) =>
            {
                if (syncingCharacterControls || string.IsNullOrEmpty(selected)) return;

                syncingCharacterControls = true;
                characterNameBox.Text = selected;
                syncingCharacterControls = false;

                refreshCharacterDropdown();
            };

            refreshCharacterDropdown();

            var taskDescBox = new TextBox()
            {
                Parent = row1,
                PlaceholderText = "Task description (optional)",
                Width = 400,
                Height = 30,
            };

            _ = new Button()
            {
                Parent = row1,
                Text = "Add Entry",
                Width = 90,
                Height = 30,
                ClickAction = () =>
                {
                    string characterName = characterNameBox.Text?.Trim();
                    if (!string.IsNullOrEmpty(characterName))
                    {
                        _service.AddEntry(characterName, taskDescBox.Text);
                        characterNameBox.Text = string.Empty;
                        taskDescBox.Text = string.Empty;
                    }
                },
            };
        }

        private void BuildEntriesPanel()
        {
            var entriesPanel = new TaskEntriesPanel(_service)
            {
                Parent = this,
            };

            entriesPanel.Populate();
        }

        protected override void DisposeControl()
        {
            _service.DetailPanelChanged -= Populate;

            base.DisposeControl();
        }
    }
}
