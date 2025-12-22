using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplateTag
    {
        public static string DefaultName => strings.NewTemplate;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TemplateTag()
        {
                
        }

        public TemplateTag(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }            
        }

        public string Group { get; set => Common.SetProperty(field, value, v => field = v, OnGroupChanged); } = string.Empty;

        public int Priority { get; set => Common.SetProperty(field, value, v => field = v, OnPriorityChanged); } = 1;

        public string Name { get; set => Common.SetProperty(field, value, v => field = v, OnNameChanged); } = DefaultName;

        [JsonIgnore]
        public DetailedTexture Icon { get; set; } = new(156025)
        {
            TextureRegion = new(32, 32, 64, 64)
        };

        public int AssetId { get; set => Common.SetProperty(field, value, v => field = v, OnAssetIdChanged); } = 156025;

        public Rectangle? TextureRegion { get; set => Common.SetProperty(field, value, v => field = v, OnTextureRegionChanged); }

        private void OnAssetIdChanged(object sender, ValueChangedEventArgs<int> e)
        {
            Icon = new(e.NewValue);
            Icon.TextureRegion = TextureRegion ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AssetId)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
        }

        private void OnTextureRegionChanged(object sender, ValueChangedEventArgs<Rectangle?> e)
        {
            Icon?.TextureRegion = e.NewValue ?? Icon.Texture?.Bounds ?? Rectangle.Empty;

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

        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return string.Empty;
            }
        }
    }
}
