using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Characters.Models
{
    public class CharacterRoutineStep : INotifyPropertyChanged
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

        public DateTime? Completed
        {
            get;
            set => SetField(ref field, NormalizeUtc(value));
        }

        public bool IsCompleted => Completed.HasValue;

        public bool Enabled
        {
            get;
            set => SetField(ref field, value);
        } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CharacterRoutineStep()
        {
        }

        public CharacterRoutineStep(string characterName, string description)
        {
            CharacterName = characterName;
            Description = description;
        }

        public void SetCompleted(bool completed, DateTime? completedAtUtc = null)
        {
            Completed = completed ? NormalizeUtc(completedAtUtc ?? DateTime.UtcNow) : null;
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        private static DateTime? NormalizeUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
            };
        }
    }
}
