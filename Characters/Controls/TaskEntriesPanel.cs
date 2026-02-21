using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Button = Kenedia.Modules.Core.Controls.Button;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;

namespace Kenedia.Modules.Characters.Controls
{
    public class TaskEntriesPanel : FlowPanel
    {
        private const int ScrollbarReservedWidth = 12;
        private const int InsertionLineLeftPadding = 5;
        private const int InsertionLineVerticalOffset = 1;
        private const int EntryRowSpacing = 3;
        private const int HeaderControlHorizontalPadding = 5;
        private const int DragHandleWidth = 14;

        private readonly TaskListService _service;
        private readonly Settings _settings;
        private readonly Panel _insertionLine;
        private readonly Dictionary<TaskEntry, TaskEntryRow> _entryRows = [];

        private TaskListModel _boundList;
        private FlowPanel _headerPanel;
        private Panel _entriesContainer;
        private Checkbox _allEntriesCheckbox;
        private Label _entriesLabel;
        private Button _nextButton;
        private Label _statusLabel;
        private Button _hideButton;
        private TaskEntryRow _draggedRow;
        private bool _isDragging;
        private bool _dragActivated;
        private bool _overrideDisplayBehaviorForDrag;
        private bool _overrideHideCompletedTasks;
        private int _pendingTargetOrder = -1;
        private bool _syncingHeaderCheckbox;

        public TaskEntriesPanel(TaskListService service, Settings settings)
        {
            _service = service;
            _settings = settings;

            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);

            _settings.CompletedTasksBehavior.SettingChanged += CompletedTasksBehavior_SettingChanged;
            _service.State.SelectedList.Changed += SelectedList_Changed;
            _service.State.TrackedEntry.Changed += TrackedEntry_Changed;

            _insertionLine = new Panel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(255, 200, 50, 220),
                Height = 3,
                Visible = false,
                ZIndex = int.MaxValue - 2,
                CaptureInput = false,
            };

