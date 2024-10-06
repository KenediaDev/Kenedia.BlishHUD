using Kenedia.Modules.Core.Models;
using System.IO;

namespace Kenedia.Modules.FashionManager.Utility
{
    public class Paths : PathCollection
    {
        public Paths()
        {
        }

        public Paths(Blish_HUD.Modules.Managers.DirectoriesManager directoriesManager, string moduleName) : base(directoriesManager, moduleName)
        {
            if (!Directory.Exists(TemplatesPath)) _ = Directory.CreateDirectory(TemplatesPath);
        }

        public string TemplatesPath => $@"{BasePath}\{ModuleName}\templates\";
    }
}
