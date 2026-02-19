using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
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
        private static readonly Color SelectedBackground = new Color(60, 60, 60, 200);
        private static readonly Color CompletedBackground = new Color(40, 80, 40, 200);
        private static readonly Color CompletedTextColor = new Color(120, 200, 120);

        private readonly TaskListService _service;
        private readonly FlowPanel _listPanel;
        private readonly TextBox _searchBox;

        public TaskListSidebar(TaskListService service, int contentHeight)
        {
            _service = service;

            Width = 200;
            HeightSizingMode = SizingMode.Fill;
            ShowBorder = true;

            var header = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                OuterControlPadding = new Vector2(5),
            };

            _ = new Button()
            {
                Parent = header,
                Text = "New List",
                Width = 180,
                Height = 30,
                ClickAction = () => _service.CreateNewList(),
            };

            _searchBox = new TextBox()
            {
                Parent = header,
                PlaceholderText = "Search lists...",
                Width = 180,
                Height = 28,
                TextChangedAction = (_) => Populate(),
            };

            _listPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 105),
                WidthSizingMode = SizingMode.Fill,
                Height = contentHeight - 110,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 2),
                CanScroll = true,
            };

            _service.ListPanelChanged += Populate;

            Populate();
        }

        public void UpdateHeight(int contentHeight)
        {
            _listPanel.Height = contentHeight - 110;
        }

        private void Populate()
        {
            _listPanel.ClearChildren();

            string filter = _searchBox?.Text?.Trim() ?? string.Empty;

            foreach (var taskList in _service.TaskLists)
            {
                if (filter.Length > 0
                    && (taskList.Name == null
                        || taskList.Name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0))
                {
                    continue;
                }

                CreateListEntry(taskList);
            }
        }

        private void CreateListEntry(TaskListModel taskList)
        {
            bool isSelected = _service.SelectedList?.Id == taskList.Id;
            bool isCompleted = taskList.Entries.Count > 0 && taskList.Entries.All(e => e.Completed);

            var entry = new Panel()
            {
                Parent = _listPanel,
                WidthSizingMode = SizingMode.Fill,
                Height = 32,
                BackgroundColor = 
                      isSelected ? SelectedBackground
                    : isCompleted ? CompletedBackground
                    : Color.Transparent,
            };

            _ = new Label()
            {
                Parent = entry,
                Text = taskList.Name,
                Location = new Point(5, 0),
                AutoSizeWidth = true,
                Height = 32,
                VerticalAlignment = VerticalAlignment.Middle,
                TextColor = isCompleted ? CompletedTextColor : Color.White,
            };

            entry.Click += (s, e) => _service.SelectList(taskList);
        }

        protected override void DisposeControl()
        {
            _service.ListPanelChanged -= Populate;

            base.DisposeControl();
        }
    }
}
