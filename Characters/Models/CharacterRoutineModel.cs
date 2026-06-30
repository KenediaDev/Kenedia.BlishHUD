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
        private ObservableCollection<CharacterRoutineStep> _steps = [];

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

        public ObservableCollection<CharacterRoutineStep> RoutineSteps
        {
            get => _steps;
            set => SetRoutineSteps(value ?? []);
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public CharacterRoutineModel()
        {
            HookRoutineSteps(_steps);
        }

        public CharacterRoutineModel(string name)
            : this()
        {
            Name = name;
        }

        public void AddRoutineStep(string characterName, string description)
        {
            RoutineSteps.Add(new CharacterRoutineStep(characterName, description));
        }

        public void RemoveRoutineStep(CharacterRoutineStep step)
        {
            RoutineSteps.Remove(step);
        }

        public void ResetCompletion()
        {
            foreach (var step in RoutineSteps)
            {
                step.Completed = null;
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

            bool resetAny = false;
            foreach (var step in RoutineSteps)
            {
                if (!step.Completed.HasValue)
                {
                    continue;
                }

                if (step.Completed.Value < boundary.Value.UtcDateTime)
                {
                    step.Completed = null;
                    resetAny = true;
                }
            }

            return resetAny;
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

        private void SetRoutineSteps(ObservableCollection<CharacterRoutineStep> steps)
        {
            if (ReferenceEquals(_steps, steps)) return;

            UnhookRoutineSteps(_steps);
            _steps = steps;
            HookRoutineSteps(_steps);
            OnPropertyChanged(nameof(RoutineSteps));
        }

        private void HookRoutineSteps(ObservableCollection<CharacterRoutineStep> steps)
        {
            if (steps is null) return;

            steps.CollectionChanged += RoutineSteps_CollectionChanged;
            foreach (var step in steps)
            {
                HookStep(step);
            }
        }

        private void UnhookRoutineSteps(ObservableCollection<CharacterRoutineStep> steps)
        {
            if (steps is null) return;

            steps.CollectionChanged -= RoutineSteps_CollectionChanged;
            foreach (var step in steps)
            {
                UnhookStep(step);
            }
        }

        private void RoutineSteps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (CharacterRoutineStep step in e.OldItems)
                {
                    UnhookStep(step);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (CharacterRoutineStep step in e.NewItems)
                {
                    HookStep(step);
                }
            }

            OnPropertyChanged(nameof(RoutineSteps));
        }

        private void HookStep(CharacterRoutineStep step)
        {
            if (step is null) return;
            step.PropertyChanged += Step_PropertyChanged;
        }

        private void UnhookStep(CharacterRoutineStep step)
        {
            if (step is null) return;
            step.PropertyChanged -= Step_PropertyChanged;
        }

        private void Step_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(RoutineSteps));
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
