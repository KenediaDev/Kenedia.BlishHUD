using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
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
                Text = "New Task List",
                Width = HeaderControlWidth,
                Height = 30,
                ClickAction = () => _service.CreateNewList(),
            };

            _searchBox = new TextBox()
            {
                Parent = _headerPanel,
                PlaceholderText = "Search lists...",
                Width = HeaderControlWidth,
                Height = 28,
                TextChangedAction = (_) => Populate(),
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

            _service.ListPanelChanged += Populate;

            UpdateLayout();
            Populate();
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

            UpdateListEntryWidths();
        }

        private void CreateListEntry(TaskListModel taskList)
        {
            bool isSelected = _service.SelectedList?.Id == taskList.Id;
            bool isCompleted = taskList.Entries.Count > 0 && taskList.Entries.All(e => e.Completed);

            var entry = new Panel()
            {
                Parent = _listPanel,
                Width = GetListEntryWidth(),
                Height = ListEntryHeight,
                BackgroundColor = 
                      isSelected ? SelectedBackground
                    : isCompleted ? CompletedBackground
                    : Color.Transparent,
            };

            _ = new Label()
            {
                Parent = entry,
                Text = taskList.Name,
                Location = new Point(ListEntryLabelX, 0),
                AutoSizeWidth = true,
                Height = ListEntryHeight,
                VerticalAlignment = VerticalAlignment.Middle,
                TextColor = isCompleted ? CompletedTextColor : Color.White,
            };

            entry.Click += (s, e) => _service.SelectList(taskList);
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
            foreach (var child in _listPanel.Children.OfType<Panel>())
            {
                child.Width = entryWidth;
            }
        }

        protected override void DisposeControl()
        {
            _service.ListPanelChanged -= Populate;

            base.DisposeControl();
        }
    }
}
