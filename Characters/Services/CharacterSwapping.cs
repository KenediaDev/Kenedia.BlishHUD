using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Gw2Mumble;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        private readonly Settings _settings;
        private readonly GameStateDetectionService _gameState;
        private readonly ObservableCollection<Character_Model> _rawCharacterModels;

        private bool _ignoreOCR;

        public CharacterSwapping(Settings settings, GameStateDetectionService gameState, ObservableCollection<Character_Model> characterModels)
        {
            _settings = settings;
            _gameState = gameState;
            _rawCharacterModels = characterModels;
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

        private List<Character_Model> CharacterModels => Characters.ModuleInstance.Data.StaticInfo.IsBeta ? _rawCharacterModels.ToList() : _rawCharacterModels.Where(e => !e.Beta).ToList();

        private bool IsTaskCanceled(CancellationToken cancellationToken)
        {
            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                if (_state is not SwappingState.LoggedOut) { _movedLeft = 0; };
                if (_state is SwappingState.MovedToStart) { _movedLeft = CharacterModels.Count; };

                _state = SwappingState.Canceled;
                return true;
            }

            return false;
        }

        public async Task MoveRight(CancellationToken cancellationToken, int amount = 1)
        {
            Characters.Logger.Info($"Move right to find {Character?.Name ?? "Unkown Character"}.");
            Status = strings.CharacterSwap_Right;
            for (int i = 0; i < amount; i++)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                await Delay(cancellationToken);
            }
        }

        public async Task MoveLeft(CancellationToken cancellationToken, int amount = 1)
        {
            Characters.Logger.Info($"Move left to find {Character?.Name ?? "Unkown Character"}.");
            Status = strings.CharacterSwap_Left;
            for (int i = 0; i < amount; i++)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                await Delay(cancellationToken);
            }
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

                        await MoveLeft(cancellationToken, Math.Min(CharacterModels.Count, _settings.CheckDistance.Value));

                        for (int i = 1; i < Math.Min(CharacterModels.Count, _settings.CheckDistance.Value * 2); i++)
                        {
                            await MoveRight(cancellationToken, 1);
                            await Delay(cancellationToken, 150);
                            if (await ConfirmName())
                            {
                                _state = SwappingState.CharacterFound;
                                return;
                            }
                        }

                        _state = SwappingState.CharacterFullyLost;
                    }

                    break;

                case SwappingState.CharacterFound:
                    if (await Login(cancellationToken))
                        _state = SwappingState.LoggingIn;
                    break;

                case SwappingState.LoggingIn:
                    if (IsLoaded())
                    {
                        _state = SwappingState.Done;
                        return;
                    }

                    break;
            }
        }

        public void Reset()
        {
            _state = SwappingState.None;
        }

        public bool Cancel()
        {
            bool canceled = _cancellationTokenSource is not null && !_cancellationTokenSource.IsCancellationRequested;

            _state = SwappingState.Canceled;
            _cancellationTokenSource?.Cancel();

            return canceled;
        }

        public void Start(Character_Model character, bool ignoreOCR = false, Logger logger = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {

                    PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
                    bool inCharSelection = _settings.UseBetaGamestate.Value ? !_gameState.IsIngame : !GameService.GameIntegration.Gw2Instance.IsInGame;

                    if (player is not null && player.Name == character.Name && !inCharSelection)
                    {
                        return;
                    }

                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new();
                    _cancellationTokenSource.CancelAfter(90000);

                    Character = character;
                    _state = GameService.GameIntegration.Gw2Instance.IsInGame ? SwappingState.None : SwappingState.LoggedOut;
                    _ignoreOCR = ignoreOCR;

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

                                    if (GameService.GameIntegration.Gw2Instance.IsInGame)
                                    {
                                        if (_settings.CloseWindowOnSwap.Value)
                                        {
                                            HideMainWindow?.Invoke();
                                        }
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
                catch { }
            });
        }

        private async Task Delay(CancellationToken cancellationToken, int? delay = null, double? partial = null)
        {
            delay ??= _settings.KeyDelay.Value;

            if (delay > 0)
            {
                if (partial is not null)
                {
                    delay = delay / 100 * (int)(partial * 100);
                }

                await Task.Delay(delay.Value, cancellationToken);
            }
        }

        private async Task<bool> LoggingOut(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return false; }

            Characters.Logger.Info("Logging out");
            if (GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                Status = strings.CharacterSwap_Logout;

                _ = await _settings.LogoutKey.Value.PerformPress(_settings.KeyDelay.Value, true, cancellationToken);

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                await Delay(cancellationToken);
                var stopwatch = new Stopwatch();

                if (_settings.UseBetaGamestate.Value)
                {
                    stopwatch.Start();
                    while (_gameState.IsIngame && stopwatch.ElapsedMilliseconds < 15000 && !cancellationToken.IsCancellationRequested)
                    {
                        await Delay(cancellationToken, 50);
                        if (cancellationToken.IsCancellationRequested) return !_gameState.IsIngame;
                    }

                    if (_settings.UseOCR.Value)
                    {
                        if (OCR.IsLoaded)
                        {
                            stopwatch.Start();
                            string txt = await OCR.Read();
                            while (stopwatch.ElapsedMilliseconds < 5000 && txt.Length <= 2 && !cancellationToken.IsCancellationRequested)
                            {
                                Characters.Logger.Info($"We are in the character selection but the OCR did only read '{txt}'. Waiting a bit longer!");
                                await Delay(cancellationToken, 250);
                                txt = await OCR.Read();
                                if (cancellationToken.IsCancellationRequested) return _gameState.IsCharacterSelection;
                            }
                        }
                        else
                        {
                            Characters.Logger.Info($"OCR did not load the engine fully. {Character?.Name ?? "Character Name"} can not be confirmed!");
                        }
                    }
                }
                else
                {
                    await Delay(cancellationToken, _settings.SwapDelay.Value);

                    if (_settings.UseOCR.Value)
                    {
                        if (OCR.IsLoaded)
                        {
                            stopwatch.Start();
                            string txt = await OCR.Read();
                            while (stopwatch.ElapsedMilliseconds < 5000 && txt.Length <= 2 && !cancellationToken.IsCancellationRequested)
                            {
                                Characters.Logger.Info($"We should be in the character selection but the OCR did only read '{txt}'. Waiting a bit longer!");
                                await Delay(cancellationToken, 250);
                                txt = await OCR.Read();
                                if (cancellationToken.IsCancellationRequested) return _gameState.IsCharacterSelection;
                            }
                        }
                        else
                        {
                            Characters.Logger.Info($"OCR did not load the engine fully. {Character?.Name ?? "Character Name"} can not be confirmed!");
                        }
                    }
                }

                stopwatch.Stop();
            }

            return !GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        private async Task MoveToFirstCharacter(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_MoveFirst;
            if (IsTaskCanceled(cancellationToken)) { return; }

            Characters.Logger.Info("Move to first Character.");
            var stopwatch = Stopwatch.StartNew();
            int moves = CharacterModels.Count - _movedLeft;
            for (int i = 0; i < moves; i++)
            {
                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    ExtendedInputService.MouseWiggle();
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

            Characters.Logger.Info($"Move to {Character?.Name ?? "Unkown Character"}.");
            var order = CharacterModels.OrderByDescending(e => e.LastLogin).ToList();

            var stopwatch = Stopwatch.StartNew();
            foreach (Character_Model character in order)
            {
                if (character == Character)
                {
                    break;
                }

                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    ExtendedInputService.MouseWiggle();
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
            if (!_settings.UseOCR.Value || _ignoreOCR || !OCR.IsLoaded) return true;
            if (Character == null || string.IsNullOrEmpty(Character.Name)) return false;

            Characters.Logger.Info($"Confirm {Character?.Name ?? "Unkown Character"}s name.");
            string ocr_result = _settings.UseOCR.Value ? await OCR.Read() : "No OCR";
            (string, int, int, int, bool) isBestMatch = ("No OCR enabled.", 0, 0, 0, false);

            if (_settings.UseOCR.Value)
            {
                Status = $"Confirm name ..." + Environment.NewLine + $"{ocr_result}";
                Characters.Logger.Info($"OCR Result: {ocr_result}.");

                if (_settings.OnlyEnterOnExact.Value)
                {
                    return Character.Name == ocr_result;
                }

                isBestMatch = Character.NameMatches(ocr_result);
                Characters.Logger.Info($"Swapping to {Character.Name} - Best result for : '{ocr_result}' is '{isBestMatch.Item1}' with edit distance of: {isBestMatch.Item2} and which is {isBestMatch.Item3} steps away in the character list. Resulting in a total difference of {isBestMatch.Item4}.");
                return isBestMatch.Item5;
            }

            return isBestMatch.Item5;
        }

        private async Task<bool> Login(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return false; }

            if (_settings.EnterOnSwap.Value)
            {
                Characters.Logger.Info($"Login to {Character?.Name ?? "Unkown Character"}.");
                Status = string.Format(strings.CharacterSwap_LoginTo, Character.Name);
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                await Delay(cancellationToken);

                if (_settings.UseBetaGamestate.Value)
                {
                    while (!_gameState.IsIngame && !cancellationToken.IsCancellationRequested)
                    {
                        await Delay(cancellationToken, 500);
                        if (cancellationToken.IsCancellationRequested) return false;
                    }
                }
                else
                {
                    while (!GameService.GameIntegration.Gw2Instance.IsInGame && !cancellationToken.IsCancellationRequested)
                    {
                        await Delay(cancellationToken, 500);
                        if (cancellationToken.IsCancellationRequested) return false;
                    }
                }
            }

            if (_settings.OpenInventoryOnEnter.Value)
            {
                _ = await _settings.InventoryKey.Value.PerformPress(_settings.KeyDelay.Value, false);
            }

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            if (player is not null && player.Name == Character.Name)
            {
                Character.UpdateCharacter(player);
                Succeeded?.Invoke(null, null);
            }

            return true;
        }

        private bool IsLoaded()
        {
            return !_settings.EnterOnSwap.Value || GameService.GameIntegration.Gw2Instance.IsInGame;
        }
    }
}
