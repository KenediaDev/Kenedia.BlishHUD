using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework.Input;
using System;
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

    public static class CharacterSwapping
    {
        private static CancellationTokenSource s_cancellationTokenSource;

        private static int s_movedLeft;

        public static event EventHandler Succeeded;
        public static event EventHandler Failed;

        public static event EventHandler Started;
        public static event EventHandler Finished;

        public static event EventHandler StatusChanged;

        private static string s_status;

        private static SwappingState s_state = SwappingState.None;

        public static string Status
        {
            set
            {
                s_status = value;
                StatusChanged?.Invoke(null, EventArgs.Empty);
            }
            get => s_status;
        }

        public static Character_Model Character { get; set; }

        private static bool IsTaskCanceled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (s_state is not SwappingState.LoggedOut) { s_movedLeft = 0; };
                if (s_state is SwappingState.MovedToStart) { s_movedLeft = Characters.ModuleInstance.CharacterModels.Count; };

                s_state = SwappingState.Canceled;
                return true;
            }

            return false;
        }

        public static async Task MoveRight(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_Right;
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
        }

        public static async Task MoveLeft(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_Left;
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
            await Delay(cancellationToken);
        }

        public static async Task Run(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return; }

            switch (s_state)
            {
                case SwappingState.None:
                    if (await LoggingOut(cancellationToken))
                    {
                        s_state = SwappingState.LoggedOut;
                    }

                    await Delay(cancellationToken);
                    break;

                case SwappingState.LoggedOut:
                    await MoveToFirstCharacter(cancellationToken);
                    s_state = SwappingState.MovedToStart;
                    s_movedLeft = 0;
                    await Delay(cancellationToken, 250);
                    break;

                case SwappingState.MovedToStart:
                    await MoveToCharacter(cancellationToken);
                    s_state = SwappingState.MovedToCharacter;
                    await Delay(cancellationToken, 250);

                    break;

                case SwappingState.MovedToCharacter:
                    if (ConfirmName())
                    {
                        s_state = SwappingState.CharacterFound;
                    }
                    else
                    {
                        s_state = SwappingState.CharacterLost;

                        await MoveLeft(cancellationToken);
                        await Delay(cancellationToken, 250);
                        if (ConfirmName())
                        {
                            s_state = SwappingState.CharacterFound;
                            return;
                        }

                        await MoveRight(cancellationToken);
                        await Delay(cancellationToken, 250);
                        if (ConfirmName())
                        {
                            s_state = SwappingState.CharacterFound;
                            return;
                        }

                        s_state = SwappingState.CharacterFullyLost;
                    }

                    break;

                case SwappingState.CharacterFound:
                    await Login(cancellationToken);
                    s_state = SwappingState.LoggingIn;
                    break;

                case SwappingState.LoggingIn:
                    if (IsLoaded())
                    {
                        s_state = SwappingState.Done;
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

        public static void Reset()
        {
            s_state = SwappingState.None;
        }

        public static void Cancel()
        {
            s_state = SwappingState.Canceled;
            s_cancellationTokenSource?.Cancel();
            //s_cancellationTokenSource = null;
        }

        public static async void Start(Character_Model character)
        {
            s_cancellationTokenSource?.Cancel();
            s_cancellationTokenSource = new();
            s_cancellationTokenSource.CancelAfter(90000);

            Character = character;
            s_state = GameService.GameIntegration.Gw2Instance.IsInGame ? SwappingState.None : SwappingState.LoggedOut;

            Started?.Invoke(null, null);
            Status = string.Format(strings.CharacterSwap_SwitchTo, Character.Name);
            while (s_state is not SwappingState.Done and not SwappingState.CharacterFullyLost and not SwappingState.Canceled && !s_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Run(s_cancellationTokenSource.Token);

                    switch (s_state)
                    {
                        case SwappingState.Done:
                            Status = strings.Status_Done;
                            Succeeded?.Invoke(null, null);
                            if (GameService.GameIntegration.Gw2Instance.IsInGame)
                            {
                                if (Characters.ModuleInstance.Settings.CloseWindowOnSwap.Value)
                                {
                                    Characters.ModuleInstance.MainWindow.Hide();
                                }
                                Character.LastLogin = DateTime.UtcNow;
                            }

                            break;

                        case SwappingState.CharacterFullyLost:
                            Status = string.Format(strings.CharacterSwap_FailedSwap, Character.Name);
                            Failed?.Invoke(null, null);

                            if (Characters.ModuleInstance.Settings.AutoSortCharacters.Value)
                            {
                                CharacterSorting.Start(Characters.ModuleInstance.CharacterModels);
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

        private static async Task Delay(CancellationToken cancellationToken, int? delay = null, double? partial = null)
        {
            delay ??= Characters.ModuleInstance.Settings.KeyDelay.Value;

            if (delay > 0)
            {
                if (partial != null)
                {
                    delay = delay / 100 * (int)(partial * 100);
                }

                await Task.Delay(delay.Value, cancellationToken);
            }
        }

        private static async Task<bool> LoggingOut(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return false; }

            if (GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                Status = strings.CharacterSwap_Logout;
                ModifierKeys mods = ModifierKeys.None;
                var primary = (VirtualKeyShort)Characters.ModuleInstance.Settings.LogoutKey.Value.PrimaryKey;

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

                if (Characters.ModuleInstance.Settings.UseBetaGamestate.Value)
                {
                    while(Core.Services.GameState.GameStatus != GameStatus.CharacterSelection && !cancellationToken.IsCancellationRequested)
                    {
                        await Delay(cancellationToken, 250);
                        if(cancellationToken.IsCancellationRequested) return Core.Services.GameState.GameStatus == GameStatus.CharacterSelection;
                    }
                }
                else
                {
                    await Delay(cancellationToken, Characters.ModuleInstance.Settings.SwapDelay.Value);
                }
            }

            return !GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        private static async Task MoveToFirstCharacter(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_MoveFirst;
            if (IsTaskCanceled(cancellationToken)) { return; }

            var stopwatch = Stopwatch.StartNew();
            int moves = Characters.ModuleInstance.CharacterModels.Count - s_movedLeft;
            for (int i = 0; i < moves; i++)
            {
                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    InputService.MouseWiggle();
                    stopwatch.Restart();
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                s_movedLeft++;
                await Delay(cancellationToken, null, 0.05);
                if (IsTaskCanceled(cancellationToken)) { return; }
            }

            return;
        }

        private static async Task MoveToCharacter(CancellationToken cancellationToken)
        {
            Status = string.Format(strings.CharacterSwap_MoveTo, Character.Name);
            if (IsTaskCanceled(cancellationToken)) { return; }

            var order = Characters.ModuleInstance.CharacterModels.OrderByDescending(e => e.LastLogin).ToList();

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

        private static bool ConfirmName()
        {
            if (!Characters.ModuleInstance.Settings.UseOCR.Value) return true;
            if (Character == null || string.IsNullOrEmpty(Character.Name)) return false;

            string ocr_result = Characters.ModuleInstance.Settings.UseOCR.Value ? Characters.ModuleInstance.OCR.Read() : "No OCR";
            (string, int, int, int, bool) isBestMatch = ("No OCR enabled.", 0, 0, 0, false);

            if (Characters.ModuleInstance.Settings.UseOCR.Value)
            {
                Status = $"Confirm name ..." + Environment.NewLine + $"{ocr_result}";
                Characters.Logger.Info($"OCR Result: {ocr_result}.");

                isBestMatch = Character.NameMatches(ocr_result);
                Characters.Logger.Info($"Best result for : '{ocr_result}' is '{isBestMatch.Item1}' with edit distance of: {isBestMatch.Item2} and which is {isBestMatch.Item3} steps away in the character list. Resulting in a total difference of {isBestMatch.Item4}.");
                return isBestMatch.Item5;
            }

            return isBestMatch.Item5;
        }

        private static async Task Login(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return; }

            if (Characters.ModuleInstance.Settings.EnterOnSwap.Value)
            {
                Status = string.Format(strings.CharacterSwap_LoginTo, Character.Name);
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                await Delay(cancellationToken);
                await Delay(cancellationToken, 1000);
            }

            return;
        }

        private static bool IsLoaded()
        {
            return !Characters.ModuleInstance.Settings.EnterOnSwap.Value || GameService.GameIntegration.Gw2Instance.IsInGame;
        }
    }
}
