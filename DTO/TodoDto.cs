using System.ComponentModel.DataAnnotations;

namespace ActionList.DTO {
    public class TodoDto {
        public Guid Id { get; set; }

        //[Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        //[Required(ErrorMessage = "State is required.")]
        public string State { get; set; }

        //[Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }
        public DateTime Created { get; set; }
    }

}
