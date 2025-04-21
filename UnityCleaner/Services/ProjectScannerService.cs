using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityCleaner.Models;

namespace UnityCleaner.Services
{
    public class ProjectScannerService
    {
        public async Task<List<UnityProject>> ScanDirectoryAsync(string directoryPath, int maxDepth = 3)
        {
            return await Task.Run(() => ScanDirectory(directoryPath, maxDepth));
        }

        private List<UnityProject> ScanDirectory(string directoryPath, int maxDepth)
        {
            var projects = new List<UnityProject>();
            
            try
            {
                // Check if the current directory is a Unity project
                var currentDir = new UnityProject(directoryPath);
                if (currentDir.IsValidUnityProject())
                {
                    projects.Add(currentDir);
                    return projects;
                }

                // If not a Unity project and we haven't reached max depth, scan subdirectories
                if (maxDepth > 0)
                {
                    foreach (var subDir in Directory.GetDirectories(directoryPath))
                    {
                        try
                        {
                            projects.AddRange(ScanDirectory(subDir, maxDepth - 1));
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Skip directories we don't have access to
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions (access denied, etc.)
            }

            return projects;
        }
    }
}
