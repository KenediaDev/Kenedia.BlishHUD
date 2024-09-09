using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class TemplateTags
    {
        private readonly ContentsManager _contentsManager;
        private readonly Paths _paths;

        private CancellationTokenSource _tokenSource;
        private List<TemplateTag> _tags;

        public TemplateTags(ContentsManager contentsManager, Paths paths)
        {
            _contentsManager = contentsManager;
            _paths = paths;
        }

        public event EventHandler<TemplateTag> TagsChanged;
        public event EventHandler<TemplateTag> TagAdded;
        public event EventHandler<TemplateTag> TagRemoved;

        public IReadOnlyList<TemplateTag> Tags => _tags ?? new();

        public async Task Load()
        {
            try
            {
                if (File.Exists($@"{_paths.ModulePath}TemplateTags.json"))
                {
                    string json = File.ReadAllText($@"{_paths.ModulePath}TemplateTags.json");
                    _tags = JsonConvert.DeserializeObject<List<TemplateTag>>(json, SerializerSettings.Default);
                }
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn("Failed to load TemplateTags.json");
                BuildsManager.Logger.Warn($"{ex}");
            }

            if (_tags is null)
            {
                string json = await new StreamReader(_contentsManager.GetFileStream(@"data\TemplateTags.json")).ReadToEndAsync();
                _tags = JsonConvert.DeserializeObject<List<TemplateTag>>(json, SerializerSettings.Default);

                File.WriteAllText($@"{_paths.ModulePath}TemplateTags.json", json);
                BuildsManager.Logger.Warn("Loaded default TemplateTags.json");
            }

            foreach (var tag in _tags)
            {
                tag.Icon = new(tag.AssetId);
            }


        }

        public TemplateTag this[string tagName] => _tags?.FirstOrDefault(x => x.Name == tagName);

        public void Add(TemplateTag tag)
        {
            _tags.Add(tag);

            OnTagAdded(tag);
            OnTagRemoved(tag);
            OnTagsChanged(tag);
        }

        private void OnTagAdded(TemplateTag tag)
        {
            TagAdded?.Invoke(this, tag);
        }

        private void OnTagsChanged(TemplateTag tag)
        {
            TagsChanged?.Invoke(this, tag);
            Task.Run(Save);
        }

        private void OnTagRemoved(TemplateTag tag)
        {
            TagRemoved?.Invoke(this, tag);
        }

        public bool Remove(TemplateTag tag)
        {
            if (_tags.Remove(tag))
            {
                OnTagRemoved(tag);
                OnTagsChanged(tag);
                return true;
            }

            return false;
        }

        public async Task Save()
        {
            try
            {
                _tokenSource?.Cancel();
                _tokenSource = new();

                await Task.Delay(1000, _tokenSource.Token);
                string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);

                if (_tokenSource.IsCancellationRequested) return;

                File.WriteAllText($@"{_paths.ModulePath}TemplateTags.json", json);
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn("Failed to save TemplateTags.json");
                BuildsManager.Logger.Warn($"{ex}");
            }
        }
    }
}
