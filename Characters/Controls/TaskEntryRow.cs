using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        private readonly Action _onCompletionChanged;
        private readonly Action<TaskEntryRow> _onDragStartRequested;
        private readonly bool _isEditing;
        private bool _handleHovered;

        public TaskEntry Entry => _entry;

        public bool IsDragging { get; set; }

        public TaskEntryRow(TaskListService service, TaskEntry entry, Action onCompletionChanged, Action<TaskEntryRow> onDragStartRequested)
        {
            _service = service;
            _entry = entry;
            _onCompletionChanged = onCompletionChanged;
            _onDragStartRequested = onDragStartRequested;

            // WidthSizingMode = SizingMode.Fill;
            Height = 32;

            _isEditing = _service.EditingEntry == _entry;
            bool isTracked = !_isEditing && !_entry.Completed && _service.GetTrackedEntryForSelectedList() == _entry;

            BackgroundColor = ResolveBackgroundColor();

            int checkboxX = _isEditing ? 5 : HandleWidth + 4;

            _ = new Checkbox()
            {
                Parent = this,
                Checked = _entry.Completed,
                Location = new Point(checkboxX, 8),
                Width = 20,
                CheckedChangedAction = (b) =>
                {
                    _service.SetEntryCompletion(_entry, b);
                    bool tracked = !b && _service.GetTrackedEntryForSelectedList() == _entry;
                    BackgroundColor = ResolveBackgroundColor();
                    _onCompletionChanged?.Invoke();
                },
            };

            if (_isEditing)
            {
                BuildEditMode();
            }
            else
            {
                BuildViewMode();
            }
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

        private void BuildEditMode()
        {
            var editNameBox = new TextBox()
            {
                Parent = this,
                Text = _entry.CharacterName,
                Location = new Point(30, 2),
                Width = 155,
                Height = 28,
            };

            var editDescBox = new TextBox()
            {
                Parent = this,
                Text = _entry.Description ?? string.Empty,
                PlaceholderText = "Description (optional)",
                Location = new Point(190, 2),
                Width = 370,
                Height = 28,
            };

            var saveButton = new Button()
            {
                Parent = this,
                Text = "Save",
                Width = 50,
                Height = 28,
                ClickAction = () =>
                {
                    string newName = editNameBox.Text?.Trim();
                    if (!string.IsNullOrEmpty(newName))
                    {
                        _service.SaveEditing(newName, editDescBox.Text);
                    }
                },
            };

            var cancelButton = new Button()
            {
                Parent = this,
                Text = "Cancel",
                Width = 55,
                Height = 28,
                ClickAction = () => _service.CancelEditing(),
            };

            Resized += (s, e) =>
            {
                var cancelButtonX = Width - CancelButtonWidth - EditButtonsRightPadding;
                cancelButton.Location = new Point(cancelButtonX, EditButtonsY);
                var saveButtonX = Width - CancelButtonWidth - EditButtonsSpacing - SaveButtonWidth - EditButtonsRightPadding;
                saveButton.Location = new Point(saveButtonX, EditButtonsY);
            };
        }

        private void BuildViewMode()
        {
            int labelX = HandleWidth + 4 + 20 + 4;
            int descX = labelX + 165;

            _ = new Label()
            {
                Parent = this,
                Text = _entry.CharacterName,
                Location = new Point(labelX, 0),
                Width = 160,
                Height = 32,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = Content.DefaultFont14,
                TextColor = ContentService.Colors.ColonialWhite,
            };

            if (!string.IsNullOrEmpty(_entry.Description))
            {
                _ = new Label()
                {
                    Parent = this,
                    Text = _entry.Description,
                    Location = new Point(descX, 0),
                    Width = 250,
                    Height = 32,
                    VerticalAlignment = VerticalAlignment.Middle,
                    Font = Content.DefaultFont12,
                    TextColor = Color.LightGray,
                };
            }

            var switchButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(784346),
                Size = new Point(16, 16),
                BasicTooltipText = "Switch to Character",
                ClickAction = (x) => _service.RequestSwitchToCharacter(_entry.CharacterName?.Trim()),
            };

            var editButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175780),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175779),
                Size = new Point(16, 16),
                BasicTooltipText = "Edit Entry",
                ClickAction = (m) => _service.StartEditing(_entry),
            };

            var removeButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2175783),
                HoveredTexture = AsyncTexture2D.FromAssetId(2175782),
                ClickedTexture = AsyncTexture2D.FromAssetId(2175784),
                Size = new Point(16, 16),
                BasicTooltipText = "Remove Entry",
                ClickAction = (m) => _service.RemoveEntry(_entry),
            };

            Resized += (s, e) =>
            {
                int x = Width - ButtonRightPadding;
                removeButton.Location = new Point(x -= ButtonSize, ButtonY);
                editButton.Location = new Point(x -= ButtonSpacing + ButtonSize, ButtonY);
                switchButton.Location = new Point(x -= ButtonSpacing + ButtonSize, ButtonY);
            };
        }

        private Color ResolveBackgroundColor() {
            bool isTracked = !_isEditing && !_entry.Completed && _service.GetTrackedEntryForSelectedList() == _entry;

            var BackgroundColor = 
                  _isEditing ? BackgroundEditing
                : _entry.Completed ? BackgroundCompleted
                : isTracked ? BackgroundTracked
                            : BackgroundDefault;
            return BackgroundColor;
        }
    }
}
