using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Utility;
using System.IO;

namespace Kenedia.Modules.Core.Models
{
    public class PathCollection
    {
        protected readonly string? ModuleName = null;
        protected readonly DirectoriesManager DirectoriesManager;
        private string? _accountName = null;

        public string? AccountName
        {
            get => _accountName;
            set => Common.SetProperty(ref _accountName, value, AddAccountFolder, !string.IsNullOrEmpty(value));
        }
        public PathCollection()
        {
            
        }

        public PathCollection(DirectoriesManager directoriesManager, string moduleName)
        {
            DirectoriesManager = directoriesManager;
            ModuleName = moduleName.Replace(' ', '_').ToLower();

            BasePath = DirectoriesManager.GetFullDirectoryPath("kenedia");

            if (!Directory.Exists(ModulePath))
            {
                _ = Directory.CreateDirectory(ModulePath);
            }

            if (!Directory.Exists(ModuleDataPath))
            {
                _ = Directory.CreateDirectory(ModuleDataPath);
            }
        }

        public string BasePath { get; }

        public string ModulePath => $@"{BasePath}\{ModuleName}\";

        public string ModuleDataPath => $@"{BasePath}\{ModuleName}\data\";

        public string SharedSettingsPath => $@"{BasePath}\shared_settings.json";

        public string? AccountPath => _accountName is not null ? $@"{ModulePath}{AccountName}\" : null;

        private void AddAccountFolder()
        {
            if (!Directory.Exists(AccountPath))
            {
                _ = Directory.CreateDirectory(AccountPath);
            }
        }
    }
}
