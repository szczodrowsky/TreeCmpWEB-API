using System.ComponentModel.DataAnnotations.Schema;

namespace TreeCmpWebAPI.Models.Domain
{
    [Table("ResponseFiles")]
    public class NewickResponseFile
    {
        public Guid Id { get; set; }
        public Guid OperationId { get; set; }
        public string FileName { get; set; }
        public string UserName { get; set; } 
        public string FileContent { get; set; }



    }
}
