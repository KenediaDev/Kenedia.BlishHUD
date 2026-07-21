using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using ImageButton = Kenedia.Modules.Core.Controls.ImageButton;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TextBox = Blish_HUD.Controls.TextBox;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterRoutineStepRow : Panel
    {
        private const int HandleWidth = 14;
        private const int ButtonSize = 24;
        private const int ButtonSpacing = 6;

        private static readonly Color BackgroundDefault = new(40, 40, 40, 150);
        private static readonly Color BackgroundCompleted = new(40, 80, 40, 150);
        private static readonly Color BackgroundEditing = new(50, 50, 70, 180);
        private static readonly Color BackgroundTracked = new(80, 70, 30, 170);

        private readonly CharacterRoutineService _service;
        private readonly ObservableCollection<Character_Model> _characterModels;
        private readonly CharacterRoutineStep _step;
        private readonly Action<CharacterRoutineStepRow> _onDragStartRequested;

        private readonly Checkbox _completionCheckbox;
        private readonly Checkbox _enabledCheckbox;
        private readonly Label _nameLabel;
        private readonly Label _descriptionLabel;
        private readonly ImageButton _switchButton;
        private readonly ImageButton _editButton;
        private readonly ImageButton _removeButton;
        private readonly AutoSuggestComboBox<Character_Model> _editCharacterSuggestionBox;
        private readonly TextBox _editDescriptionBox;
        private readonly ImageButton _saveButton;
        private readonly ImageButton _cancelButton;

        private bool _isEditing;
        private bool _hasAppliedModeVisibility;
        private bool _handleHovered;
        private bool _syncingCheckboxes;

        public CharacterRoutineStep Step => _step;

        public bool IsDragging { get; set; }

        public CharacterRoutineStepRow(CharacterRoutineService service, ObservableCollection<Character_Model> characterModels, CharacterRoutineStep step, Action<CharacterRoutineStepRow> onDragStartRequested)
        {
            _service = service;
            _characterModels = characterModels;
            _step = step;
            _onDragStartRequested = onDragStartRequested;

            Height = 32;
            _step.PropertyChanged += Step_PropertyChanged;
            _service.State.TrackedStep.Changed += TrackedStep_Changed;

            _completionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = _step.IsCompleted,
                Location = new Point(HandleWidth + 4, 8),
                Width = 20,
                CheckedChangedAction = (completed) =>
                {
                    if (_syncingCheckboxes) return;
                    _service.SetRoutineStepCompletion(_step, completed);
                },
            };

            _enabledCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = _step.Enabled,
                Location = new Point(HandleWidth, 8),
                Width = 20,
                Visible = false,
                CheckedChangedAction = (enabled) =>
                {
                    if (_syncingCheckboxes) return;
                    _service.SetRoutineStepEnabled(_step, enabled);
                },
            };

            int labelX = HandleWidth + 28;
            int descX = labelX + 165;

            _nameLabel = new Label()
            {
                Parent = this,
                Location = new Point(labelX, 0),
                Width = 160,
                Height = 32,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = Content.DefaultFont14,
                TextColor = ContentService.Colors.ColonialWhite,
            };

            _descriptionLabel = new Label()
            {
                Parent = this,
                Location = new Point(descX, 0),
                Width = 250,
                Height = 32,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = Content.DefaultFont12,
                TextColor = Color.LightGray,
            };

            _switchButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(157092),
                ClickedTexture = AsyncTexture2D.FromAssetId(157093),
                HoveredTexture = AsyncTexture2D.FromAssetId(157094),
                Size = new Point(ButtonSize, ButtonSize),
                BasicTooltipText = string.Format(strings.Switch, strings.Character),
                ClickAction = (_) => _service.RequestSwitchToCharacter(_step.CharacterName),
            };

            _editButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175779),
                Size = new Point(ButtonSize, ButtonSize),
                BasicTooltipText = strings.EditEntry,
                ClickAction = (_) => SetEditMode(true),
            };

            _removeButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175783),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175782),
                ClickedTexture = AsyncTexture2D.FromAssetId(2175784),
                Size = new Point(ButtonSize, ButtonSize),
                BasicTooltipText = strings.RemoveRoutineStep,
                ClickAction = (_) => ConfirmRemoveRoutineStep(),
            };

            _editCharacterSuggestionBox = new AutoSuggestComboBox<Character_Model>()
            {
                Parent = this,
                PlaceholderText = strings.SearchCharacterName,
                Location = new Point(labelX - 8, 2),
                Width = 155,
                Height = 28,
                MaxSuggestionHeight = 300,
                SelectableFactory = (character) => new CharacterSelectable(_editCharacterSuggestionBox, character),
                Items = characterModels,
                AllowBlankSelection = true,
                BlankSelectionText = strings.Unassigned,
            };

            _editDescriptionBox = new TextBox()
            {
                Parent = this,
                PlaceholderText = strings.RoutineStepDescriptionPlaceholder,
                Location = new Point(descX - 8, 2),
                Width = 326,
                Height = 28,
            };

            _saveButton = new ButtonImage()
            {
                Parent = this,
                BasicTooltipText = strings.Save,
                Texture = TexturesService.GetTextureFromRef(textures_common.Save, nameof(textures_common.Save)),
                HoveredTexture = TexturesService.GetTextureFromRef(textures_common.Save_Hovered, nameof(textures_common.Save_Hovered)),
                ClickedTexture = TexturesService.GetTextureFromRef(textures_common.Save_Active, nameof(textures_common.Save_Active)),
                Size = new Point(ButtonSize, ButtonSize),
                ClickAction = (_) =>
                {
                    _service.UpdateRoutineStep(_step, _editCharacterSuggestionBox.Selected?.Name, _editDescriptionBox.Text);
                    SetEditMode(false);
                },
            };

            _cancelButton = new ButtonImage()
            {
                Parent = this,
                BasicTooltipText = strings.Cancel,
                Texture = TexturesService.GetTextureFromRef(textures_common.Cancel, nameof(textures_common.Cancel)),
                HoveredTexture = TexturesService.GetTextureFromRef(textures_common.Cancel_Hovered, nameof(textures_common.Cancel_Hovered)),
                ClickedTexture = TexturesService.GetTextureFromRef(textures_common.Cancel_Active, nameof(textures_common.Cancel_Active)),
                Size = new Point(ButtonSize, ButtonSize),
                ClickAction = (_) =>
                {
                    SetEditMode(false);
                    RefreshFromStep();
                },
            };

            RefreshFromStep();
            SetEditMode(false);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            int x = Width - 5;
            _removeButton?.Location = new Point(x -= ButtonSize, (Height - ButtonSize) / 2);
            _editButton?.Location = new Point(x -= ButtonSpacing + ButtonSize, (Height - ButtonSize) / 2);
            _switchButton?.Location = new Point(x -= ButtonSpacing + ButtonSize, (Height - ButtonSize) / 2);

            _saveButton?.Location = _editButton?.Location ?? Point.Zero;
            _cancelButton?.Location = _removeButton?.Location ?? Point.Zero;

            _descriptionLabel?.Size = new Point(Math.Max(0, _switchButton?.Left ?? 0 - _descriptionLabel.Left - (ButtonSpacing * 2)), _descriptionLabel.Height);
            _editDescriptionBox?.Size = new Point(Math.Max(0, (_switchButton?.Right ?? 0) - _editDescriptionBox.Left), _editDescriptionBox.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (IsDragging)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Rectangle.Empty, Color.Black * 0.3f);
            }

            if (!_isEditing)
            {
                DrawDragHandle(spriteBatch);
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            _handleHovered = !_isEditing && RelativeMousePosition.X < HandleWidth;
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);
            _handleHovered = false;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            if (!_isEditing && RelativeMousePosition.X < HandleWidth)
            {
                _onDragStartRequested?.Invoke(this);
            }
        }

        protected override void DisposeControl()
        {
            _step.PropertyChanged -= Step_PropertyChanged;
            _service.State.TrackedStep.Changed -= TrackedStep_Changed;
            base.DisposeControl();
        }

        private async void ConfirmRemoveRoutineStep()
        {
            string stepName = !string.IsNullOrWhiteSpace(_step.CharacterName)
                ? _step.CharacterName
                : !string.IsNullOrWhiteSpace(_step.Description)
                    ? _step.Description
                    : strings.Unassigned;

            var result = await new BaseDialog(
                strings.DeleteConfirmationTitle,
                string.Format(strings.ConfirmCharacterRoutineStepDelete, stepName))
            {
                DesiredWidth = 360,
                AutoSize = true,
            }.ShowDialog();

            if (result == DialogResult.OK)
            {
                _service.RemoveRoutineStep(_step);
            }
        }

        private void DrawDragHandle(SpriteBatch spriteBatch)
        {
            Color color = _handleHovered ? new Color(200, 200, 200) : new Color(100, 100, 100);
            const int dotSize = 2;
            const int gapH = 3;
            const int gapV = 3;
            const int cols = 2;
            const int rows = 3;
            int totalWidth = cols * dotSize + (cols - 1) * gapH;
            int totalHeight = rows * dotSize + (rows - 1) * gapV;
            int startX = (HandleWidth - totalWidth) / 2;
            int startY = (Height - totalHeight) / 2;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int x = startX + col * (dotSize + gapH);
                    int y = startY + row * (dotSize + gapV);
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(x, y, dotSize, dotSize), Rectangle.Empty, color);
                }
            }
        }

        private void SetEditMode(bool editing)
        {
            if (_isEditing == editing && _hasAppliedModeVisibility) return;

            _isEditing = editing;

            if (_isEditing)
            {
                _editCharacterSuggestionBox.Selected = _characterModels.FirstOrDefault(c => c.Name.Equals(_step.CharacterName, StringComparison.OrdinalIgnoreCase));
                _editCharacterSuggestionBox.Text = _editCharacterSuggestionBox.Selected?.Name ?? _step.CharacterName ?? string.Empty;
                _editDescriptionBox.Text = _step.Description ?? string.Empty;
            }

            _completionCheckbox.Visible = !_isEditing;
            _nameLabel.Visible = !_isEditing;
            _descriptionLabel.Visible = !_isEditing && !string.IsNullOrEmpty(_step.Description);
            _switchButton.Visible = !_isEditing;
            _editButton.Visible = !_isEditing;
            _removeButton.Visible = !_isEditing;

            _enabledCheckbox.Visible = _isEditing;
            _editCharacterSuggestionBox.Visible = _isEditing;
            _editDescriptionBox.Visible = _isEditing;
            _saveButton.Visible = _isEditing;
            _cancelButton.Visible = _isEditing;

            if (_isEditing)
            {
                _handleHovered = false;
            }

            _hasAppliedModeVisibility = true;
            RefreshVisualState();
        }

        private void RefreshFromStep()
        {
            _syncingCheckboxes = true;
            _completionCheckbox.Checked = _step.IsCompleted;
            _enabledCheckbox.Checked = _step.Enabled;
            _syncingCheckboxes = false;

            _nameLabel.Text = _step.CharacterName ?? string.Empty;
            _descriptionLabel.Text = _step.Description ?? string.Empty;
            _descriptionLabel.Visible = !_isEditing && !string.IsNullOrEmpty(_step.Description);

            if (_isEditing)
            {
                _editCharacterSuggestionBox.Selected = _characterModels.FirstOrDefault(c => c.Name.Equals(_step.CharacterName, StringComparison.OrdinalIgnoreCase));
                _editCharacterSuggestionBox.Text = _editCharacterSuggestionBox.Selected?.Name ?? _step.CharacterName ?? string.Empty;
                _editDescriptionBox.Text = _step.Description ?? string.Empty;
            }

            RefreshVisualState();
        }

        private Color ResolveBackgroundColor()
        {
            bool isTracked = !_isEditing && !_step.IsCompleted && _service.GetTrackedStepForSelectedRoutine() == _step;

            return _isEditing ? BackgroundEditing
                : _step.IsCompleted ? BackgroundCompleted
                : isTracked ? BackgroundTracked
                : BackgroundDefault;
        }

        private void RefreshVisualState()
        {
            BackgroundColor = ResolveBackgroundColor();
        }

        private void Step_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterRoutineStep.CharacterName)
                || e.PropertyName == nameof(CharacterRoutineStep.Description)
                || e.PropertyName == nameof(CharacterRoutineStep.Completed)
                || e.PropertyName == nameof(CharacterRoutineStep.Enabled)
                || string.IsNullOrEmpty(e.PropertyName))
            {
                RefreshFromStep();
            }
        }

        private void TrackedStep_Changed(object sender, StateVarChangedEventArgs<CharacterRoutineStep> e)
        {
            if (ReferenceEquals(e.NewValue, _step) || ReferenceEquals(e.OldValue, _step))
            {
                RefreshVisualState();
            }
        }
    }
}
