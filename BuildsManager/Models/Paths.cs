using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public string TemplatesPath => $@"{BasePath}\{ModuleName}\templates\";
    }
}
