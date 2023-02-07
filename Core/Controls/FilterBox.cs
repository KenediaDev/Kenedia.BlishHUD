using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Controls
{
    public class FilterBox : TextBox
    {
        private double _lastFiltering = 0;
        private bool _performFiltering;

        public FilterBox()
        {
            TextChanged += FilterBox_TextChanged;
        }

        public double FilteringDelay { get; set; } = 0;

        public Action<string> PerformFiltering { get; set; }

        public bool FilteringOnEnter { get; set; }

        public bool FilteringOnTextChange { get; set; }

        public void RequestFilter()
        {
            _performFiltering = true;
        }

        public void ForceFilter()
        {
            PerformFiltering?.Invoke(Text);
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if(_performFiltering && gameTime.TotalGameTime.TotalMilliseconds - _lastFiltering >= FilteringDelay)
            {
                _lastFiltering = gameTime.TotalGameTime.TotalMilliseconds;

                PerformFiltering?.Invoke(Text);
            }
        }

        protected override void OnEnterPressed(EventArgs e)
        {
            base.OnEnterPressed(e);
            if(FilteringOnEnter) _performFiltering = true;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            TextChanged -= FilterBox_TextChanged;
        }

        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            if (FilteringOnTextChange) _performFiltering = true;
        }
    }
}
