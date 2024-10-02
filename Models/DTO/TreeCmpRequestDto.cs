using System.ComponentModel;

namespace TreeCmpWebAPI.Models.DTO
{
    public class TreeCmpRequestDto
    {

        public string? comparisionMode { get; set; }

        public string? newickFirstString { get; set; }

        public string? newickSecondString { get; set; }

        public string? windowWidth { get; set; }

        public string[] rootedMetrics { get; set; }

        public string[] unrootedMetrics { get; set; }

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

    }
}