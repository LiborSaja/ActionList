namespace ActionList.DTO {
    public class ErrorResponse {
        public bool IsError { get; set; } = true;
        public ErrorDetail Error { get; set; }
    }

    public class ErrorDetail {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<string> Details { get; set; }
    }
}
