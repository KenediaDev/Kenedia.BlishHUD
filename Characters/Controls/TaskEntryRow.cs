using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using Button = Kenedia.Modules.Core.Controls.Button;
using Checkbox = Kenedia.Modules.Core.Controls.Checkbox;
using ImageButton = Kenedia.Modules.Core.Controls.ImageButton;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TextBox = Blish_HUD.Controls.TextBox;

namespace Kenedia.Modules.Characters.Controls
{
    public class TaskEntryRow : Panel
    {
        private const int HandleWidth = 14;
        private const int ButtonSize = 16;
        private const int ButtonSpacing = 6;
        private const int ButtonRightPadding = 15;
        private const int ButtonY = 8;
        private const int SaveButtonWidth = 50;
        private const int CancelButtonWidth = 55;
        private const int EditButtonsSpacing = 5;
        private const int EditButtonsY = 2;
        private const int EditButtonsRightPadding = 10;

        private static readonly Color BackgroundDefault = new Color(40, 40, 40, 150);
        private static readonly Color BackgroundCompleted = new Color(40, 80, 40, 150);
        private static readonly Color BackgroundEditing = new Color(50, 50, 70, 180);
        private static readonly Color BackgroundTracked = new Color(80, 70, 30, 170);

        private readonly TaskListService _service;
        private readonly TaskEntry _entry;
        private readonly Action<TaskEntryRow> _onDragStartRequested;

        private readonly Checkbox _completionCheckbox;
        private readonly Label _nameLabel;
        private readonly Label _descriptionLabel;
        private readonly ImageButton _switchButton;
        private readonly ImageButton _editButton;
        private readonly ImageButton _removeButton;
        private readonly TextBox _editNameBox;
        private readonly TextBox _editDescBox;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        private bool _isEditing;
        private bool _hasAppliedModeVisibility;
        private bool _handleHovered;
        private bool _syncingCompletionCheckbox;

        public TaskEntry Entry => _entry;

        public bool IsDragging { get; set; }

        public TaskEntryRow(TaskListService service, TaskEntry entry, Action<TaskEntryRow> onDragStartRequested)
        {
            _service = service;
            _entry = entry;
            _onDragStartRequested = onDragStartRequested;

            Height = 32;
            _entry.PropertyChanged += Entry_PropertyChanged;
            _service.State.TrackedEntry.Changed += TrackedEntry_Changed;

            _completionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = _entry.Completed,
                Location = new Point(HandleWidth + 4, 8),
                Width = 20,
                CheckedChangedAction = (completed) =>
                {
                    if (_syncingCompletionCheckbox) return;

                    _service.SetEntryCompletion(_entry, completed);
                },
            };

            int labelX = HandleWidth + 4 + 20 + 4;
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
                Texture = AsyncTexture2D.FromAssetId(784346),
                Size = new Point(16, 16),
                BasicTooltipText = "Switch to Character",
                ClickAction = (_) => _service.RequestSwitchToCharacter(_entry.CharacterName?.Trim()),
            };

