using Kenedia.Modules.Characters.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public class TaskListService : IDisposable
    {
        public sealed class TaskListState
        {
            public StateVar<ObservableCollection<TaskListModel>> TaskLists { get; } = new();

            public StateVar<TaskListModel> SelectedList { get; } = new();

            public StateVar<TaskEntry> TrackedEntry { get; } = new();
        }

        public event Action<string> SwitchToCharacterRequested;
        public event Action FinishSelectionRequested;

        private readonly ObservableCollection<TaskListModel> _taskLists;
        private readonly Action _requestSave;

        private TaskListModel _selectedList;
        private TaskEntry _trackedEntryPendingCompletion;

        public TaskListService(ObservableCollection<TaskListModel> taskLists, Action requestSave)
        {
            _taskLists = taskLists;
            _requestSave = requestSave;
            State.TaskLists.Value = _taskLists;
        }

        public TaskListModel SelectedList => _selectedList;

        public ObservableCollection<TaskListModel> TaskLists => _taskLists;

        public TaskListState State { get; } = new();

        public bool ApplyScheduledResets()
        {
            bool anyReset = false;

            foreach (var taskList in _taskLists)
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
            var newList = new TaskListModel($"Task List {_taskLists.Count + 1}");
            _taskLists.Add(newList);
            _requestSave?.Invoke();

            SelectList(newList);
        }

        public void DeleteSelectedList()
        {
            if (_selectedList is null) return;

            var previousList = _selectedList;
            var previousTrackedEntry = _trackedEntryPendingCompletion;

            _selectedList = null;
            _trackedEntryPendingCompletion = null;

            _taskLists.Remove(previousList);
            _requestSave?.Invoke();

            if (previousTrackedEntry is not null)
            {
                State.TrackedEntry.Value = null;
            }

            State.SelectedList.Value = null;
        }

        public void SelectList(TaskListModel taskList)
        {
            if (ReferenceEquals(_selectedList, taskList)) return;

            _selectedList = taskList;
            State.SelectedList.Value = _selectedList;
            State.TrackedEntry.Value = GetTrackedEntryForSelectedList();
        }

        public void UpdateSelectedListName(string name)
        {
            if (_selectedList is null) return;

            _selectedList.Name = name;
            _requestSave?.Invoke();
        }

        public void UpdateSelectedListResetFrequency(ResetFrequency frequency)
        {
            if (_selectedList is null) return;

            _selectedList.ResetFrequency = frequency;
            _requestSave?.Invoke();
        }

        public void AddEntry(string characterName, string description)
        {
            if (_selectedList is null || string.IsNullOrEmpty(characterName)) return;

            _selectedList.AddEntry(characterName, description);
            _requestSave?.Invoke();
        }

        public void RemoveEntry(TaskEntry entry)
        {
            if (_selectedList is null || entry is null) return;
            if (!_selectedList.Entries.Contains(entry)) return;

            bool trackedEntryRemoved = ReferenceEquals(_trackedEntryPendingCompletion, entry);
            if (trackedEntryRemoved)
            {
                _trackedEntryPendingCompletion = null;
            }

            _selectedList.RemoveEntry(entry);
            _requestSave?.Invoke();

            if (trackedEntryRemoved)
            {
                State.TrackedEntry.Value = null;
            }
        }

        public void ReorderEntry(TaskEntry entry, int targetIndex)
        {
            if (_selectedList is null || entry is null) return;

            var entries = _selectedList.Entries.OrderBy(e => e.Order).ToList();
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
            if (entry is null || string.IsNullOrWhiteSpace(characterName)) return;

            var list = FindListByEntry(entry);
            if (list is null) return;

            entry.CharacterName = characterName.Trim();
            entry.Description = description?.Trim();
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
                State.TrackedEntry.Value = null;
            }

            _requestSave?.Invoke();
        }

        public void SetAllEntriesCompletion(bool completed)
        {
            if (_selectedList is null) return;

            bool changedAny = false;
            foreach (var entry in _selectedList.Entries)
            {
                if (entry.Completed != completed)
                {
                    changedAny = true;
                }

                entry.Completed = completed;
            }

            bool trackedEntryCleared = false;
            if (completed && _trackedEntryPendingCompletion is not null && _selectedList.Entries.Contains(_trackedEntryPendingCompletion))
            {
                _trackedEntryPendingCompletion = null;
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
            var list = _selectedList;
            var trackedEntry = _trackedEntryPendingCompletion;
            if (trackedEntry is null || list is null)
            {
                return null;
            }

            if (!list.Entries.Contains(trackedEntry))
            {
                return null;
            }

            return trackedEntry;
        }

        public void RequestSwitchToCharacter(string characterName)
        {
            if (!string.IsNullOrEmpty(characterName))
            {
                SwitchToCharacterRequested?.Invoke(characterName);
            }
        }

        public void SwitchToNextIncompleteEntry()
        {
            if (_selectedList is null) return;

            bool changedCompletion = false;
            var trackedEntry = GetTrackedEntryForSelectedList();
            var previousTrackedEntry = trackedEntry;
            if (trackedEntry is not null)
            {
                trackedEntry.Completed = true;
                changedCompletion = true;
                _trackedEntryPendingCompletion = null;
            }

            var nextEntry = _selectedList.Entries
                .OrderBy(e => e.Order)
                .FirstOrDefault(e => !e.Completed);

            string nextCharacterName = nextEntry?.CharacterName?.Trim();
            if (nextEntry is not null && !string.IsNullOrEmpty(nextCharacterName))
            {
                SwitchToCharacterRequested?.Invoke(nextCharacterName);
                _trackedEntryPendingCompletion = nextEntry;
            }

            if (!ReferenceEquals(previousTrackedEntry, _trackedEntryPendingCompletion))
            {
                State.TrackedEntry.Value = _trackedEntryPendingCompletion;
            }

            if (changedCompletion)
            {
                _requestSave?.Invoke();

                if (nextEntry is null)
                {
                    FinishSelectionRequested?.Invoke();
                }
            }
        }

        public void Dispose()
        {
        }

        private TaskListModel FindListByEntry(TaskEntry entry)
        {
            if (entry is null) return null;

            return _taskLists.FirstOrDefault(list => list.Entries.Contains(entry));
        }
    }
}
