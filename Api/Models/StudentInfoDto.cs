namespace StudentAdWindowsApp.Api.Models
{
	public class StudentInfoDto
	{
		public string StudentId { get; set; } = "";
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string DisplayName { get; set; } = "";

		public string Email { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Office { get; set; } = "";

		// ⭐ Profile / Home folder
		public string ProfilePath { get; set; } = "";
		public string LogonScript { get; set; } = "";
		public string HomeDirectory { get; set; } = "";
		public string HomeDrive { get; set; } = "";

		public bool Enabled { get; set; }
	}
}
