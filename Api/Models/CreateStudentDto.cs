namespace StudentAdWindowsApp.Api.Models
{
    public class CreateStudentDto
    {
        public string StudentId { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }
}
