using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kenedia.Modules.Characters.Models
{
    public enum ResetFrequency
    {
        None,
        Daily,
        Weekly,
    }

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

        [JsonProperty("resetFrequency")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResetFrequency ResetFrequency { get; set; } = ResetFrequency.None;

        [JsonProperty("lastReset")]
        public DateTimeOffset? LastReset { get; set; }

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
    }
}
