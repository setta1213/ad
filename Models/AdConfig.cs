namespace StudentAdWindowsApp.Models
{
    public class AdConfig
    {
        public string Domain { get; set; } = "";
        public string OuPath { get; set; } = "";
        public string AdminUser { get; set; } = "";
        public string AdminPassword { get; set; } = "";
        public bool IsStarted { get; set; }
        public int ApiPort { get; set; } = 5000;

    }
}
