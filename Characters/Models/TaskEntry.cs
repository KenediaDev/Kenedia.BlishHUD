using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Characters.Models
{
    public class TaskEntry : INotifyPropertyChanged
    {
        [JsonProperty("characterName")]
        public string CharacterName
        {
            get;
            set => SetField(ref field, value);
        } = string.Empty;

        [JsonProperty("description")]
        public string Description
        {
            get;
            set => SetField(ref field, value);
        } = string.Empty;

        [JsonProperty("completed")]
        public bool Completed
        {
            get;
            set => SetField(ref field, value);
        } = false;

        [JsonProperty("enabled")]
        public bool Enabled
        {
            get;
            set => SetField(ref field, value);
        } = true;

        [JsonProperty("order")]
        public int Order
        {
            get;
            set => SetField(ref field, value);
        } = 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TaskEntry()
        {
        }

        public TaskEntry(string characterName, string description, int order)
        {
            CharacterName = characterName;
            Description = description;
            Order = order;
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
