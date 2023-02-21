using Kenedia.Modules.Core.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Utility
{
    public static class UI
    {
        public static (Label, CtrlT) CreateLabeledControl<CtrlT>(Blish_HUD.Controls.Container parent, string text, int labelWidth = 175, int controlWidth = 100, int height = 25)
        where CtrlT : Blish_HUD.Controls.Control, new()
        {
            var p = new Panel()
            {
                Parent = parent,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            var label = new Label()
            {
                Parent = p,
                Width = labelWidth,
                Height = height,
                Text = text,
            };

            var num = new CtrlT()
            {
                Location = new(label.Right + 5, 0),
                Width = controlWidth,
                Height = height,
                Parent = p,
            };

            void Disposed(object s, EventArgs e)
            {
                num.Disposed -= Disposed;
                label.Disposed -= Disposed;

                num.Dispose();
                label.Dispose();
                p.Dispose();
            }

            num.Disposed += Disposed;
            label.Disposed += Disposed;

            return (label, num);
        }

    }
}
