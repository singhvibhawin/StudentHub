﻿namespace ConnectingDatabase.Models
{
    public class StudentModel
    {
        public IEnumerable<Student> students { get; set; }
        public Student student { get; set; }
        public IEnumerable<DegreeMaster> Degrees { get; set; }
    }
}
