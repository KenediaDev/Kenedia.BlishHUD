using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

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

        [JsonProperty("Priority")]
        private int _priority = 1;

        [JsonProperty("Group")]
        private string _group = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonIgnore]
        public string Group { get => _group; set => Common.SetProperty(ref _group, value, OnGroupChanged); }

        [JsonIgnore]
        public int Priority { get => _priority; set => Common.SetProperty(ref _priority, value, OnPriorityChanged); }

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

        private void OnAssetIdChanged(object sender, ValueChangedEventArgs<int> e)
        {
            Icon = new(e.NewValue);
            Icon.TextureRegion = TextureRegion ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AssetId)));
        }

        private void OnTextureRegionChanged(object sender, ValueChangedEventArgs<Rectangle?> e)
        {
            if (Icon is not null)
                Icon.TextureRegion = e.NewValue ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextureRegion)));
        }

        private void OnNameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }

        private void OnPriorityChanged(object sender, ValueChangedEventArgs<int> e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Priority)));
        }

        private void OnGroupChanged(object sender, ValueChangedEventArgs<string> e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Group)));
        }
    }
}
