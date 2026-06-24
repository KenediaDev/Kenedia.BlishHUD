using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using static Kenedia.Modules.Characters.Services.TextureManager;
using Button = Kenedia.Modules.Core.Controls.Button;
using Dropdown = Kenedia.Modules.Core.Controls.Dropdown;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using TextBox = Blish_HUD.Controls.TextBox;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterSelectable : SelectableItem<Character_Model>
    {
        public CharacterSelectable(AutoSuggestComboBox<Character_Model> owner, Character_Model character) : base(owner, character)
        {
            Character = character;
            Height = 20;
            Width = 100;
        }

        public Character_Model Character { get; }

        public override bool MatchesQuery(string query)
        {
            string character_name = Character.Name.ToLowerInvariant();
            return character_name.Contains(query);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
        }

        protected override void DrawItem(SpriteBatch spriteBatch, Rectangle bounds, bool is_selected)
        {
            if (Character is null) return;

            var textColor = MouseOver || is_selected ? HighlightForeground : DefaultTextColor;
            spriteBatch.DrawStringOnCtrl(this, Character.Name, ContentService.Content.DefaultFont14, new Rectangle(6, 0, Math.Max(0, Width - 12), Height), textColor);
        }

        protected override void OnParent_Resized(object sender, ResizedEventArgs e)
        {
            base.OnParent_Resized(sender, e);
            
            this.SetSize(Parent?.ContentRegion.Width ?? Width, Height);
        }
    }

    public class TaskListDetailPanel : FlowPanel
    {
        private readonly TextureManager _textureManager;
        private readonly TaskListService _service;
        private readonly Settings _settings;
        private readonly ObservableCollection<Character_Model> _characterModels;

        private readonly Label _placeholderLabel;
        private readonly FlowPanel _contentPanel;
        private Label _nameLabel;
        private readonly TextBox _nameBox;
        private Label _resetLabel;
        private readonly Dropdown _resetDropdown;
        private readonly AutoSuggestComboBox<Character_Model> _characterSuggestionBox;
        private readonly TaskEntriesPanel _entriesPanel;

        private TaskListModel _boundList;
        private bool _syncingListMetadata;
        private bool _syncingCharacterControls;
        private ImageButton _deleteButton;
        private Character_Model _selectedCharacter;
        private bool _pendingInitialLayout = true;

        public CharacterSwapping CharacterSwapping { get; }

        public TaskListDetailPanel(TextureManager textureManager, TaskListService service, CharacterSwapping characterSwapping, Settings settings, ObservableCollection<Character_Model> characterModels, int width)
        {
            _textureManager = textureManager;
            _service = service;
            _settings = settings;
            CharacterSwapping = characterSwapping;
            _characterModels = characterModels;

            Width = width;
            HeightSizingMode = SizingMode.Fill;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 5);
            OuterControlPadding = new Vector2(5);

            _placeholderLabel = new Label()
            {
                Parent = this,
                Text = strings.TaskListPlaceholder,
                Font = Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                TextColor = Color.LightGray,
            };

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
            };

            var namePanel = new FlowPanel()
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _nameLabel = new Label()
            {
                Parent = namePanel,
                Text = $"{strings.Name}:",
                Width = 75,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            _nameBox = new TextBox()
            {
                Parent = namePanel,
                Width = 300,
                Height = 30,
            };
            _nameBox.TextChanged += (_, _) =>
            {
                if (_syncingListMetadata) return;
                _service.UpdateSelectedListName(_nameBox.Text);
            };

            _deleteButton = new ImageButton()
            {
                Parent = namePanel,
                BasicTooltipText = strings.DeleteList,
                Width = 30,
                Height = 30,
                Texture = _textureManager.GetControlTexture(ControlTextures.Delete_Button),
                HoveredTexture = _textureManager.GetControlTexture(ControlTextures.Delete_Button_Hovered),
                ClickAction = (b) => ConfirmDeleteSelectedList(),
            };

            var resetPanel = new FlowPanel()
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
            };

            _resetLabel = new Label()
            {
                Parent = resetPanel,
                Text = strings.AutoResetField,
                Width = 75,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            _resetDropdown = new Dropdown()
            {
                Parent = resetPanel,
                Width = 220,
                Height = 30,
            };

            _resetDropdown.Items.Add(strings.ResetNone);
            _resetDropdown.Items.Add(strings.ResetDaily);
            _resetDropdown.Items.Add(strings.ResetWeekly);
            _resetDropdown.ValueChangedAction = (selected) =>
            {
                if (_syncingListMetadata) return;

                var frequency = selected switch
                {
                    _ when selected == strings.ResetDaily => ResetFrequency.Daily,
                    _ when selected == strings.ResetWeekly => ResetFrequency.Weekly,
                    _ => ResetFrequency.None,
                };

                _service.UpdateSelectedListResetFrequency(frequency);
            };

            _entriesPanel = new TaskEntriesPanel(_service, settings, characterModels)
            {
                Parent = _contentPanel,
            };

            _service.State.SelectedList.Changed += SelectedList_Changed;
            BindSelectedList(_service.SelectedList);
        }

        private async void ConfirmDeleteSelectedList()
        {
            TaskListModel list = _service.SelectedList;
            if (list is null) return;

            var result = await new BaseDialog(
                strings.DeleteConfirmationTitle,
                string.Format(strings.ConfirmTaskListDelete, list.Name))
            {
                DesiredWidth = 380,
                AutoSize = true,
            }.ShowDialog();

            if (result == DialogResult.OK && ReferenceEquals(_service.SelectedList, list))
            {
                _service.DeleteSelectedList();
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (_pendingInitialLayout && Parent is not null)
            {
                _pendingInitialLayout = false;
                RecalculateLayout();
                _entriesPanel?.RecalculateLayout();
            }
        }

        protected override void DisposeControl()
        {
            _service.State.SelectedList.Changed -= SelectedList_Changed;

            base.DisposeControl();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            _deleteButton?.SetLocation(Width - 10 - _deleteButton.Width, Top);
            _nameBox?.SetSize(Math.Max(100, _deleteButton.Left - _nameBox.Left - 5), _nameBox.Height);
            _resetDropdown?.SetSize(Math.Max(100, _deleteButton.Right - _nameBox.Left), _resetDropdown.Height);
        }

        private void BindSelectedList(TaskListModel list)
        {
            _boundList = list;

            bool hasList = _boundList is not null;
            _placeholderLabel.Visible = !hasList;
            _contentPanel.Visible = hasList;

            _entriesPanel.BindSelectedList(_boundList);
            if (!hasList)
            {
                return;
            }

            _syncingListMetadata = true;
            _nameBox.Text = _boundList.Name ?? string.Empty;
            _resetDropdown.SelectedItem = _boundList.ResetFrequency switch
            {
                ResetFrequency.Daily => strings.ResetDaily,
                ResetFrequency.Weekly => strings.ResetWeekly,
                _ => strings.ResetNone,
            };
            _syncingListMetadata = false;
        }

        private bool IsBoundList(TaskListModel list)
        {
            return _boundList is not null && ReferenceEquals(_boundList, list);
        }

        private void SelectedList_Changed(object sender, StateVarChangedEventArgs<TaskListModel> e)
        {
            if (!ReferenceEquals(e.OldValue, e.NewValue))
            {
                BindSelectedList(e.NewValue);
                return;
            }

            if (!IsBoundList(e.NewValue)) return;

            _syncingListMetadata = true;
            _nameBox.Text = e.NewValue.Name ?? string.Empty;
            _resetDropdown.SelectedItem = e.NewValue.ResetFrequency switch
            {
                ResetFrequency.Daily => strings.ResetDaily,
                ResetFrequency.Weekly => strings.ResetWeekly,
                _ => strings.ResetNone,
            };
            _syncingListMetadata = false;
        }
    }
}
