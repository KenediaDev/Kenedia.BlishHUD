using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using Kenedia.Modules.Core.Models;
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.Utility;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class PropertyAndValueChangedEventArgs: EventArgs
    {
        public string PropertyName { get; set; }

        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public PropertyAndValueChangedEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public delegate void PropertyAndValueChangedEventHandler (object sender, PropertyAndValueChangedEventArgs e);

    public class TagGroups : IEnumerable<TagGroup>
    {
        private readonly System.Timers.Timer _timer;
        private readonly ContentsManager _contentsManager;
        private readonly Paths _paths;

        private CancellationTokenSource _tokenSource;
        private List<TagGroup> _groups = [];
        private bool _saveRequested;

        public TagGroups(ContentsManager contentsManager, Paths paths)
        {
            _contentsManager = contentsManager;
            _paths = paths;

            _timer = new(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        public event PropertyAndValueChangedEventHandler GroupChanged;
        public event EventHandler<TagGroup> GroupAdded;
        public event EventHandler<TagGroup> GroupRemoved;

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

            if (File.Exists($@"{_paths.ModulePath}TagGroups.json"))
            {
                try
                {
                    if (File.Exists($@"{_paths.ModulePath}TagGroups.json"))
                    {
                        string json = File.ReadAllText($@"{_paths.ModulePath}TagGroups.json");
                        _groups = JsonConvert.DeserializeObject<List<TagGroup>>(json, SerializerSettings.Default);
                    }
                }
                catch (Exception ex)
                {
                    BuildsManager.Logger.Warn("Failed to load TagGroups.json");
                    BuildsManager.Logger.Warn($"{ex}");
                }
            }
            else
            {
                string json = await new StreamReader(_contentsManager.GetFileStream(@"data\TagGroups.json")).ReadToEndAsync();
                _groups = JsonConvert.DeserializeObject<List<TagGroup>>(json, SerializerSettings.Default);

                File.WriteAllText($@"{_paths.ModulePath}TagGroups.json", json);
                BuildsManager.Logger.Warn("Loaded default TagGroups.json");
            }

            foreach (var tag in _groups)
            {
                tag.Icon = new(tag.AssetId);
                tag.PropertyChanged += Tag_PropertyChanged;
            }
        }

        private void Tag_PropertyChanged(object sender, PropertyAndValueChangedEventArgs e)
        {
            if (sender is TagGroup tag)
            {
                OnTagChanged(tag, e);
            }
        }

        private void Tag_TagChanged(object sender, PropertyAndValueChangedEventArgs e)
        {
            if (sender is TagGroup tag)
            {
                OnTagChanged(tag, e);
            }
        }

        private void OnTagChanged(TagGroup tag, PropertyAndValueChangedEventArgs e)
        {
            GroupChanged?.Invoke(tag, e);
            RequestSave();
        }

        public void Add(TagGroup tag)
        {
            if (_groups.Any(t => t.Name == tag.Name))
            {
                return;
            }

            _groups.Add(tag);
            tag.PropertyChanged += Tag_TagChanged;

            OnTagAdded(tag);
        }

        private void OnTagAdded(TagGroup tag)
        {
            GroupAdded?.Invoke(this, tag);
            RequestSave();
        }

        private void OnTagRemoved(TagGroup tag)
        {
            GroupRemoved?.Invoke(this, tag);
            RequestSave();
        }

        public bool Remove(TagGroup tag)
        {
            if (_groups.Remove(tag))
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
                string json = JsonConvert.SerializeObject(_groups, SerializerSettings.Default);

                if (_tokenSource.IsCancellationRequested) return;

                File.WriteAllText($@"{_paths.ModulePath}TagGroups.json", json);
            }
            catch (Exception ex)
            {
                if (ex is not TaskCanceledException)
                {
                    BuildsManager.Logger.Warn("Failed to save TagGroups.json");
                    BuildsManager.Logger.Warn($"{ex}");
                }
            }
        }

        public IEnumerator<TagGroup> GetEnumerator()
        {
            _groups.Sort(TemplateTagComparer.CompareGroups);

            return _groups.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
