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
using System.Linq;
using static Blish_HUD.ContentService;
using Button = Kenedia.Modules.Core.Controls.Button;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterRoutineStepsPanel : FlowPanel
    {
        private const int ScrollbarReservedWidth = 12;
        private const int InsertionLineLeftPadding = 5;
        private const int InsertionLineVerticalOffset = 1;
        private const int StepRowSpacing = 3;

        private readonly CharacterRoutineService _service;
        private readonly Settings _settings;
        private readonly ObservableCollection<Character_Model> _characterModels;
        private readonly Panel _insertionLine;
        private readonly Dictionary<CharacterRoutineStep, CharacterRoutineStepRow> _stepRows = [];

        private CharacterRoutineModel _boundRoutine;
        private Panel _headerPanel;
        private Panel _stepsContainer;
        private Checkbox _allStepsCheckbox;
        private Label _stepsLabel;
        private Button _nextButton;
        private Label _statusLabel;
        private Button _hideButton;
        private Separator _separator;
        private FlowPanel _newStepRow;
        private AutoSuggestComboBox<Character_Model> _characterSuggestionBox;
        private CharacterRoutineStepRow _draggedStepRow;
        private bool _isDragging;
        private bool _dragActivated;
        private bool _overrideDisplayBehaviorForDrag;
        private bool _overrideHideCompletedSteps;
        private int _pendingTargetIndex = -1;
        private bool _syncingHeaderCheckbox;
        private Blish_HUD.Controls.TextBox _stepDescriptionBox;
        private ImageButton _addStepButton;

        public CharacterRoutineStepsPanel(CharacterRoutineService service, Settings settings, ObservableCollection<Character_Model> characterModels)
        {
            _service = service;
            _settings = settings;
            _characterModels = characterModels;
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);

            _settings.CompletedRoutineStepsBehavior.SettingChanged += CompletedRoutineStepsBehavior_SettingChanged;
            _service.State.SelectedRoutine.Changed += SelectedRoutine_Changed;
            _service.State.TrackedStep.Changed += TrackedStep_Changed;
            _service.State.StepSwitchStatus.Changed += StepSwitchStatus_Changed;

            _insertionLine = new Panel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                BackgroundColor = Colors.ColonialWhite,
                Height = 3,
                Visible = false,
                ZIndex = int.MaxValue - 2,
                CaptureInput = false,
            };

            BuildHeader();
            BuildNewStepRow();

            _stepsContainer = new Panel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
            };

            _stepsContainer.Resized += (_, _) => RefreshStepRowsLayout();
            BindSelectedRoutine(_service.SelectedRoutine);
        }

        public void BindSelectedRoutine(CharacterRoutineModel selectedRoutine)
        {
            CancelDrag();
            if (_boundRoutine is not null)
            {
                _boundRoutine.PropertyChanged -= BoundRoutine_PropertyChanged;
            }

            _boundRoutine = selectedRoutine;
            _overrideHideCompletedSteps = false;

            if (_boundRoutine is not null)
            {
                _boundRoutine.PropertyChanged += BoundRoutine_PropertyChanged;
            }

            SyncRowsWithBoundRoutine();
            RefreshHeader();
            RefreshStepRowsLayout(preserveScroll: false);
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

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (!_isDragging || _stepsContainer == null || _draggedStepRow == null)
            {
                return;
            }

            _dragActivated = true;
            UpdateInsertionIndicator(GameService.Input.Mouse.Position, _stepsContainer.AbsoluteBounds);
        }

        protected override void DisposeControl()
        {
            CancelDrag();

            if (_boundRoutine is not null)
            {
                _boundRoutine.PropertyChanged -= BoundRoutine_PropertyChanged;
            }

            _service.State.SelectedRoutine.Changed -= SelectedRoutine_Changed;
            _service.State.TrackedStep.Changed -= TrackedStep_Changed;
            _service.State.StepSwitchStatus.Changed -= StepSwitchStatus_Changed;
            _settings.CompletedRoutineStepsBehavior.SettingChanged -= CompletedRoutineStepsBehavior_SettingChanged;
            _headerPanel.Resized -= HeaderPanel_Resized;

            foreach (var row in _stepRows.Values.ToList())
            {
                row.Dispose();
            }

            _stepRows.Clear();
            _insertionLine?.Dispose();

            base.DisposeControl();
        }

        private void BuildHeader()
        {
            _headerPanel = new Panel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Width = Width,
            };
            _headerPanel.Resized += HeaderPanel_Resized;

            _allStepsCheckbox = new Checkbox()
            {
                Parent = _headerPanel,
                Location = new(10, 0),
                Width = 20,
                Height = 28,
                CheckedChangedAction = (isChecked) =>
                {
                    if (_syncingHeaderCheckbox) return;

                    _service.SetAllRoutineStepsCompletion(isChecked);
                },
            };

            _nextButton = new Button()
            {
                Parent = _headerPanel,
                Text = strings.Next,
                Width = 90,
                Height = 28,
                Location = new Point(500, 0),
                ClickAction = () => _service.SwitchToNextIncompleteRoutineStep(),
            };

            _stepsLabel = new Label()
            {
                Parent = _headerPanel,
                Text = "Steps",
                Font = Content.DefaultFont16,
                AutoSizeWidth = false,
                Height = 28,
                Width = _headerPanel.ContentRegion.Right - _allStepsCheckbox.Right - 10 - _nextButton.Width,
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
                BasicTooltipText = strings.CompletedRoutineStepsHiddenTooltip,
                ClickAction = ToggleOverrideHideCompletedSteps,
            };

            _separator = new Separator()
            {
                Parent = this,
                Height = 1,
                Color = Color.LightGray * 0.5f,
            };
        }

        private void BuildNewStepRow()
        {
            _newStepRow = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _addStepButton = new ImageButton()
            {
                Parent = _newStepRow,
                Texture = AsyncTexture2D.FromAssetId(155902),
                DisabledTexture = AsyncTexture2D.FromAssetId(155903),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                Size = new Point(30, 30),
                BasicTooltipText = strings.AddRoutineStep,
                Enabled = true,
                ClickAction = (_) => _service.AddRoutineStep(_characterSuggestionBox.Selected?.Name, _stepDescriptionBox.Text),
            };

            _characterSuggestionBox = new AutoSuggestComboBox<Character_Model>()
            {
                Parent = _newStepRow,
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

            _stepDescriptionBox = new Blish_HUD.Controls.TextBox()
            {
                Parent = _newStepRow,
                PlaceholderText = strings.RoutineStepDescriptionPlaceholder,
                Width = 515,
                Height = 30,
            };
        }

        private void UpdateLayout()
        {
            _separator?.SetSize(Width, _separator.Height);
            _stepDescriptionBox?.SetSize(_stepDescriptionBox?.Parent?.ContentRegion.Right - (_characterSuggestionBox?.Right ?? 0) - 15);
            _characterSuggestionBox?.MaxSuggestionHeight = Height - 20;

            _nextButton?.SetLocation(new Point(_headerPanel?.Width - _nextButton.Width ?? 0, _nextButton.Location.Y));
            _stepsLabel?.SetLocation(new Point(_characterSuggestionBox?.Left ?? 0, _stepsLabel.Location.Y));
            _statusLabel?.SetLocation(new Point(_stepDescriptionBox?.Left ?? 0, _statusLabel.Location.Y));
            _statusLabel?.SetSize(_stepDescriptionBox?.Width - 5 - (_nextButton?.Width ?? 0), _stepsLabel.Height);
        }

        private void CompletedRoutineStepsBehavior_SettingChanged(object sender, ValueChangedEventArgs<Settings.CompletedRoutineStepsDisplayBehavior> e)
        {
            if (_isDragging)
            {
                return;
            }

            RefreshHeader();
            RefreshStepRowsLayout();
        }

        private void ToggleOverrideHideCompletedSteps()
        {
            _overrideHideCompletedSteps = !_overrideHideCompletedSteps;
            RefreshHeader();
            RefreshStepRowsLayout();
        }

        private void UpdateInsertionIndicator(Point mousePos, Rectangle containerBounds)
        {
            var visibleRows = _stepRows.Values.Where(row => row.Visible).ToList();

            int dragIndex = _boundRoutine?.RoutineSteps.IndexOf(_draggedStepRow.Step) ?? -1;
            int insertionVisualIndex = visibleRows.Count;
            int insertionY = visibleRows.Count > 0 ? visibleRows.Last().AbsoluteBounds.Bottom : containerBounds.Top;

            for (int i = 0; i < visibleRows.Count; i++)
            {
                var rowBounds = visibleRows[i].AbsoluteBounds;
                int rowMiddle = rowBounds.Top + rowBounds.Height / 2;

                if (mousePos.Y < rowMiddle)
                {
                    insertionVisualIndex = i;
                    insertionY = rowBounds.Top;
                    break;
                }
            }

            int targetIndex = insertionVisualIndex <= dragIndex
                ? insertionVisualIndex
                : insertionVisualIndex - 1;

            _pendingTargetIndex = targetIndex;

            bool withinBounds = insertionY >= containerBounds.Top && insertionY <= containerBounds.Bottom;
            if (targetIndex != dragIndex && withinBounds)
            {
                _insertionLine.Location = new Point(containerBounds.Left + InsertionLineLeftPadding, insertionY - InsertionLineVerticalOffset);
                _insertionLine.Width = Math.Max(0, containerBounds.Width - 2 * InsertionLineLeftPadding - (ScrollbarReservedWidth - 10));
                _insertionLine.Visible = true;
            }
            else
            {
                _insertionLine.Visible = false;
            }
        }

        private void OnDragStart(CharacterRoutineStepRow row)
        {
            if (_isDragging || _boundRoutine is null) return;

            _overrideDisplayBehaviorForDrag = true;
            RefreshHeader();
            RefreshStepRowsLayout();

            _stepRows.TryGetValue(row.Step, out _draggedStepRow);
            if (_draggedStepRow is null)
            {
                _overrideDisplayBehaviorForDrag = false;
                RefreshHeader();
                RefreshStepRowsLayout();
                return;
            }

            _isDragging = true;
            _dragActivated = false;
            _draggedStepRow.IsDragging = true;
            _pendingTargetIndex = _boundRoutine.RoutineSteps.IndexOf(row.Step);

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

            var step = _draggedStepRow?.Step;
            int targetIndex = _pendingTargetIndex;
            bool shouldReorder = _dragActivated
                && step is not null
                && targetIndex >= 0
                && targetIndex != _boundRoutine?.RoutineSteps.IndexOf(step);

            if (_draggedStepRow is not null)
            {
                _draggedStepRow.IsDragging = false;
            }

            _draggedStepRow = null;
            _isDragging = false;
            _dragActivated = false;
            _overrideDisplayBehaviorForDrag = false;
            _pendingTargetIndex = -1;

            if (shouldReorder)
            {
                _service.ReorderRoutineStep(step, targetIndex);
            }

            RefreshHeader();
            RefreshStepRowsLayout();
        }

        private void CancelDrag()
        {
            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseReleased;

            if (_draggedStepRow is not null)
            {
                _draggedStepRow.IsDragging = false;
            }

            bool hadDragState = _isDragging || _overrideDisplayBehaviorForDrag;
            _draggedStepRow = null;
            _isDragging = false;
            _dragActivated = false;
            _overrideDisplayBehaviorForDrag = false;
            _pendingTargetIndex = -1;
            _insertionLine.Visible = false;

            if (hadDragState)
            {
                RefreshHeader();
                RefreshStepRowsLayout();
            }
        }

        private void HeaderPanel_Resized(object sender, ResizedEventArgs e)
        {
            UpdateLayout();
        }

        private Settings.CompletedRoutineStepsDisplayBehavior GetEffectiveBehavior()
        {
            return _overrideDisplayBehaviorForDrag
                ? Settings.CompletedRoutineStepsDisplayBehavior.Nothing
                : _settings.CompletedRoutineStepsBehavior.Value;
        }

        private IEnumerable<CharacterRoutineStep> GetDisplayedRoutineSteps(CharacterRoutineModel characterRoutine)
        {
            if (characterRoutine is null) return [];

            var orderedRoutineSteps = characterRoutine.RoutineSteps.AsEnumerable();
            return GetEffectiveBehavior() switch
            {
                Settings.CompletedRoutineStepsDisplayBehavior.HideCompletedRoutineSteps
                    => _overrideHideCompletedSteps ? orderedRoutineSteps : orderedRoutineSteps.Where(step => !step.IsCompleted),
                Settings.CompletedRoutineStepsDisplayBehavior.MoveCompletedRoutineStepsToBottomOfDisplay
                    => orderedRoutineSteps.Where(step => !step.IsCompleted).Concat(orderedRoutineSteps.Where(step => step.IsCompleted)),
                _ => orderedRoutineSteps,
            };
        }

        private void CharacterSuggestionBox_SelectedItemChanged(object sender, Core.Models.ValueChangedEventArgs<Character_Model> e)
        {
            if (_characterSuggestionBox.Selected is not null)
            {
                _service.AddRoutineStep(_characterSuggestionBox.Selected.Name, _stepDescriptionBox.Text);
            }

            _characterSuggestionBox.Selected = null;
        }

        private int GetStepRowWidth()
        {
            return _stepsContainer is null
                ? 0
                : Math.Max(0, _stepsContainer.Width - (_stepsContainer.HasVisibleVerticalScrollbar() ? (ScrollbarReservedWidth + 10) : 0));
        }

        private void RefreshHeader()
        {
            bool hasRoutine = _boundRoutine is not null;
            _headerPanel.Visible = hasRoutine;
            _stepsContainer.Visible = hasRoutine;

            if (!hasRoutine)
            {
                return;
            }

            int completedCount = _boundRoutine.RoutineSteps.Count(step => step.IsCompleted);
            int totalCount = _boundRoutine.RoutineSteps.Count;
            _stepsLabel.Text = $"Steps ({completedCount}/{totalCount})";

            bool allChecked = totalCount > 0 && completedCount == totalCount;
            _syncingHeaderCheckbox = true;
            _allStepsCheckbox.Checked = allChecked;
            _allStepsCheckbox.BasicTooltipText = allChecked ? strings.UncheckAll : strings.CheckAll;
            _syncingHeaderCheckbox = false;

            CharacterRoutineStep pendingCompletionStep = _service.GetTrackedStepForSelectedRoutine();
            int incompleteCount = _boundRoutine.RoutineSteps.Count(step => step.Enabled && !step.IsCompleted);
            bool hasIncompleteSteps = incompleteCount > 0;
            UpdateNextButton(pendingCompletionStep, incompleteCount, hasIncompleteSteps);
            UpdateStatusLabel(pendingCompletionStep);

            bool canUnhideCompletedSteps = GetEffectiveBehavior() == Settings.CompletedRoutineStepsDisplayBehavior.HideCompletedRoutineSteps
                && _boundRoutine.RoutineSteps.Any(step => step.IsCompleted);
            _hideButton.Text = _overrideHideCompletedSteps ? strings.HideComplete : strings.UnhideComplete;
            _hideButton.Visible = canUnhideCompletedSteps;
            _hideButton.Enabled = canUnhideCompletedSteps;
        }

        private void UpdateNextButton(CharacterRoutineStep pendingCompletionStep, int incompleteCount, bool hasIncompleteSteps)
        {
            if (!hasIncompleteSteps)
            {
                _nextButton.Text = strings.Next;
                _nextButton.Enabled = false;
                _nextButton.BasicTooltipText = strings.AllRoutineStepsComplete;
                return;
            }

            CharacterRoutineStepSwitchStatus switchStatus = _service.State.StepSwitchStatus.Value;
            bool isReadyToComplete = pendingCompletionStep is not null && switchStatus == CharacterRoutineStepSwitchStatus.ReadyToComplete;
            bool isReadyToFinish = incompleteCount == 1 && isReadyToComplete;

            _nextButton.Text = switchStatus == CharacterRoutineStepSwitchStatus.Failed
                ? strings.Retry
                : isReadyToFinish ? strings.Finish : strings.Next;

            switch (switchStatus)
            {
                case CharacterRoutineStepSwitchStatus.Switching:
                    _nextButton.Enabled = false;
                    _nextButton.BasicTooltipText = string.Format(strings.CharacterSwap_SwitchTo, pendingCompletionStep?.CharacterName);
                    break;
                case CharacterRoutineStepSwitchStatus.Failed:
                    _nextButton.Enabled = true;
                    _nextButton.BasicTooltipText = string.Format(strings.RoutineStepSwitchFailed, pendingCompletionStep?.CharacterName);
                    break;
                case CharacterRoutineStepSwitchStatus.CharacterNotFound:
                    _nextButton.Enabled = false;
                    _nextButton.BasicTooltipText = string.Format(strings.RoutineStepCharacterNotFound, pendingCompletionStep?.CharacterName);
                    break;
                case CharacterRoutineStepSwitchStatus.CharacterNotAssigned:
                    _nextButton.Enabled = false;
                    _nextButton.BasicTooltipText = strings.RoutineStepCharacterNotAssigned;
                    break;
                case CharacterRoutineStepSwitchStatus.ReadyToComplete:
                    _nextButton.Enabled = true;
                    _nextButton.BasicTooltipText = isReadyToFinish
                        ? strings.CompleteCharacterRoutine
                        : string.Format(strings.NextClickMarksComplete, pendingCompletionStep?.CharacterName);
                    break;
                default:
                    _nextButton.Enabled = true;
                    _nextButton.BasicTooltipText = strings.SwitchToFirstIncomplete;
                    break;
            }
        }

        private void RefreshStepRowsLayout(bool preserveScroll = true)
        {
            if (_stepsContainer is null || _boundRoutine is null) return;

            int previousScrollOffset = preserveScroll ? _stepsContainer.VerticalScrollOffset : 0;
            int rowWidth = GetStepRowWidth();

            var displayedSteps = GetDisplayedRoutineSteps(_boundRoutine).ToList();
            var displayedStepSet = new HashSet<CharacterRoutineStep>(displayedSteps);

            int y = 0;
            foreach (var step in displayedSteps)
            {
                if (!_stepRows.TryGetValue(step, out var row))
                {
                    continue;
                }

                row.Visible = true;
                row.Width = rowWidth;
                row.Location = new Point(0, y);
                y += row.Height + StepRowSpacing;
            }

            foreach (var row in _stepRows)
            {
                if (!displayedStepSet.Contains(row.Key))
                {
                    row.Value.Visible = false;
                }
            }

            if (preserveScroll)
            {
                int maxOffset = Math.Max(0, y - _stepsContainer.ContentRegion.Height);
                _stepsContainer.VerticalScrollOffset = Math.Max(0, Math.Min(previousScrollOffset, maxOffset));
            }
        }

        private void SyncRowsWithBoundRoutine()
        {
            if (_boundRoutine is null)
            {
                foreach (var step in _stepRows.Keys.ToList())
                {
                    RemoveRow(step);
                }

                return;
            }

            var validSteps = new HashSet<CharacterRoutineStep>(_boundRoutine.RoutineSteps);

            foreach (var step in _stepRows.Keys.ToList())
            {
                if (!validSteps.Contains(step))
                {
                    RemoveRow(step);
                }
            }

            foreach (var step in _boundRoutine.RoutineSteps)
            {
                if (!_stepRows.ContainsKey(step))
                {
                    AddRow(step);
                }
            }
        }

        private void AddRow(CharacterRoutineStep step)
        {
            if (step is null || _stepsContainer is null || _stepRows.ContainsKey(step)) return;

            _stepRows[step] = new CharacterRoutineStepRow(_service, _characterModels, step, OnDragStart)
            {
                Parent = _stepsContainer,
                WidthSizingMode = SizingMode.Standard,
                Width = GetStepRowWidth(),
            };
        }

        private void RemoveRow(CharacterRoutineStep step)
        {
            if (step is null) return;
            if (!_stepRows.TryGetValue(step, out var row)) return;

            if (ReferenceEquals(_draggedStepRow, row))
            {
                CancelDrag();
            }

            row.Dispose();
            _stepRows.Remove(step);
        }

        private bool IsBoundRoutine(CharacterRoutineModel routine)
        {
            return _boundRoutine is not null && ReferenceEquals(_boundRoutine, routine);
        }

        private void SelectedRoutine_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineModel> e)
        {
            if (!ReferenceEquals(e.OldValue, e.NewValue))
            {
                BindSelectedRoutine(e.NewValue);
                return;
            }

            if (!IsBoundRoutine(e.NewValue)) return;

            SyncRowsWithBoundRoutine();
            RefreshHeader();
            RefreshStepRowsLayout();
        }

        private void TrackedStep_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineStep> e)
        {
            if (_boundRoutine is null || !ReferenceEquals(_boundRoutine, _service.SelectedRoutine))
            {
                return;
            }

            RefreshHeader();
        }

        private void StepSwitchStatus_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineStepSwitchStatus> e)
        {
            if (_boundRoutine is null || !ReferenceEquals(_boundRoutine, _service.SelectedRoutine))
            {
                return;
            }

            RefreshHeader();
        }

        private void BoundRoutine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_boundRoutine is null || !ReferenceEquals(sender, _boundRoutine))
            {
                return;
            }

            SyncRowsWithBoundRoutine();
            RefreshHeader();
            RefreshStepRowsLayout();
        }

        private void UpdateStatusLabel(CharacterRoutineStep pendingCompletionStep)
        {
            if (pendingCompletionStep is null)
            {
                _statusLabel.Text = string.Empty;
                _statusLabel.TextColor = Color.LightGray;
                return;
            }

            string characterName = pendingCompletionStep.CharacterName;
            switch (_service.State.StepSwitchStatus.Value)
            {
                case CharacterRoutineStepSwitchStatus.Switching:
                    _statusLabel.Text = string.Format(strings.CharacterSwap_SwitchTo, characterName);
                    _statusLabel.TextColor = Color.LightYellow;
                    break;
                case CharacterRoutineStepSwitchStatus.ReadyToComplete:
                    _statusLabel.Text = string.Format(strings.NextClickMarksComplete, characterName);
                    _statusLabel.TextColor = Color.LightGreen;
                    break;
                case CharacterRoutineStepSwitchStatus.Failed:
                    _statusLabel.Text = string.Format(strings.RoutineStepSwitchFailed, characterName);
                    _statusLabel.TextColor = Color.OrangeRed;
                    break;
                case CharacterRoutineStepSwitchStatus.CharacterNotFound:
                    _statusLabel.Text = string.Format(strings.RoutineStepCharacterNotFound, characterName);
                    _statusLabel.TextColor = Color.OrangeRed;
                    break;
                case CharacterRoutineStepSwitchStatus.CharacterNotAssigned:
                    _statusLabel.Text = strings.RoutineStepCharacterNotAssigned;
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
