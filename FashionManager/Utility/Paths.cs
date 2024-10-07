using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Models;
using System.IO;

namespace Kenedia.Modules.FashionManager.Utility
{
    public class Paths : PathCollection
    {
        public Paths() : base()
        {
            
        }

        public Paths(DirectoriesManager directoriesManager, Module module) : base(directoriesManager, module)
        {
            if (!Directory.Exists(TemplatesPath)) _ = Directory.CreateDirectory(TemplatesPath);
            if (!Directory.Exists(DataPath)) _ = Directory.CreateDirectory(DataPath);
        }

        public string TemplatesPath => $@"{BasePath}\{ModuleName}\templates\";

        public string DataPath => $@"{BasePath}\{ModuleName}\data\";
    }
}
