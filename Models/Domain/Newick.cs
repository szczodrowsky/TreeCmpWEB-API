﻿using System.ComponentModel.DataAnnotations.Schema;

namespace TreeCmpWebAPI.Models.Domain
{
    public class Newick
    {
        public Guid Id { get; set; }

        public Guid OperationId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string UserName { get; set; }

        public string? comparisionMode { get; set; } 

        public string? newickFirstString { get; set; }

        public string? newickSecondString { get; set; }

        public string? windowWidth { get; set; } 

        public string[] rootedMetrics { get; set; } 

        public string[] unrootedMetrics { get; set; }

        public bool? normalizedDistances { get; set; } 

        public bool? pruneTrees { get; set; }

        public bool? includeSummary { get; set; }

        public bool? zeroWeightsAllowed { get; set; }

        public bool? bifurcationTreesOnly { get; set; }

    }
}
