// List<T> and LINQ are used by the Student class.
using System.Collections.Generic;
using System.Linq;

namespace StudentManagementSystem
{
    // CONCEPT: class
    // A class is a blueprint used to create objects.
    // Student represents one actual student in our application.
    // public means other classes, such as MainForm, are allowed to use Student.
    public class Student
    {
        // CONCEPT: property with get/set
        // A property stores data and controls how other code reads or changes it.
        // string is the data type used for text values.
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // CONCEPT: List<T> and generic type
        // List<Grade> is a generic collection: <Grade> says this list stores
        // Grade objects. Each Student owns its grades.
        public List<Grade> Grades { get; set; } = new List<Grade>();

        // CONCEPT: expression-bodied property
        // This calculates a value whenever Average is read.
        // CONCEPT: LINQ and lambda expression
        // Average is a LINQ method. grade => grade.GradeValue is a lambda
        // expression telling LINQ which value should be averaged.
        public double Average => Grades.Count == 0 ? 0 : Grades.Average(grade => grade.GradeValue);
    }
}
