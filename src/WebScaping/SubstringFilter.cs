using System.Collections.Generic;
using System.Linq;

namespace CampfireTools.WebScaping 
{
    public class SubstringFilter
    {
        public List<string>? RequiredSubstrings { get; set; }
        public List<string>? ForbiddenSubstrings { get; set; }

        public SubstringFilter(List<string>? requiredSubstrings = null, List<string>? forbiddenSubstrings = null)
        {
            RequiredSubstrings = requiredSubstrings;
            ForbiddenSubstrings = forbiddenSubstrings;
        }

        // Filter list of strings in a way that only strings that contain all required substrings and none of the forbidden substrings are returned
        public List<string> Filter(List<string> data)
        {
            List<string> filteredData = new List<string>();
            foreach (string str in data)
            {
                // Check if string contains all required substrings
                if (RequiredSubstrings != null && RequiredSubstrings.Any() && !RequiredSubstrings.All(substring => str.ToLower().Contains(substring.ToLower())))
                {
                    continue;
                }

                // Check if string contains any forbidden substrings
                if (ForbiddenSubstrings != null && ForbiddenSubstrings.Any() && ForbiddenSubstrings.Any(substring => str.ToLower().Contains(substring.ToLower())))
                {
                    continue;
                }

                filteredData.Add(str);
            }

            return filteredData;
        }
    }
}

