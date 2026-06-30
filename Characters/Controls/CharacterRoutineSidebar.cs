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
    public class CharacterRoutineSidebar : Panel
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

        private readonly CharacterRoutineService _service;
        private readonly FlowPanel _headerPanel;
        private readonly FlowPanel _listPanel;
        private readonly TextBox _searchBox;
        private readonly Dictionary<Guid, SidebarEntry> _listRoutineEntries = [];

        public CharacterRoutineSidebar(CharacterRoutineService service)
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
                Text = strings.NewCharacterRoutine,
                Width = HeaderControlWidth,
                Height = 30,
                ClickAction = () => _service.CreateNewRoutine(),
            };

            _searchBox = new TextBox()
            {
                Parent = _headerPanel,
                PlaceholderText = strings.SearchCharacterRoutines,
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

            _service.State.CharacterRoutines.Changed += CharacterRoutines_Changed;
            _service.State.SelectedRoutine.Changed += SelectedRoutine_Changed;

            foreach (var characterRoutine in _service.CharacterRoutines)
            {
                EnsureListEntry(characterRoutine);
            }

            UpdateLayout();
            UpdateFilterVisibility();
            UpdateAllEntryVisuals();

            var firstCharacterRoutine = _service.CharacterRoutines.FirstOrDefault();
            if (firstCharacterRoutine != null)
            {
                _service.SelectRoutine(firstCharacterRoutine);
            }
        }

        private void EnsureListEntry(CharacterRoutineModel characterRoutine)
        {
            if (characterRoutine is null) return;
            if (_listRoutineEntries.ContainsKey(characterRoutine.Id)) return;

            var entryPanel = new Panel()
            {
                Parent = _listPanel,
                Width = GetListEntryWidth(),
                Height = ListEntryHeight,
            };

            var nameLabel = new Label()
            {
                Parent = entryPanel,
                Text = characterRoutine.Name,
                Location = new Point(ListEntryLabelX, 0),
                AutoSizeWidth = true,
                Height = ListEntryHeight,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            entryPanel.Click += (_, _) => _service.SelectRoutine(characterRoutine);

            _listRoutineEntries[characterRoutine.Id] = new SidebarEntry()
            {
                CharacterRoutine = characterRoutine,
                EntryPanel = entryPanel,
                NameLabel = nameLabel,
            };

            UpdateListEntryVisual(characterRoutine);
        }

        private void RemoveListEntry(CharacterRoutineModel characterRoutine)
        {
            if (characterRoutine is null) return;
            if (!_listRoutineEntries.TryGetValue(characterRoutine.Id, out var entry)) return;

            entry.EntryPanel.Dispose();
            _listRoutineEntries.Remove(characterRoutine.Id);
        }

        private void UpdateFilterVisibility()
        {
            foreach (var entry in _listRoutineEntries.Values)
            {
                entry.EntryPanel.Visible = MatchesFilter(entry.CharacterRoutine);
            }

            UpdateListEntryWidths();
        }

        private bool MatchesFilter(CharacterRoutineModel characterRoutine)
        {
            string filter = _searchBox?.Text?.Trim() ?? string.Empty;
            if (filter.Length == 0) return true;

            return !string.IsNullOrEmpty(characterRoutine?.Name)
                   && characterRoutine.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void UpdateListEntryVisual(CharacterRoutineModel characterRoutine)
        {
            if (characterRoutine is null) return;
            if (!_listRoutineEntries.TryGetValue(characterRoutine.Id, out var entry)) return;

            bool isSelected = _service.SelectedRoutine?.Id == characterRoutine.Id;
            bool isCompleted = characterRoutine.RoutineSteps.Count > 0 && characterRoutine.RoutineSteps.All(step => step.IsCompleted);

            entry.EntryPanel.BackgroundColor =
                  isSelected ? SelectedBackground
                : isCompleted ? CompletedBackground
                : Color.Transparent;

            entry.NameLabel.Text = characterRoutine.Name;
            entry.NameLabel.TextColor = isCompleted ? CompletedTextColor : Color.White;
            entry.EntryPanel.Visible = MatchesFilter(characterRoutine);
        }

        private void UpdateAllEntryVisuals()
        {
            foreach (var characterRoutine in _service.CharacterRoutines)
            {
                UpdateListEntryVisual(characterRoutine);
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
            foreach (var child in _listRoutineEntries.Values)
            {
                child.EntryPanel.Width = entryWidth;
            }
        }

        protected override void DisposeControl()
        {
            _service.State.CharacterRoutines.Changed -= CharacterRoutines_Changed;
            _service.State.SelectedRoutine.Changed -= SelectedRoutine_Changed;

            base.DisposeControl();
        }

        private void CharacterRoutines_Changed(object sender, StateVarChangedEventArgs<ObservableCollection<CharacterRoutineModel>> e)
        {
            var currentLists = _service.CharacterRoutines;
            var currentIds = new HashSet<Guid>(currentLists.Select(list => list.Id));

            foreach (var existingId in _listRoutineEntries.Keys.ToList())
            {
                if (!currentIds.Contains(existingId))
                {
                    if (_listRoutineEntries.TryGetValue(existingId, out var existing))
                    {
                        existing.EntryPanel.Dispose();
                        _listRoutineEntries.Remove(existingId);
                    }
                }
            }

            foreach (var characterRoutine in currentLists)
            {
                EnsureListEntry(characterRoutine);
            }

            UpdateFilterVisibility();
            UpdateAllEntryVisuals();
        }

        private void SelectedRoutine_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineModel> e)
        {
            UpdateListEntryVisual(e.OldValue);
            UpdateListEntryVisual(e.NewValue);
        }

        private sealed class SidebarEntry
        {
            public CharacterRoutineModel CharacterRoutine { get; init; }

            public Panel EntryPanel { get; init; }

            public Label NameLabel { get; init; }
        }
    }
}
