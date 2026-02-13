using Newtonsoft.Json;
using System;

namespace Kenedia.Modules.Characters.Models
{
    public class TaskEntry
    {
        [JsonProperty("characterName")]
        public string CharacterName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        public TaskEntry()
        {
        }

        public TaskEntry(string characterName, string description, int order)
        {
            CharacterName = characterName;
            Description = description;
            Order = order;
        }
    }
}
