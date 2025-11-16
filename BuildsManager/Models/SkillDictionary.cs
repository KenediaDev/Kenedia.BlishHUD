using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class SkillDictionary : Dictionary<int, Skill>
    {

        public Skill? Get(int id)
        {
            _ = TryGetValue(id, out Skill? skill);

            return skill;
        }
    }
}
