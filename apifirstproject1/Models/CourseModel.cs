﻿namespace Graduate_Project_BackEnd.Models
{
    public class CourseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desciption { get; set; }
        public bool IsGraduate { get; set; }
        public int MinStd { get; set; } = 3;
        public int MaxStd { get; set; } = 5;
        public InstructorModel Instructor { get; set; }
        public int InstructorID { get; set; }

        public ICollection<TeamModel> Teams { get; set; }
        public ICollection<Courses_StudentsModel> Students { get; set; }

    }
}
