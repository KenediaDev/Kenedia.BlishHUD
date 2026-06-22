using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Kenedia.Modules.Core.Controls
{
    public abstract class SelectableItem<T> : Control
    {
        public SelectableItem(T item)
        {
            Item = item;
        }

        public new Container Parent
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

        public T Item { get; set; }

        public abstract bool MatchesQuery(string query);

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawStringOnCtrl(this, Item?.ToString() ?? string.Empty, Content.DefaultFont14, bounds, Color.White);
        }
    }

    public class AutoSuggestTextBox<T> : TextBox
    {
        private FlowPanel _suggestionPanel;

        private IEnumerable<SelectableItem<T>> _selectables = [];
        private bool _queueQuery;
        private double _lastQueryTime;
        private bool _suggestionsOpen;

        public AutoSuggestTextBox()
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
        }

        public double QueryDelay { get; set; }

        public int MaxSuggestionHeight { get; set; } = 200;

        public StringComparison Comparison { get; set; } = StringComparison.OrdinalIgnoreCase;

        public Func<T, SelectableItem<T>> SelectableFactory { get; set; }

        public ObservableCollection<T> Items
        {
            get;
            set
            {
                if (field == value) return;
                field?.CollectionChanged -= AutoSuggestTextBox_CollectionChanged;

                field = value;

                if (field == null || !field.Any())
                {
                    Clear();
                }

                field?.CollectionChanged += AutoSuggestTextBox_CollectionChanged;
            }
        }

        private void AutoSuggestTextBox_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems?.Cast<T>() ?? [];
            foreach (var item in newItems)
            {
                var selectable = SelectableFactory?.Invoke(item);

                if (selectable != null)
                {
                    selectable.Parent = _suggestionPanel;
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
        }

        private void Clear()
        {
            _selectables.DisposeAll();
            _selectables = [];
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
        }

        private void UpdateSuggestionPanelHeight()
        {
            int visibleCount = _selectables.Count(s => s.Visible);
            int visibleHeight = _selectables.Where(s => s.Visible).Sum(s => s.Height);
            int spacing = Math.Max(0, visibleCount - 1) * (int)_suggestionPanel.ControlPadding.Y;
            int padding = _suggestionPanel.Height - _suggestionPanel.ContentRegion.Height;
            _suggestionPanel.Height = (int)Math.Min(MaxSuggestionHeight, visibleHeight + spacing + padding);
        }

        private void UpdateSuggestionPanelVisibility()
        {
            bool hasVisibleSuggestions = _selectables.Any(s => s.Visible);
            bool keepOpen = Focused || _suggestionPanel.MouseOver;

            if (Focused && hasVisibleSuggestions)
            {
                _suggestionsOpen = true;
            }
            else if (!keepOpen)
            {
                _suggestionsOpen = false;
            }

            _suggestionPanel.Visible = hasVisibleSuggestions && _suggestionsOpen;
        }
    }
}
