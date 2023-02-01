using Blish_HUD.Controls;
using Kenedia.Modules.Core.Interfaces;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Linq;

namespace Kenedia.Modules.Characters.Controls
{
    public class FontFlowPanel : FlowPanel, IFontControl
    {
        private BitmapFont _font;

        public BitmapFont Font
        {
            get => _font;
            set
            {
                if (_font != value && value != null)
                {
                    _font = value;
                    OnFontChanged();
                }
            }
        }

        public string Text
        {
            get
            {
                var ctrl = (IFontControl)Children.FirstOrDefault(e => e is IFontControl);
                return ctrl?.Text;
            }
            set
            {
            }
        }

        protected virtual void OnFontChanged(object sender = null, EventArgs e = null)
        {
            foreach (IFontControl ctrl in Children.Cast<IFontControl>())
            {
                ctrl.Font = Font;
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);

            if (Font != null && e.ChangedChild.GetType() == typeof(IFontControl))
            {
                (e.ChangedChild as IFontControl).Font = Font;
            }
        }
    }
}
