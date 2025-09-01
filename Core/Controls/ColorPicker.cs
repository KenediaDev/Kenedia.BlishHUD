using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kenedia.Modules.Core.Controls
{
    public class ColorPicker : Panel
    {
        private readonly Panel _idleBackgroundPreview;
        private readonly NumberBox _red_box;
        private readonly NumberBox _green_box;
        private readonly NumberBox _blue_box;
        private readonly NumberBox _alpha_box;
        private int _r;
        private int _g;
        private int _b;
        private int _a;
        private Color _selected_Color;

        public event ValueChangedEventHandler<Color> ColorChanged;

        public ColorPicker()
        {
            _idleBackgroundPreview = new Panel()
            {
                Parent = this,
                Location = new(0, 0),
                Size = new(20),
                BackgroundColor = Color.Transparent
            };

            _red_box = new NumberBox()
            {
                Parent = this,
                Location = new(_idleBackgroundPreview.Right + 5, 0),
                Value = R,
                MinValue = 0,
                MaxValue = 255,
                ShowButtons = false,
                ValueChangedAction = (v) => R = v
            };

            _green_box = new NumberBox()
            {
                Parent = this,
                Location = new(_red_box.Right + 5, 0),
                Value = G,
                MinValue = 0,
                MaxValue = 255,
                ShowButtons = false,
                ValueChangedAction = (v) => G = v
            };

            _blue_box = new NumberBox()
            {
                Parent = this,
                Location = new(_green_box.Right + 5, 0),
                Value = B,
                MinValue = 0,
                MaxValue = 255,
                ShowButtons = false,
                ValueChangedAction = (v) => B = v
            };

            _alpha_box = new NumberBox()
            {
                Parent = this,
                Location = new(_blue_box.Right + 5, 0),
                Value = A,
                MinValue = 0,
                MaxValue = 255,
                ShowButtons = false,
                ValueChangedAction = (v) => A = v
            };
        }

        public Action<Color> OnColorChangedAction { get; set; }

        public int R
        {
            get => _r;
            set
            {
                if (value is >= 0 and <= 255)
                {
                    Common.SetProperty(ref _r, value, SetColor);
                }
            }
        }

        public int G
        {
            get => _g;
            set
            {
                if (value is >= 0 and <= 255)
                {
                    Common.SetProperty(ref _g, value, SetColor);
                }
            }
        }

        public int B
        {
            get => _b;
            set
            {
                if (value is >= 0 and <= 255)
                {
                    Common.SetProperty(ref _b, value, SetColor);
                }
            }
        }

        public int A
        {
            get => _a;
            set 
            {
                if (value is >= 0 and <= 255)
                {
                    Common.SetProperty(ref _a, value, SetColor);
                }
            }
        }

        public Color SelectedColor
        {
            get => _selected_Color;
            set => Common.SetProperty(ref _selected_Color, value, ApplyColor);
        }

        public Color MultipliedSelectedColor => Color.FromNonPremultiplied(R, G, B, A);

        private void ApplyColor(object sender, ValueChangedEventArgs<Color> e)
        {
            if (e.NewValue is Color col)
            {
                _r = col.R;
                _g = col.G;
                _b = col.B;
                _a = col.A;
                _selected_Color = new Color(R, G, B, A);

                ApplyColorsToControls();
            }
        }

        private void ApplyColorsToControls()
        {
            _red_box.Value = R;
            _blue_box.Value = B;
            _green_box.Value = G;
            _alpha_box.Value = A;

            _idleBackgroundPreview.BackgroundColor = Color.FromNonPremultiplied(R, G, B, A);
        }

        private void SetColor(object sender, ValueChangedEventArgs<int> e)
        {
            var old_color = _selected_Color;
            _selected_Color = new Color(R, G, B, A);

            ApplyColorsToControls();

            ColorChanged?.Invoke(this, new(old_color, SelectedColor));
            OnColorChangedAction?.Invoke(SelectedColor);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = 5;
            int preview_width = 20;
            int number_of_boxes = 4;
            int input_width = (ContentRegion.Width - preview_width - padding - (padding * (number_of_boxes - 1))) / number_of_boxes;

            _idleBackgroundPreview?.SetLocation(0, 0);
            _idleBackgroundPreview?.SetSize(preview_width, preview_width);

            int red_x = (_idleBackgroundPreview?.Right ?? 0) + padding;
            _red_box?.SetLocation(red_x, 0);
            _red_box?.SetSize(input_width);

            int green_x = (_red_box?.Right ?? 0) + padding;
            _green_box?.SetLocation(green_x, 0);
            _green_box?.SetSize(input_width);

            int blue_x = (_green_box?.Right ?? 0) + padding;
            _blue_box?.SetLocation(blue_x, 0);
            _blue_box?.SetSize(input_width);

            int alpha_x = (_blue_box?.Right ?? 0) + padding;
            _alpha_box?.SetLocation(alpha_x, 0);
            _alpha_box?.SetSize(input_width);

        }
    }
}
