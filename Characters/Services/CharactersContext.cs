using Blish_HUD;
using Blish_HUD.Contexts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public class CharactersContext : Context
    {
        private static readonly Logger Logger = Logger.GetLogger<CharactersContext>();

        public CharactersContext(CharacterSwapping characterSwapping)
        {
            
        }

        public enum ContextStatus
        {
            Ready,
            SwitchingRequested,
            SwitchingCharacter,
            LoadingCharacter,
            Error,
            Success,
        }

        public ContextStatus Status { get; private set; }

        public async Task SwitchCharacter(string name)
        {
            // Load the context
            Status = ContextStatus.SwitchingRequested;
            Logger.Debug($"WE SHOULD SWITCH CHARACTERS TO {name}");
            await Task.Delay(1000);

            Status = ContextStatus.SwitchingCharacter;
            Logger.Debug($"Look for a character named {name}...");
            await Task.Delay(1000);

            Logger.Debug($"Read the name of the character ...");
            await Task.Delay(1000);

            Logger.Debug($"Confirm it matches {name} ....");
            await Task.Delay(1000);

            Logger.Debug($"Load into the game...");
            await Task.Delay(5000);

            Status = ContextStatus.Success;
            Logger.Debug($"Character switched to {name}");
            await Task.CompletedTask;
        }
    }
}
