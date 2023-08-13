using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class NumberBox : Panel, ILocalizable
    {
        private readonly AsyncTexture2D _addButton = AsyncTexture2D.FromAssetId(155927);
        private readonly AsyncTexture2D _addButtonHovered = AsyncTexture2D.FromAssetId(155928);

        private readonly AsyncTexture2D _minusButton = AsyncTexture2D.FromAssetId(155925);
        private readonly AsyncTexture2D _minusButtonHovered = AsyncTexture2D.FromAssetId(155926);

        private readonly TextBox _inputField = new();

        private Rectangle _addRectangle;
        private Rectangle _minusRectangle;
        private string _lastText = $"{0}";
        private int _value;
        private Func<string> _setLocalizedTooltip;

        public NumberBox()
        {
            _inputField.Parent = this;
            _inputField.TextChanged += InputField_TextChanged;
            _inputField.InputFocusChanged += InputField_TextChanged;
            _inputField.HorizontalAlignment = HorizontalAlignment.Center;

            Width = 100;
            Height = 20;

            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public event EventHandler<int> ValueChanged;

        public Action<int> ValueChangedAction { get; set; }

        public new Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public new string BasicTooltipText
        {
            get => _inputField.BasicTooltipText;
            set => _inputField.BasicTooltipText = value;
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                _inputField.Text = $"{value}";
            }
        }

        public int Step { get; set; } = 1;

        public int MaxValue { get; set; } = int.MaxValue;

        public int MinValue { get; set; } = int.MinValue;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Height, Width);

            _addRectangle = new(Width - size, (Height - size) / 2, size, size);
            _minusRectangle = new(Width - (size * 2), (Height - size) / 2, size, size);

            _inputField.Width = Math.Max(0, Width - (size * 2) - 2);
            _inputField.Height = Height;
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (_addRectangle.Width > 0 && _addRectangle.Height > 0)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    _addRectangle.Contains(RelativeMousePosition) ? _addButtonHovered : _addButton,
                    _addRectangle,
                    new(6, 6, 20, 20),
                    Color.White,
                    0f,
                    default);
            }

            if (_minusRectangle.Width > 0 && _minusRectangle.Height > 0)
            {
                spriteBatch.DrawOnCtrl(
                this,
                _minusRectangle.Contains(RelativeMousePosition) ? _minusButtonHovered : _minusButton,
                _minusRectangle,
                new(6, 6, 20, 20),
                Color.White,
                0f,
                default);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_addRectangle.Contains(RelativeMousePosition)) Value += Step;
            if (_minusRectangle.Contains(RelativeMousePosition)) Value -= Step;
        }

        private void InputField_TextChanged(object sender, EventArgs e)
        {
            bool isNumeric = int.TryParse(_inputField.Text, out int value);

            if (!isNumeric)
            {
                _inputField.Text = _lastText;
                _inputField.CursorIndex = _lastText.Length;
                return;
            }

            _lastText = _inputField.Text;

            if (!_inputField.Focused)
            {
                if (isNumeric)
                {
                    bool exceedsLimits = value > MaxValue || value < MinValue;

                    if (exceedsLimits)
                    {
                        Value = Math.Max(Math.Min(value, MaxValue), MinValue);
                    }
                    else
                    {
                        _value = value;
                        ValueChangedAction?.Invoke(Value);
                        ValueChanged?.Invoke(this, Value);
                    }
                }
                else
                {
                    _inputField.Text = $"{Value}";
                }
            }
        }

        new public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }
    }
}
