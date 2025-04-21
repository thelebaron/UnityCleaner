using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCleaner.Models;

namespace UnityCleaner.Services
{
    public class ProjectCleanerService
    {
        public async Task CleanProjectsAsync(IEnumerable<UnityProject> projects, IEnumerable<CleanPattern> patterns, bool useRecycleBin = false)
        {
            var enabledPatterns = patterns.Where(p => p.IsEnabled).Select(p => p.Pattern);

            await Task.Run(() =>
            {
                foreach (var project in projects.Where(p => p.IsSelectedForCleaning))
                {
                    project.Clean(enabledPatterns, useRecycleBin);
                }
            });
        }
    }
}
