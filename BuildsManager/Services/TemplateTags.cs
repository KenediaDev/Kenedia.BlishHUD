using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class TemplateTags : IEnumerable<TemplateTag>
    {
        private readonly System.Timers.Timer _timer;
        private readonly ContentsManager _contentsManager;
        private readonly Paths _paths;

        private CancellationTokenSource _tokenSource;
        private List<TemplateTag> _tags;
        private bool _saveRequested;

        public TagGroups TagGroups { get; }

        public TemplateTags(ContentsManager contentsManager, Paths paths, TagGroups tagGroups)
        {
            _contentsManager = contentsManager;
            _paths = paths;
            TagGroups = tagGroups;

            _timer = new(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        public event PropertyChangedEventHandler TagChanged;
        public event EventHandler<TemplateTag> TagAdded;
        public event EventHandler<TemplateTag> TagRemoved;

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_saveRequested)
            {
                _timer.Stop();

                await Save();
            }
        }

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
                tag.PropertyChanged += Tag_PropertyChanged;
            }

            OrderTags();
        }

        private void Tag_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TemplateTag tag)
            {
                OnTagChanged(tag, e);
            }
        }

        private void Tag_TagChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TemplateTag tag)
            {
                OnTagChanged(tag, e);
            }
        }

        private void OrderTags()
        {
            _tags = [.. _tags
                .OrderBy(x => x.Priority > 0)
                .ThenBy(x => x.Group == string.Empty)
                .ThenBy(x => x.Group)
                .ThenBy(x => x.Priority)
                .ThenBy(x => x.Name)];
        }

        private void OnTagChanged(TemplateTag tag, PropertyChangedEventArgs e)
        {
            OrderTags();
            TagChanged?.Invoke(tag, e);
            RequestSave();
        }

        public void Add(TemplateTag tag)
        {
            if (_tags.Any(t => t.Name == tag.Name)) return;

            _tags.Add(tag);
            tag.PropertyChanged += Tag_TagChanged;

            OnTagAdded(tag);
        }

        private void OnTagAdded(TemplateTag tag)
        {
            TagAdded?.Invoke(this, tag);
            RequestSave();
        }

        private void OnTagRemoved(TemplateTag tag)
        {
            TagRemoved?.Invoke(this, tag);
            RequestSave();
        }

        public bool Remove(TemplateTag tag)
        {
            if (_tags.Remove(tag))
            {
                tag.PropertyChanged -= Tag_TagChanged;
                OnTagRemoved(tag);
                return true;
            }

            return false;
        }

        private void RequestSave()
        {
            _saveRequested = true;

            if (_saveRequested)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        public async Task Save()
        {
            try
            {
                _tokenSource?.Cancel();
                _tokenSource = new();

                await Task.Delay(1000, _tokenSource.Token);
                string json = JsonConvert.SerializeObject(_tags, SerializerSettings.Default);

                if (_tokenSource.IsCancellationRequested) return;

                File.WriteAllText($@"{_paths.ModulePath}TemplateTags.json", json);
            }
            catch (Exception ex)
            {
                if (ex is not TaskCanceledException)
                {
                    BuildsManager.Logger.Warn("Failed to save TemplateTags.json");
                    BuildsManager.Logger.Warn($"{ex}");
                }
            }
        }

        public IEnumerator<TemplateTag> GetEnumerator()
        {
            return _tags.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
