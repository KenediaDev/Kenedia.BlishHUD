using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kenedia.Modules.Characters.Models
{
    public class TaskListModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("entries")]
        public ObservableCollection<TaskEntry> Entries { get; set; } = [];

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public TaskListModel()
        {
        }

        public TaskListModel(string name)
        {
            Name = name;
        }

        public void AddEntry(string characterName, string description)
        {
            int nextOrder = Entries.Count > 0 ? Entries.Max(e => e.Order) + 1 : 0;
            Entries.Add(new TaskEntry(characterName, description, nextOrder));
        }

        public void RemoveEntry(TaskEntry entry)
        {
            Entries.Remove(entry);
            ReorderEntries();
        }

        public void ReorderEntries()
        {
            int order = 0;
            foreach (var entry in Entries.OrderBy(e => e.Order))
            {
                entry.Order = order++;
            }
        }

        public void ResetCompletion()
        {
            foreach (var entry in Entries)
            {
                entry.Completed = false;
            }
        }
    }
}
