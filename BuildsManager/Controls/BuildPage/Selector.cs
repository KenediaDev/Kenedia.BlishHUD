using Blish_HUD.Content;
using Colors = Blish_HUD.ContentService.Colors;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Blish_HUD.Input;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public interface IBaseApiData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public AsyncTexture2D Icon { get; }
    }

    public enum SelectableType
    {
        None,
        Skill,
        Item,
        Pet,
    }

    public class Selectable<IBaseApiData> : Blish_HUD.Controls.Control
    {
        private AsyncTexture2D _texture;
        private IBaseApiData _data;

        public IBaseApiData Data { get => _data; set => Common.SetProperty(ref _data, value, ApplyData); }

        public Selectable()
        {
            Size = new Point(64);
        }

        public SelectableType Type { get; private set; }

        public Rectangle TextureRegion { get; private set; }

        public Action<IBaseApiData> OnClickAction { get; set; }

        public bool IsSelected { get; set; }

        private void ApplyData(object sender, Core.Models.ValueChangedEventArgs<IBaseApiData> e)
        {
            if (Data is null) return;

            switch (Data)
            {
                case Skill skill:
                    Type = SelectableType.Skill;
                    //TextureRegion = new(14, 14, 100, 100);
                    int sPadding = (int)(skill.Icon.Width * 0.109375);
                    TextureRegion = new(sPadding, sPadding, skill.Icon.Width - (sPadding * 2), skill.Icon.Height - (sPadding * 2));
                    _texture = skill.Icon;
                    Tooltip = new SkillTooltip() { Skill = skill };
                    break;

                case Pet pet:
                    Type = SelectableType.Pet;
                    TextureRegion = new(16, 16, 200, 200);
                    _texture = pet.Icon;
                    break;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Data is null) return;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Data is null) return;

            spriteBatch.DrawOnCtrl(this, _texture, bounds, TextureRegion, Color.White);

            if (IsSelected)
                spriteBatch.DrawFrame(this, bounds, Colors.ColonialWhite, 3);

            if (MouseOver)
                spriteBatch.DrawFrame(this, bounds, Colors.ColonialWhite, 2);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            if (Data is null) return;

            OnClickAction?.Invoke(Data);
        }
    }

    public class SkillSelector : Selector<Skill>
    {
        private readonly DetailedTexture _selectingFrame = new(157147);

        public SkillSelector()
        {
            ContentPanel.BorderWidth = new(2, 0, 2, 2);

        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            RecalculateLayout();

            int p = 10;
            Rectangle r = new(Point.Zero.Add(new(-p, -2)), new(Width + (p * 2), SelectableSize.Y + 8));
            ContentPanel.BorderWidth = new(2);
            HeaderPanel.BorderWidth = new(0, 0, 0, 2);
            HeaderPanel.BorderColor = Color.Transparent;

            spriteBatch.DrawCenteredRotationOnCtrl(this, _selectingFrame.Texture, r, _selectingFrame.TextureRegion, Color.White, 0.0F, true, true);

            r = new(new(0, 0), new(ContentPanel.Width, SelectableSize.Y));
            HeaderPanel?.SetBounds(r);
        }

        protected override void Recalculate(object sender, Core.Models.ValueChangedEventArgs<Point> e)
        {
            base.Recalculate(sender, e);

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (ContentPanel is not null && HeaderPanel is not null)
            {
                int p = 24;
                Rectangle r = new(Point.Zero.Substract(new(p, _selectingFrame.Size.Y + 3)), new(Width + (p * 2), SelectableSize.Y + 6));
                r = new(Point.Zero, new(ContentPanel.Width, SelectableSize.Y));
                HeaderPanel?.SetBounds(r);
            }
        }

        protected override Blish_HUD.Controls.CaptureType CapturesInput()
        {
            return HeaderPanel.MouseOver ? Blish_HUD.Controls.CaptureType.None : base.CapturesInput();
        }
    }

    public class Selector<T> : FlowPanel where T : IBaseApiData
    {
        private readonly FlowPanel _flowPanel;
        private readonly Label _label;
        private Point _selectableSize = new(64);
        private Point _topCenterAnchor;
        private Action<T> _onClickAction;
        private T _selectedItem;
        protected readonly Panel HeaderPanel;
        protected readonly Panel ContentPanel;

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

            _flowPanel = new()
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
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if(MouseOver) return;

            Visible = false;
        }

        public SelectableType Type { get; set; } = SelectableType.None;

        public List<T> Items { get; } = new();

        public T SelectedItem { get => _selectedItem; set => Common.SetProperty(ref _selectedItem, value, ApplySelected); }

        public List<Selectable<T>> Controls { get; } = new();

        public Action<T> OnClickAction { get => _onClickAction; set => Common.SetProperty(ref _onClickAction, value, ApplyAction); }

        public Point SelectableSize { get => _selectableSize; set => Common.SetProperty(ref _selectableSize, value, Recalculate); }

        public Point TopCenterAnchor { get => _topCenterAnchor; set => Common.SetProperty(ref _topCenterAnchor, value, MoveToFit); }

        public string Label { get => _label.Text; set => _label.Text = value; }

        private void ApplySelected(object sender, Core.Models.ValueChangedEventArgs<T> e)
        {
            switch (Type)
            {
                case SelectableType.Skill:
                    Controls.ForEach(c => c.IsSelected = (c.Data as Skill) == (SelectedItem as Skill));
                    break;
                case SelectableType.Pet:
                    Controls.ForEach(c => c.IsSelected = (c.Data as Pet) == (SelectedItem as Pet));
                    break;
                default:
                    break;
            }
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

        private void MoveToFit(object sender, Core.Models.ValueChangedEventArgs<Point> e)
        {
            Location = new Point(TopCenterAnchor.X - (Width / 2), TopCenterAnchor.Y);
        }

        private Selectable<T> CreateSelectable(T item)
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
                Parent = _flowPanel,
                Size = SelectableSize,
                Data = item,
                OnClickAction = OnClickAction,
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

            _flowPanel?.Invalidate();
            ContentPanel?.Invalidate();

            if (_flowPanel is not null)
            {
                Point p = _flowPanel.Size.Substract(_flowPanel.ContentRegion.Size);
                _flowPanel.Width = p.X + (SelectableSize.X * 4) + ((int)_flowPanel.ControlPadding.X * 3);
                _flowPanel.Height = p.Y + (SelectableSize.Y * Math.Max(1, Items.Count / 4)) + ((int)_flowPanel.ControlPadding.Y * (Math.Max(1, Items.Count / 4) - 1));

                _flowPanel.RecalculateLayout();
            }

            if (_label is not null && _flowPanel is not null)
            {
                _label.Width = _flowPanel.Width;
                _label.Location = new(0, _flowPanel.Bottom);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (HeaderPanel is not null)
            {
                CaptureInput = !HeaderPanel.MouseOver;
                HeaderPanel.CaptureInput = !HeaderPanel.MouseOver;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Visible = false;
        }
    }
}
