
namespace ActionList.DTO {
    public class TodoDto {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
    }

}
