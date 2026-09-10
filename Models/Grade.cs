namespace StudentManagementSystem
{
    // CONCEPT: class
    // Grade is a blueprint for one course result belonging to a student.
    public class Grade
    {
        // These properties store the student link, course name, and numeric result.
        // get and set allow forms and ExcelManager to read and update the values.
        public string StudentId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public double GradeValue { get; set; }
    }
}
