using Blish_HUD.Modules.Managers;
using System.IO;

namespace Kenedia.Modules.Core.Models
{
#nullable enable
    public class PathCollection
    {
        private readonly string? _moduleName = null;
        private string? _accountName = null;
        private readonly DirectoriesManager _directoriesManager;

        public string? AccountName
        {
            get => _accountName;
            set
            {
                _accountName = value;
                if (!string.IsNullOrEmpty(value)) AddAccountFolder();
            }
        }

        public PathCollection(DirectoriesManager directoriesManager, string moduleName)
        {
            _directoriesManager = directoriesManager;
            _moduleName = moduleName.Replace(' ', '_').ToLower();

            BasePath = _directoriesManager.GetFullDirectoryPath("kenedia");

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

        public string ModulePath => $@"{BasePath}\{_moduleName}\";

        public string ModuleDataPath => $@"{BasePath}\{_moduleName}\data\";

        public string SharedSettingsPath => $@"{BasePath}\shared_settings.json";

        public string? AccountPath => _accountName != null ? $@"{ModulePath}\{AccountName}\" : null;

        private void AddAccountFolder()
        {
            if (!Directory.Exists(AccountPath))
            {
                _ = Directory.CreateDirectory(AccountPath);
            }
        }
    }
#nullable disable
}
