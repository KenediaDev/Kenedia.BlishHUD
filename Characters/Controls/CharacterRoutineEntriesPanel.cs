using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using static Blish_HUD.ContentService;
using Button = Kenedia.Modules.Core.Controls.Button;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterRoutineEntriesPanel : FlowPanel
    {
        private const int SCROLLBARRESERVEDWIDTH = 12;
        private const int INSERTIONLINELEFTPADDING = 5;
        private const int INSERTIONLINEVERTICALOFFSET = 1;
        private const int ENTRYROWSPACING = 3;

        private readonly CharacterRoutineService _service;
        private readonly Settings _settings;
        private readonly ObservableCollection<Character_Model> _characterModels;
        private readonly Panel _insertionLine;
        private readonly Dictionary<CharacterRoutineEntry, CharacterRoutineEntryRow> _entryRows = [];

        private CharacterRoutineModel _boundList;
        private Panel _headerPanel;
        private Panel _entriesContainer;
        private Checkbox _allRoutineEntriesCheckbox;
        private Label _entriesLabel;
        private Button _nextButton;
        private Label _statusLabel;
        private Button _hideButton;
        private Separator _separator;
        private FlowPanel _newRoutineEntryRow;
        private AutoSuggestComboBox<Character_Model> _characterSuggestionBox;
        private CharacterRoutineEntryRow _draggedRow;
        private bool _isDragging;
        private bool _dragActivated;
        private bool _overrideDisplayBehaviorForDrag;
        private bool _overrideHideCompletedRoutineEntries;
        private int _pendingTargetOrder = -1;
        private bool _syncingHeaderCheckbox;
        private Blish_HUD.Controls.TextBox _routineEntryDescriptionBox;
        private ImageButton _addRoutineEntryButton;

        public CharacterRoutineEntriesPanel(CharacterRoutineService service, Settings settings, ObservableCollection<Character_Model> character_Models)
        {
            _service = service;
            _settings = settings;
            _characterModels = character_Models;
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);

            _settings.CompletedRoutineEntriesBehavior.SettingChanged += CompletedRoutineEntriesBehavior_SettingChanged;
            _service.State.SelectedRoutine.Changed += SelectedRoutine_Changed;
            _service.State.TrackedEntry.Changed += TrackedEntry_Changed;
            _service.State.EntrySwitchStatus.Changed += EntrySwitchStatus_Changed;

            _insertionLine = new Panel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = Colors.ColonialWhite,
                Height = 3,
                Visible = false,
                ZIndex = int.MaxValue - 2,
                CaptureInput = false,
            };

            _headerPanel = new Panel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Width = Width,
            };
            _headerPanel.Resized += HeaderPanel_Resized;

            _allRoutineEntriesCheckbox = new Checkbox()
            {
                Parent = _headerPanel,
                Location = new(10, 0),
                Width = 20,
                Height = 28,
                CheckedChangedAction = (isChecked) =>
                {
                    if (_syncingHeaderCheckbox) return;

                    _service.SetAllRoutineEntriesCompletion(isChecked);
                },
            };

            _nextButton = new Button()
            {
                Parent = _headerPanel,
                Text = strings.Next,
                Width = 90,
                Height = 28,
                Location = new Point(500, 0),
                ClickAction = () => _service.SwitchToNextIncompleteRoutineEntry(),
            };

            _entriesLabel = new Label()
            {
                Parent = _headerPanel,
                Text = strings.RoutineEntries,
                Font = Content.DefaultFont16,
                AutoSizeWidth = false,
                Height = 28,
                Width = _headerPanel.ContentRegion.Right - _allRoutineEntriesCheckbox.Right - 10 - _nextButton.Width,
                Location = new Point(50, 0),
                VerticalAlignment = VerticalAlignment.Middle,
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
                BasicTooltipText = strings.CompletedRoutineEntriesHiddenTooltip,
                ClickAction = () => ToggleOverrideHideCompletedRoutineEntries(),
            };

            _separator = new Separator()
            {
                Parent = this,
                Height = 1,
                Color = Color.LightGray * 0.5f,
            };

            _newRoutineEntryRow = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _addRoutineEntryButton = new ImageButton()
            {
                Parent = _newRoutineEntryRow,
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                Size = new Point(30, 30),
                BasicTooltipText = strings.AddRoutineEntry,
                Enabled = true,
                ClickAction = (_) =>
                {
                    _service.AddRoutineEntry(_characterSuggestionBox.Selected?.Name, _routineEntryDescriptionBox.Text);
                }
            };

            _characterSuggestionBox = new AutoSuggestComboBox<Character_Model>()
            {
                Parent = _newRoutineEntryRow,
                PlaceholderText = strings.SearchCharacterName,
                Width = 160,
                Height = 30,
                MaxSuggestionHeight = 300,
                SelectableFactory = (character) => new CharacterSelectable(_characterSuggestionBox, character),
                Items = _characterModels,
                AllowBlankSelection = true,
                SetSelectedText = false,
                BlankSelectionText = strings.Unassigned,
            };
            _characterSuggestionBox.SelectedItemChanged += CharacterSuggestionBox_SelectedItemChanged;

            _routineEntryDescriptionBox = new Blish_HUD.Controls.TextBox()
            {
                Parent = _newRoutineEntryRow,
                PlaceholderText = strings.RoutineEntryDescriptionPlaceholder,
                Width = 515,
                Height = 30,
            };

            _entriesContainer = new Panel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
            };

            _entriesContainer.Resized += (_, _) => RefreshRowsLayout();
            BindSelectedRoutine(_service.SelectedRoutine);
        }

        private void CharacterRoutineEntriesPanel_Shown(object sender, EventArgs e)
        {
            OnResized(null);
        }

        public void BindSelectedRoutine(CharacterRoutineModel selectedList)
        {
            CancelDrag();
            if (_boundList is not null)
            {
                _boundList.PropertyChanged -= BoundList_PropertyChanged;
            }

            _boundList = selectedList;
            _overrideHideCompletedRoutineEntries = false;

            if (_boundList is not null)
            {
                _boundList.PropertyChanged += BoundList_PropertyChanged;
            }

            SyncRowsWithBoundList();
            RefreshHeader();
            RefreshRowsLayout(preserveScroll: false);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            UpdateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            _separator?.SetSize(Width, _separator.Height);
            _routineEntryDescriptionBox?.SetSize(_routineEntryDescriptionBox?.Parent?.ContentRegion.Right - (_characterSuggestionBox?.Right ?? 0) - 15);
            _characterSuggestionBox?.MaxSuggestionHeight = Height - 20;

            _nextButton?.SetLocation(new Point(_headerPanel?.Width - _nextButton.Width ?? 0, _nextButton.Location.Y));
            _entriesLabel?.SetLocation(new Point(_characterSuggestionBox?.Left ?? 0, _entriesLabel.Location.Y));
            _statusLabel?.SetLocation(new Point(_routineEntryDescriptionBox?.Left ?? 0, _statusLabel.Location.Y));
            _statusLabel?.SetSize(_routineEntryDescriptionBox?.Width - 5 - (_nextButton?.Width ?? 0), _entriesLabel.Height);
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
            if (_boundList is not null)
            {
                _boundList.PropertyChanged -= BoundList_PropertyChanged;
            }
            _service.State.SelectedRoutine.Changed -= SelectedRoutine_Changed;
            _service.State.TrackedEntry.Changed -= TrackedEntry_Changed;
            _service.State.EntrySwitchStatus.Changed -= EntrySwitchStatus_Changed;
            _settings.CompletedRoutineEntriesBehavior.SettingChanged -= CompletedRoutineEntriesBehavior_SettingChanged;
            _headerPanel.Resized -= HeaderPanel_Resized;

            foreach (var row in _entryRows.Values.ToList())
            {
                row.Dispose();
            }

            _entryRows.Clear();
            _insertionLine?.Dispose();
            base.DisposeControl();
        }

        private void CompletedRoutineEntriesBehavior_SettingChanged(object sender, ValueChangedEventArgs<Settings.CompletedRoutineEntriesDisplayBehavior> e)
        {
            if (!_isDragging)
            {
                RefreshHeader();
                RefreshRowsLayout();
            }
        }

        private void ToggleOverrideHideCompletedRoutineEntries()
        {
            _overrideHideCompletedRoutineEntries = !_overrideHideCompletedRoutineEntries;
            RefreshHeader();
            RefreshRowsLayout();
        }

        private void UpdateInsertionIndicator(Point mousePos, Rectangle containerBounds)
        {
            var allRows = _entryRows.Values
                .Where(r => r.Visible)
                .ToList();

            int dragIndex = _boundList?.RoutineEntries.IndexOf(_draggedRow.Entry) ?? -1;
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
                _insertionLine.Location = new Point(containerBounds.Left + INSERTIONLINELEFTPADDING, insertionY - INSERTIONLINEVERTICALOFFSET);
                _insertionLine.Width = Math.Max(0, containerBounds.Width - 2 * INSERTIONLINELEFTPADDING - (SCROLLBARRESERVEDWIDTH - 10));
                _insertionLine.Visible = true;
            }
            else
            {
                _insertionLine.Visible = false;
            }
        }

        private void OnDragStart(CharacterRoutineEntryRow row)
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
            _pendingTargetOrder = _boundList.RoutineEntries.IndexOf(row.Entry);

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
            bool shouldReorder = _dragActivated && entry != null && targetOrder >= 0 && targetOrder != _boundList?.RoutineEntries.IndexOf(entry);

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
                _service.ReorderRoutineEntry(entry, targetOrder);
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

        private void HeaderPanel_Resized(object sender, ResizedEventArgs e)
        {
            UpdateLayout();
        }

        private Settings.CompletedRoutineEntriesDisplayBehavior GetEffectiveBehavior()
        {
            return _overrideDisplayBehaviorForDrag
                ? Settings.CompletedRoutineEntriesDisplayBehavior.Nothing
                : _settings.CompletedRoutineEntriesBehavior.Value;
        }

        private IEnumerable<CharacterRoutineEntry> GetDisplayedRoutineEntries(CharacterRoutineModel characterRoutine)
        {
            if (characterRoutine is null) return [];

            var orderedRoutineEntries = characterRoutine.RoutineEntries.AsEnumerable();
            return GetEffectiveBehavior() switch
            {
                Settings.CompletedRoutineEntriesDisplayBehavior.HideCompletedRoutineEntries
                    => _overrideHideCompletedRoutineEntries ? orderedRoutineEntries : orderedRoutineEntries.Where(e => !e.Completed),
                Settings.CompletedRoutineEntriesDisplayBehavior.MoveCompletedRoutineEntriesToBottomOfDisplay
                    => orderedRoutineEntries.Where(e => !e.Completed).Concat(orderedRoutineEntries.Where(e => e.Completed)),
                _ => orderedRoutineEntries,
            };
        }

        private void CharacterSuggestionBox_SelectedItemChanged(object sender, Core.Models.ValueChangedEventArgs<Character_Model> e)
        {
            if (_characterSuggestionBox.Selected != null)
            {
                _service.AddRoutineEntry(_characterSuggestionBox.Selected?.Name, _routineEntryDescriptionBox.Text);
            }

            _characterSuggestionBox.Selected = null;
        }

        private int GetEntryRowWidth()
        {
            return _entriesContainer == null
                ? 0
                : Math.Max(0, _entriesContainer.Width - (_entriesContainer?.HasVisibleVerticalScrollbar() == true ? (SCROLLBARRESERVEDWIDTH + 10) : 0));
        }

        private void RefreshHeader()
        {
            bool hasList = _boundList is not null;
            _headerPanel.Visible = hasList;
            _entriesContainer.Visible = hasList;

            if (!hasList) return;

            int completedCount = _boundList.RoutineEntries.Count(e => e.Completed);
            int totalCount = _boundList.RoutineEntries.Count;
            _entriesLabel.Text = strings.RoutineEntries + $" ({completedCount}/{totalCount})";

            bool allChecked = totalCount > 0 && completedCount == totalCount;
            _syncingHeaderCheckbox = true;
            _allRoutineEntriesCheckbox.Checked = allChecked;
            _allRoutineEntriesCheckbox.BasicTooltipText = allChecked ? strings.UncheckAll : strings.CheckAll;
            _syncingHeaderCheckbox = false;

            CharacterRoutineEntry pendingCompletionEntry = _service.GetTrackedEntryForSelectedRoutine();
            int incompleteCount = _boundList.RoutineEntries.Count(e => e.Enabled && !e.Completed);
            bool hasIncompleteRoutineEntries = incompleteCount > 0;
            UpdateNextButton(pendingCompletionEntry, incompleteCount, hasIncompleteRoutineEntries);

            UpdateStatusLabel(pendingCompletionEntry);

            bool canUnhideCompletedRoutineEntries = GetEffectiveBehavior() == Settings.CompletedRoutineEntriesDisplayBehavior.HideCompletedRoutineEntries
                                           && _boundList.RoutineEntries.Any(e => e.Completed);
            _hideButton.Text = _overrideHideCompletedRoutineEntries ? strings.HideComplete : strings.UnhideComplete;
            _hideButton.Visible = canUnhideCompletedRoutineEntries;
            _hideButton.Enabled = canUnhideCompletedRoutineEntries;
        }

        private void UpdateNextButton(CharacterRoutineEntry pendingCompletionEntry, int incompleteCount, bool hasIncompleteRoutineEntries)
        {
            if (!hasIncompleteRoutineEntries)
            {
                _nextButton.Text = strings.Next;
                _nextButton.Enabled = false;
                _nextButton.BasicTooltipText = strings.AllRoutineEntriesComplete;
                return;
            }

            CharacterRoutineEntrySwitchStatus switchStatus = _service.State.EntrySwitchStatus.Value;
            bool isReadyToComplete = pendingCompletionEntry is not null
                && switchStatus == CharacterRoutineEntrySwitchStatus.ReadyToComplete;
            bool isReadyToFinish = incompleteCount == 1 && isReadyToComplete;

            _nextButton.Text = switchStatus == CharacterRoutineEntrySwitchStatus.Failed
                ? strings.Retry
                : isReadyToFinish ? strings.Finish : strings.Next;

            switch (switchStatus)
            {
                case CharacterRoutineEntrySwitchStatus.Switching:
                    _nextButton.Enabled = false;
                    _nextButton.BasicTooltipText = string.Format(strings.CharacterSwap_SwitchTo, pendingCompletionEntry?.CharacterName);
                    break;
                case CharacterRoutineEntrySwitchStatus.Failed:
                    _nextButton.Enabled = true;
                    _nextButton.BasicTooltipText = string.Format(strings.RoutineEntrySwitchFailed, pendingCompletionEntry?.CharacterName);
                    break;
                case CharacterRoutineEntrySwitchStatus.CharacterNotFound:
                    _nextButton.Enabled = false;
                    _nextButton.BasicTooltipText = string.Format(strings.RoutineEntryCharacterNotFound, pendingCompletionEntry?.CharacterName);
                    break;
                case CharacterRoutineEntrySwitchStatus.CharacterNotAssigned:
                    _nextButton.Enabled = false;
                    _nextButton.BasicTooltipText = strings.RoutineEntryCharacterNotAssigned;
                    break;
                case CharacterRoutineEntrySwitchStatus.ReadyToComplete:
                    _nextButton.Enabled = true;
                    _nextButton.BasicTooltipText = isReadyToFinish
                        ? strings.CompleteCharacterRoutine
                        : string.Format(strings.NextClickMarksComplete, pendingCompletionEntry?.CharacterName);
                    break;
                default:
                    _nextButton.Enabled = true;
                    _nextButton.BasicTooltipText = strings.SwitchToFirstIncomplete;
                    break;
            }
        }

        private void RefreshRowsLayout(bool preserveScroll = true)
        {
            if (_entriesContainer is null || _boundList is null) return;

            int previousScrollOffset = preserveScroll ? _entriesContainer.VerticalScrollOffset : 0;
            int rowWidth = GetEntryRowWidth();

            var displayedRoutineEntries = GetDisplayedRoutineEntries(_boundList).ToList();
            var displayedEntrySet = new HashSet<CharacterRoutineEntry>(displayedRoutineEntries);

            int y = 0;
            foreach (var entry in displayedRoutineEntries)
            {
                if (!_entryRows.TryGetValue(entry, out var row))
                {
                    continue;
                }

                row.Visible = true;
                row.Width = rowWidth;
                row.Location = new Point(0, y);
                y += row.Height + ENTRYROWSPACING;
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

            var validRoutineEntries = new HashSet<CharacterRoutineEntry>(_boundList.RoutineEntries);

            foreach (var entry in _entryRows.Keys.ToList())
            {
                if (!validRoutineEntries.Contains(entry))
                {
                    RemoveRow(entry);
                }
            }

            foreach (var entry in _boundList.RoutineEntries)
            {
                if (!_entryRows.ContainsKey(entry))
                {
                    AddRow(entry);
                }
            }
        }

        private void AddRow(CharacterRoutineEntry entry)
        {
            if (entry is null || _entriesContainer is null || _entryRows.ContainsKey(entry)) return;

            _entryRows[entry] = new CharacterRoutineEntryRow(_service, _characterModels, entry, OnDragStart)
            {
                Parent = _entriesContainer,
                WidthSizingMode = SizingMode.Standard,
                Width = GetEntryRowWidth(),
            };
        }

        private void RemoveRow(CharacterRoutineEntry entry)
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

        private bool IsBoundList(CharacterRoutineModel list)
        {
            return _boundList is not null && ReferenceEquals(_boundList, list);
        }

        private void SelectedRoutine_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineModel> e)
        {
            if (!ReferenceEquals(e.OldValue, e.NewValue))
            {
                BindSelectedRoutine(e.NewValue);
                return;
            }

            if (!IsBoundList(e.NewValue)) return;

            SyncRowsWithBoundList();
            RefreshHeader();
            RefreshRowsLayout();
        }

        private void TrackedEntry_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineEntry> e)
        {
            if (_boundList is null) return;
            if (!ReferenceEquals(_boundList, _service.SelectedRoutine)) return;

            RefreshHeader();
        }

        private void EntrySwitchStatus_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineEntrySwitchStatus> e)
        {
            if (_boundList is null || !ReferenceEquals(_boundList, _service.SelectedRoutine)) return;

            RefreshHeader();
        }

        private void BoundList_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_boundList is null || !ReferenceEquals(sender, _boundList)) return;

            SyncRowsWithBoundList();
            RefreshHeader();
            RefreshRowsLayout();
        }

        private void UpdateStatusLabel(CharacterRoutineEntry pendingCompletionEntry)
        {
            if (pendingCompletionEntry is null)
            {
                _statusLabel.Text = string.Empty;
                _statusLabel.TextColor = Color.LightGray;
                return;
            }

            string characterName = pendingCompletionEntry.CharacterName;
            switch (_service.State.EntrySwitchStatus.Value)
            {
                case CharacterRoutineEntrySwitchStatus.Switching:
                    _statusLabel.Text = string.Format(strings.CharacterSwap_SwitchTo, characterName);
                    _statusLabel.TextColor = Color.LightYellow;
                    break;
                case CharacterRoutineEntrySwitchStatus.ReadyToComplete:
                    _statusLabel.Text = string.Format(strings.NextClickMarksComplete, characterName);
                    _statusLabel.TextColor = Color.LightGreen;
                    break;
                case CharacterRoutineEntrySwitchStatus.Failed:
                    _statusLabel.Text = string.Format(strings.RoutineEntrySwitchFailed, characterName);
                    _statusLabel.TextColor = Color.OrangeRed;
                    break;
                case CharacterRoutineEntrySwitchStatus.CharacterNotFound:
                    _statusLabel.Text = string.Format(strings.RoutineEntryCharacterNotFound, characterName);
                    _statusLabel.TextColor = Color.OrangeRed;
                    break;
                case CharacterRoutineEntrySwitchStatus.CharacterNotAssigned:
                    _statusLabel.Text = strings.RoutineEntryCharacterNotAssigned;
                    _statusLabel.TextColor = Color.OrangeRed;
                    break;
                default:
                    _statusLabel.Text = string.Empty;
                    _statusLabel.TextColor = Color.LightGray;
                    break;
            }
        }
    }
}
