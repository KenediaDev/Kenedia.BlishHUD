using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Input;
using System.Threading;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    /// <summary>
    /// CtrlType = PetControl | EntryType = Pet => Creates PetControl for each Pet
    /// </summary>
    /// <typeparam name="CtrlType"></typeparam>
    /// <typeparam name="EntryType"></typeparam>
    public abstract class Selector<CtrlType, EntryType> : Panel
        where CtrlType : SkillConnectionControl, new()
    {
        protected readonly FilterBox Filter;
        protected readonly FlowPanel SelectionPanel;
        private Dictionary<int, EntryType> _items = new();
        protected CancellationTokenSource CancellationTokenSource;

        public Selector()
        {
            Filter = new()
            {
                Parent = this,
                TextChangedAction = (t) => _ = FilterItems(t),
                Location = new(0, 0),
                BackgroundColor = Color.Gray * 0.5F,
                Width = 300,
            };

            SelectionPanel = new FlowPanel()
            {
                Parent = this,
                Location = new(0, Filter.Bottom + 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 300,
                CanScroll = true,
                ControlPadding = new(2),
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ContentPadding = new(2),
            };

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        public event EventHandler<CtrlType> ItemClicked;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Filter != null) Filter.Width = Width;

            if (SelectionPanel != null)
            {
                SelectionPanel.Width = Width;

                foreach (var item in SelectionPanel.Children)
                {
                    item.Width = SelectionPanel.ContentRegion.Width - 20;
                }
            }
        }

        private async void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (Parent == Graphics.SpriteScreen && Visible && !MouseOver)
            {
                Hide();
                await Task.Delay(125);
            }
        }

        public CtrlType Target { get; set; }

        public Dictionary<int, EntryType> Items { get => _items; set => Common.SetProperty(ref _items, value, ApplyItems); }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            RecalculateLayout();
            Filter.Focused = true;
        }

        protected async virtual void Clicked(object sender, MouseEventArgs e)
        {
            ItemClicked?.Invoke(sender, (CtrlType)sender);

            if (Parent == Graphics.SpriteScreen)
            {
                Hide();
                await Task.Delay(125);
            }
        }

        protected abstract Task FilterItems(string obj);

        protected virtual void ApplyItems()
        {
            //SelectionPanel.Children.Clear();

            //foreach (var item in Items)
            //{
            //    _ = new CtrlType()
            //    {
            //        Entry = item,
            //        Parent = SelectionPanel,
            //        Height = 40,
            //        BackgroundColor = Color.Red,
            //    };
            //}

            //FilterItems(string.Empty);
        }

        public virtual void CreateUI()
        {

        }

        protected async Task<bool> WaitAndCatch()
        {
            try
            {
                CancellationTokenSource?.Cancel();
                CancellationTokenSource = new();
                await Task.Delay(250, CancellationTokenSource.Token);
                return !CancellationTokenSource.IsCancellationRequested;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
