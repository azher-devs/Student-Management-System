// CONCEPT: business logic layer
// This class contains student rules and operations.
// It sits between the Windows Forms and ExcelManager:
// Forms -> StudentService -> ExcelManager.
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentManagementSystem
{
    // CONCEPT: class
    // StudentService is a business-logic class. It does not create controls
    // and it does not know how Excel cells are written.
    public class StudentService
    {
        // This field is a dependency: the service needs a data-access object
        // to save and load data. The service does not write Excel directly.
        private readonly ExcelManager excelManager;

        // CONCEPT: constructor and parameter
        // The caller supplies the data-access object as an argument.
        public StudentService(ExcelManager excelManager)
        {
            this.excelManager = excelManager;
        }

        // Load is a business-layer method that delegates storage work to the
        // data-access layer and returns the application data to the form.
        public List<Student> LoadStudents()
        {
            return excelManager.LoadStudents();
        }

        // AddStudent applies business rules before adding and saving a student.
        public void AddStudent(List<Student> students, Student student)
        {
            ValidateStudent(students, student, checkUniqueId: true);
            students.Add(student);
            excelManager.SaveStudents(students);
        }

        // UpdateStudent keeps the existing student's grades while changing the
        // editable student information.
        public void UpdateStudent(List<Student> students, Student updatedStudent)
        {
            ValidateStudent(students, updatedStudent, checkUniqueId: false);

            var existingStudent = students.FirstOrDefault(item =>
                item.StudentId.Equals(updatedStudent.StudentId, StringComparison.OrdinalIgnoreCase));

            if (existingStudent == null)
            {
                throw new InvalidOperationException("The selected student no longer exists.");
            }

            existingStudent.FullName = updatedStudent.FullName;
            existingStudent.Major = updatedStudent.Major;
            existingStudent.Email = updatedStudent.Email;
            excelManager.SaveStudents(students);
        }

        // DeleteStudent removes the whole student object. Because the grades are
        // inside that object, they are also left out when the list is saved.
        public void DeleteStudent(List<Student> students, Student student)
        {
            if (!students.Remove(student))
            {
                throw new InvalidOperationException("The selected student no longer exists.");
            }

            excelManager.SaveStudents(students);
        }

        // CONCEPT: validation and exception
        // Business rules belong here so every future UI could use the same rules.
        // ArgumentException communicates invalid input back to the presentation layer.
        private static void ValidateStudent(List<Student> students, Student student, bool checkUniqueId)
        {
            if (string.IsNullOrWhiteSpace(student.StudentId))
            {
                throw new ArgumentException("Student ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(student.FullName))
            {
                throw new ArgumentException("Full Name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(student.Major))
            {
                throw new ArgumentException("Major cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(student.Email))
            {
                throw new ArgumentException("Email cannot be empty.");
            }

            if (checkUniqueId && students.Any(item =>
                    item.StudentId.Equals(student.StudentId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Student ID must be unique.");
            }
        }
    }
}
