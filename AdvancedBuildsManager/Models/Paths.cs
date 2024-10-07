using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Models;
using System.IO;

namespace Kenedia.Modules.AdvancedBuildsManager.Models
{
    public class Paths : PathCollection
    {
        public Paths()
        {
            
        }

        public Paths(DirectoriesManager directoriesManager, Module module) : base(directoriesManager, module)
        {
            if(!Directory.Exists(TemplatesPath)) _ = Directory.CreateDirectory(TemplatesPath);
        }

        public string TemplatesPath => $@"{BasePath}\{ModuleName}\templates\";
    }
}
