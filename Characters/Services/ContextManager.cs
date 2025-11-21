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
