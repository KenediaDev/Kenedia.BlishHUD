using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Characters.Models
{
    public class TaskEntry : INotifyPropertyChanged
    {
        private string _characterName;
        private string _description;
        private bool _completed;
        private int _order;

        [JsonProperty("characterName")]
        public string CharacterName
        {
            get => _characterName;
            set => SetField(ref _characterName, value);
        }

        [JsonProperty("description")]
        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        [JsonProperty("completed")]
        public bool Completed
        {
            get => _completed;
            set => SetField(ref _completed, value);
        }

        [JsonProperty("order")]
        public int Order
        {
            get => _order;
            set => SetField(ref _order, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
