using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Characters.Models
{
    public enum ResetFrequency
    {
        None,
        Daily,
        Weekly,
    }

    public class CharacterRoutineModel : INotifyPropertyChanged
    {
        private ObservableCollection<CharacterRoutineEntry> _entries = [];

        public Guid Id
        {
            get;
            set => SetField(ref field, value);
        } = Guid.NewGuid();

        public string Name
        {
            get;
            set => SetField(ref field, value);
        } = string.Empty;

        public ObservableCollection<CharacterRoutineEntry> RoutineEntries
        {
            get => _entries;
            set => SetRoutineEntries(value ?? []);
        }

        public DateTimeOffset Created
        {
            get;
            set => SetField(ref field, value);
        } = DateTimeOffset.UtcNow;

        [JsonConverter(typeof(StringEnumConverter))]
        public ResetFrequency ResetFrequency
        {
            get;
            set => SetField(ref field, value);
        } = ResetFrequency.None;

        public DateTimeOffset? LastReset
        {
            get;
            set => SetField(ref field, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CharacterRoutineModel()
        {
            HookRoutineEntries(_entries);
        }

        public CharacterRoutineModel(string name)
            : this()
        {
            Name = name;
        }

        public void AddRoutineEntry(string characterName, string description)
        {
            RoutineEntries.Add(new CharacterRoutineEntry(characterName, description));
        }

        public void RemoveRoutineEntry(CharacterRoutineEntry entry)
        {
            RoutineEntries.Remove(entry);
        }

        public void ResetCompletion()
        {
            foreach (var entry in RoutineEntries)
            {
                entry.Completed = false;
            }
        }

        /// <summary>
        /// Checks whether a scheduled reset is due and, if so, resets all entries.
        /// Returns true if a reset was performed.
        /// </summary>
        public bool CheckAndApplyScheduledReset()
        {
            if (ResetFrequency == ResetFrequency.None)
            {
                return false;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset? boundary = GetMostRecentResetBoundary(now);

            if (boundary == null)
            {
                return false;
            }

            // If we've never reset, or the last reset was before the most recent boundary, reset now.
            if (LastReset == null || LastReset.Value < boundary.Value)
            {
                ResetCompletion();
                LastReset = now;
                return true;
            }

            return false;
        }

        private DateTimeOffset? GetMostRecentResetBoundary(DateTimeOffset now)
        {
            switch (ResetFrequency)
            {
                case ResetFrequency.Daily:
                    // Daily reset at 00:00 UTC
                    return new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);

                case ResetFrequency.Weekly:
                    // Weekly reset on Monday at 07:30 UTC
                    DateTime utcNow = now.UtcDateTime;
                    int daysSinceMonday = ((int)utcNow.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    DateTime monday = utcNow.Date.AddDays(-daysSinceMonday);
                    var mondayReset = new DateTimeOffset(monday.Year, monday.Month, monday.Day, 7, 30, 0, TimeSpan.Zero);

                    // If we haven't reached 07:30 on this Monday yet, use last Monday
                    if (now < mondayReset)
                    {
                        mondayReset = mondayReset.AddDays(-7);
                    }

                    return mondayReset;

                default:
                    return null;
            }
        }

        private void SetRoutineEntries(ObservableCollection<CharacterRoutineEntry> entries)
        {
            if (ReferenceEquals(_entries, entries)) return;

            UnhookRoutineEntries(_entries);
            _entries = entries;
            HookRoutineEntries(_entries);
            OnPropertyChanged(nameof(RoutineEntries));
        }

        private void HookRoutineEntries(ObservableCollection<CharacterRoutineEntry> entries)
        {
            if (entries is null) return;

            entries.CollectionChanged += RoutineEntries_CollectionChanged;
            foreach (var entry in entries)
            {
                HookEntry(entry);
            }
        }

        private void UnhookRoutineEntries(ObservableCollection<CharacterRoutineEntry> entries)
        {
            if (entries is null) return;

            entries.CollectionChanged -= RoutineEntries_CollectionChanged;
            foreach (var entry in entries)
            {
                UnhookEntry(entry);
            }
        }

        private void RoutineEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (CharacterRoutineEntry entry in e.OldItems)
                {
                    UnhookEntry(entry);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (CharacterRoutineEntry entry in e.NewItems)
                {
                    HookEntry(entry);
                }
            }

            OnPropertyChanged(nameof(RoutineEntries));
        }

        private void HookEntry(CharacterRoutineEntry entry)
        {
            if (entry is null) return;
            entry.PropertyChanged += Entry_PropertyChanged;
        }

        private void UnhookEntry(CharacterRoutineEntry entry)
        {
            if (entry is null) return;
            entry.PropertyChanged -= Entry_PropertyChanged;
        }

        private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(RoutineEntries));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
