// CONCEPT: using directive
// These namespaces provide file-system operations, collections, LINQ, and
// the ClosedXML types used to work with .xlsx files.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// CONCEPT: NuGet package
// ClosedXML is a library installed through NuGet. It gives C# convenient
// classes for creating and editing Excel workbooks without writing Excel XML.
using ClosedXML.Excel;

// CONCEPT: namespace
// A namespace groups related types and helps prevent name conflicts.
namespace StudentManagementSystem
{
    // CONCEPT: data-access layer
    // ExcelManager is responsible for reading and writing application data.
    // It is the bottom layer of the 3-tier design. Forms and services should
    // not know how Excel cells are opened or saved.
    public class ExcelManager
    {
        // CONCEPT: property
        // FilePath is read-only outside this class. Other code can read the path,
        // but only the constructor decides where the workbook is stored.
        public string FilePath { get; }

        // CONCEPT: constructor
        // A constructor runs when an object is created with new ExcelManager().
        public ExcelManager()
        {
            // Path.Combine safely joins folders for the current operating system.
            // AppContext.BaseDirectory is the folder where the application runs.
            FilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Students.xlsx");
        }

        // CONCEPT: method
        // This method creates the workbook only when it is missing.
        public void EnsureFileExists()
        {
            // CONCEPT: if statement
            // if runs its block only when the condition is true.
            if (File.Exists(FilePath))
            {
                // return exits the method early because there is nothing to create.
                return;
            }

            // Directory.CreateDirectory also works if the directory already exists.
            // The ! is the null-forgiving operator: we know this path has a directory.
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            // CONCEPT: using statement and IDisposable
            // XLWorkbook represents an Excel workbook (.xlsx file).
            // using releases workbook resources when this method ends.
            using var workbook = new XLWorkbook();
            CreateStudentsSheet(workbook);
            CreateGradesSheet(workbook);
            workbook.SaveAs(FilePath);
        }

        // This method returns a generic collection containing Student objects.
        // The forms use the returned list as the application's in-memory data.
        public List<Student> LoadStudents()
        {
            EnsureFileExists();

            // CONCEPT: object initialization
            // This creates an empty List<Student> object.
            var students = new List<Student>();

            // Opening a file also uses using so ClosedXML can release its resources.
            using var workbook = new XLWorkbook(FilePath);

            // CONCEPT: interface
            // IXLWorksheet is an interface type supplied by ClosedXML. An interface
            // describes operations an object supports; here it represents a worksheet.
            // CONCEPT: worksheet
            // A worksheet is one sheet inside an Excel workbook.
            var studentSheet = workbook.Worksheet("Students");

            // CONCEPT: foreach
            // foreach visits each item in a collection one at a time.
            // Skip(1) is LINQ that ignores the first row because it contains headers.
            foreach (var row in studentSheet.RowsUsed().Skip(1))
            {
                // Cell(1) means column 1. Empty rows should not become students.
                if (row.Cell(1).IsEmpty())
                {
                    continue;
                }

                // CONCEPT: object initialization
                // This creates a Student object and sets several properties at once.
                students.Add(new Student
                {
                    StudentId = row.Cell(1).GetString(),
                    FullName = row.Cell(2).GetString(),
                    Major = row.Cell(3).GetString(),
                    Email = row.Cell(4).GetString()
                });
            }

            // TryGetWorksheet safely checks whether the Grades sheet exists.
            // The out variable receives the worksheet when the lookup succeeds.
            if (workbook.Worksheets.TryGetWorksheet("Grades", out var gradeSheet))
            {
                foreach (var row in gradeSheet.RowsUsed().Skip(1))
                {
                    var studentId = row.Cell(1).GetString();
                    if (string.IsNullOrWhiteSpace(studentId))
                    {
                        continue;
                    }

                    // CONCEPT: lambda expression and searching
                    // Find checks the list and returns the first matching Student,
                    // or null when no student has this ID.
                    var student = students.Find(item => item.StudentId == studentId);
                    if (student != null)
                    {
                        // Add puts the new Grade object into the student's List<Grade>.
                        student.Grades.Add(new Grade
                        {
                            StudentId = studentId,
                            CourseName = row.Cell(2).GetString(),
                            GradeValue = row.Cell(3).GetDouble()
                        });
                    }
                }
            }

            return students;
        }

        // CONCEPT: parameter
        // students is a parameter: it is the list supplied by the calling form.
        // This method writes that list to Excel.
        public void SaveStudents(List<Student> students)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            // A new workbook is created and filled with the current in-memory data.
            using var workbook = new XLWorkbook();
            var studentSheet = CreateStudentsSheet(workbook);
            var gradeSheet = CreateGradesSheet(workbook);
            var studentRow = 2;
            var gradeRow = 2;

            // Write one row for every Student object.
            foreach (var student in students)
            {
                // Cell(row, column) represents one cell in an Excel worksheet.
                studentSheet.Cell(studentRow, 1).Value = student.StudentId;
                studentSheet.Cell(studentRow, 2).Value = student.FullName;
                studentSheet.Cell(studentRow, 3).Value = student.Major;
                studentSheet.Cell(studentRow, 4).Value = student.Email;
                studentRow++;

                // A student can have many Grade objects, so this nested foreach
                // writes each student's grades to the Grades worksheet.
                foreach (var grade in student.Grades)
                {
                    gradeSheet.Cell(gradeRow, 1).Value = student.StudentId;
                    gradeSheet.Cell(gradeRow, 2).Value = grade.CourseName;
                    gradeSheet.Cell(gradeRow, 3).Value = grade.GradeValue;
                    gradeRow++;
                }
            }

            // Columns() returns a range of columns. AdjustToContents makes their
            // widths fit the values, which makes the workbook easier to read.
            studentSheet.Columns().AdjustToContents();
            gradeSheet.Columns().AdjustToContents();
            workbook.SaveAs(FilePath);
        }

        // CONCEPT: static method
        // These helper methods do not use an ExcelManager object field, so they
        // can be static. They receive the workbook as a parameter.
        private static IXLWorksheet CreateStudentsSheet(XLWorkbook workbook)
        {
            // Add returns an IXLWorksheet representing a new Excel worksheet.
            var sheet = workbook.Worksheets.Add("Students");

            // The first row is the header row. Later rows hold student data.
            sheet.Cell(1, 1).Value = "StudentId";
            sheet.Cell(1, 2).Value = "FullName";
            sheet.Cell(1, 3).Value = "Major";
            sheet.Cell(1, 4).Value = "Email";
            sheet.Row(1).Style.Font.Bold = true;
            return sheet;
        }

        private static IXLWorksheet CreateGradesSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Grades");
            sheet.Cell(1, 1).Value = "StudentId";
            sheet.Cell(1, 2).Value = "CourseName";
            sheet.Cell(1, 3).Value = "Grade";
            sheet.Row(1).Style.Font.Bold = true;
            return sheet;
        }
    }
}
