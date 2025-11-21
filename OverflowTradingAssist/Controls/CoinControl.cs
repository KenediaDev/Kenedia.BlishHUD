using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.ComponentModel;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class CoinControl : Panel
    {
        private DetailedTexture _goldCoin = new(156904);
        private DetailedTexture _copperCoin = new(156902);
        private DetailedTexture _silverCoin = new(156907);

        private TextBox _goldCoinTextBox;
        private TextBox _silverCoinTextBox;
        private TextBox _copperCoinTextBox;

        private decimal _value;
        private bool _created;
        private bool _hideBackground = false;

        public CoinControl()
        {
            _goldCoinTextBox = new TextBox()
            {
                Parent = this,
                Width = 80,
                PlaceholderText = "0",
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
                Text = "0",
                HideBackground = _hideBackground,
                TextChangedAction = (s) =>
                {
                    if (int.TryParse(s.Replace(",", "").Replace(".", ""), out int gold))
                    {
                        _goldCoinTextBox.Text = $"{(gold < 0 ? 0 : gold)}";
                        ParseValue();
                    }
                },
            };

            _silverCoinTextBox = new TextBox()
            {
                Parent = this,
                Width = 35,
                PlaceholderText = "0",
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
                Text = "0",
                HideBackground = _hideBackground,
                TextChangedAction = (s) =>
                {
                    if (int.TryParse(s.Replace(",", "").Replace(".", ""), out int silver))
                    {
                        _silverCoinTextBox.Text = $"{(silver > 99 ? 99 : silver < 0 ? 0 : silver)}";
                        ParseValue();
                    }
                },
            };

            _copperCoinTextBox = new TextBox()
            {
                Parent = this,
                Width = 35,
                PlaceholderText = "0",
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
                Text = "0",
                HideBackground = _hideBackground,
                TextChangedAction = (s) =>
                {
                    if (int.TryParse(s.Replace(",", "").Replace(".", ""), out int copper))
                    {
                        _copperCoinTextBox.Text = $"{(copper > 99 ? 99 : copper < 0 ? 0 : copper)}";
                        ParseValue();
                    }
                },
            };

            _created = true;

            Input.Keyboard.KeyPressed += Keyboard_KeyPressed;

            PropertyChanged += CoinControl_PropertyChanged;
        }

        public decimal Value { get => _value; internal set => Common.SetProperty(ref _value, value, OnValueChanged); }

        public Action<decimal> ValueChangedAction { get; internal set; }

        public bool HideBackground { get => _hideBackground; internal set => Common.SetProperty(ref _hideBackground, value, OnShowFieldChanged); }

        public BitmapFont Font { get => _goldCoinTextBox.Font; set => _goldCoinTextBox.Font = _silverCoinTextBox.Font = _copperCoinTextBox.Font = value; }

        private void CoinControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Enabled))
            {
                _goldCoinTextBox.Enabled = Enabled;
                _silverCoinTextBox.Enabled = Enabled;
                _copperCoinTextBox.Enabled = Enabled;
            }
        }

        private void Keyboard_KeyPressed(object sender, Blish_HUD.Input.KeyboardEventArgs e)
        {
            if (e.Key is Microsoft.Xna.Framework.Input.Keys.Tab)
            {
                if (_goldCoinTextBox.Focused)
                {
                    _goldCoinTextBox.Focused = false;
                    _silverCoinTextBox.Focused = true;
                    _silverCoinTextBox.SelectionStart = 0;
                    _silverCoinTextBox.SelectionEnd = _silverCoinTextBox.Text.Length;

                    _goldCoinTextBox.SelectionStart = 0;
                    _goldCoinTextBox.SelectionEnd = 0;
                    _goldCoinTextBox.CursorIndex = 0;
                }
                else if (_silverCoinTextBox.Focused)
                {
                    _silverCoinTextBox.Focused = false;
                    _copperCoinTextBox.Focused = true;
                    _copperCoinTextBox.SelectionStart = 0;
                    _copperCoinTextBox.SelectionEnd = _copperCoinTextBox.Text.Length;

                    _silverCoinTextBox.SelectionStart = 0;
                    _silverCoinTextBox.SelectionEnd = 0;
                    _silverCoinTextBox.CursorIndex = 0;
                }
                else if (_copperCoinTextBox.Focused)
                {
                    _copperCoinTextBox.Focused = false;
                    _goldCoinTextBox.Focused = true;
                    _goldCoinTextBox.SelectionStart = 0;
                    _goldCoinTextBox.SelectionEnd = _goldCoinTextBox.Text.Length;

                    _copperCoinTextBox.SelectionStart = 0;
                    _copperCoinTextBox.SelectionEnd = 0;
                    _copperCoinTextBox.CursorIndex = 0;
                }
            }
        }

        private void ParseValue()
        {
            if (decimal.TryParse(_goldCoinTextBox.Text, out decimal gold) && decimal.TryParse(_silverCoinTextBox.Text, out decimal silver) && decimal.TryParse(_copperCoinTextBox.Text, out decimal copper))
            {
                _value = (gold * 10000) + (silver * 100) + copper;
                ValueChangedAction?.Invoke(_value);
            }
        }

        private void OnShowFieldChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (_goldCoinTextBox is not null)
                _goldCoinTextBox.HideBackground = e.NewValue;

            if (_silverCoinTextBox is not null)
                _silverCoinTextBox.HideBackground = e.NewValue;

            if (_copperCoinTextBox is not null)
                _copperCoinTextBox.HideBackground = e.NewValue;
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            RecalculateLayout();

            _goldCoin?.Draw(this, spriteBatch);
            _silverCoin?.Draw(this, spriteBatch);
            _copperCoin?.Draw(this, spriteBatch);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int width = ContentRegion.Width - 25;
            int height = ContentRegion.Height;

            if (_created)
            {
                _goldCoinTextBox?.SetLocation(0, 0);
                _goldCoinTextBox?.SetSize(width - (_silverCoinTextBox.Width + _silverCoin.Bounds.Width + _copperCoinTextBox.Width + _copperCoin.Bounds.Width), height);
                _goldCoin.Bounds = new(_goldCoinTextBox.Right, _goldCoinTextBox.Top, height, height);

                _silverCoinTextBox?.SetLocation(_goldCoin.Bounds.Right, 0);
                _silverCoinTextBox?.SetSize(_silverCoinTextBox.Width, height);
                _silverCoin.Bounds = new(_silverCoinTextBox.Right, _silverCoinTextBox.Top, height, height);

                _copperCoinTextBox?.SetLocation(_silverCoin.Bounds.Right, 0);
                _copperCoinTextBox?.SetSize(_copperCoinTextBox.Width, height);
                _copperCoin.Bounds = new(_copperCoinTextBox.Right, _copperCoinTextBox.Top, height, height);
            }
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs<decimal> e)
        {
            SetValueToText(e.NewValue);
        }

        private void SetValueToText(decimal value)
        {
            int copper = (int)(value % 100);
            int silver = (int)(value / 100 % 100);
            int gold = (int)(value / 10000);

            _goldCoinTextBox.Text = $"{gold}";
            _silverCoinTextBox.Text = $"{silver}";
            _copperCoinTextBox.Text = $"{copper}";
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _goldCoin = null;
            _silverCoin = null;
            _copperCoin = null;
        }
    }
}
