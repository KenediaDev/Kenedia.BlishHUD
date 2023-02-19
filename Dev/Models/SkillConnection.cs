using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Dev.Models
{
    public enum Enviroment
    {
        Unkown,
        Any,
        Terrestrial,
        Aquatic,
    }

    public class SkillConnection
    {
        public int Id { get; set; }

        public Enviroment Enviroment { get; set; } = Enviroment.Any;

        public int? Parent { get; set; }

        public List<int> Chain { get; set; }

        public List<int> Bundle { get; set; }

        public List<int> Transform { get; set; }

        public List<int> FlipSkills { get; set; }

        public List<int> AdrenalinStages { get; set; }

        public SkillWeaponType? Weapon { get; set; }
        
        public SkillWeaponType? DualWeapon { get; set; }

        public Specializations? Specialization { get; set; }

        public Dictionary<int, List<int>> Traited { get; set; }

        public Attunement? Attunement { get; set; }

        public Attunement? DualAttunement { get; set; }

        public int? Ambush { get; set; }

        public int? Stealth { get; set; }

        public int? Toolbelt { get; set; }

        public int? AdrenalinCharges { get; set; }
    }
}
