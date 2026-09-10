// CONCEPT: business logic layer
// GradeService contains grade rules and operations.
// Forms call this class instead of saving grades directly to Excel.
using System;
using System.Collections.Generic;

namespace StudentManagementSystem
{
    // GradeService is responsible for grade validation and grade changes.
    public class GradeService
    {
        // The data-access object is kept private so only this service controls
        // when grade changes are saved.
        private readonly ExcelManager excelManager;

        public GradeService(ExcelManager excelManager)
        {
            this.excelManager = excelManager;
        }

        // AddGrade validates the grade, connects it to the student, and saves it.
        public void AddGrade(List<Student> allStudents, Student student, Grade grade)
        {
            ValidateGrade(grade);
            grade.StudentId = student.StudentId;
            student.Grades.Add(grade);
            excelManager.SaveStudents(allStudents);
        }

        // UpdateGrade changes the selected grade after validating the new values.
        public void UpdateGrade(List<Student> allStudents, Student student, Grade existingGrade, Grade updatedGrade)
        {
            ValidateGrade(updatedGrade);
            existingGrade.CourseName = updatedGrade.CourseName;
            existingGrade.GradeValue = updatedGrade.GradeValue;
            excelManager.SaveStudents(allStudents);
        }

        // DeleteGrade removes one grade and saves the remaining data.
        public void DeleteGrade(List<Student> allStudents, Student student, Grade grade)
        {
            if (!student.Grades.Remove(grade))
            {
                throw new InvalidOperationException("The selected grade no longer exists.");
            }

            excelManager.SaveStudents(allStudents);
        }

        // Grade validation is business logic rather than UI logic. This keeps the
        // 0-to-100 rule in one place.
        private static void ValidateGrade(Grade grade)
        {
            if (string.IsNullOrWhiteSpace(grade.CourseName))
            {
                throw new ArgumentException("Course Name is required.");
            }

            if (grade.GradeValue < 0 || grade.GradeValue > 100)
            {
                throw new ArgumentException("Grade must be between 0 and 100.");
            }
        }
    }
}
