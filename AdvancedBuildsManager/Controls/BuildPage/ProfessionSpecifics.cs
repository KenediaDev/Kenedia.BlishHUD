using Blish_HUD.Controls;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Gw2Sharp.Models;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.BuildPage
{
    public abstract class ProfessionSpecifics : Control
    {
        protected Template InternTemplate;

        public ProfessionSpecifics()
        {
            ClipsBounds = false;
            ZIndex = int.MaxValue;
        }

        public Template Template
        {
            get => InternTemplate; set
            {
                var temp = InternTemplate;
                if (Common.SetProperty(ref InternTemplate, value, ApplyTemplate, value is not null))
                {
                    if (temp is not null) temp.PropertyChanged -= Temp_Changed;
                    if (InternTemplate is not null) InternTemplate.PropertyChanged += Temp_Changed;
                }
            }
        }

        private void Temp_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public ProfessionType Profession { get; protected set; }

        protected virtual void ApplyTemplate()
        {
            RecalculateLayout();
        }
    }
}
