using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class TraitIcon : DetailedTexture
    {
        private Trait _trait;

        public TraitIcon()
        {
        }

        public Trait Trait { get => _trait; set => Common.SetProperty(ref _trait, value, ApplyTrait); }

        public bool Selected { get; set; }

        private void ApplyTrait()
        {
            Texture = Trait?.Icon;

            if (Trait is not null && Trait.Icon is not null)
            {
                int padding = Trait.Icon.Width / 16;
                TextureRegion = new(padding, padding, Trait.Icon.Width - (padding * 2), Trait.Icon.Height - (padding * 2));
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Trait = null;
        }
    }
}