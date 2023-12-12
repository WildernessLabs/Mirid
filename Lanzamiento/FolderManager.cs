namespace Lanzamiento
{
    internal static class FolderManager
    {
        public static void CopyAndDeleteFiles(string sourceFolder, string targetFolder)
        {
            try
            {
                // Copy files from source to target
                CopyFilesRecursively(sourceFolder, targetFolder);
                Console.WriteLine($"Copied from {sourceFolder} tp {targetFolder}");

                // Delete files from target that are not in source
                DeleteFilesNotInSource(sourceFolder, targetFolder);
                Console.WriteLine($"Deleted orphan files from {targetFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void CopyFilesRecursively(string sourceFolder, string targetFolder)
        {
            foreach (string sourceFilePath in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                try
                {
                    // Skip hidden directories and .git directories
                    string sourceDirectory = Path.GetDirectoryName(sourceFilePath);
                    if (IsIgnoredDirectory(sourceDirectory))
                    {
                        continue;
                    }

                    // Calculate the relative path of the source file
                    string relativePath = sourceFilePath.Substring(sourceFolder.Length + 1);

                    // Create the corresponding target file path
                    string targetFilePath = Path.Combine(targetFolder, relativePath);

                    // Ensure that the target directory exists
                    string targetDirectory = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Copy the file
                    File.Copy(sourceFilePath, targetFilePath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file {sourceFilePath}: {ex.Message}");
                }
            }
        }

        static void DeleteFilesNotInSource(string sourceFolder, string targetFolder)
        {
            foreach (string targetFilePath in Directory.GetFiles(targetFolder, "*", SearchOption.AllDirectories))
            {
                try
                {
                    // Skip hidden directories and .git directories
                    string targetDirectory = Path.GetDirectoryName(targetFilePath);
                    if (IsIgnoredDirectory(targetDirectory))
                    {
                        continue;
                    }

                    // Calculate the relative path of the target file
                    string relativePath = targetFilePath.Substring(targetFolder.Length + 1);

                    // Create the corresponding source file path
                    string sourceFilePath = Path.Combine(sourceFolder, relativePath);

                    // Delete the file from target if it doesn't exist in source
                    if (!File.Exists(sourceFilePath))
                    {
                        File.Delete(targetFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file {targetFilePath}: {ex.Message}");
                }
            }
        }


        static bool IsIgnoredDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return true;
            }

            if ((File.GetAttributes(directoryPath) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                return true;
            }

            if (directoryPath.Contains(".git") && directoryPath.Contains(".github") == false)
            {
                return true;
            }

            return false;
        }
    }
}
