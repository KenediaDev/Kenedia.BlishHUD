using Gw2Sharp.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Models
{

    public class TemplateCollection : IEnumerable<Template>
    {
        private ObservableCollection<Template> _templates = [];

        public NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? TemplateChanged;

        public TemplateCollection()
        {
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

        private void Template_ProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            TemplateChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Template.Profession)));
        }

        private void Template_NameChanged(object sender, ValueChangedEventArgs<string> e)
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
    }
}
