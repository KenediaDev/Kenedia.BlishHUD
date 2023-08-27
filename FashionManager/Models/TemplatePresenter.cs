using Gw2Sharp.WebApi.V2.Models;
using System;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.FashionManager.Models
{
    public class TemplatePresenter
    {
        private FashionTemplate _template;

        public TemplatePresenter()
        {
            
        }

        public event EventHandler<EventArgs> Loaded;
        public event EventHandler<FashionTemplateArmorChanged> ArmorChanged;
        public event EventHandler<FashionTemplateGatheringToolChanged> GatheringToolChanged;
        public event EventHandler<FashionTemplateWeaponChanged> WeaponChanged;
        public event EventHandler<FashionTemplateMountChanged> MountChanged;
        public event EventHandler<ValueChangedEventArgs<SkinBack>> BackChanged;

        public FashionTemplate Template { get => _template; set => Common.SetProperty(ref _template, value, SetupTemplate); }

        private void SetupTemplate(object sender, ValueChangedEventArgs<FashionTemplate> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.ArmorChanged -= Template_ArmorChanged;
                e.OldValue.GatheringToolChanged -= Template_GatheringToolChanged;
                e.OldValue.WeaponChanged -= Template_WeaponChanged;
                e.OldValue.MountChanged -= Template_MountChanged;
                e.OldValue.BackChanged -= Template_BackChanged;
                e.OldValue.Loaded -= Template_Loaded;
            }

            if (e.NewValue != null)
            {
                e.NewValue.ArmorChanged += Template_ArmorChanged;
                e.NewValue.GatheringToolChanged += Template_GatheringToolChanged;
                e.NewValue.WeaponChanged += Template_WeaponChanged;
                e.NewValue.MountChanged += Template_MountChanged;
                e.NewValue.BackChanged += Template_BackChanged;
                e.NewValue.Loaded += Template_Loaded;
            }
        }

        private void Template_Loaded(object sender, EventArgs e)
        {
            Loaded?.Invoke(this, e);
        }

        private void Template_BackChanged(object sender, ValueChangedEventArgs<SkinBack> e)
        {
            BackChanged?.Invoke(this, e);
        }

        private void Template_MountChanged(object sender, FashionTemplateMountChanged e)
        {
            MountChanged?.Invoke(this, e);
        }

        private void Template_WeaponChanged(object sender, FashionTemplateWeaponChanged e)
        {
            WeaponChanged?.Invoke(this, e);
        }

        private void Template_GatheringToolChanged(object sender, FashionTemplateGatheringToolChanged e)
        {
            GatheringToolChanged?.Invoke(this, e);
        }

        private void Template_ArmorChanged(object sender, FashionTemplateArmorChanged e)
        {
            ArmorChanged?.Invoke(this, e);
        }
    }
}
