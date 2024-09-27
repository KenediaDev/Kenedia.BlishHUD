using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class TemplateFactory
    {
        public TemplateFactory(Data data)
        {
            Data = data;
        }

        public Data Data { get; }

        public Template CreateTemplate(string? name = null)
        {
            var t = new Template(Data);

            if (name is not null)
            {
                t.Name = name;
            }

            t.TriggerEvents = true;

            return t;
        }

        public Template CreateTemplate(string? buildCode, string? gearCode)
        {
            var t = new Template(Data, buildCode, gearCode)
            {
                TriggerEvents = true
            };

            return t;
        }

        public Template CreateTemplate(string? name, string ? buildCode, string? gearCode)
        {
            var t = new Template(Data, buildCode, gearCode);

            if (!string.IsNullOrEmpty(name))
            {
                t.Name = name;
            }

            t.TriggerEvents = true;
            return t;

        }

        public Template CreateTemplate(string name, string buildCode, string gearCode, string description, UniqueObservableCollection<string> tags, Races? race, ProfessionType? profession, int? elitespecId, DateTime? lastModified)
        {
            var t = new Template(Data, name, buildCode, gearCode, description, tags, race, profession, elitespecId)
            {
                LastModified = lastModified ?? DateTime.Now,
                TriggerEvents = true,
            };
          
            return t;
        }
    }
}
