using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
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

        private FlowPanel _entriesContainer;
        private TaskEntryRow _draggedRow;
        private bool _isDragging;
        private bool _dragActivated;
        private bool _overrideDisplayBehaviorForDrag;
        private bool _overrideHideCompletedTasks;
        private int _pendingTargetOrder = -1;

        public TaskEntriesPanel(TaskListService service, Settings settings)
        {
            _service = service;
            _settings = settings;

            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);

            _settings.CompletedTasksBehavior.SettingChanged += CompletedTasksBehavior_SettingChanged;
            _overrideHideCompletedTasks = false;

            _insertionLine = new Panel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = new Color(255, 200, 50, 220),
                Height = 3,
                Visible = false,
                ZIndex = int.MaxValue - 2,
                CaptureInput = false,
            };
        }

        public void Populate()
        {
            CancelDrag();
            ClearChildren();

            var selectedList = _service.SelectedList;
            if (selectedList is null) return;

            var (allEntriesCheckbox, syncingState) = BuildHeader(selectedList);
            BuildEntryRows(selectedList, allEntriesCheckbox, syncingState);
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
            _settings.CompletedTasksBehavior.SettingChanged -= CompletedTasksBehavior_SettingChanged;
            _insertionLine?.Dispose();
            base.DisposeControl();
        }

        private void CompletedTasksBehavior_SettingChanged(object sender, ValueChangedEventArgs<Settings.CompletedTasksDisplayBehavior> e)
        {
            if (!_isDragging)
            {
                Populate();
            }
        }

        private void ToggleOverrideHideCompletedTasks()
        {
            _overrideHideCompletedTasks = !_overrideHideCompletedTasks;
            Populate();
        }

        private void UpdateInsertionIndicator(Point mousePos, Rectangle containerBounds)
        {
            var allRows = _entriesContainer.Children.OfType<TaskEntryRow>()
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
            if (_isDragging) return;

            _overrideDisplayBehaviorForDrag = true;
            Populate();

            _draggedRow = _entriesContainer?.Children
                .OfType<TaskEntryRow>()
                .FirstOrDefault(r => ReferenceEquals(r.Entry, row.Entry));

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
            if (!_dragActivated) return;

            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseReleased;
            _insertionLine.Visible = false;

            var entry = _draggedRow?.Entry;
            int targetOrder = _pendingTargetOrder;

            if (_draggedRow != null)
            {
                _draggedRow.IsDragging = false;
            }

            _draggedRow = null;
            _isDragging = false;
            _dragActivated = false;
            _overrideDisplayBehaviorForDrag = false;
            _pendingTargetOrder = -1;

            if (entry != null && targetOrder >= 0 && targetOrder != entry.Order)
            {
                _service.ReorderEntry(entry, targetOrder);
            }
            else
            {
                Populate();
            }
        }

        private void CancelDrag()
        {
            if (!_isDragging) return;

            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseReleased;

            if (_draggedRow != null)
            {
                _draggedRow.IsDragging = false;
            }

            _draggedRow = null;
            _isDragging = false;
            _dragActivated = false;
            _overrideDisplayBehaviorForDrag = false;
            _pendingTargetOrder = -1;
            _insertionLine.Visible = false;
        }

        private (Checkbox checkbox, CheckboxSyncState syncState) BuildHeader(TaskListModel selectedList)
        {
            var headerPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(HeaderControlHorizontalPadding, 0),
                OuterControlPadding = new Vector2(HeaderControlHorizontalPadding + DragHandleWidth, 0),
            };

            var syncState = new CheckboxSyncState();
            bool allChecked = selectedList.Entries.Count > 0 && selectedList.Entries.All(e => e.Completed);

            var allEntriesCheckbox = new Checkbox()
            {
                Parent = headerPanel,
                Checked = allChecked,
                Width = 20,
                Height = 20,
                BasicTooltipText = allChecked ? "Uncheck All" : "Check All",
                CheckedChangedAction = (isChecked) =>
                {
                    if (syncState.IsSyncing) return;

                    _service.SetAllEntriesCompletion(isChecked);
                },
            };

            _ = new Label()
            {
                Parent = headerPanel,
                Text = $"Entries ({selectedList.Entries.Count})",
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            TaskEntry pendingCompletionEntry = _service.GetTrackedEntryForSelectedList();
            bool hasIncompleteEntries = selectedList.Entries.Any(e => !e.Completed);

            _ = new Button()
            {
                Parent = headerPanel,
                Text = "Next",
                Width = 70,
                Height = 28,
                Enabled = hasIncompleteEntries,
                BasicTooltipText = hasIncompleteEntries
                    ? "Switch to the first incomplete task entry."
                    : "All tasks in this list are complete.",
                ClickAction = () => _service.SwitchToNextIncompleteEntry(),
            };

            bool canUnhideCompletedTasks = GetEffectiveBehavior() == Settings.CompletedTasksDisplayBehavior.HideCompletedTasks
                                           && selectedList.Entries.Any(e => e.Completed);
            _ = new Button()
            {
                Parent = headerPanel,
                Text = _overrideHideCompletedTasks ? "Hide Complete" : "Unhide Complete",
                Width = 165,
                Height = 28,
                Visible = canUnhideCompletedTasks,
                Enabled = canUnhideCompletedTasks,
                BasicTooltipText = "Completed tasks are hidden based on your settings.",
                ClickAction = () => ToggleOverrideHideCompletedTasks(),
            };

            _ = new Label()
            {
                Parent = headerPanel,
                Text = pendingCompletionEntry is not null
                    ? $"Next click marks '{pendingCompletionEntry.CharacterName}' complete."
                    : "",
                AutoSizeHeight = true,
                Width = 380,
                Height = 28,
                VerticalAlignment = VerticalAlignment.Middle,
                TextColor = pendingCompletionEntry is not null ? Color.LightGreen : Color.LightGray,
            };

            return (allEntriesCheckbox, syncState);
        }

        private void BuildEntryRows(TaskListModel selectedList, Checkbox allEntriesCheckbox, CheckboxSyncState syncState)
        {
            _entriesContainer = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, EntryRowSpacing),
                CanScroll = true,
            };

            _entriesContainer.Resized += (_, _) => UpdateEntryRowWidths();

            int rowWidth = GetEntryRowWidth();
            foreach (var entry in GetDisplayedEntries(selectedList))
            {
                _ = new TaskEntryRow(_service, entry, () =>
                {
                    syncState.IsSyncing = true;
                    bool nowAllChecked = selectedList.Entries.Count > 0 && selectedList.Entries.All(e => e.Completed);
                    allEntriesCheckbox.Checked = nowAllChecked;
                    allEntriesCheckbox.BasicTooltipText = nowAllChecked ? "Uncheck All" : "Check All";
                    syncState.IsSyncing = false;
                }, OnDragStart)
                {
                    Parent = _entriesContainer,
                    WidthSizingMode = SizingMode.Standard,
                    Width = rowWidth,
                };
            }

            UpdateEntryRowWidths();
        }

        private Settings.CompletedTasksDisplayBehavior GetEffectiveBehavior()
        {
            return _overrideDisplayBehaviorForDrag
                ? Settings.CompletedTasksDisplayBehavior.Nothing
                : _settings.CompletedTasksBehavior.Value;
        }

        private IOrderedEnumerable<TaskEntry> GetEntriesInSavedOrder(TaskListModel selectedList)
        {
            return selectedList.Entries.OrderBy(e => e.Order);
        }

        private System.Collections.Generic.IEnumerable<TaskEntry> GetDisplayedEntries(TaskListModel selectedList)
        {
            var orderedEntries = GetEntriesInSavedOrder(selectedList);
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

        private void UpdateEntryRowWidths()
        {
            if (_entriesContainer == null) return;

            int rowWidth = GetEntryRowWidth();
            foreach (var row in _entriesContainer.Children.OfType<TaskEntryRow>())
            {
                row.Width = rowWidth;
            }
        }

        private class CheckboxSyncState
        {
            public bool IsSyncing { get; set; }
        }
    }
}
