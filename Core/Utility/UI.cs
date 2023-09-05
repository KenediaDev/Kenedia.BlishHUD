using Blish_HUD;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Control = Blish_HUD.Controls.Control;
using Container = Blish_HUD.Controls.Container;
using VerticalAlignment = Blish_HUD.Controls.VerticalAlignment;
using Kenedia.Modules.Core.Controls;
using MonoGame.Extended.BitmapFonts;
using System;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Interfaces;

namespace Kenedia.Modules.Core.Utility
{
    public static class UI
    {
        public static BitmapFont GetFont(FontSize fontSize, FontStyle style)
        {
            return GameService.Content.GetFont(FontFace.Menomonia, fontSize, style);
        }

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

        public static void WrapWithLabel(Func<string> localizedLabelContent, Func<string> localizedTooltip, Container parent, int width, Control ctrl)
        {
            var flowPanel = new FlowPanel()
            {
                Parent = parent,
                Width = width,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                SetLocalizedTooltip = localizedTooltip,
            };

            var label = new Label()
            {
                Parent = flowPanel,
                Height = ctrl.Height,
                Width = (width - flowPanel.ContentPadding.Horizontal - ((int)flowPanel.ControlPadding.X *2)) / 2,
                SetLocalizedText = localizedLabelContent,
                SetLocalizedTooltip = localizedTooltip,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            ctrl.Parent = flowPanel;
            ctrl.Width = label.Width;

            if(ctrl is ILocalizable localizable)
            {
                localizable.SetLocalizedTooltip = localizedTooltip;
            }
        }
    }
}
