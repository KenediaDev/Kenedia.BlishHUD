using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Models
{
    public class AccountData
    {
        public string AccountName { get; set; }

        public TagList Tags { get; } = new TagList();

        public ObservableCollection<Character_Model> CharacterModels { get; } = new();

        public Character_Model CurrentCharacterModel { get; set; }

        public PathCollection Paths { get; set; }

        public List<CharacterCard> CharacterCards { get; set; } = new();

        public List<Character_Model> LoadedModels { get; set; } = new();
    }
}
