using System.ComponentModel.DataAnnotations;

namespace ActionList.DTO {
    public class TodoUpdateDto {
        public string Title { get; set; }
        public string Content { get; set; }        
        public string State { get; set; }
    }
}
