namespace TreeCmpWebAPI.Models.DTO
{
    public class CombinedNewickData
    {
        public Guid OperationId { get; set; }

        public string? newickFirstString { get; set; }
        public string? newickSecondString { get; set; }

        public string? FileContent {  get; set; }




    }
}
