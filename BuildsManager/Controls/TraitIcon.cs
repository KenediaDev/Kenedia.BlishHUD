﻿using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Controls
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
            Texture = TexturesService.GetAsyncTexture(Trait?.IconAssetId);

            if (Trait is not null && Texture is not null)
            {
                int padding = Texture.Width / 16;
                TextureRegion = new(padding, padding, Texture.Width - (padding * 2), Texture.Height - (padding * 2));
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Trait = null;
        }
    }
}