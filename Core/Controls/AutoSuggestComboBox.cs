using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.Core.Controls
{
    public abstract class SelectableItem<T> : Control
    {
        protected static readonly Color HighlightBackground = new(45, 37, 25, 255);
        protected static readonly Color DefaultTextColor = Color.FromNonPremultiplied(239, 240, 239, 255);
        protected static readonly Color HighlightForeground = Colors.Chardonnay;

        public SelectableItem(AutoSuggestComboBox<T> owner, T item)
        {
            Owner = owner;
            Item = item;
        }

        public new Blish_HUD.Controls.Container Parent
        {
            get => base.Parent;
            set
            {
                base.Parent?.Resized -= OnParent_Resized;
                var parent = base.Parent = value;
                parent?.Resized += OnParent_Resized;
                Width = (parent?.Width - 12) ?? Width;
            }
        }

        protected virtual void OnParent_Resized(object sender, ResizedEventArgs e)
        {
        }

        public AutoSuggestComboBox<T> Owner { get; }

        public T Item { get; set; }

        public abstract bool MatchesQuery(string query);

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            bool is_selected = Owner?.Selected?.Equals(Item) ?? false;
            bounds = new(bounds.X, bounds.Y, bounds.Width - AutoSuggestComboBox<T>.TextureArrow.Width - 5, bounds.Height);

            if (MouseOver || is_selected)
            {
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(2, 2, Math.Max(0, Width - AutoSuggestComboBox<T>.TextureArrow.Width - 5), Math.Max(0, Height - 4)), HighlightBackground);
            }

            DrawItem(spriteBatch, bounds, is_selected);
        }

        protected virtual void DrawItem(SpriteBatch spriteBatch, Rectangle bounds, bool is_selected)
        {
            var textColor = MouseOver ? HighlightForeground : DefaultTextColor;
            spriteBatch.DrawStringOnCtrl(this, Item.ToString(), ContentService.Content.DefaultFont14, new Rectangle(6, 0, Math.Max(0, Width - 12), Height), textColor);
        }

        public virtual string GetDisplayText()
        {
            return Item.ToString();
        }
    }

    public class BlankSelectableItem<T> : SelectableItem<T>
    {
        public BlankSelectableItem(AutoSuggestComboBox<T> owner, string displayText = "") : base(owner, default)
        {
            DisplayText = displayText ?? string.Empty;
            Height = 20;
        }

        public string DisplayText { get; }

        public override bool MatchesQuery(string query)
        {
            return true;
        }

        protected override void DrawItem(SpriteBatch spriteBatch, Rectangle bounds, bool is_selected)
        {
            if (!string.IsNullOrEmpty(DisplayText))
            {
                var textColor = MouseOver || is_selected ? HighlightForeground : DefaultTextColor;
                spriteBatch.DrawStringOnCtrl(this, DisplayText, ContentService.Content.DefaultFont14, new Rectangle(6, 0, Math.Max(0, Width - 12), Height), textColor);
            }
        }

        public override string GetDisplayText()
        {
            return string.Empty;
        }
    }

    public class AutoSuggestComboBox<T> : TextBox
    {
        public static readonly TextureRegion2D TextureArrow = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow");
        public static readonly TextureRegion2D TextureArrowActive = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("inputboxes/dd-arrow-active");

        private FlowPanel _suggestionPanel;

        private IEnumerable<SelectableItem<T>> _selectables = [];
        private SelectableItem<T> _selectedItem = null;
        private BlankSelectableItem<T> _blankSelectable;
        private bool _queueQuery;
        private double _lastQueryTime;
        private bool _suggestionsOpen;

        public event ValueChangedEventHandler<T> SelectedItemChanged;

        public AutoSuggestComboBox()
        {
            _suggestionPanel = new FlowPanel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Visible = false,
                Width = Width,
                Height = 200,
                ShowBorder = true,
                BorderColor = Color.Black * 0.6f,
                BackgroundColor = new Color(20, 20, 20, 235),
                ZIndex = int.MaxValue / 2,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5, 0, 0, 0),
            };

            Input.Mouse.LeftMouseButtonPressed += InputOnMousedOffDropdownPanel;
            Input.Mouse.RightMouseButtonPressed += InputOnMousedOffDropdownPanel;
            RebuildBlankSelectable();
        }

        public double QueryDelay { get; set; }

        public bool SetSelectedText { get; set; } = true;

        public int MaxSuggestionHeight { get; set; } = 200;

        public bool AllowBlankSelection
        {
            get;
            set
            {
                field = value;
                RebuildBlankSelectable();
            }
        } = true;

        public string BlankSelectionText
        {
            get;
            set
            {
                field = value ?? string.Empty;
                RebuildBlankSelectable();
            }
        } = string.Empty;

        public StringComparison Comparison { get; set; } = StringComparison.OrdinalIgnoreCase;

        public T? Selected
        {
            get;
            set
            {
                if (!Equals(field, value))
                {
                    T oldValue = field;
                    field = value;
                    SelectedItemChanged?.Invoke(this, new Models.ValueChangedEventArgs<T>(oldValue, field));
                    _selectedItem = GetAllSelectables().FirstOrDefault(s => Equals(s.Item, field));

                    if (SetSelectedText)
                    {
                        Text = _selectedItem?.GetDisplayText() ?? string.Empty;
                    }
                }
            }
        }

        public Func<T, SelectableItem<T>> SelectableFactory { get; set; }

        public ObservableCollection<T> Items
        {
            get;
            set
            {
                if (field == value) return;
                field?.CollectionChanged -= Items_CollectionChanged;

                field = value;

                RebuildSelectables();
                field?.CollectionChanged += Items_CollectionChanged;
                RebuildBlankSelectable();
            }
        }

        private void InputOnMousedOffDropdownPanel(object sender, MouseEventArgs e)
        {
            if (!base.MouseOver && _suggestionsOpen)
            {
                if (!_suggestionPanel.MouseOver && !_selectables.Any(s => s.MouseOver))
                {
                    _suggestionsOpen = false;
                    _suggestionPanel.Hide();
                }
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems?.Cast<T>() ?? [];
            foreach (var item in newItems)
            {
                var selectable = SelectableFactory?.Invoke(item);

                if (selectable != null)
                {
                    selectable.Parent = _suggestionPanel;
                    selectable.Click += Selectable_Click;
                    _selectables = [.. _selectables, selectable];
                }
            }

            var oldItems = e.OldItems?.Cast<T>() ?? [];
            var toRemove = _selectables.Where(s => oldItems.Contains(s.Item)).ToArray();
            foreach (var item in toRemove)
            {
                item?.Dispose();
            }

            _selectables = _selectables.Where(s => !oldItems.Contains(s.Item)).ToArray();
            _queueQuery = true;
        }

        private void RebuildSelectables()
        {
            _selectables.DisposeAll();
            _selectables = [];

            if (Items is null)
            {
                return;
            }

            foreach (var item in Items)
            {
                var selectable = SelectableFactory?.Invoke(item);

                if (selectable != null)
                {
                    selectable.Parent = _suggestionPanel;
                    selectable.Click += Selectable_Click;
                    _selectables = [.. _selectables, selectable];
                }
            }

            _queueQuery = true;
        }

        private void Selectable_Click(object sender, MouseEventArgs e)
        {
            if (sender is SelectableItem<T> selectable)
            {
                Selected = selectable.Item;
                _suggestionsOpen = false;
                _suggestionPanel.Hide();
            }
        }

        private void Clear()
        {
            _selectables.DisposeAll();
            _selectables = [];
            _blankSelectable?.Dispose();
            _blankSelectable = null;
        }

        protected override void OnTextChanged(object sender, EventArgs e)
        {
            base.OnTextChanged(sender, e);
            _queueQuery = true;
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
            _suggestionsOpen = false;
            _suggestionPanel.Hide();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            _suggestionPanel?.Width = Width;
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

            _suggestionPanel?.SetLocation(AbsoluteBounds.Location.X, AbsoluteBounds.Location.Y + AbsoluteBounds.Height);
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (!Visible)
            {
                _suggestionsOpen = false;
                _suggestionPanel?.Hide();
                return;
            }

            if (_queueQuery)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastQueryTime < QueryDelay)
                {
                    return;
                }

                UpdateSuggestions();

                _queueQuery = false;
                _lastQueryTime = gameTime.TotalGameTime.TotalMilliseconds;
            }

            _suggestionPanel?.SetLocation(AbsoluteBounds.Location.X, AbsoluteBounds.Location.Y + AbsoluteBounds.Height);
            UpdateSuggestionPanelVisibility();
        }

        private void UpdateSuggestions()
        {
            string queryString = Text?.ToLowerInvariant() ?? string.Empty;

            _suggestionPanel.SuspendLayout();
            _blankSelectable?.Visible = true;

            if (!string.IsNullOrEmpty(queryString))
            {
                foreach (var selectable in _selectables)
                {
                    selectable.Visible = selectable.MatchesQuery(queryString);
                }
            }
            else
            {
                foreach (var selectable in _selectables)
                {
                    selectable.Visible = true;
                }
            }

            _suggestionPanel.ResumeLayout(true);
            _suggestionPanel.SetLocation(AbsoluteBounds.Location.X, AbsoluteBounds.Location.Y + AbsoluteBounds.Height);
            UpdateSuggestionPanelHeight();
            UpdateSuggestionPanelVisibility();
            _suggestionPanel.Invalidate();
            _suggestionPanel.SortChildren<SelectableItem<T>>((a, b) => string.Compare(a.GetDisplayText(), b.GetDisplayText(), Comparison));
        }

        private void UpdateSuggestionPanelHeight()
        {
            int visibleCount = GetAllSelectables().Count(s => s.Visible);
            int visibleHeight = GetAllSelectables().Where(s => s.Visible).Sum(s => s.Height);
            int spacing = Math.Max(0, visibleCount - 1) * (int)_suggestionPanel.ControlPadding.Y;
            int padding = _suggestionPanel.Height - _suggestionPanel.ContentRegion.Height;
            _suggestionPanel.Height = (int)Math.Min(MaxSuggestionHeight, visibleHeight + spacing + padding);
        }

        private void UpdateSuggestionPanelVisibility()
        {
            bool hasVisibleSuggestions = GetAllSelectables().Any(s => s.Visible);
            bool keepOpen = Focused || _suggestionPanel.MouseOver || GetAllSelectables().Any(s => s.MouseOver);

            if (Focused && hasVisibleSuggestions)
            {
                _suggestionsOpen = true;
            }

            _suggestionPanel.Visible = hasVisibleSuggestions && _suggestionsOpen;
            if (!_suggestionPanel.Visible)
            {
                if (SetSelectedText)
                {
                    Text = _selectedItem?.GetDisplayText() ?? string.Empty;
                }
            }
        }

        private IEnumerable<SelectableItem<T>> GetAllSelectables()
        {
            if (_blankSelectable is not null)
            {
                yield return _blankSelectable;
            }

            foreach (var selectable in _selectables)
            {
                yield return selectable;
            }
        }

        private void RebuildBlankSelectable()
        {
            _blankSelectable?.Dispose();
            _blankSelectable = null;

            if (!AllowBlankSelection || _suggestionPanel is null)
            {
                return;
            }

            _blankSelectable = new BlankSelectableItem<T>(this, BlankSelectionText)
            {
                Parent = _suggestionPanel,
            };
            _blankSelectable.Click += Selectable_Click;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, (base.Enabled && base.MouseOver) ? TextureArrowActive : TextureArrow, new Rectangle(_size.X - TextureArrow.Width - 5, _size.Y / 2 - TextureArrow.Height / 2, TextureArrow.Width, TextureArrow.Height));
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Clear();
            _suggestionPanel?.Dispose();
            _suggestionPanel = null;

            Input.Mouse.LeftMouseButtonPressed -= InputOnMousedOffDropdownPanel;
            Input.Mouse.RightMouseButtonPressed -= InputOnMousedOffDropdownPanel;
        }
    }
}
