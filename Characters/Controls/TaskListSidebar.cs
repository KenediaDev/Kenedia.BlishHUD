using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Button = Kenedia.Modules.Core.Controls.Button;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;

namespace Kenedia.Modules.Characters.Controls
{
    public class TaskListSidebar : Panel
    {
        private const int SidebarWidth = 200;
        private const int HeaderOuterPadding = 5;
        private const int HeaderControlWidth = 180;
        private const int HeaderControlSpacing = 5;
        private const int HeaderToListOffset = 5;
        private const int ListBottomPadding = 15;
        private const int ScrollbarReservedWidth = 12;
        private const int ListEntryHeight = 32;
        private const int ListEntryLabelX = 5;
        private const int ListEntryLeftPadding = 8;

        private static readonly Color SelectedBackground = new Color(60, 60, 60, 200);
        private static readonly Color CompletedBackground = new Color(40, 80, 40, 200);
        private static readonly Color CompletedTextColor = new Color(120, 200, 120);

        private readonly TaskListService _service;
        private readonly FlowPanel _headerPanel;
        private readonly FlowPanel _listPanel;
        private readonly TextBox _searchBox;
        private readonly Dictionary<Guid, SidebarEntry> _listEntries = [];

        public TaskListSidebar(TaskListService service)
        {
            _service = service;

            Width = SidebarWidth;
            HeightSizingMode = SizingMode.Fill;
            ShowBorder = true;

            _headerPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, HeaderControlSpacing),
                OuterControlPadding = new Vector2(HeaderOuterPadding),
            };

            _ = new Button()
            {
                Parent = _headerPanel,
                Text = strings.NewTaskList,
                Width = HeaderControlWidth,
                Height = 30,
                ClickAction = () => _service.CreateNewList(),
            };

            _searchBox = new TextBox()
            {
                Parent = _headerPanel,
                PlaceholderText = strings.SearchTaskLists,
                Width = HeaderControlWidth,
                Height = 28,
                TextChangedAction = (_) => UpdateFilterVisibility(),
            };

            _listPanel = new FlowPanel()
            {
                Parent = this,
                Location = Point.Zero,
                WidthSizingMode = SizingMode.Fill,
                Height = 0,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 2),
                OuterControlPadding = new Vector2(ListEntryLeftPadding, 0),
                CanScroll = true,
            };

            Resized += (_, _) => UpdateLayout();
            _headerPanel.Resized += (_, _) => UpdateLayout();
            _listPanel.Resized += (_, _) => UpdateListEntryWidths();

            _service.State.TaskLists.Changed += TaskLists_Changed;
            _service.State.SelectedList.Changed += SelectedList_Changed;

            foreach (var taskList in _service.TaskLists)
            {
                EnsureListEntry(taskList);
            }

            UpdateLayout();
            UpdateFilterVisibility();
            UpdateAllEntryVisuals();
        }

        private void EnsureListEntry(TaskListModel taskList)
        {
            if (taskList is null) return;
            if (_listEntries.ContainsKey(taskList.Id)) return;

            var entryPanel = new Panel()
            {
                Parent = _listPanel,
                Width = GetListEntryWidth(),
                Height = ListEntryHeight,
            };

            var nameLabel = new Label()
            {
                Parent = entryPanel,
                Text = taskList.Name,
                Location = new Point(ListEntryLabelX, 0),
                AutoSizeWidth = true,
                Height = ListEntryHeight,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            entryPanel.Click += (_, _) => _service.SelectList(taskList);

            _listEntries[taskList.Id] = new SidebarEntry()
            {
                TaskList = taskList,
                EntryPanel = entryPanel,
                NameLabel = nameLabel,
            };

            UpdateListEntryVisual(taskList);
        }

        private void RemoveListEntry(TaskListModel taskList)
        {
            if (taskList is null) return;
            if (!_listEntries.TryGetValue(taskList.Id, out var entry)) return;

            entry.EntryPanel.Dispose();
            _listEntries.Remove(taskList.Id);
        }

        private void UpdateFilterVisibility()
        {
            foreach (var entry in _listEntries.Values)
            {
                entry.EntryPanel.Visible = MatchesFilter(entry.TaskList);
            }

            UpdateListEntryWidths();
        }

        private bool MatchesFilter(TaskListModel taskList)
        {
            string filter = _searchBox?.Text?.Trim() ?? string.Empty;
            if (filter.Length == 0) return true;

            return !string.IsNullOrEmpty(taskList?.Name)
                   && taskList.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void UpdateListEntryVisual(TaskListModel taskList)
        {
            if (taskList is null) return;
            if (!_listEntries.TryGetValue(taskList.Id, out var entry)) return;

            bool isSelected = _service.SelectedList?.Id == taskList.Id;
            bool isCompleted = taskList.Entries.Count > 0 && taskList.Entries.All(e => e.Completed);

            entry.EntryPanel.BackgroundColor =
                  isSelected ? SelectedBackground
                : isCompleted ? CompletedBackground
                : Color.Transparent;

            entry.NameLabel.Text = taskList.Name;
            entry.NameLabel.TextColor = isCompleted ? CompletedTextColor : Color.White;
            entry.EntryPanel.Visible = MatchesFilter(taskList);
        }

        private void UpdateAllEntryVisuals()
        {
            foreach (var taskList in _service.TaskLists)
            {
                UpdateListEntryVisual(taskList);
            }
        }

        private void UpdateLayout()
        {
            int listTop = _headerPanel.Bottom + HeaderToListOffset;

            _listPanel.Location = new Point(0, listTop);
            _listPanel.Height = Math.Max(0, Height - listTop - ListBottomPadding);

            UpdateListEntryWidths();
        }

        private int GetListEntryWidth()
        {
            return Math.Max(0, _listPanel.Width - ListEntryLeftPadding - ScrollbarReservedWidth);
        }

        private void UpdateListEntryWidths()
        {
            int entryWidth = GetListEntryWidth();
            foreach (var child in _listEntries.Values)
            {
                child.EntryPanel.Width = entryWidth;
            }
        }

        protected override void DisposeControl()
        {
            _service.State.TaskLists.Changed -= TaskLists_Changed;
            _service.State.SelectedList.Changed -= SelectedList_Changed;

            base.DisposeControl();
        }

        private void TaskLists_Changed(object sender, StateVarChangedEventArgs<ObservableCollection<TaskListModel>> e)
        {
            var currentLists = _service.TaskLists;
            var currentIds = new HashSet<Guid>(currentLists.Select(list => list.Id));

            foreach (var existingId in _listEntries.Keys.ToList())
            {
                if (!currentIds.Contains(existingId))
                {
                    if (_listEntries.TryGetValue(existingId, out var existing))
                    {
                        existing.EntryPanel.Dispose();
                        _listEntries.Remove(existingId);
                    }
                }
            }

            foreach (var taskList in currentLists)
            {
                EnsureListEntry(taskList);
            }

            UpdateFilterVisibility();
            UpdateAllEntryVisuals();
        }

        private void SelectedList_Changed(object sender, StateVarChangedEventArgs<TaskListModel> e)
        {
            UpdateListEntryVisual(e.OldValue);
            UpdateListEntryVisual(e.NewValue);
        }

        private sealed class SidebarEntry
        {
            public TaskListModel TaskList { get; init; }

            public Panel EntryPanel { get; init; }

            public Label NameLabel { get; init; }
        }
    }
}