            BuildControls();
            BindSelectedList(_service.SelectedList);
        }

        public void BindSelectedList(TaskListModel selectedList)
        {
            CancelDrag();
            _boundList = selectedList;
            _overrideHideCompletedTasks = false;

            SyncRowsWithBoundList();
            RefreshHeader();
            RefreshRowsLayout(preserveScroll: false);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (!_isDragging || _entriesContainer == null || _draggedRow == null) return;

            _dragActivated = true;

            var mousePos = GameService.Input.Mouse.Position;
            var containerBounds = _entriesContainer.AbsoluteBounds;
            
            UpdateInsertionIndicator(mousePos, containerBounds);
        }

        protected override void DisposeControl()
        {
            CancelDrag();
            _service.State.SelectedList.Changed -= SelectedList_Changed;
            _service.State.TrackedEntry.Changed -= TrackedEntry_Changed;
            _settings.CompletedTasksBehavior.SettingChanged -= CompletedTasksBehavior_SettingChanged;

            foreach (var row in _entryRows.Values.ToList())
            {
                row.Dispose();
            }

            _entryRows.Clear();
            _insertionLine?.Dispose();
            base.DisposeControl();
        }

        private void CompletedTasksBehavior_SettingChanged(object sender, ValueChangedEventArgs<Settings.CompletedTasksDisplayBehavior> e)
        {
            if (!_isDragging)
            {
                RefreshHeader();
                RefreshRowsLayout();
            }
        }

        private void ToggleOverrideHideCompletedTasks()
        {
            _overrideHideCompletedTasks = !_overrideHideCompletedTasks;
            RefreshHeader();
            RefreshRowsLayout();
        }

        private void UpdateInsertionIndicator(Point mousePos, Rectangle containerBounds)
        {
            var allRows = _entryRows.Values
                .Where(r => r.Visible)
                .OrderBy(r => r.Entry.Order)
                .ToList();

            int dragIndex = _draggedRow.Entry.Order;
            int insertionVisualIndex = allRows.Count;
            int insertionY = allRows.Count > 0 ? allRows.Last().AbsoluteBounds.Bottom : containerBounds.Top;

            for (int i = 0; i < allRows.Count; i++)
            {
                var rowBounds = allRows[i].AbsoluteBounds;
                int rowMiddle = rowBounds.Top + rowBounds.Height / 2;

                if (mousePos.Y < rowMiddle)
                {
                    insertionVisualIndex = i;
                    insertionY = rowBounds.Top;
                    break;
                }
            }

            int targetOrder = insertionVisualIndex <= dragIndex
                ? insertionVisualIndex
                : insertionVisualIndex - 1;

            _pendingTargetOrder = targetOrder;

            bool withinBounds = insertionY >= containerBounds.Top && insertionY <= containerBounds.Bottom;
            if (targetOrder != dragIndex && withinBounds)
            {
                _insertionLine.Location = new Point(containerBounds.Left + InsertionLineLeftPadding, insertionY - InsertionLineVerticalOffset);
                _insertionLine.Width = Math.Max(0, containerBounds.Width - 2*InsertionLineLeftPadding - ScrollbarReservedWidth);
                _insertionLine.Visible = true;
            }
            else
            {
                _insertionLine.Visible = false;
            }
        }

        private void OnDragStart(TaskEntryRow row)
        {
            if (_isDragging || _boundList is null) return;

            _overrideDisplayBehaviorForDrag = true;
            RefreshHeader();
            RefreshRowsLayout();

            _entryRows.TryGetValue(row.Entry, out _draggedRow);
            if (_draggedRow is null)
            {
                _overrideDisplayBehaviorForDrag = false;
                RefreshHeader();
                RefreshRowsLayout();
                return;
            }

            _isDragging = true;
            _dragActivated = false;
            _draggedRow.IsDragging = true;
            _pendingTargetOrder = row.Entry.Order;

            GameService.Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseReleased;
        }

        private void OnGlobalMouseReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            CompleteDrop();
        }

        private void CompleteDrop()
        {
            if (!_isDragging) return;

            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseReleased;
            _insertionLine.Visible = false;

            var entry = _draggedRow?.Entry;
            int targetOrder = _pendingTargetOrder;
            bool shouldReorder = _dragActivated && entry != null && targetOrder >= 0 && targetOrder != entry.Order;

            if (_draggedRow != null)
            {
                _draggedRow.IsDragging = false;
            }

            _draggedRow = null;
            _isDragging = false;
            _dragActivated = false;
            _overrideDisplayBehaviorForDrag = false;
            _pendingTargetOrder = -1;

            if (shouldReorder)
            {
                _service.ReorderEntry(entry, targetOrder);
            }

            RefreshHeader();
            RefreshRowsLayout();
        }

        private void CancelDrag()
        {
            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseReleased;

            if (_draggedRow != null)
            {
                _draggedRow.IsDragging = false;
            }

            bool hadDragState = _isDragging || _overrideDisplayBehaviorForDrag;
            _draggedRow = null;
            _isDragging = false;
            _dragActivated = false;
            _overrideDisplayBehaviorForDrag = false;
            _pendingTargetOrder = -1;
            _insertionLine.Visible = false;

            if (hadDragState)
            {
                RefreshHeader();
                RefreshRowsLayout();
            }
        }

        private void BuildControls()
        {
            _headerPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(HeaderControlHorizontalPadding, 0),
                OuterControlPadding = new Vector2(HeaderControlHorizontalPadding + DragHandleWidth, 0),
            };

            _allEntriesCheckbox = new Checkbox()
            {
                Parent = _headerPanel,
                Width = 20,
                Height = 28,
                CheckedChangedAction = (isChecked) =>
                {
                    if (_syncingHeaderCheckbox) return;

                    _service.SetAllEntriesCompletion(isChecked);
                },
            };

            _entriesLabel = new Label()
            {
                Parent = _headerPanel,
                Text = "Tasks",
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                Height = 28,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            _nextButton = new Button()
            {
                Parent = _headerPanel,
                Text = "Next",
                Width = 70,
                Height = 28,
                ClickAction = () => _service.SwitchToNextIncompleteEntry(),
            };

            _statusLabel = new Label()
            {
                Parent = _headerPanel,
                Width = 380,
                Height = 28,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            _hideButton = new Button()
            {
                Parent = _headerPanel,
                Width = 165,
                Height = 28,
                BasicTooltipText = "Completed tasks are hidden based on your settings.",
                ClickAction = () => ToggleOverrideHideCompletedTasks(),
            };

            _entriesContainer = new Panel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
            };

            _headerPanel.Resized += (_, _) => UpdateHeaderLayout();
            _entriesContainer.Resized += (_, _) => RefreshRowsLayout();
        }

        private Settings.CompletedTasksDisplayBehavior GetEffectiveBehavior()
        {
            return _overrideDisplayBehaviorForDrag
                ? Settings.CompletedTasksDisplayBehavior.Nothing
                : _settings.CompletedTasksBehavior.Value;
        }

        private IOrderedEnumerable<TaskEntry> GetEntriesInSavedOrder(TaskListModel taskList)
        {
            return taskList.Entries.OrderBy(e => e.Order);
        }

        private IEnumerable<TaskEntry> GetDisplayedEntries(TaskListModel taskList)
        {
            if (taskList is null) return [];

            var orderedEntries = GetEntriesInSavedOrder(taskList);
            return GetEffectiveBehavior() switch
            {
                Settings.CompletedTasksDisplayBehavior.HideCompletedTasks
                    => _overrideHideCompletedTasks ? orderedEntries : orderedEntries.Where(e => !e.Completed),
                Settings.CompletedTasksDisplayBehavior.MoveCompletedTasksToBottomOfDisplay
                    => orderedEntries.Where(e => !e.Completed).Concat(orderedEntries.Where(e => e.Completed)),
                _ => orderedEntries,
            };
        }

        private int GetEntryRowWidth()
        {
            if (_entriesContainer == null) return 0;
            return Math.Max(0, _entriesContainer.Width - ScrollbarReservedWidth);
        }

        private void RefreshHeader()
        {
            bool hasList = _boundList is not null;
            _headerPanel.Visible = hasList;
            _entriesContainer.Visible = hasList;

            if (!hasList) return;

            int completedCount = _boundList.Entries.Count(e => e.Completed);
            int totalCount = _boundList.Entries.Count;
            _entriesLabel.Text = $"Tasks ({completedCount}/{totalCount})";

            bool allChecked = totalCount > 0 && completedCount == totalCount;
            _syncingHeaderCheckbox = true;
            _allEntriesCheckbox.Checked = allChecked;
            _allEntriesCheckbox.BasicTooltipText = allChecked ? "Uncheck All" : "Check All";
            _syncingHeaderCheckbox = false;

            TaskEntry pendingCompletionEntry = _service.GetTrackedEntryForSelectedList();
            int incompleteCount = totalCount - completedCount;
            bool hasIncompleteEntries = incompleteCount > 0;
            bool isLastIncomplete = incompleteCount == 1 && pendingCompletionEntry != null;

            _nextButton.Text = isLastIncomplete ? "Finish" : "Next";
            _nextButton.Enabled = hasIncompleteEntries;
            _nextButton.BasicTooltipText = hasIncompleteEntries
                ? (isLastIncomplete ? "Complete the task list." : "Switch to the first incomplete task entry.")
                : "All tasks in this list are complete.";

            _statusLabel.Text = pendingCompletionEntry is not null
                ? $"Next click marks '{pendingCompletionEntry.CharacterName}' complete."
                : string.Empty;
            _statusLabel.TextColor = pendingCompletionEntry is not null ? Color.LightGreen : Color.LightGray;

            bool canUnhideCompletedTasks = GetEffectiveBehavior() == Settings.CompletedTasksDisplayBehavior.HideCompletedTasks
                                           && _boundList.Entries.Any(e => e.Completed);
            _hideButton.Text = _overrideHideCompletedTasks ? "Hide Complete" : "Unhide Complete";
            _hideButton.Visible = canUnhideCompletedTasks;
            _hideButton.Enabled = canUnhideCompletedTasks;

            UpdateHeaderLayout();
        }

        private void RefreshRowsLayout(bool preserveScroll = true)
        {
            if (_entriesContainer is null || _boundList is null) return;

            int previousScrollOffset = preserveScroll ? _entriesContainer.VerticalScrollOffset : 0;
            int rowWidth = GetEntryRowWidth();

            var displayedEntries = GetDisplayedEntries(_boundList).ToList();
            var displayedEntrySet = new HashSet<TaskEntry>(displayedEntries);

            int y = 0;
            foreach (var entry in displayedEntries)
            {
                if (!_entryRows.TryGetValue(entry, out var row))
                {
                    continue;
                }

                row.Visible = true;
                row.Width = rowWidth;
                row.Location = new Point(0, y);
                y += row.Height + EntryRowSpacing;
            }

            foreach (var row in _entryRows)
            {
                if (!displayedEntrySet.Contains(row.Key))
                {
                    row.Value.Visible = false;
                }
            }

            if (preserveScroll)
            {
                int maxOffset = Math.Max(0, y - _entriesContainer.ContentRegion.Height);
                _entriesContainer.VerticalScrollOffset = Math.Max(0, Math.Min(previousScrollOffset, maxOffset));
            }
        }

        private void SyncRowsWithBoundList()
        {
            if (_boundList is null)
            {
                foreach (var entry in _entryRows.Keys.ToList())
                {
                    RemoveRow(entry);
                }

                return;
            }

            var validEntries = new HashSet<TaskEntry>(_boundList.Entries);

            foreach (var entry in _entryRows.Keys.ToList())
            {
                if (!validEntries.Contains(entry))
                {
                    RemoveRow(entry);
                }
            }

            foreach (var entry in _boundList.Entries)
            {
                if (!_entryRows.ContainsKey(entry))
                {
                    AddRow(entry);
                }
            }
        }

        private void AddRow(TaskEntry entry)
        {
            if (entry is null || _entriesContainer is null || _entryRows.ContainsKey(entry)) return;

            _entryRows[entry] = new TaskEntryRow(_service, entry, OnDragStart)
            {
                Parent = _entriesContainer,
                WidthSizingMode = SizingMode.Standard,
                Width = GetEntryRowWidth(),
            };
        }

        private void RemoveRow(TaskEntry entry)
        {
            if (entry is null) return;
            if (!_entryRows.TryGetValue(entry, out var row)) return;

            if (ReferenceEquals(_draggedRow, row))
            {
                CancelDrag();
            }

            row.Dispose();
            _entryRows.Remove(entry);
        }

        private bool IsBoundList(TaskListModel list)
        {
            return _boundList is not null && ReferenceEquals(_boundList, list);
        }

        private void UpdateHeaderLayout()
        {
            if (_headerPanel is null || _statusLabel is null || _entriesLabel is null || _hideButton is null) return;

            int pad = HeaderControlHorizontalPadding;
            int outerPad = pad + DragHandleWidth;
            int fixedWidth = outerPad + 20 + pad + _entriesLabel.Width + pad + 70 + pad;
            if (_hideButton.Visible)
            {
                fixedWidth += pad + 165;
            }

            _statusLabel.Width = Math.Max(0, _headerPanel.Width - fixedWidth);
        }

        private void SelectedList_Changed(object sender, StateVarChangedEventArgs<TaskListModel> e)
        {
            if (!ReferenceEquals(e.OldValue, e.NewValue))
            {
                BindSelectedList(e.NewValue);
                return;
            }

            if (!IsBoundList(e.NewValue)) return;

            SyncRowsWithBoundList();
            RefreshHeader();
            RefreshRowsLayout();
        }

        private void TrackedEntry_Changed(object sender, StateVarChangedEventArgs<TaskEntry> e)
        {
            if (_boundList is null) return;
            if (!ReferenceEquals(_boundList, _service.SelectedList)) return;

            RefreshHeader();
        }
    }
}
