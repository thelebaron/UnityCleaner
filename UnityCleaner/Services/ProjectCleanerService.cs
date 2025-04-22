using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCleaner.Models;

namespace UnityCleaner.Services
{
    public class ProjectCleanerService
    {
        public delegate void ProgressUpdateHandler(int projectsCompleted, int totalProjects, string currentProject);
        public event ProgressUpdateHandler? ProgressUpdated;

        public async Task CleanProjectsAsync(IEnumerable<UnityProject> projects, IEnumerable<CleanPattern> patterns, bool useRecycleBin = false)
        {
            var enabledPatterns = patterns.Where(p => p.IsEnabled).Select(p => p.Pattern);
            var selectedProjects = projects.Where(p => p.IsSelectedForCleaning).ToList();
            var totalProjects = selectedProjects.Count;
            var completedProjects = 0;

            await Task.Run(() =>
            {
                foreach (var project in selectedProjects)
                {
                    // Report progress before starting to clean this project
                    ProgressUpdated?.Invoke(completedProjects, totalProjects, project.ProjectName);

                    // Clean the project
                    project.Clean(enabledPatterns, useRecycleBin);

                    // Update progress after cleaning
                    completedProjects++;
                    ProgressUpdated?.Invoke(completedProjects, totalProjects, project.ProjectName);
                }
            });
        }
    }
}
