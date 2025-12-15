using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TagGroup
    {
        public static string DefaultName => strings.GroupNotDefined;

        [JsonProperty("TextureRegion")]
        private Rectangle? _textureRegion;

        public event PropertyAndValueChangedEventHandler? PropertyChanged;

        public TagGroup()
        {
                
        }

        public TagGroup(string name) : this()
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }            
        }

        [field: JsonProperty("Priority")]
        [JsonIgnore]
        public int Priority { get; set => Common.SetProperty(field, value, v => field = v, OnPriorityChanged); } = 1;

        [field: JsonProperty("Name")]
        [JsonIgnore]
        public string Name { get; set => Common.SetProperty(field, value, v => field = v, OnNameChanged); } = DefaultName;

        [JsonIgnore]
        public DetailedTexture Icon { get; set; } = new(156025)
        {
            TextureRegion = new Rectangle(44, 48, 43, 46),
        };

        [field: JsonProperty("AssetId")]
        [JsonIgnore]
        public int AssetId { get; set => Common.SetProperty(field, value, v => field = v, OnAssetIdChanged); } = 156025;

        [JsonIgnore]
        public Rectangle? TextureRegion { get => _textureRegion; set => Common.SetProperty(ref _textureRegion, value, OnTextureRegionChanged); }

        public static TagGroup Empty { get; internal set; } = new();

        private void OnAssetIdChanged(object sender, ValueChangedEventArgs<int> e)
        {
            Icon = new(e.NewValue);
            Icon.TextureRegion = TextureRegion ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

            PropertyChanged?.Invoke(this, new PropertyAndValueChangedEventArgs(nameof(AssetId), e.OldValue, e.NewValue));
        }

        private void OnTextureRegionChanged(object sender, ValueChangedEventArgs<Rectangle?> e)
        {
            if (Icon is not null)
                Icon.TextureRegion = e.NewValue ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

            PropertyChanged?.Invoke(this, new PropertyAndValueChangedEventArgs(nameof(TextureRegion), e.OldValue, e.NewValue));
        }

        private void OnNameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            PropertyChanged?.Invoke(this, new PropertyAndValueChangedEventArgs(nameof(Name), e.OldValue, e.NewValue));
        }

        private void OnPriorityChanged(object sender, ValueChangedEventArgs<int> e)
        {
            PropertyChanged?.Invoke(this, new PropertyAndValueChangedEventArgs(nameof(Priority), e.OldValue, e.NewValue));
        }

    }
}
