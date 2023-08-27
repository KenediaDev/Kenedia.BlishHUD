using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kenedia.Modules.Core.Extensions
{
    public static class DirectoryExtension
    {
        public static void MoveFiles(string sourceDirectory, string targetDirectory)
        {
            try
            {
                // Get all file paths in the source directory
                string[] filePaths = Directory.GetFiles(sourceDirectory);

                // Copy each file to the target directory
                foreach (string filePath in filePaths)
                {
                    string fileName = Path.GetFileName(filePath);
                    string targetPath = Path.Combine(targetDirectory, fileName);
                    File.Copy(filePath, targetPath, true); // Set the last parameter to true to overwrite if the file already exists
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
