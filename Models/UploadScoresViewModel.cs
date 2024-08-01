namespace ConnectingDatabase.Models
{
    public class UploadScoresViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public List<SubjectMaster> Subjects { get; set; }
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
        public List<Score> Scores { get; set; }
        public List<int> Marks { get; set; }
    }
}
