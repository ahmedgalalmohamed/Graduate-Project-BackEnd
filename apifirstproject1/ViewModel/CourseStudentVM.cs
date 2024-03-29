﻿namespace Graduate_Project_BackEnd.ViewModel
{
    public class CourseStudentVM
    {
        public int AvailableTeams { get; set; }
        public int AvailableStudents { get; set; }
        public int? AvailableProffessors { get; set; }
        public int? ProId { get; set; }
        public int? MyTeam { get; set; }
        public int? TeamLeader { get; set; }
        public int? MinStd { get; set; }
        public int? MaxStd { get; set; }
        public string Name { get; set; }
        public bool ? IsGraduate { get; set; }
        public string Description { get; set; }
    }
}
