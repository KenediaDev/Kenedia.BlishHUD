using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System.Linq;
using Button = Kenedia.Modules.Core.Controls.Button;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;

namespace Kenedia.Modules.Characters.Controls
{
    public class TaskEntriesPanel : FlowPanel
    {
        private readonly TaskListService _service;

        public TaskEntriesPanel(TaskListService service)
        {
            _service = service;

            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);
        }

        public void Populate()
        {
            ClearChildren();

            var selectedList = _service.SelectedList;
            if (selectedList is null) return;

            var (allEntriesCheckbox, syncingState) = BuildHeader(selectedList);
            BuildEntryRows(selectedList, allEntriesCheckbox, syncingState);
        }

        private (Checkbox checkbox, CheckboxSyncState syncState) BuildHeader(TaskListModel selectedList)
        {
            var headerPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                OuterControlPadding = new Vector2(5, 0),
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
            var entriesContainer = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 3),
                CanScroll = true,
            };

            foreach (var entry in selectedList.Entries.OrderBy(e => e.Order))
            {
                _ = new TaskEntryRow(_service, entry, () =>
                {
                    syncState.IsSyncing = true;
                    bool nowAllChecked = selectedList.Entries.Count > 0 && selectedList.Entries.All(e => e.Completed);
                    allEntriesCheckbox.Checked = nowAllChecked;
                    allEntriesCheckbox.BasicTooltipText = nowAllChecked ? "Uncheck All" : "Check All";
                    syncState.IsSyncing = false;
                })
                {
                    Parent = entriesContainer,
                };
            }
        }

        private class CheckboxSyncState
        {
            public bool IsSyncing { get; set; }
        }
    }
}
