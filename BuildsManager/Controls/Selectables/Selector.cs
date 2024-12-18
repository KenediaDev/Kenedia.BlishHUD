﻿using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using SizingMode = Blish_HUD.Controls.SizingMode;
using Control = Blish_HUD.Controls.Control;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Blish_HUD;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager.Controls.Selectables
{
    public class Selector<T> : FlowPanel where T : IBaseApiData
    {
        private Control? _anchor;
        private readonly Label _label;
        private Point _selectableSize = new(64);
        private Action<T> _onClickAction;
        private T _selectedItem;
        private int _selectablePerRow = 4;
        private Point _anchorOffset;
        protected readonly Panel HeaderPanel;
        protected readonly Panel ContentPanel;
        protected readonly FlowPanel FlowPanel;

        protected Rectangle BlockInputRegion;

        public Selector()
        {
            HeaderPanel = new()
            {
                Parent = this,
            };

            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;

            ContentPanel = new()
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                BackgroundColor = new Color(16, 16, 16) * 0.9F,
                ContentPadding = new(8),
                BorderColor = Color.Black,
                BorderWidth = new(2),
                Parent = this,
            };

            FlowPanel = new()
            {
                Parent = ContentPanel,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new(4),
                ContentPadding = new(1),
            };

            _label = new()
            {
                Parent = ContentPanel,
                Font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular),
                AutoSizeHeight = true,
                TextColor = Color.White,
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
            };

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += Mouse_RightMouseButtonPressed;
        }

        private void Mouse_RightMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!Visible) return;

            if (HeaderPanel?.MouseOver == true && !BlockInputRegion.Contains(RelativeMousePosition))
            {
                Visible = false;
            }

            if (MouseOver) return;

            Visible = false;
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!Visible) return;

            if (HeaderPanel?.MouseOver == true && !BlockInputRegion.Contains(RelativeMousePosition))
            {
                Visible = false;
            }

            if (MouseOver) return;

            Visible = false;
        }

        public SelectableType Type { get; set; } = SelectableType.None;

        public List<T> Items { get; } = [];

        public T SelectedItem { get => _selectedItem; set => Common.SetProperty(ref _selectedItem, value, ApplySelected); }

        public List<Selectable<T>> Controls { get; } = [];

        public Action<T> OnClickAction { get => _onClickAction; set => Common.SetProperty(ref _onClickAction, value, ApplyAction); }

        public int SelectablePerRow { get => _selectablePerRow; set => Common.SetProperty(ref _selectablePerRow, value, RecalculateLayout); }

        public Point SelectableSize { get => _selectableSize; set => Common.SetProperty(ref _selectableSize, value, Recalculate); }

        public Control Anchor { get => _anchor; set => Common.SetProperty(ref _anchor, value, RecalculateLayout); }

        public Point AnchorOffset { get => _anchorOffset; set => Common.SetProperty(ref _anchorOffset, value, RecalculateLayout); }

        public string Label { get => _label.Text; set => _label.Text = value; }

        public bool PassSelected { get; set; } = true;

        private void ApplySelected(object sender, Core.Models.ValueChangedEventArgs<T> e)
        {
            OnDataApplied(e.NewValue);
        }

        protected virtual void OnDataApplied(T item)
        {

        }

        private void ApplyAction(object sender, Core.Models.ValueChangedEventArgs<Action<T>> e)
        {
            Controls.ForEach(c => c.OnClickAction = OnClickAction);
        }

        protected virtual void Recalculate(object sender, Core.Models.ValueChangedEventArgs<Point> e)
        {
            Controls.ForEach(c => c.Size = SelectableSize);
            RecalculateLayout();
        }

        protected virtual Selectable<T> CreateSelectable(T item)
        {
            Type = item switch
            {
                Skill => SelectableType.Skill,
                Pet => SelectableType.Pet,
                _ => SelectableType.None,
            };

            Visible = true;

            return new()
            {
                Parent = FlowPanel,
                Size = SelectableSize,
                Data = item,
                OnClickAction = OnClickAction,
                IsSelected = PassSelected && item.Equals(SelectedItem),
            };
        }

        public void Add(T item)
        {
            Items.Add(item);
            Controls.Add(CreateSelectable(item));

            RecalculateLayout();
        }

        public void Remove(T item)
        {
            _ = Items.Remove(item);

            if (Controls.FirstOrDefault(c => c.Data.Equals(item)) is Selectable<T> selectable)
            {
                _ = Controls.Remove(selectable);
                selectable.Dispose();
            }

            RecalculateLayout();
        }

        public void Clear()
        {
            Items.Clear();

            Controls.DisposeAll();
            Controls.Clear();

            RecalculateLayout();
        }

        public void SetItems(IEnumerable<T> items)
        {
            Items.Clear();
            Controls.DisposeAll();
            Controls.Clear();

            Items.AddRange(items);
            Controls.AddRange(items.Select(CreateSelectable));

            RecalculateLayout();
        }

        public void AddItems(IEnumerable<T> items)
        {
            Items.AddRange(items);
            Controls.AddRange(items.Select(CreateSelectable));

            RecalculateLayout();
        }

        public void RemoveItems(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _ = Items.Remove(item);

                if (Controls.FirstOrDefault(c => c.Data.Equals(item)) is Selectable<T> selectable)
                {
                    _ = Controls.Remove(selectable);
                    selectable.Dispose();
                }
            }

            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            FlowPanel?.Invalidate();
            ContentPanel?.Invalidate();

            if (FlowPanel is not null)
            {
                Point p = FlowPanel.Size.Substract(FlowPanel.ContentRegion.Size);
                FlowPanel.Width = p.X + (SelectableSize.X * SelectablePerRow) + ((int)FlowPanel.ControlPadding.X * (SelectablePerRow - 1));
                FlowPanel.Height = p.Y + (SelectableSize.Y * Math.Max(1, (int)Math.Ceiling(Items.Count / (decimal)SelectablePerRow))) + ((int)FlowPanel.ControlPadding.Y * Math.Max(1, (int)Math.Ceiling(Items.Count / ((decimal)SelectablePerRow - 1))));

                FlowPanel.RecalculateLayout();
            }

            if (_label is not null && FlowPanel is not null)
            {
                _label.Width = FlowPanel.Width;
                _label.Location = new(0, FlowPanel.Bottom);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            if (Anchor?.IsDrawn() != true)
            {
                Visible = false;
            };

            base.Draw(spriteBatch, drawBounds, scissor);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            if (!Visible) return;
            SetCapture();
            MoveToAnchor();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

        }

        protected virtual void SetCapture()
        {
            if (HeaderPanel is not null)
            {
                CaptureInput = !HeaderPanel.MouseOver;
                HeaderPanel.CaptureInput = !HeaderPanel.MouseOver;
            }
        }

        protected virtual void MoveToAnchor()
        {

            if (Anchor is not null)
                Location = new(Anchor.AbsoluteBounds.Center.X - (Width / 2) + AnchorOffset.X, Anchor.AbsoluteBounds.Top + AnchorOffset.Y);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed -= Mouse_RightMouseButtonPressed;

        }
    }
}
