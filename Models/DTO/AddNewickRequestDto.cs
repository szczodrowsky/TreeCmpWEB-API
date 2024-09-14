using System.ComponentModel.DataAnnotations.Schema;

namespace TreeCmpWebAPI.Models.DTO
{
    public class AddNewickRequestDto
    {
        public string? comparisionMode { get; set; }

        public string? newickFirstString { get; set; }

        public string? newickSecondString { get; set; }

        public string? windowWidth { get; set; }

        public string[]? rootedMetrics { get; set; }

        public string[]? unrootedMetrics { get; set; }

        public bool? normalizedDistances { get; set; }

        public bool? pruneTrees { get; set; }

        public bool? includeSummary { get; set; }

        public bool? zeroWeightsAllowed { get; set; }

        public bool? bifurcationTreesOnly { get; set; }


    }
}

/////te metryki trzeba cos wymyśleć
/// tak o to nie pójdzie
/// 61 odcinek jak dodawać contexty dokłądnme