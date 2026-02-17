using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
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
        private readonly TaskListService _service;
        private readonly TaskEntry _entry;
        private readonly Action _onCompletionChanged;

        public TaskEntryRow(TaskListService service, TaskEntry entry, Action onCompletionChanged)
        {
            _service = service;
            _entry = entry;
            _onCompletionChanged = onCompletionChanged;

            WidthSizingMode = SizingMode.Fill;
            Height = 32;

            bool isEditing = _service.EditingEntry == _entry;

            BackgroundColor = isEditing
                ? new Color(50, 50, 70, 180)
                : _entry.Completed ? new Color(40, 80, 40, 150) : new Color(40, 40, 40, 150);

            _ = new Checkbox()
            {
                Parent = this,
                Checked = _entry.Completed,
                Location = new Point(5, 6),
                Width = 20,
                CheckedChangedAction = (b) =>
                {
                    _service.SetEntryCompletion(_entry, b);
                    BackgroundColor = b ? new Color(40, 80, 40, 150) : new Color(40, 40, 40, 150);
                    _onCompletionChanged?.Invoke();
                },
            };

            if (isEditing)
            {
                BuildEditMode();
            }
            else
            {
                BuildViewMode();
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
                var scrollbarOffset = 12;
                cancelButton.Location = new Point(Width - 55 - scrollbarOffset, 2);
                saveButton.Location = new Point(Width - 55 - 5 - 50 - scrollbarOffset, 2);
            };
        }

        private void BuildViewMode()
        {
            _ = new Label()
            {
                Parent = this,
                Text = _entry.CharacterName,
                Location = new Point(30, 0),
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
                    Location = new Point(195, 0),
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

            var moveUpButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(298213),
                Size = new Point(16, 16),
                BasicTooltipText = "Move Up",
                ClickAction = (m) => _service.MoveEntryUp(_entry),
            };

            var moveDownButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(155953),
                Size = new Point(16, 16),
                BasicTooltipText = "Move Down",
                ClickAction = (m) => _service.MoveEntryDown(_entry),
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
                var scrollbarOffset = 12;
                switchButton.Location = new Point(Width - 114 - scrollbarOffset, 8);
                moveUpButton.Location = new Point(Width - 92 - scrollbarOffset, 8);
                moveDownButton.Location = new Point(Width - 70 - scrollbarOffset, 8);
                editButton.Location = new Point(Width - 48 - scrollbarOffset, 8);
                removeButton.Location = new Point(Width - 26 - scrollbarOffset, 8);
            };
        }
    }
}
