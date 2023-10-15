using Blish_HUD.Content;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplateTag
    {
        [JsonProperty("AssetId")]
        private int _assetId = 156025;

        [JsonProperty("Name")]
        private string _name = "Custom Tag";

        [JsonProperty("TextureRegion")]
        private Rectangle? _textureRegion;

        [JsonIgnore]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, OnNameChanged); }

        [JsonIgnore]
        public DetailedTexture Icon { get; set; } = new(156025)
        {
            TextureRegion = new Rectangle(44, 48, 43, 46),
        };

        [JsonIgnore]
        public int AssetId { get => _assetId; set => Common.SetProperty(ref _assetId, value, OnAssetIdChanged); }

        [JsonIgnore]
        public Rectangle? TextureRegion { get => _textureRegion; set => Common.SetProperty(ref _textureRegion, value, OnTextureRegionChanged); }

        [JsonIgnore]
        public Action OnTagChanged { get; set; }

        private void OnAssetIdChanged(object sender, ValueChangedEventArgs<int> e)
        {
            Icon = new(e.NewValue);
            Icon.TextureRegion = TextureRegion ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

            OnTagChanged?.Invoke();
        }

        private void OnTextureRegionChanged(object sender, ValueChangedEventArgs<Rectangle?> e)
        {
            if (Icon is not null)
                Icon.TextureRegion = e.NewValue ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

            OnTagChanged?.Invoke();
        }

        private void OnNameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            OnTagChanged?.Invoke();
        }
    }
}
