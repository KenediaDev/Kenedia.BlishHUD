using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Characters.Models
{
    public class CharacterRoutineEntry : INotifyPropertyChanged
    {
        public string CharacterName
        {
            get;
            set => SetField(ref field, value);
        } = string.Empty;

        public string Description
        {
            get;
            set => SetField(ref field, value);
        } = string.Empty;

        public bool Completed
        {
            get;
            set => SetField(ref field, value);
        } = false;

        public bool Enabled
        {
            get;
            set => SetField(ref field, value);
        } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CharacterRoutineEntry()
        {
        }

        public CharacterRoutineEntry(string characterName, string description)
        {
            CharacterName = characterName;
            Description = description;
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
