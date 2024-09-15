using System.ComponentModel.DataAnnotations.Schema;

namespace TreeCmpWebAPI.Models.Domain
{
    [Table("ResponseFiles")]
    public class NewickResponseFile
    {
        public Guid Id { get; set; }  // Klucz główny, generowany automatycznie
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }
        public string UserName { get; set; }  // Powinien odpowiadać kolumnie UserId w tabeli
        public string FileContent { get; set; }

    }
}
