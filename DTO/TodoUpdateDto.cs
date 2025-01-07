using System.ComponentModel.DataAnnotations;

namespace ActionList.DTO {
    public class TodoUpdateDto {
        public string Title { get; set; }
        public string Content { get; set; }
        [Required]
        [RegularExpression("Open|In progress|Finished", ErrorMessage = "Invalid state.")]
        public string State { get; set; }
    }
}
