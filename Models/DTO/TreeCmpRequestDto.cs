using System.ComponentModel;

namespace TreeCmpWebAPI.Models.DTO
{
    public class TreeCmpRequestDto
    {
        public string ComparisonMode { get; set; } 
        public string NewickFirstString { get; set; } 
        public string? NewickSecondString { get; set; } 
        
        public string? windowWidth { get; set; }
        public string[] RootedMetrics { get; set; } 
        public string[] UnrootedMetrics { get; set; } 
        public string[] Metrics { get; set; }

        [DefaultValue(false)]
        public bool normalizedDistances { get; set; }

        [DefaultValue(false)]
        public bool pruneTrees { get; set; }

        [DefaultValue(false)]
        public bool includeSummary { get; set; }

        [DefaultValue(false)]
        public bool zeroWeightsAllowed { get; set; }

        public string InputFile { get; set; } 
        public string? RefTreeFile { get; set; } 
        public string OutputFile { get; set; }

    }
}
