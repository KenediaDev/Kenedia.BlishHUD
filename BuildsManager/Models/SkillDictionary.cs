using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System.Collections.Generic;

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
