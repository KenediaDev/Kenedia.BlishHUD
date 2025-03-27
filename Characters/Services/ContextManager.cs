using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public class ContextManager
    {
        public ContextManager(CharactersContext charactersContext)
        {
            CharactersContext = charactersContext;
        }

        public CharactersContext CharactersContext { get; }

    }
}
