using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Button = Kenedia.Modules.Core.Controls.Button;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using Color = Microsoft.Xna.Framework.Color;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using ImageButton = Kenedia.Modules.Core.Controls.ImageButton;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StandardWindow = Kenedia.Modules.Core.Views.StandardWindow;
using TextBox = Blish_HUD.Controls.TextBox;

namespace Kenedia.Modules.Characters.Views
{
    public class TaskListWindow : StandardWindow
    {
        public event Action<string> SwitchToCharacterRequested;

        private readonly Settings _settings;
        // private readonly TextureManager _textureManager;
        private readonly ObservableCollection<Character_Model> _characterModels;
        private readonly ObservableCollection<TaskListModel> _taskLists;
        private readonly Action _requestSave;

        private readonly FlowPanel _contentPanel;
        private readonly FlowPanel _listPanel;
        private readonly FlowPanel _detailPanel;

        private TaskListModel _selectedList;
        private TaskEntry _editingEntry;
        private bool _created;

        public TaskListWindow(
            AsyncTexture2D background,
            Rectangle windowRegion,
            Rectangle contentRegion,
            Settings settings,
            // TextureManager textureManager,
            ObservableCollection<Character_Model> characterModels,
            ObservableCollection<TaskListModel> taskLists,
            Action requestSave)
            : base(background, windowRegion, contentRegion)
        {
            _settings = settings;
            // _textureManager = textureManager;
            _characterModels = characterModels;
            _taskLists = taskLists;
            _requestSave = requestSave;

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(5, 0),
            };

            // Left side: task list browser
            var listSection = new Panel()
            {
                Parent = _contentPanel,
                Width = 200,
                HeightSizingMode = SizingMode.Fill,
                ShowBorder = true,
            };

            var listHeader = new FlowPanel()
            {
                Parent = listSection,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                OuterControlPadding = new Vector2(5),
            };

            _ = new Label()
            {
                Parent = listHeader,
                Text = "Task Lists",
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _ = new Button()
            {
                Parent = listHeader,
                Text = "New List",
                Width = 180,
                Height = 30,
                ClickAction = () => CreateNewList(),
            };

            _listPanel = new FlowPanel()
            {
                Parent = listSection,
                Location = new Point(0, 70),
                WidthSizingMode = SizingMode.Fill,
                Height = contentRegion.Height - 75,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 2),
                CanScroll = true,
            };

            // Right side: selected list details
            _detailPanel = new FlowPanel()
            {
                Parent = _contentPanel,
                Width = contentRegion.Width - 210,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                OuterControlPadding = new Vector2(5),
            };

            _taskLists.CollectionChanged += TaskLists_CollectionChanged;

            PopulateListPanel();

            _created = true;
        }

