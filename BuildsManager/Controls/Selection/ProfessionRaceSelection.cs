using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Blish_HUD.Content;
using System.Linq;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Utility;
using System;
using System.ComponentModel;
using Gw2Sharp.Models;
using Kenedia.Modules.Core.DataModels;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class ProfessionRaceSelection : BaseSelection
    {
        private SelectionType _type = SelectionType.Race;
        private List<ProfessionRaceSelectable> _races = new();
        private List<ProfessionRaceSelectable> _professions = new();

        public ProfessionRaceSelection()
        {
            ProfessionRaceSelectable ctrl;
            Search.Dispose();

            BackgroundImage = AsyncTexture2D.FromAssetId(155963);
            SelectionContent.Location = Point.Zero;
            SelectionContent.HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            SelectionContent.ShowBorder = false;
            SelectionContent.ContentPadding = new(0);

            HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard;
            Width = 225;

            BorderColor = Color.Black;
            BorderWidth = new(2);
            ContentPadding = new(5);

            foreach (Races race in Enum.GetValues(typeof(Races)))
            {
                _races.Add(ctrl = new()
                {
                    Parent = SelectionContent,
                    SelectionType = SelectionType.Race,
                    Value = race,
                    OnClickAction = (v) => OnClickAction?.Invoke(v),
                });
            }

            foreach (ProfessionType profession in Enum.GetValues(typeof(ProfessionType)))
            {
                _professions.Add(ctrl = new()
                {
                    Parent = SelectionContent,
                    SelectionType = SelectionType.Profession,
                    Value = profession,
                    OnClickAction = (v) => OnClickAction?.Invoke(v),
                });
            }

            OnTypeChanged(this, null);
            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!Hovered) Hide();
        }

        public enum SelectionType
        {
            None,
            Profession,
            Race,
        }

        public SelectionType Type
        {
            get => _type;
            set => Common.SetProperty(ref _type, value, OnTypeChanged);
        }

        public Action<Enum> OnClickAction { get; set; }

        private void OnTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            SelectionContent.FilterChildren<ProfessionRaceSelectable>(e => e.SelectionType == Type);

            var ctrl = SelectionContent.Children.FirstOrDefault();
            Height = ContentPadding.Vertical + (SelectionContent.Children.Where(e => e.Visible).Count() * (Math.Max(ctrl.Height, 48) + (int)SelectionContent.ControlPadding.Y));

            SelectionContent.Invalidate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            TextureRectangle = new(25, 25, Width, Height);
        }

        protected override void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnSelectionContent_Resized(sender, e);

            foreach (var child in SelectionContent.Children)
            {
                child.Width = SelectionContent.Width;
            }
        }
    }
}
