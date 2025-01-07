namespace ActionList.Model {
    public class Todo {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int State { get; set; }
        public string Content { get; set; }
    }

    public enum TodoState {
        Open = 1,
        InProgress = 2,
        Finished = 3
    }
}
