using Blish_HUD;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models
{

    public class TemplateCollection : IEnumerable<Template>
    {
        private ObservableCollection<Template> _templates = [];

        public NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? TemplateChanged;

        public event EventHandler? Loaded;

        public bool IsLoaded { get; private set; }

        public TemplateCollection(Logger logger, Paths paths, TemplateFactory templateFactory, TemplateConverter templateConverter)
        {
            Logger = logger;
            Paths = paths;
            TemplateFactory = templateFactory;
            TemplateConverter = templateConverter;
            _templates.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }

        public void Add(Template template)
        {
            if (template is null)
            {
                return;
            }

            _templates.Add(template);

            template.LastModifiedChanged += Template_LastModifiedChanged;
            template.NameChanged += Template_NameChanged;
            template.ProfessionChanged += Template_ProfessionChanged;
        }

        private void Template_ProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<ProfessionType> e)
        {
            TemplateChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Template.Profession)));
        }

        private void Template_NameChanged(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            TemplateChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Template.Name)));
        }

        private void Template_LastModifiedChanged(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            TemplateChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Template.LastModified)));
        }

        public bool Remove(Template template)
        {
            if (template is null)
            {
                return false;
            }

            template.LastModifiedChanged -= Template_LastModifiedChanged;
            template.NameChanged -= Template_NameChanged;
            template.ProfessionChanged -= Template_ProfessionChanged;

            return _templates.Remove(template);
        }

        public void Clear()
        {
            _templates.Clear();
        }

        public int Count => _templates.Count;

        public Logger Logger { get; }

        public Paths Paths { get; }

        public TemplateFactory TemplateFactory { get; }

        public TemplateConverter TemplateConverter { get; }

        public IEnumerator<Template> GetEnumerator()
        {
            return _templates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string GetNewName(string name)
        {
            if(_templates.All(t => t.Name != name))
            {
                return name;
            }

            for (int i = 1; i < int.MaxValue; i++)
            {
                string newName = $"{name} ({i})";

                if (_templates.All(t => t.Name != newName))
                {
                    return newName;
                }
            }

            return name;
        }

        public async Task Load()
        {
            Logger.Info($"LoadTemplates");
            IsLoaded = false;
            _templates.CollectionChanged -= OnCollectionChanged;

            var time = new Stopwatch();
            time.Start();

            try
            {
                string[] templateFiles = Directory.GetFiles(Paths.TemplatesPath);

                _templates.Clear();

                JsonSerializerSettings settings = new();
                settings.Converters.Add(TemplateConverter);

                Logger.Info($"Loading {templateFiles.Length} Templates ...");
                foreach (string file in templateFiles)
                {
                    //Read files async and create templates 
                    using StreamReader reader = new(file);
                    string json = await reader.ReadToEndAsync();

                    Template template = JsonConvert.DeserializeObject<Template>(json, settings);
                    template.SaveRequested = false;

                    _templates.Add(template);
                }

                if (_templates.Count == 0)
                {
                    _templates.Add(TemplateFactory.CreateTemplate());
                }

                time.Stop();
                Logger.Info($"Time to load {templateFiles.Length} templates {time.ElapsedMilliseconds}ms. {_templates.Count} out of {templateFiles.Length} templates got loaded.");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message);
                Logger.Warn($"Loading Templates failed!");
            }

            _templates.CollectionChanged += OnCollectionChanged;

            IsLoaded = true;
            Loaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
