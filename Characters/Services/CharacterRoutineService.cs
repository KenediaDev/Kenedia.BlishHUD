using Blish_HUD;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public enum CharacterRoutineEntrySwitchStatus
    {
        None,
        Switching,
        ReadyToComplete,
        Failed,
        CharacterNotFound,
        CharacterNotAssigned,
    }

    public class CharacterRoutineService : IDisposable
    {
        public sealed class CharacterRoutineState
        {
            public StateVar<ObservableCollection<CharacterRoutineModel>> CharacterRoutines { get; } = new();

            public StateVar<CharacterRoutineModel> SelectedRoutine { get; } = new();

            public StateVar<CharacterRoutineEntry> TrackedEntry { get; } = new();

            public StateVar<CharacterRoutineEntrySwitchStatus> EntrySwitchStatus { get; } = new();
        }

        private readonly Action _requestSave;
        private CharacterRoutineEntry _trackedEntryPendingCompletion;
        private bool _trackedEntrySwitchSucceeded;

        public CharacterRoutineService(CharacterSwapping characterSwapping, ObservableCollection<Character_Model> characterModels, ObservableCollection<CharacterRoutineModel> characterRoutines, Action requestSave)
        {
            CharacterSwapping = characterSwapping;
            CharacterModels = characterModels;
            CharacterRoutines = characterRoutines;
            _requestSave = requestSave;
            State.CharacterRoutines.Value = CharacterRoutines;

            CharacterSwapping.Succeeded += CharacterSwapping_Succeeded;
            CharacterSwapping.Failed += CharacterSwapping_Failed;
        }

        public CharacterRoutineModel SelectedRoutine { get; private set; }

        public ObservableCollection<CharacterRoutineModel> CharacterRoutines { get; }

        public CharacterRoutineState State { get; } = new();
        public CharacterSwapping CharacterSwapping { get; }
        public ObservableCollection<Character_Model> CharacterModels { get; }

        public bool ApplyScheduledResets()
        {
            bool anyReset = false;

            foreach (var characterRoutine in CharacterRoutines)
            {
                if (characterRoutine.CheckAndApplyScheduledReset())
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

        public void CreateNewRoutine()
        {
            var newRoutine = new CharacterRoutineModel(string.Format(strings.CharacterRoutineDefaultName, CharacterRoutines.Count + 1));
            CharacterRoutines.Add(newRoutine);
            _requestSave?.Invoke();

            SelectRoutine(newRoutine);
        }

        public void DeleteSelectedRoutine()
        {
            if (SelectedRoutine is null) return;

            var previousRoutine = SelectedRoutine;
            var previousTrackedEntry = _trackedEntryPendingCompletion;

            SelectedRoutine = null;
            _trackedEntryPendingCompletion = null;
            _trackedEntrySwitchSucceeded = false;
            State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.None;

            CharacterRoutines.Remove(previousRoutine);
            _requestSave?.Invoke();

            if (previousTrackedEntry is not null)
            {
                State.TrackedEntry.Value = null;
            }

            State.SelectedRoutine.Value = null;
        }

        public void SelectRoutine(CharacterRoutineModel characterRoutine)
        {
            if (ReferenceEquals(SelectedRoutine, characterRoutine)) return;

            SelectedRoutine = characterRoutine;
            State.SelectedRoutine.Value = SelectedRoutine;
            State.TrackedEntry.Value = GetTrackedEntryForSelectedRoutine();
        }

        public void UpdateSelectedRoutineName(string name)
        {
            if (SelectedRoutine is null) return;

            SelectedRoutine.Name = name;
            _requestSave?.Invoke();
        }

        public void UpdateSelectedRoutineResetFrequency(ResetFrequency frequency)
        {
            if (SelectedRoutine is null) return;

            SelectedRoutine.ResetFrequency = frequency;
            _requestSave?.Invoke();
        }

        public void AddRoutineEntry(string characterName, string description)
        {
            SelectedRoutine.AddRoutineEntry(characterName, description);
            _requestSave?.Invoke();
        }

        public void RemoveRoutineEntry(CharacterRoutineEntry entry)
        {
            if (SelectedRoutine is null || entry is null) return;
            if (!SelectedRoutine.RoutineEntries.Contains(entry)) return;

            bool trackedEntryRemoved = ReferenceEquals(_trackedEntryPendingCompletion, entry);
            if (trackedEntryRemoved)
            {
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.None;
            }

            SelectedRoutine.RemoveRoutineEntry(entry);
            _requestSave?.Invoke();

            if (trackedEntryRemoved)
            {
                State.TrackedEntry.Value = null;
            }
        }

        public void ReorderRoutineEntry(CharacterRoutineEntry entry, int targetIndex)
        {
            if (SelectedRoutine is null || entry is null) return;

            int currentIndex = SelectedRoutine.RoutineEntries.IndexOf(entry);
            if (currentIndex < 0 || currentIndex == targetIndex) return;

            int insertAt = Math.Min(targetIndex, SelectedRoutine.RoutineEntries.Count - 1);
            SelectedRoutine.RoutineEntries.Move(currentIndex, insertAt);

            _requestSave?.Invoke();
        }

        public void UpdateRoutineEntry(CharacterRoutineEntry entry, string characterName, string description)
        {
            if (entry is null) return;

            var list = FindRoutineByEntry(entry);
            if (list is null) return;

            entry.CharacterName = characterName?.Trim();
            entry.Description = description?.Trim();

            if (ReferenceEquals(_trackedEntryPendingCompletion, entry))
            {
                _trackedEntrySwitchSucceeded = IsCurrentCharacter(entry.CharacterName);
                State.EntrySwitchStatus.Value = _trackedEntrySwitchSucceeded
                    ? CharacterRoutineEntrySwitchStatus.ReadyToComplete
                    : string.IsNullOrWhiteSpace(entry.CharacterName)
                        ? CharacterRoutineEntrySwitchStatus.CharacterNotAssigned
                        : CharacterRoutineEntrySwitchStatus.None;
            }

            _requestSave?.Invoke();
        }

        public void SetRoutineEntryCompletion(CharacterRoutineEntry entry, bool completed)
        {
            if (entry is null) return;
            if (entry.Completed == completed) return;

            entry.Completed = completed;

            if (completed && ReferenceEquals(_trackedEntryPendingCompletion, entry))
            {
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.None;
                State.TrackedEntry.Value = null;
            }

            _requestSave?.Invoke();
        }

        public void SetAllRoutineEntriesCompletion(bool completed)
        {
            if (SelectedRoutine is null) return;

            bool changedAny = false;
            foreach (var entry in SelectedRoutine.RoutineEntries)
            {
                if (entry.Completed != completed)
                {
                    changedAny = true;
                }

                entry.Completed = completed;
            }

            bool trackedEntryCleared = false;
            if (completed && _trackedEntryPendingCompletion is not null && SelectedRoutine.RoutineEntries.Contains(_trackedEntryPendingCompletion))
            {
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.None;
                trackedEntryCleared = true;
            }

            if (!changedAny && !trackedEntryCleared) return;

            _requestSave?.Invoke();

            if (trackedEntryCleared)
            {
                State.TrackedEntry.Value = null;
            }
        }

        public CharacterRoutineEntry GetTrackedEntryForSelectedRoutine()
        {
            var list = SelectedRoutine;
            var trackedEntry = _trackedEntryPendingCompletion;
            return trackedEntry is null || list is null
                ? null
                : !list.RoutineEntries.Contains(trackedEntry) || !trackedEntry.Enabled || trackedEntry.Completed
                    ? null
                    : trackedEntry;
        }

        public void RequestSwitchToCharacter(string characterName)
        {
            var character = CharacterModels.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            if (character != null)
            {
                CharacterSwapping.Start(character);
            }
        }

        public void SwitchToNextIncompleteRoutineEntry()
        {
            if (SelectedRoutine is null) return;

            bool changedCompletion = false;
            var trackedEntry = GetTrackedEntryForSelectedRoutine();
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
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.None;
            }

            var nextEntry = GetNextIncompleteRoutineEntry(SelectedRoutine);

            if (nextEntry is not null)
            {
                _trackedEntryPendingCompletion = nextEntry;
                _trackedEntrySwitchSucceeded = IsCurrentCharacter(nextEntry.CharacterName);
                if (_trackedEntrySwitchSucceeded)
                {
                    State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.ReadyToComplete;
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

        private CharacterRoutineModel FindRoutineByEntry(CharacterRoutineEntry entry)
        {
            return entry is null ? null : CharacterRoutines.FirstOrDefault(list => list.RoutineEntries.Contains(entry));
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
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.ReadyToComplete;
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
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.Failed;
            }
        }

        private bool CanCompleteTrackedEntry(CharacterRoutineEntry entry)
        {
            return entry is not null && (_trackedEntrySwitchSucceeded || IsCurrentCharacter(entry.CharacterName));
        }

        private bool TryStartSwitchForEntry(CharacterRoutineEntry entry)
        {
            string characterName = entry?.CharacterName?.Trim();
            if (string.IsNullOrEmpty(characterName))
            {
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.CharacterNotAssigned;
                return false;
            }

            var character = CharacterModels.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            if (character is null)
            {
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.CharacterNotFound;
                return false;
            }

            _trackedEntrySwitchSucceeded = IsCurrentCharacter(character.Name);
            if (!_trackedEntrySwitchSucceeded)
            {
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.Switching;
                CharacterSwapping.Start(character);
            }
            else
            {
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.ReadyToComplete;
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

        private static bool DoesEntryMatchCharacter(CharacterRoutineEntry entry, Character_Model character)
        {
            return entry is not null
                && character is not null
                && !string.IsNullOrWhiteSpace(entry.CharacterName)
                && entry.CharacterName.Equals(character.Name, StringComparison.OrdinalIgnoreCase);
        }

        public void SetRoutineEntryEnabled(CharacterRoutineEntry entry, bool enabled)
        {
            if (entry is null) return;
            if (entry.Enabled == enabled) return;

            var list = FindRoutineByEntry(entry);
            if (list is null) return;

            entry.Enabled = enabled;

            bool trackedEntryChanged = false;
            if (ReferenceEquals(_trackedEntryPendingCompletion, entry) && !enabled)
            {
                _trackedEntryPendingCompletion = null;
                _trackedEntrySwitchSucceeded = false;
                State.EntrySwitchStatus.Value = CharacterRoutineEntrySwitchStatus.None;
                trackedEntryChanged = true;
            }

            if (ReferenceEquals(list, SelectedRoutine))
            {
                var trackedEntry = GetTrackedEntryForSelectedRoutine();
                if (!ReferenceEquals(State.TrackedEntry.Value, trackedEntry))
                {
                    State.TrackedEntry.Value = trackedEntry;
                }
                else if (trackedEntryChanged)
                {
                    State.TrackedEntry.Value = null;
                }
            }

            _requestSave?.Invoke();
        }

        public CharacterRoutineEntry GetNextIncompleteRoutineEntry(CharacterRoutineModel characterRoutine)
        {
            return characterRoutine?.RoutineEntries.FirstOrDefault(e => e.Enabled && !e.Completed);
        }
    }
}
