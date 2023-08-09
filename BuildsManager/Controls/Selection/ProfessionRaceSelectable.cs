using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Utility;
using System;
using System.ComponentModel;
using static Blish_HUD.ContentService;
using Gw2Sharp.Models;
using Kenedia.Modules.Core.DataModels;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class ProfessionRaceSelectable : Panel
    {
        private readonly bool _created;
        private readonly Image _icon;
        private readonly Label _name;

        private Enum _value = ProfessionType.Guardian;
        private ProfessionRaceSelection.SelectionType _selectionType = ProfessionRaceSelection.SelectionType.Profession;

        public ProfessionRaceSelectable()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            BorderWidth = new(2);
            BorderColor = Color.Black;
            BackgroundColor = Color.Black * 0.4F;
            HoveredBorderColor = Colors.ColonialWhite;
            ContentPadding = new(5);
            ClipInputToBounds = false;

            _name = new()
            {
                Parent = this,
                Font = Content.DefaultFont18,
                TextColor = Color.White,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Middle,
            };

            _icon = new()
            {
                Parent = this,
                Size = new(36),
                Location = new(2, 2),
            };

            _created = true;
            SetValue(this, null);

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (Hovered && this.IsVisible())
                OnClickAction?.Invoke(Value);
        }

        public Enum Value { get => _value; set => Common.SetProperty(ref _value, value, SetValue); }

        public Action<Enum> OnClickAction { get; set; }

        public ProfessionRaceSelection.SelectionType SelectionType
        {
            get => _selectionType;
            set => Common.SetProperty(ref _selectionType, value, () => Value = null);
        }

        private void SetValue(object sender, PropertyChangedEventArgs e)
        {
            if (Value == null)
            {
                _name.Text = null;
                _icon.Texture = null;
                return;
            }

            switch (SelectionType)
            {
                case ProfessionRaceSelection.SelectionType.Profession:
                    if (BuildsManager.Data.Professions.TryGetValue((ProfessionType)Value, out var profession))
                    {
                        _name.Text = profession.Name;
                        _icon.Texture = profession.IconBig;
                    }
                    break;

                case ProfessionRaceSelection.SelectionType.Race:
                    if (BuildsManager.Data.Races.TryGetValue((Races)Value, out var race))
                    {
                        _name.Text = race.Name;
                        _icon.Texture = race.Icon;
                    }
                    break;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

            _name.SetLocation(_icon.Right + 10, _icon.Top - 2);
            _name.SetSize(Right - _icon.Right, _icon.Height);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
        }
    }
}
