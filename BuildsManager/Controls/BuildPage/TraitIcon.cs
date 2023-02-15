using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class TraitIcon : DetailedTexture
    {
        private Trait _trait;

        public Trait Trait { get => _trait; set => Common.SetProperty(ref _trait, value, ApplyTrait); }

        public bool Selected { get; set; }

        private void ApplyTrait()
        {
            Texture = Trait?.Icon;
        }
    }
}