            _editButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175779),
                Size = new Point(16, 16),
                BasicTooltipText = "Edit Entry",
                ClickAction = (_) => SetEditMode(true),
            };

            _removeButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175783),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175782),
                ClickedTexture = AsyncTexture2D.FromAssetId(2175784),
                Size = new Point(16, 16),
                BasicTooltipText = "Remove Entry",
                ClickAction = (_) => _service.RemoveEntry(_entry),
            };

            _editNameBox = new TextBox()
            {
                Parent = this,
                Location = new Point(30, 2),
                Width = 155,
                Height = 28,
            };

            _editDescBox = new TextBox()
            {
                Parent = this,
                PlaceholderText = "Description (optional)",
                Location = new Point(190, 2),
                Width = 370,
                Height = 28,
            };

            _saveButton = new Button()
            {
                Parent = this,
                Text = "Save",
                Width = SaveButtonWidth,
                Height = 28,
                ClickAction = () =>
                {
                    string newName = _editNameBox.Text?.Trim();
                    if (string.IsNullOrEmpty(newName)) return;

                    _service.UpdateEntry(_entry, newName, _editDescBox.Text);
                    SetEditMode(false);
                },
            };

            _cancelButton = new Button()
            {
                Parent = this,
                Text = "Cancel",
                Width = CancelButtonWidth,
                Height = 28,
                ClickAction = () =>
                {
                    SetEditMode(false);
                    RefreshFromEntry();
                },
            };

            Resized += (_, _) =>
            {
                int x = Width - ButtonRightPadding;
                _removeButton.Location = new Point(x -= ButtonSize, ButtonY);
                _editButton.Location = new Point(x -= ButtonSpacing + ButtonSize, ButtonY);
                _switchButton.Location = new Point(x -= ButtonSpacing + ButtonSize, ButtonY);

                int cancelButtonX = Width - CancelButtonWidth - EditButtonsRightPadding;
                _cancelButton.Location = new Point(cancelButtonX, EditButtonsY);
                int saveButtonX = Width - CancelButtonWidth - EditButtonsSpacing - SaveButtonWidth - EditButtonsRightPadding;
                _saveButton.Location = new Point(saveButtonX, EditButtonsY);
            };

            RefreshFromEntry();
            SetEditMode(false);
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

        private void DrawDragHandle(SpriteBatch spriteBatch)
        {
            var color = _handleHovered ? new Color(200, 200, 200) : new Color(100, 100, 100);
            int dotSize = 2;
            int gapH = 3;
            int gapV = 3;
            int cols = 2;
            int rows = 3;
            int totalWidth = cols * dotSize + (cols - 1) * gapH;
            int totalHeight = rows * dotSize + (rows - 1) * gapV;
            int startX = (HandleWidth - totalWidth) / 2;
            int startY = (Height - totalHeight) / 2;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int x = startX + c * (dotSize + gapH);
                    int y = startY + r * (dotSize + gapV);
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                        new Rectangle(x, y, dotSize, dotSize), Rectangle.Empty, color);
                }
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

        private void SetEditMode(bool editing)
        {
            if (_isEditing == editing && _hasAppliedModeVisibility) return;

            _isEditing = editing;

            if (_isEditing)
            {
                _editNameBox.Text = _entry.CharacterName ?? string.Empty;
                _editDescBox.Text = _entry.Description ?? string.Empty;
            }

            _completionCheckbox.Visible = !_isEditing;
            _nameLabel.Visible = !_isEditing;
            _descriptionLabel.Visible = !_isEditing && !string.IsNullOrEmpty(_entry.Description);
            _switchButton.Visible = !_isEditing;
            _editButton.Visible = !_isEditing;
            _removeButton.Visible = !_isEditing;

            _editNameBox.Visible = _isEditing;
            _editDescBox.Visible = _isEditing;
            _saveButton.Visible = _isEditing;
            _cancelButton.Visible = _isEditing;

            if (_isEditing)
            {
                _handleHovered = false;
            }

            _hasAppliedModeVisibility = true;
            RefreshVisualState();
        }

        private void RefreshFromEntry()
        {
            _syncingCompletionCheckbox = true;
            _completionCheckbox.Checked = _entry.Completed;
            _syncingCompletionCheckbox = false;

            _nameLabel.Text = _entry.CharacterName ?? string.Empty;
            _descriptionLabel.Text = _entry.Description ?? string.Empty;
            _descriptionLabel.Visible = !_isEditing && !string.IsNullOrEmpty(_entry.Description);

            if (_isEditing)
            {
                _editNameBox.Text = _entry.CharacterName ?? string.Empty;
                _editDescBox.Text = _entry.Description ?? string.Empty;
            }

            RefreshVisualState();
        }

        private Color ResolveBackgroundColor()
        {
            bool isTracked = !_isEditing && !_entry.Completed && _service.GetTrackedEntryForSelectedList() == _entry;

            var backgroundColor =
                  _isEditing ? BackgroundEditing
                : _entry.Completed ? BackgroundCompleted
                : isTracked ? BackgroundTracked
                            : BackgroundDefault;
            return backgroundColor;
        }

        public void RefreshVisualState()
        {
            BackgroundColor = ResolveBackgroundColor();
        }

        protected override void DisposeControl()
        {
            _entry.PropertyChanged -= Entry_PropertyChanged;
            _service.State.TrackedEntry.Changed -= TrackedEntry_Changed;
            base.DisposeControl();
        }

        private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskEntry.CharacterName)
                || e.PropertyName == nameof(TaskEntry.Description)
                || e.PropertyName == nameof(TaskEntry.Completed)
                || e.PropertyName == nameof(TaskEntry.Order)
                || string.IsNullOrEmpty(e.PropertyName))
            {
                RefreshFromEntry();
            }
        }

        private void TrackedEntry_Changed(object sender, StateVarChangedEventArgs<TaskEntry> e)
        {
            if (ReferenceEquals(e.NewValue, _entry) || ReferenceEquals(e.OldValue, _entry))
            {
                RefreshVisualState();
            }
        }
    }
}
