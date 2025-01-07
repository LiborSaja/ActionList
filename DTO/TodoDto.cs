using System.ComponentModel.DataAnnotations;

namespace ActionList.DTO {
    public class TodoDto {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [Range(0, 2, ErrorMessage = "State must be 0 (open), 1 (in progress), or 2 (finished).")]
        public int? State { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }
        public DateTime Created { get; set; }
    }
}
