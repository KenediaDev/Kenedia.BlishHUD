using Kenedia.Modules.Characters.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public class TaskListService : IDisposable
    {
        public event Action<string> SwitchToCharacterRequested;
        public event Action ListPanelChanged;
        public event Action DetailPanelChanged;

        private readonly ObservableCollection<TaskListModel> _taskLists;
        private readonly Action _requestSave;

        private TaskListModel _selectedList;
        private TaskEntry _editingEntry;
        private TaskEntry _trackedEntryPendingCompletion;

        public TaskListService(ObservableCollection<TaskListModel> taskLists, Action requestSave)
        {
            _taskLists = taskLists;
            _requestSave = requestSave;
            _taskLists.CollectionChanged += OnTaskListsCollectionChanged;
        }

        public TaskListModel SelectedList => _selectedList;

        public TaskEntry EditingEntry => _editingEntry;

        public ObservableCollection<TaskListModel> TaskLists => _taskLists;

        public void CreateNewList()
        {
            var newList = new TaskListModel($"Task List {_taskLists.Count + 1}");
            _taskLists.Add(newList);
            _requestSave?.Invoke();

            SelectList(newList);
        }

        public void DeleteSelectedList()
        {
            var toDelete = _selectedList;
            _selectedList = null;
            _editingEntry = null;

            _taskLists.Remove(toDelete);
            _requestSave?.Invoke();
            DetailPanelChanged?.Invoke();
        }

        public void SelectList(TaskListModel taskList)
        {
            _selectedList = taskList;
            _editingEntry = null;

            ListPanelChanged?.Invoke();
            DetailPanelChanged?.Invoke();
        }

        public void UpdateSelectedListName(string name)
        {
            if (_selectedList is null) return;

            _selectedList.Name = name;
            _requestSave?.Invoke();
            ListPanelChanged?.Invoke();
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
            DetailPanelChanged?.Invoke();
        }

        public void RemoveEntry(TaskEntry entry)
        {
            if (_selectedList is null) return;

            _selectedList.RemoveEntry(entry);
            _requestSave?.Invoke();
            DetailPanelChanged?.Invoke();
        }

        public void ReorderEntry(TaskEntry entry, int targetIndex)
        {
            if (_selectedList is null) return;

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
            DetailPanelChanged?.Invoke();
        }

        public void StartEditing(TaskEntry entry)
        {
            _editingEntry = entry;
            DetailPanelChanged?.Invoke();
        }

        public void SaveEditing(string characterName, string description)
        {
            if (_editingEntry is null || string.IsNullOrEmpty(characterName)) return;

            _editingEntry.CharacterName = characterName;
            _editingEntry.Description = description?.Trim();
            _editingEntry = null;
            _requestSave?.Invoke();
            DetailPanelChanged?.Invoke();
        }

        public void CancelEditing()
        {
            _editingEntry = null;
            DetailPanelChanged?.Invoke();
        }

        public void SetEntryCompletion(TaskEntry entry, bool completed)
        {
            entry.Completed = completed;
            _requestSave?.Invoke();
        }

        public void SetAllEntriesCompletion(bool completed)
        {
            if (_selectedList is null) return;

            foreach (var entry in _selectedList.Entries)
            {
                entry.Completed = completed;
            }

            _requestSave?.Invoke();
            DetailPanelChanged?.Invoke();
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

            if (changedCompletion)
            {
                _requestSave?.Invoke();
            }

            DetailPanelChanged?.Invoke();
        }

        private void OnTaskListsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListPanelChanged?.Invoke();
        }

        public void Dispose()
        {
            _taskLists.CollectionChanged -= OnTaskListsCollectionChanged;
        }
    }
}
