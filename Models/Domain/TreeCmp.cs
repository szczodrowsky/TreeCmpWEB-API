using System.ComponentModel;

namespace TreeCmpWebAPI.Models.Domain
{
    public class TreeCmp
    {
        public Guid Id { get; set; }

        public string? comparisionMode { get; set; }

        public string? newickFirstString { get; set; }

        public string? newickSecondString { get; set; }

        public string? windowWidth { get; set; }

        public string[] rootedMetrics { get; set; }

        public string[] unrootedMetrics { get; set; }
        public string[] Metrics { get; set; }

        [DefaultValue(false)]
        public bool normalizedDistances { get; set; }

        [DefaultValue(false)]
        public bool pruneTrees { get; set; }

        [DefaultValue(false)]
        public bool includeSummary { get; set; }

        [DefaultValue(false)]
        public bool zeroWeightsAllowed { get; set; }


        [DefaultValue(false)]
        public bool? bifurcationTreesOnly { get; set; }

        public string InputFile { get; set; }
        public string? RefTreeFile { get; set; }
        public string OutputFile { get; set; }
        public string UserName { get; set; }

    }
}
