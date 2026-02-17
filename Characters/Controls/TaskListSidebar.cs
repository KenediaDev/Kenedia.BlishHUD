using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Button = Kenedia.Modules.Core.Controls.Button;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;

namespace Kenedia.Modules.Characters.Controls
{
    public class TaskListSidebar : Panel
    {
        private readonly TaskListService _service;
        private readonly FlowPanel _listPanel;

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

            _ = new Label()
            {
                Parent = header,
                Text = "Task Lists",
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _ = new Button()
            {
                Parent = header,
                Text = "New List",
                Width = 180,
                Height = 30,
                ClickAction = () => _service.CreateNewList(),
            };

            _listPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 70),
                WidthSizingMode = SizingMode.Fill,
                Height = contentHeight - 75,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 2),
                CanScroll = true,
            };

            _service.ListPanelChanged += Populate;

            Populate();
        }

        public void UpdateHeight(int contentHeight)
        {
            _listPanel.Height = contentHeight - 75;
        }

        private void Populate()
        {
            _listPanel.ClearChildren();

            foreach (var taskList in _service.TaskLists)
            {
                CreateListEntry(taskList);
            }
        }

        private void CreateListEntry(TaskListModel taskList)
        {
            var entry = new Panel()
            {
                Parent = _listPanel,
                WidthSizingMode = SizingMode.Fill,
                Height = 32,
                BackgroundColor = _service.SelectedList?.Id == taskList.Id
                    ? new Color(60, 60, 60, 200)
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
