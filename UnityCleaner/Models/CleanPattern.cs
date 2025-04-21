using System;

namespace UnityCleaner.Models
{
    public class CleanPattern
    {
        public string Pattern { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDefault { get; set; }

        public CleanPattern(string pattern, bool isEnabled = true, bool isDefault = false)
        {
            Pattern = pattern;
            IsEnabled = isEnabled;
            IsDefault = isDefault;
        }
    }
}
