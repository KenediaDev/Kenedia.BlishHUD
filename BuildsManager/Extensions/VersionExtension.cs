namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class VersionExtension
    {
        public static SemVer.Version Increment(this SemVer.Version version)
        {
            return version = new(version.Major, version.Minor, version.Patch + 1);            
        }
    }
}
