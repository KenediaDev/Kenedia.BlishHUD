using Blish_HUD;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public enum TaskEntrySwitchStatus
    {
        None,
        Switching,
        ReadyToComplete,
        Failed,
        CharacterNotFound,
        CharacterNotAssigned,
    }

    public class TaskListService : IDisposable
    {
        public sealed class TaskListState
        {
            public StateVar<ObservableCollection<TaskListModel>> TaskLists { get; } = new();

            public StateVar<TaskListModel> SelectedList { get; } = new();

            public StateVar<TaskEntry> TrackedEntry { get; } = new();

            public StateVar<TaskEntrySwitchStatus> EntrySwitchStatus { get; } = new();
        }

        private readonly Action _requestSave;
        private TaskEntry _trackedEntryPendingCompletion;
        private bool _trackedEntrySwitchSucceeded;

        public TaskListService(CharacterSwapping characterSwapping, ObservableCollection<Character_Model> characterModels, ObservableCollection<TaskListModel> taskLists, Action requestSave)
        {
            CharacterSwapping = characterSwapping;
            CharacterModels = characterModels;
            TaskLists = taskLists;
            _requestSave = requestSave;
            State.TaskLists.Value = TaskLists;

            CharacterSwapping.Succeeded += CharacterSwapping_Succeeded;
            CharacterSwapping.Failed += CharacterSwapping_Failed;
        }

        public TaskListModel SelectedList { get; private set; }

        public ObservableCollection<TaskListModel> TaskLists { get; }

        public TaskListState State { get; } = new();
        public CharacterSwapping CharacterSwapping { get; }
        public ObservableCollection<Character_Model> CharacterModels { get; }

        public bool ApplyScheduledResets()
        {
            bool anyReset = false;

            foreach (var taskList in TaskLists)
            {
                if (taskList.CheckAndApplyScheduledReset())
                {
                    anyReset = true;
                }
            }

            if (anyReset)
            {
                _requestSave?.Invoke();
            }

            return anyReset;
        }

        public void CreateNewList()
        {
            var newList = new TaskListModel(string.Format(strings.TaskListDefaultName, TaskLists.Count + 1));
            TaskLists.Add(newList);
            _requestSave?.Invoke();

            SelectList(newList);
        }

        public void DeleteSelectedList()
        {
            if (SelectedList is null) return;

            var previousList = SelectedList;
            var previousTrackedEntry = _trackedEntryPendingCompletion;

            SelectedList = null;
            _trackedEntryPendingCompletion = null;
            _trackedEntrySwitchSucceeded = false;
            State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.None;

            TaskLists.Remove(previousList);
            _requestSave?.Invoke();

            if (previousTrackedEntry is not null)
            {
                State.TrackedEntry.Value = null;
            }

            State.SelectedList.Value = null;
        }

        public void SelectList(TaskListModel taskList)
        {
            if (ReferenceEquals(SelectedList, taskList)) return;

            SelectedList = taskList;
            State.SelectedList.Value = SelectedList;
            State.TrackedEntry.Value = GetTrackedEntryForSelectedList();
        }

        public void UpdateSelectedListName(string name)
        {
            if (SelectedList is null) return;

            SelectedList.Name = name;
            _requestSave?.Invoke();
        }

        public void UpdateSelectedListResetFrequency(ResetFrequency frequency)
        {
            if (SelectedList is null) return;

            SelectedList.ResetFrequency = frequency;
            _requestSave?.Invoke();
        }

        public void AddEntry(string characterName, string description)
        {
            SelectedList.AddEntry(characterName, description);
            _requestSave?.Invoke();
        }

        public void RemoveEntry(TaskEntry entry)
        {
            if (SelectedList is null || entry is null) return;
            if (!SelectedList.Entries.Contains(entry)) return;

            bool trackedEntryRemoved = ReferenceEquals(_trackedEntryPendingCompletion, entry);
            if (trackedEntryRemoved)
            {
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.None;
            }

            SelectedList.RemoveEntry(entry);
            _requestSave?.Invoke();

            if (trackedEntryRemoved)
            {
                State.TrackedEntry.Value = null;
            }
        }

        public void ReorderEntry(TaskEntry entry, int targetIndex)
        {
            if (SelectedList is null || entry is null) return;

            var entries = SelectedList.Entries.OrderBy(e => e.Order).ToList();
            int currentIndex = entries.IndexOf(entry);
            if (currentIndex < 0 || currentIndex == targetIndex) return;

            entries.Remove(entry);
            int insertAt = Math.Min(targetIndex, entries.Count);
            entries.Insert(insertAt, entry);

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Order = i;
            }

            _requestSave?.Invoke();
        }

        public void UpdateEntry(TaskEntry entry, string characterName, string description)
        {
            if (entry is null) return;

            var list = FindListByEntry(entry);
            if (list is null) return;

            entry.CharacterName = characterName?.Trim();
            entry.Description = description?.Trim();

            if (ReferenceEquals(_trackedEntryPendingCompletion, entry))
            {
                _trackedEntrySwitchSucceeded = IsCurrentCharacter(entry.CharacterName);
                State.EntrySwitchStatus.Value = _trackedEntrySwitchSucceeded
                    ? TaskEntrySwitchStatus.ReadyToComplete
                    : string.IsNullOrWhiteSpace(entry.CharacterName)
                        ? TaskEntrySwitchStatus.CharacterNotAssigned
                        : TaskEntrySwitchStatus.None;
            }

            _requestSave?.Invoke();
        }

        public void SetEntryCompletion(TaskEntry entry, bool completed)
        {
            if (entry is null) return;
            if (entry.Completed == completed) return;

            entry.Completed = completed;

            if (completed && ReferenceEquals(_trackedEntryPendingCompletion, entry))
            {
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.None;
                State.TrackedEntry.Value = null;
            }

            _requestSave?.Invoke();
        }

        public void SetAllEntriesCompletion(bool completed)
        {
            if (SelectedList is null) return;

            bool changedAny = false;
            foreach (var entry in SelectedList.Entries)
            {
                if (entry.Completed != completed)
                {
                    changedAny = true;
                }

                entry.Completed = completed;
            }

            bool trackedEntryCleared = false;
            if (completed && _trackedEntryPendingCompletion is not null && SelectedList.Entries.Contains(_trackedEntryPendingCompletion))
            {
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.None;
                trackedEntryCleared = true;
            }

            if (!changedAny && !trackedEntryCleared) return;

            _requestSave?.Invoke();

            if (trackedEntryCleared)
            {
                State.TrackedEntry.Value = null;
            }
        }

        public TaskEntry GetTrackedEntryForSelectedList()
        {
            var list = SelectedList;
            var trackedEntry = _trackedEntryPendingCompletion;
            return trackedEntry is null || list is null ? null : !list.Entries.Contains(trackedEntry) ? null : trackedEntry;
        }

        public void RequestSwitchToCharacter(string characterName)
        {
            var character = CharacterModels.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            if (character != null)
            {
                CharacterSwapping.Start(character);
            }
        }

        public void SwitchToNextIncompleteEntry()
        {
            if (SelectedList is null) return;

            bool changedCompletion = false;
            var trackedEntry = GetTrackedEntryForSelectedList();
            var previousTrackedEntry = trackedEntry;
            if (trackedEntry is not null)
            {
                if (!CanCompleteTrackedEntry(trackedEntry))
                {
                    TryStartSwitchForEntry(trackedEntry);
                    return;
                }

                trackedEntry.Completed = true;
                changedCompletion = true;
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.None;
            }

            var nextEntry = SelectedList.Entries
                .OrderBy(e => e.Order)
                .FirstOrDefault(e => !e.Completed);

            if (nextEntry is not null)
            {
                _trackedEntryPendingCompletion = nextEntry;
                _trackedEntrySwitchSucceeded = IsCurrentCharacter(nextEntry.CharacterName);
                if (_trackedEntrySwitchSucceeded)
                {
                    State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.ReadyToComplete;
                }
                else
                {
                    TryStartSwitchForEntry(nextEntry);
                }
            }

            if (!ReferenceEquals(previousTrackedEntry, _trackedEntryPendingCompletion))
            {
                State.TrackedEntry.Value = _trackedEntryPendingCompletion;
            }

            if (changedCompletion)
            {
                _requestSave?.Invoke();
            }
        }

        public void Dispose()
        {
            CharacterSwapping.Succeeded -= CharacterSwapping_Succeeded;
            CharacterSwapping.Failed -= CharacterSwapping_Failed;
        }

        private TaskListModel FindListByEntry(TaskEntry entry)
        {
            return entry is null ? null : TaskLists.FirstOrDefault(list => list.Entries.Contains(entry));
        }

        private void CharacterSwapping_Succeeded(object sender, EventArgs e)
        {
            var trackedEntry = _trackedEntryPendingCompletion;
            if (trackedEntry is null)
            {
                return;
            }

            if (DoesEntryMatchCharacter(trackedEntry, CharacterSwapping.Character))
            {
                _trackedEntrySwitchSucceeded = true;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.ReadyToComplete;
            }
        }

        private void CharacterSwapping_Failed(object sender, EventArgs e)
        {
            var trackedEntry = _trackedEntryPendingCompletion;
            if (trackedEntry is null)
            {
                return;
            }

            if (DoesEntryMatchCharacter(trackedEntry, CharacterSwapping.Character))
            {
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.Failed;
            }
        }

        private bool CanCompleteTrackedEntry(TaskEntry entry)
        {
            return entry is not null && (_trackedEntrySwitchSucceeded || IsCurrentCharacter(entry.CharacterName));
        }

        private bool TryStartSwitchForEntry(TaskEntry entry)
        {
            string characterName = entry?.CharacterName?.Trim();
            if (string.IsNullOrEmpty(characterName))
            {
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.CharacterNotAssigned;
                return false;
            }

            var character = CharacterModels.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            if (character is null)
            {
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.CharacterNotFound;
                return false;
            }

            _trackedEntrySwitchSucceeded = IsCurrentCharacter(character.Name);
            if (!_trackedEntrySwitchSucceeded)
            {
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.Switching;
                CharacterSwapping.Start(character);
            }
            else
            {
                State.EntrySwitchStatus.Value = TaskEntrySwitchStatus.ReadyToComplete;
            }

            return true;
        }

        private bool IsCurrentCharacter(string characterName)
        {
            string currentCharacterName = GameService.Gw2Mumble.PlayerCharacter?.Name;
            return !string.IsNullOrWhiteSpace(characterName)
                && !string.IsNullOrWhiteSpace(currentCharacterName)
                && currentCharacterName.Equals(characterName.Trim(), StringComparison.OrdinalIgnoreCase)
                && GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        private static bool DoesEntryMatchCharacter(TaskEntry entry, Character_Model character)
        {
            return entry is not null
                && character is not null
                && !string.IsNullOrWhiteSpace(entry.CharacterName)
                && entry.CharacterName.Equals(character.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
