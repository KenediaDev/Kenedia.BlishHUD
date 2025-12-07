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
using Kenedia.Modules.Core.Services;
using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class ProfessionRaceSelectable : Panel
    {
        private readonly bool _created;
        private readonly Image _icon;
        private readonly Label _name;

        public ProfessionRaceSelectable(Data data)
        {
            Data = data;

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

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;

            if (Data.IsLoaded)
            {
                SetValue(this, null);
            }
            else
            {
                Data.Loaded += Data_Loaded;
            }
        }

        private void Data_Loaded(object sender, EventArgs e)
        {
            SetValue(this, null);
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (Hovered && this.IsVisible())
                OnClickAction?.Invoke(Value);
        }

        public Enum Value { get; set => Common.SetProperty(ref field, value, SetValue); } = ProfessionType.Guardian;

        public Action<Enum> OnClickAction { get; set; }

        public ProfessionRaceSelection.SelectionType SelectionType
        {
            get;
            set => Common.SetProperty(ref field, value, () => Value = null);
        } = ProfessionRaceSelection.SelectionType.Profession;

        public Data Data { get; }

        public override void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            base.UserLocale_SettingChanged(sender, e);

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
                    if (Data.Professions.TryGetValue((ProfessionType)Value, out var profession))
                    {
                        _name.SetLocalizedText = () => profession?.Name;
                        _icon.Texture = TexturesService.GetAsyncTexture(profession?.IconBigAssetId);
                    }

                    break;

                case ProfessionRaceSelection.SelectionType.Race:
                    if (Data.Races.TryGetValue((Races)Value, out var race))
                    {
                        _name.SetLocalizedText = () => race?.Name;
                        _icon.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(race?.IconPath);
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
