using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Models;
using System.IO;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class Paths : PathCollection
    {
        public Paths()
        {
        }

        public Paths(DirectoriesManager directoriesManager, string moduleName) : base(directoriesManager, moduleName)
        {
            if(!Directory.Exists(TemplatesPath)) _ = Directory.CreateDirectory(TemplatesPath);
            if(!Directory.Exists(ItemMapPath)) _ = Directory.CreateDirectory(ItemMapPath);
        }

        public string TemplatesPath => $@"{BasePath}\{ModuleName}\templates\";

        public string ItemMapPath => $@"{ModuleDataPath}\itemmap\";
    }
}
