using Blish_HUD;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public enum CharacterRoutineStepSwitchStatus
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

            public StateVar<CharacterRoutineStep> TrackedStep { get; } = new();

            public StateVar<CharacterRoutineStepSwitchStatus> StepSwitchStatus { get; } = new();
        }

        private readonly Action _requestSave;
        private CharacterRoutineStep _trackedStepPendingCompletion;
        private bool _trackedStepSwitchSucceeded;

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
                if (_trackedStepPendingCompletion is not null && GetTrackedStepForSelectedRoutine() is null)
                {
                    _trackedStepPendingCompletion = null;
                    _trackedStepSwitchSucceeded = false;
                    State.TrackedStep.Value = null;
                    State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;
                }

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
            var previousTrackedStep = _trackedStepPendingCompletion;

            SelectedRoutine = null;
            _trackedStepPendingCompletion = null;
            _trackedStepSwitchSucceeded = false;
            State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;

            CharacterRoutines.Remove(previousRoutine);
            _requestSave?.Invoke();

            if (previousTrackedStep is not null)
            {
                State.TrackedStep.Value = null;
            }

            State.SelectedRoutine.Value = null;
        }

        public void SelectRoutine(CharacterRoutineModel characterRoutine)
        {
            if (ReferenceEquals(SelectedRoutine, characterRoutine)) return;

            SelectedRoutine = characterRoutine;
            State.SelectedRoutine.Value = SelectedRoutine;
            State.TrackedStep.Value = GetTrackedStepForSelectedRoutine();
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
            if (SelectedRoutine.CheckAndApplyScheduledReset())
            {
                ClearTrackedStepIfReset();
            }

            _requestSave?.Invoke();
        }

        public void AddRoutineStep(string characterName, string description)
        {
            SelectedRoutine?.AddRoutineStep(characterName, description);
            _requestSave?.Invoke();
        }

        public void RemoveRoutineStep(CharacterRoutineStep step)
        {
            if (SelectedRoutine is null || step is null) return;
            if (!SelectedRoutine.RoutineSteps.Contains(step)) return;

            bool trackedStepRemoved = ReferenceEquals(_trackedStepPendingCompletion, step);
            if (trackedStepRemoved)
            {
                _trackedStepPendingCompletion = null;
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;
            }

            SelectedRoutine.RemoveRoutineStep(step);
            _requestSave?.Invoke();

            if (trackedStepRemoved)
            {
                State.TrackedStep.Value = null;
            }
        }

        public void ReorderRoutineStep(CharacterRoutineStep step, int targetIndex)
        {
            if (SelectedRoutine is null || step is null) return;

            int currentIndex = SelectedRoutine.RoutineSteps.IndexOf(step);
            if (currentIndex < 0 || currentIndex == targetIndex) return;

            int insertAt = Math.Min(targetIndex, SelectedRoutine.RoutineSteps.Count - 1);
            SelectedRoutine.RoutineSteps.Move(currentIndex, insertAt);

            _requestSave?.Invoke();
        }

        public void UpdateRoutineStep(CharacterRoutineStep step, string characterName, string description)
        {
            if (step is null) return;

            var routine = FindRoutineByStep(step);
            if (routine is null) return;

            step.CharacterName = characterName?.Trim();
            step.Description = description?.Trim();

            if (ReferenceEquals(_trackedStepPendingCompletion, step))
            {
                _trackedStepSwitchSucceeded = IsCurrentCharacter(step.CharacterName);
                State.StepSwitchStatus.Value = _trackedStepSwitchSucceeded
                    ? CharacterRoutineStepSwitchStatus.ReadyToComplete
                    : string.IsNullOrWhiteSpace(step.CharacterName)
                        ? CharacterRoutineStepSwitchStatus.CharacterNotAssigned
                        : CharacterRoutineStepSwitchStatus.None;
            }

            _requestSave?.Invoke();
        }

        public void SetRoutineStepCompletion(CharacterRoutineStep step, bool completed)
        {
            if (step is null) return;
            if (step.IsCompleted == completed) return;

            step.SetCompleted(completed);

            if (completed && ReferenceEquals(_trackedStepPendingCompletion, step))
            {
                _trackedStepPendingCompletion = null;
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;
                State.TrackedStep.Value = null;
            }

            _requestSave?.Invoke();
        }

        public void SetAllRoutineStepsCompletion(bool completed)
        {
            if (SelectedRoutine is null) return;

            bool changedAny = false;
            foreach (var step in SelectedRoutine.RoutineSteps)
            {
                if (step.IsCompleted != completed)
                {
                    changedAny = true;
                }

                step.SetCompleted(completed);
            }

            bool trackedStepCleared = false;
            if (completed && _trackedStepPendingCompletion is not null && SelectedRoutine.RoutineSteps.Contains(_trackedStepPendingCompletion))
            {
                _trackedStepPendingCompletion = null;
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;
                trackedStepCleared = true;
            }

            if (!changedAny && !trackedStepCleared) return;

            _requestSave?.Invoke();

            if (trackedStepCleared)
            {
                State.TrackedStep.Value = null;
            }
        }

        public CharacterRoutineStep GetTrackedStepForSelectedRoutine()
        {
            var routine = SelectedRoutine;
            var trackedStep = _trackedStepPendingCompletion;
            return trackedStep is null || routine is null
                ? null
                : !routine.RoutineSteps.Contains(trackedStep) || !trackedStep.Enabled || trackedStep.IsCompleted
                    ? null
                    : trackedStep;
        }

        public void RequestSwitchToCharacter(string characterName)
        {
            var character = CharacterModels.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            if (character is not null)
            {
                CharacterSwapping.Start(character);
            }
        }

        public void SwitchToNextIncompleteRoutineStep()
        {
            if (SelectedRoutine is null) return;

            bool changedCompletion = false;
            var trackedStep = GetTrackedStepForSelectedRoutine();
            var previousTrackedStep = trackedStep;
            if (trackedStep is not null)
            {
                if (!CanCompleteTrackedStep(trackedStep))
                {
                    TryStartSwitchForStep(trackedStep);
                    return;
                }

                trackedStep.SetCompleted(true);
                changedCompletion = true;
                _trackedStepPendingCompletion = null;
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;
            }

            var nextStep = GetNextIncompleteRoutineStep(SelectedRoutine);

            if (nextStep is not null)
            {
                _trackedStepPendingCompletion = nextStep;
                _trackedStepSwitchSucceeded = IsCurrentCharacter(nextStep.CharacterName);
                if (_trackedStepSwitchSucceeded)
                {
                    State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.ReadyToComplete;
                }
                else
                {
                    TryStartSwitchForStep(nextStep);
                }
            }

            if (!ReferenceEquals(previousTrackedStep, _trackedStepPendingCompletion))
            {
                State.TrackedStep.Value = _trackedStepPendingCompletion;
            }

            if (changedCompletion)
            {
                _requestSave?.Invoke();
            }
        }

        public void SetRoutineStepEnabled(CharacterRoutineStep step, bool enabled)
        {
            if (step is null) return;
            if (step.Enabled == enabled) return;

            var routine = FindRoutineByStep(step);
            if (routine is null) return;

            step.Enabled = enabled;

            bool trackedStepChanged = false;
            if (ReferenceEquals(_trackedStepPendingCompletion, step) && !enabled)
            {
                _trackedStepPendingCompletion = null;
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;
                trackedStepChanged = true;
            }

            if (ReferenceEquals(routine, SelectedRoutine))
            {
                var trackedStep = GetTrackedStepForSelectedRoutine();
                if (!ReferenceEquals(State.TrackedStep.Value, trackedStep))
                {
                    State.TrackedStep.Value = trackedStep;
                }
                else if (trackedStepChanged)
                {
                    State.TrackedStep.Value = null;
                }
            }

            _requestSave?.Invoke();
        }

        public CharacterRoutineStep GetNextIncompleteRoutineStep(CharacterRoutineModel characterRoutine)
        {
            return characterRoutine?.RoutineSteps.FirstOrDefault(step => step.Enabled && !step.IsCompleted);
        }

        public void Dispose()
        {
            CharacterSwapping.Succeeded -= CharacterSwapping_Succeeded;
            CharacterSwapping.Failed -= CharacterSwapping_Failed;
        }

        private CharacterRoutineModel FindRoutineByStep(CharacterRoutineStep step)
        {
            return step is null ? null : CharacterRoutines.FirstOrDefault(routine => routine.RoutineSteps.Contains(step));
        }

        private void CharacterSwapping_Succeeded(object sender, EventArgs e)
        {
            var trackedStep = _trackedStepPendingCompletion;
            if (trackedStep is null)
            {
                return;
            }

            if (DoesStepMatchCharacter(trackedStep, CharacterSwapping.Character))
            {
                _trackedStepSwitchSucceeded = true;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.ReadyToComplete;
            }
        }

        private void CharacterSwapping_Failed(object sender, EventArgs e)
        {
            var trackedStep = _trackedStepPendingCompletion;
            if (trackedStep is null)
            {
                return;
            }

            if (DoesStepMatchCharacter(trackedStep, CharacterSwapping.Character))
            {
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.Failed;
            }
        }

        private bool CanCompleteTrackedStep(CharacterRoutineStep step)
        {
            return step is not null && (_trackedStepSwitchSucceeded || IsCurrentCharacter(step.CharacterName));
        }

        private bool TryStartSwitchForStep(CharacterRoutineStep step)
        {
            string characterName = step?.CharacterName?.Trim();
            if (string.IsNullOrEmpty(characterName))
            {
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.CharacterNotAssigned;
                return false;
            }

            var character = CharacterModels.FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
            if (character is null)
            {
                _trackedStepSwitchSucceeded = false;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.CharacterNotFound;
                return false;
            }

            _trackedStepSwitchSucceeded = IsCurrentCharacter(character.Name);
            if (!_trackedStepSwitchSucceeded)
            {
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.Switching;
                CharacterSwapping.Start(character);
            }
            else
            {
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.ReadyToComplete;
            }

            return true;
        }

        private void ClearTrackedStepIfReset()
        {
            if (_trackedStepPendingCompletion is null)
            {
                return;
            }

            if (GetTrackedStepForSelectedRoutine() is null)
            {
                _trackedStepPendingCompletion = null;
                _trackedStepSwitchSucceeded = false;
                State.TrackedStep.Value = null;
                State.StepSwitchStatus.Value = CharacterRoutineStepSwitchStatus.None;
            }
        }

        private bool IsCurrentCharacter(string characterName)
        {
            string currentCharacterName = GameService.Gw2Mumble.PlayerCharacter?.Name;
            return !string.IsNullOrWhiteSpace(characterName)
                && !string.IsNullOrWhiteSpace(currentCharacterName)
                && currentCharacterName.Equals(characterName.Trim(), StringComparison.OrdinalIgnoreCase)
                && GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        private static bool DoesStepMatchCharacter(CharacterRoutineStep step, Character_Model character)
        {
            return step is not null
                && character is not null
                && !string.IsNullOrWhiteSpace(step.CharacterName)
                && step.CharacterName.Equals(character.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
