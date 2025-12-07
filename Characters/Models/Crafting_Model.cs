using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.Characters.Models
{
    public class CharacterCrafting
    {
        public CraftingDisciplineType Id { get; set; }

        public int Rating { get; set; }

        public bool Active { get; set; }
    }
}
