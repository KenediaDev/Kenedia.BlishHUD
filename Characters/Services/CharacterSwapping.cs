using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Gw2Mumble;
using Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public enum SwappingState
    {
        None,
        LoggedOut,
        MovedToStart,
        MovedToCharacter,
        CharacterRead,
        CharacterFound,
        CharacterLost,
        MovedLeft,
        CheckedLeft,
        MovedRight,
        CheckedRight,
        CharacterFullyLost,
        LoggingIn,
        Done,
        Canceled,
    }

    public class CharacterSwapping
    {
        private readonly SettingsModel _settings;
        private readonly GameState _gameState;
        private readonly ObservableCollection<Character_Model> _characterModels;

        public CharacterSwapping(SettingsModel settings, GameState gameState, ObservableCollection<Character_Model> characterModels)
        {
            _settings = settings;
            _gameState = gameState;
            _characterModels = characterModels;
        }

        public OCR OCR { get; set; }

        public Action HideMainWindow { get; set; }

        public CharacterSorting CharacterSorting { get; set; }

        private CancellationTokenSource _cancellationTokenSource;

        private int _movedLeft;

        public event EventHandler Succeeded;
        public event EventHandler Failed;

        public event EventHandler Started;
        public event EventHandler Finished;

        public event EventHandler StatusChanged;

        private string _status;

        private SwappingState _state = SwappingState.None;

        public string Status
        {
            set
            {
                _status = value;
                StatusChanged?.Invoke(null, EventArgs.Empty);
            }
            get => _status;
        }

        public Character_Model Character { get; set; }

        private bool IsTaskCanceled(CancellationToken cancellationToken)
        {
            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                if (_state is not SwappingState.LoggedOut) { _movedLeft = 0; };
                if (_state is SwappingState.MovedToStart) { _movedLeft = _characterModels.Count; };

                _state = SwappingState.Canceled;
                return true;
            }

            return false;
        }

        public async Task MoveRight(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_Right;
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
        }

        public async Task MoveLeft(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_Left;
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
            await Delay(cancellationToken);
        }

        public async Task<bool> IsNoKeyPressed(CancellationToken cancellationToken)
        {
            while (GameService.Input.Keyboard.KeysDown.Count > 0)
            {
                if (IsTaskCanceled(cancellationToken)) { return false; }

                await Delay(cancellationToken, 250);
            }

            return true;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return; }

            switch (_state)
            {
                case SwappingState.None:
                    if (await LoggingOut(cancellationToken))
                    {
                        _state = SwappingState.LoggedOut;
                    }

                    await Delay(cancellationToken);
                    break;

                case SwappingState.LoggedOut:
                    await MoveToFirstCharacter(cancellationToken);
                    _state = SwappingState.MovedToStart;
                    _movedLeft = 0;
                    await Delay(cancellationToken, 250);
                    break;

                case SwappingState.MovedToStart:
                    await MoveToCharacter(cancellationToken);
                    _state = SwappingState.MovedToCharacter;
                    await Delay(cancellationToken, 250);

                    break;

                case SwappingState.MovedToCharacter:
                    if (await ConfirmName())
                    {
                        _state = SwappingState.CharacterFound;
                    }
                    else
                    {
                        _state = SwappingState.CharacterLost;

                        await MoveLeft(cancellationToken);
                        await Delay(cancellationToken, 250);
                        if (await ConfirmName())
                        {
                            _state = SwappingState.CharacterFound;
                            return;
                        }

                        await MoveRight(cancellationToken);
                        await Delay(cancellationToken, 250);
                        if (await ConfirmName())
                        {
                            _state = SwappingState.CharacterFound;
                            return;
                        }

                        _state = SwappingState.CharacterFullyLost;
                    }

                    break;

                case SwappingState.CharacterFound:
                    await Login(cancellationToken);
                    _state = SwappingState.LoggingIn;
                    break;

                case SwappingState.LoggingIn:
                    if (IsLoaded())
                    {
                        _state = SwappingState.Done;
                        return;
                    }

                    await Delay(cancellationToken, 500);

                    if (GameService.GameIntegration.Gw2Instance.IsInGame)
                    {
                        Character.LastLogin = DateTime.UtcNow;
                    }

                    break;
            }
        }

        public void Reset()
        {
            _state = SwappingState.None;
        }

        public void Cancel()
        {
            _state = SwappingState.Canceled;
            _cancellationTokenSource?.Cancel();
            //s_cancellationTokenSource = null;
        }

        public async void Start(Character_Model character)
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            bool inCharSelection = _settings.UseBetaGamestate.Value ? _gameState.IsCharacterSelection : !GameService.GameIntegration.Gw2Instance.IsInGame;

            if (player != null && player.Name == character.Name && !inCharSelection)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();
            _cancellationTokenSource.CancelAfter(90000);

            Character = character;
            _state = GameService.GameIntegration.Gw2Instance.IsInGame ? SwappingState.None : SwappingState.LoggedOut;

            Started?.Invoke(null, null);
            Status = string.Format(strings.CharacterSwap_SwitchTo, Character.Name);
            while (_state is not SwappingState.Done and not SwappingState.CharacterFullyLost and not SwappingState.Canceled && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Run(_cancellationTokenSource.Token);

                    switch (_state)
                    {
                        case SwappingState.Done:
                            Status = strings.Status_Done;
                            Succeeded?.Invoke(null, null);
                            if (GameService.GameIntegration.Gw2Instance.IsInGame)
                            {
                                if (_settings.CloseWindowOnSwap.Value)
                                {
                                    HideMainWindow?.Invoke();
                                }

                                Character.LastLogin = DateTime.UtcNow;
                                Character?.Save();
                            }

                            break;

                        case SwappingState.CharacterFullyLost:
                            Status = string.Format(strings.CharacterSwap_FailedSwap, Character.Name);
                            Failed?.Invoke(null, null);

                            if (_settings.AutoSortCharacters.Value)
                            {
                                CharacterSorting.Start();
                            }

                            break;
                    }
                }
                catch (TaskCanceledException)
                {

                }
            }

            Finished?.Invoke(null, null);
        }

        private async Task Delay(CancellationToken cancellationToken, int? delay = null, double? partial = null)
        {
            delay ??= _settings.KeyDelay.Value;

            if (delay > 0)
            {
                if (partial != null)
                {
                    delay = delay / 100 * (int)(partial * 100);
                }

                await Task.Delay(delay.Value, cancellationToken);
            }
        }

        private async Task<bool> LoggingOut(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return false; }

            if (GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                Status = strings.CharacterSwap_Logout;
                ModifierKeys mods = ModifierKeys.None;
                var primary = (VirtualKeyShort)_settings.LogoutKey.Value.PrimaryKey;

                foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                {
                    if (mod != ModifierKeys.None && mods.HasFlag(mod))
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Press(Characters.ModKeyMapping[(int)mod], false);
                        if (IsTaskCanceled(cancellationToken)) { return false; }
                    }
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, false);
                await Delay(cancellationToken);

                // Trigger other Modules such as GatherTools
                Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, true);

                foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                {
                    if (mod != ModifierKeys.None && mods.HasFlag(mod))
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Release(Characters.ModKeyMapping[(int)mod], false);
                        if (IsTaskCanceled(cancellationToken)) { return false; }
                    }
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                await Delay(cancellationToken);

                if (_settings.UseBetaGamestate.Value)
                {
                    while (!_gameState.IsCharacterSelection && !cancellationToken.IsCancellationRequested)
                    {
                        await Delay(cancellationToken, 250);
                        if (cancellationToken.IsCancellationRequested) return _gameState.IsCharacterSelection;
                    }
                }
                else
                {
                    await Delay(cancellationToken, _settings.SwapDelay.Value);
                }
            }

            return !GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        private async Task MoveToFirstCharacter(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_MoveFirst;
            if (IsTaskCanceled(cancellationToken)) { return; }

            var stopwatch = Stopwatch.StartNew();
            int moves = _characterModels.Count - _movedLeft;
            for (int i = 0; i < moves; i++)
            {
                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    InputService.MouseWiggle();
                    stopwatch.Restart();
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                _movedLeft++;
                await Delay(cancellationToken, null, 0.05);
                if (IsTaskCanceled(cancellationToken)) { return; }
            }

            return;
        }

        private async Task MoveToCharacter(CancellationToken cancellationToken)
        {
            Status = string.Format(strings.CharacterSwap_MoveTo, Character.Name);
            if (IsTaskCanceled(cancellationToken)) { return; }

            var order = _characterModels.OrderByDescending(e => e.LastLogin).ToList();

            var stopwatch = Stopwatch.StartNew();
            foreach (Character_Model character in order)
            {
                if (character == Character)
                {
                    break;
                }

                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    InputService.MouseWiggle();
                    stopwatch.Restart();
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                await Delay(cancellationToken);
                if (IsTaskCanceled(cancellationToken)) { return; }
            }

            return;
        }

        private async Task<bool> ConfirmName()
        {
            if (!_settings.UseOCR.Value) return true;
            if (Character == null || string.IsNullOrEmpty(Character.Name)) return false;

            string ocr_result = _settings.UseOCR.Value ? await OCR.Read() : "No OCR";
            (string, int, int, int, bool) isBestMatch = ("No OCR enabled.", 0, 0, 0, false);

            if (_settings.UseOCR.Value)
            {
                Status = $"Confirm name ..." + Environment.NewLine + $"{ocr_result}";
                Characters.Logger.Info($"OCR Result: {ocr_result}.");

                isBestMatch = Character.NameMatches(ocr_result);
                Characters.Logger.Info($"Best result for : '{ocr_result}' is '{isBestMatch.Item1}' with edit distance of: {isBestMatch.Item2} and which is {isBestMatch.Item3} steps away in the character list. Resulting in a total difference of {isBestMatch.Item4}.");
                return isBestMatch.Item5;
            }

            return isBestMatch.Item5;
        }

        private async Task Login(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return; }

            if (_settings.EnterOnSwap.Value)
            {
                Status = string.Format(strings.CharacterSwap_LoginTo, Character.Name);
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                await Delay(cancellationToken);
                await Delay(cancellationToken, 1000);
            }

            return;
        }

        private bool IsLoaded()
        {
            return !_settings.EnterOnSwap.Value || GameService.GameIntegration.Gw2Instance.IsInGame;
        }
    }
}