        private void TaskLists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PopulateListPanel();
        }

        private void CreateNewList()
        {
            var newList = new TaskListModel($"Task List {_taskLists.Count + 1}");
            _taskLists.Add(newList);
            _requestSave?.Invoke();

            SelectList(newList);
        }

        private void PopulateListPanel()
        {
            _listPanel.ClearChildren();

            foreach (var taskList in _taskLists)
            {
                var listEntry = new Panel()
                {
                    Parent = _listPanel,
                    WidthSizingMode = SizingMode.Fill,
                    Height = 32,
                    BackgroundColor = _selectedList?.Id == taskList.Id ? new Color(60, 60, 60, 200) : Color.Transparent,
                };

                _ = new Label()
                {
                    Parent = listEntry,
                    Text = taskList.Name,
                    Location = new Point(5, 0),
                    AutoSizeWidth = true,
                    Height = 32,
                    VerticalAlignment = VerticalAlignment.Middle,
                };

                listEntry.Click += (s, e) => SelectList(taskList);
            }
        }

        private void SelectList(TaskListModel taskList)
        {
            _selectedList = taskList;
            _editingEntry = null;

            PopulateListPanel();
            PopulateDetailPanel();
        }

        private void PopulateDetailPanel()
        {
            _detailPanel.ClearChildren();

            if (_selectedList == null)
            {
                _ = new Label()
                {
                    Parent = _detailPanel,
                    Text = "Select or create a task list to get started.",
                    Font = Content.DefaultFont16,
                    AutoSizeWidth = true,
                    AutoSizeHeight = true,
                    TextColor = Color.LightGray,
                };

                return;
            }

            // List name editor
            var namePanel = new FlowPanel()
            {
                Parent = _detailPanel,
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
                Text = _selectedList.Name,
                Width = 300,
                Height = 30,
            };
            nameBox.TextChanged += (s, e) =>
            {
                _selectedList.Name = nameBox.Text;
                _requestSave?.Invoke();
                PopulateListPanel();
            };

            _ = new Button()
            {
                Parent = namePanel,
                Text = "Delete List",
                Width = 90,
                Height = 30,
                ClickAction = () =>
                {
                    var toDelete = _selectedList;
                    _selectedList = null;

                    _taskLists.Remove(toDelete);
                    _requestSave?.Invoke();
                    PopulateDetailPanel();
                },
            };

            // Reset frequency setting
            var resetPanel = new FlowPanel()
            {
                Parent = _detailPanel,
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

            resetDropdown.SelectedItem = _selectedList.ResetFrequency switch
            {
                ResetFrequency.Daily => "Daily (00:00 UTC)",
                ResetFrequency.Weekly => "Weekly (Mon 07:30 UTC)",
                _ => "None",
            };

            resetDropdown.ValueChangedAction = (selected) =>
            {
                _selectedList.ResetFrequency = selected switch
                {
                    "Daily (00:00 UTC)" => ResetFrequency.Daily,
                    "Weekly (Mon 07:30 UTC)" => ResetFrequency.Weekly,
                    _ => ResetFrequency.None,
                };

                _requestSave?.Invoke();
            };

            // Add entry section
            var addEntryPanelRow1 = new FlowPanel()
            {
                Parent = _detailPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };
            
            var addEntryPanelRow2 = new FlowPanel()
            {
                Parent = _detailPanel,
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
                Parent = addEntryPanelRow1,
                PlaceholderText = "Search String",
                Width = 190,
                Height = 30,
            };

            var characterDropdown = new Dropdown()
            {
                Parent = addEntryPanelRow2,
                Width = 190,
                Height = 30,
            };

            bool syncingCharacterControls = false;
            Action refreshCharacterDropdown = () =>
            {
                if (syncingCharacterControls)
                {
                    return;
                }

                syncingCharacterControls = true;

                string currentText = characterNameBox.Text ?? string.Empty;
                string selectedItem = characterDropdown.SelectedItem;

                var filteredCharacterNames = allCharacterNames
                    .Where(name => string.IsNullOrEmpty(currentText) || name.IndexOf(currentText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                characterDropdown.Items.Clear();
                foreach (var characterName in filteredCharacterNames)
                {
                    characterDropdown.Items.Add(characterName);
                }

                string matchingItem = filteredCharacterNames
                    .FirstOrDefault(name => string.Equals(name, currentText, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(matchingItem))
                {
                    characterDropdown.SelectedItem = matchingItem;
                }
                else if (!string.IsNullOrEmpty(selectedItem) && filteredCharacterNames.Contains(selectedItem))
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
                if (syncingCharacterControls || string.IsNullOrEmpty(selected))
                {
                    return;
                }

                syncingCharacterControls = true;
                characterNameBox.Text = selected;
                syncingCharacterControls = false;

                refreshCharacterDropdown();
            };

            refreshCharacterDropdown();

            var taskDescBox = new TextBox()
            {
                Parent = addEntryPanelRow1,
                PlaceholderText = "Task description (optional)",
                Width = 400,
                Height = 30,
            };

            _ = new Button()
            {
                Parent = addEntryPanelRow1,
                Text = "Add Entry",
                Width = 90,
                Height = 30,
                ClickAction = () =>
                {
                    string characterName = characterNameBox.Text?.Trim();
                    if (!string.IsNullOrEmpty(characterName))
                    {
                        _selectedList.AddEntry(characterName, taskDescBox.Text);
                        characterNameBox.Text = string.Empty;
                        taskDescBox.Text = string.Empty;
                        _requestSave?.Invoke();
                        PopulateDetailPanel();
                    }
                },
            };

            // Entries header
            var entriesHeaderPanel = new FlowPanel()
            {
                Parent = _detailPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                OuterControlPadding = new Vector2(5, 0),
            };

            bool syncingAllEntriesCheckbox = false;
            bool allEntriesChecked = _selectedList.Entries.Count > 0 && _selectedList.Entries.All(e => e.Completed);
            var allEntriesCheckbox = new Checkbox()
            {
                Parent = entriesHeaderPanel,
                Checked = allEntriesChecked,
                Width = 20,
                Height = 20,
                BasicTooltipText = allEntriesChecked ? "Uncheck All" : "Check All",
                CheckedChangedAction = (isChecked) =>
                {
                    if (syncingAllEntriesCheckbox)
                    {
                        return;
                    }

                    foreach (var taskEntry in _selectedList.Entries)
                    {
                        taskEntry.Completed = isChecked;
                    }

                    _requestSave?.Invoke();
                    PopulateDetailPanel();
                },
            };

            _ = new Label()
            {
                Parent = entriesHeaderPanel,
                Text = $"Entries ({_selectedList.Entries.Count})",
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            // Entries list
            var entriesPanel = new FlowPanel()
            {
                Parent = _detailPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 3),
                CanScroll = true,
            };

            foreach (var entry in _selectedList.Entries.OrderBy(e => e.Order))
            {
                CreateEntryRow(entriesPanel, entry, () =>
                {
                    syncingAllEntriesCheckbox = true;
                    bool allEntriesChecked = _selectedList.Entries.Count > 0 && _selectedList.Entries.All(e => e.Completed);
                    allEntriesCheckbox.Checked = allEntriesChecked;
                    allEntriesCheckbox.BasicTooltipText = allEntriesChecked ? "Uncheck All" : "Check All";
                    syncingAllEntriesCheckbox = false;
                });
            }
        }

        private void CreateEntryRow(FlowPanel parent, TaskEntry entry, Action onCompletionChanged = null)
        {
            bool isEditing = _editingEntry == entry;

            var rowPanel = new Panel()
            {
                Parent = parent,
                WidthSizingMode = SizingMode.Fill,
                Height = 32,
                BackgroundColor = isEditing
                    ? new Color(50, 50, 70, 180)
                    : entry.Completed ? new Color(40, 80, 40, 150) : new Color(40, 40, 40, 150),
            };

            _ = new Checkbox()
            {
                Parent = rowPanel,
                Checked = entry.Completed,
                Location = new Point(5, 6),
                Width = 20,
                CheckedChangedAction = (b) =>
                {
                    entry.Completed = b;
                    rowPanel.BackgroundColor = b ? new Color(40, 80, 40, 150) : new Color(40, 40, 40, 150);
                    onCompletionChanged?.Invoke();
                    _requestSave?.Invoke();
                },
            };

            if (isEditing)
            {
                // Editable character name
                var editNameBox = new TextBox()
                {
                    Parent = rowPanel,
                    Text = entry.CharacterName,
                    Location = new Point(30, 2),
                    Width = 155,
                    Height = 28,
                };

                // Editable description
                var editDescBox = new TextBox()
                {
                    Parent = rowPanel,
                    Text = entry.Description ?? string.Empty,
                    PlaceholderText = "Description (optional)",
                    Location = new Point(190, 2),
                    Width = 370,
                    Height = 28,
                };

                // Save button
                var saveButton = new Button()
                {
                    Parent = rowPanel,
                    Text = "Save",
                    Width = 50,
                    Height = 28,
                    ClickAction = () =>
                    {
                        string newName = editNameBox.Text?.Trim();
                        if (!string.IsNullOrEmpty(newName))
                        {
                            entry.CharacterName = newName;
                            entry.Description = editDescBox.Text?.Trim();
                            _editingEntry = null;
                            _requestSave?.Invoke();
                            PopulateDetailPanel();
                        }
                    },
                };

                // Cancel button
                var cancelButton = new Button()
                {
                    Parent = rowPanel,
                    Text = "Cancel",
                    Width = 55,
                    Height = 28,
                    ClickAction = () =>
                    {
                        _editingEntry = null;
                        PopulateDetailPanel();
                    },
                };

                rowPanel.Resized += (s, e) =>
                {
                    var scrollbarOffset = 12;
                    cancelButton.Location = new Point(rowPanel.Width - 55 - scrollbarOffset, 2);
                    saveButton.Location = new Point(rowPanel.Width - 55 - 5 - 50 - scrollbarOffset, 2);
                };
            }
            else
            {
                // Character name
                _ = new Label()
                {
                    Parent = rowPanel,
                    Text = entry.CharacterName,
                    Location = new Point(30, 0),
                    Width = 160,
                    Height = 32,
                    VerticalAlignment = VerticalAlignment.Middle,
                    Font = Content.DefaultFont14,
                    TextColor = entry.Completed ? Color.Gray : ContentService.Colors.ColonialWhite,
                };

                // Task description
                if (!string.IsNullOrEmpty(entry.Description))
                {
                    _ = new Label()
                    {
                        Parent = rowPanel,
                        Text = entry.Description,
                        Location = new Point(195, 0),
                        Width = 250,
                        Height = 32,
                        VerticalAlignment = VerticalAlignment.Middle,
                        Font = Content.DefaultFont12,
                        TextColor = entry.Completed ? Color.Gray : Color.LightGray,
                    };
                }

                var switchButton = new ImageButton()
                {
                    Parent = rowPanel,
                    Texture = AsyncTexture2D.FromAssetId(784346),
                    Size = new Point(16, 16),
                    BasicTooltipText = "Switch to Character",
                    ClickAction = (x) =>
                    {
                        string characterName = entry.CharacterName?.Trim();
                        if (!string.IsNullOrEmpty(characterName))
                        {
                            SwitchToCharacterRequested?.Invoke(characterName);
                        }
                    },
                };

                // Move up button
                var moveUpButton = new ImageButton()
                {
                    Parent = rowPanel,
                    Texture = AsyncTexture2D.FromAssetId(155953),
                    Size = new Point(16, 16),
                    BasicTooltipText = "Move Up",
                    ClickAction = (m) =>
                    {
                        if (entry.Order > 0)
                        {
                            var swapEntry = _selectedList.Entries.FirstOrDefault(e => e.Order == entry.Order - 1);
                            if (swapEntry != null)
                            {
                                swapEntry.Order++;
                                entry.Order--;
                                _requestSave?.Invoke();
                                PopulateDetailPanel();
                            }
                        }
                    },
                };

                // Move down button
                var moveDownButton = new ImageButton()
                {
                    Parent = rowPanel,
                    Texture = AsyncTexture2D.FromAssetId(155954),
                    Size = new Point(16, 16),
                    BasicTooltipText = "Move Down",
                    ClickAction = (m) =>
                    {
                        int maxOrder = _selectedList.Entries.Max(e => e.Order);
                        if (entry.Order < maxOrder)
                        {
                            var swapEntry = _selectedList.Entries.FirstOrDefault(e => e.Order == entry.Order + 1);
                            if (swapEntry != null)
                            {
                                swapEntry.Order--;
                                entry.Order++;
                                _requestSave?.Invoke();
                                PopulateDetailPanel();
                            }
                        }
                    },
                };

                // Edit button
                var editButton = new ImageButton()
                {
                    Parent = rowPanel,
                    Texture = AsyncTexture2D.FromAssetId(2175780),
                    HoveredTexture = AsyncTexture2D.FromAssetId(2175779),
                    Size = new Point(16, 16),
                    BasicTooltipText = "Edit Entry",
                    ClickAction = (m) =>
                    {
                        _editingEntry = entry;
                        PopulateDetailPanel();
                    },
                };

                // Remove button
                var removeButton = new ImageButton()
                {
                    Parent = rowPanel,
                    Texture = AsyncTexture2D.FromAssetId(2175783),
                    HoveredTexture = AsyncTexture2D.FromAssetId(2175782),
                    ClickedTexture = AsyncTexture2D.FromAssetId(2175784),
                    Size = new Point(16, 16),
                    BasicTooltipText = "Remove Entry",
                    ClickAction = (m) =>
                    {
                        _selectedList.RemoveEntry(entry);
                        _requestSave?.Invoke();
                        PopulateDetailPanel();
                    },
                };

                rowPanel.Resized += (s, e) =>
                {
                    var scrollbarOffset = 12;
                    switchButton.Location = new Point(rowPanel.Width - 114 - scrollbarOffset, 8);
                    moveUpButton.Location = new Point(rowPanel.Width - 92 - scrollbarOffset, 8);
                    moveDownButton.Location = new Point(rowPanel.Width - 70 - scrollbarOffset, 8);
                    editButton.Location = new Point(rowPanel.Width - 48 - scrollbarOffset, 8);
                    removeButton.Location = new Point(rowPanel.Width - 26 - scrollbarOffset, 8);
                };
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (_created)
            {
                _detailPanel.Width = ContentRegion.Width - 210;
                _listPanel.Height = ContentRegion.Height - 75;
            }
        }

        protected override void DisposeControl()
        {
            _taskLists.CollectionChanged -= TaskLists_CollectionChanged;

            _contentPanel?.Dispose();
            _listPanel?.Dispose();
            _detailPanel?.Dispose();

            base.DisposeControl();
        }
    }
}
