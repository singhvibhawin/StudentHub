namespace ConnectingDatabase.Models
{
    public class ApprovedDataViewModel
    {
        public int UserId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
        public List<int> SubjectIds { get; set; }
        public int YearId { get; set; }
        public int SemesterId { get; set; }

        // Add properties for the additional data
        public List<SubjectMaster> Subjects { get; set; }
        public List<SemesterEnrollment> Semesters { get; set; }
        public List<YearMaster> Years { get; set; }

    }

}
