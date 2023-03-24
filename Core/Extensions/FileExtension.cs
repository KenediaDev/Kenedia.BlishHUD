using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.Core.Extensions
{
    public static class FileExtension
    {
        public static async Task<bool> WaitForFileUnlock(string path, int maxDuration = 2500, CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;
            bool locked = IsFileLocked(new FileInfo(path));

            for (int i = 0; i < maxDuration / 250; i++)
            {
                if (locked && cancellationToken?.IsCancellationRequested != true)
                {
                    await Task.Delay(250, (CancellationToken)cancellationToken);
                    locked = IsFileLocked(new FileInfo(path));
                }
            }

            return !locked && cancellationToken?.IsCancellationRequested != true;
        }

        public static bool IsFileLocked(FileInfo file)
        {
            if(!File.Exists(file.FullName)) return false;

            try
            {
                using FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
