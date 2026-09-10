// These namespaces provide events, collections, drawing types, LINQ, and
// Windows Forms controls.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudentManagementSystem
{
    // CONCEPT: presentation layer
    // MainForm is part of the presentation layer. It displays controls and
    // responds to user actions, but delegates business rules to services.
    // CONCEPT: inheritance
    // MainForm : Form means MainForm inherits the behavior of the Windows Forms
    // Form class. A Form is a window that can contain controls.
    // public allows Program and other classes to create this form.
    public class MainForm : Form
    {
        // CONCEPT: field
        // A field is a variable that belongs to an object and keeps data while
        // the form is open. readonly prevents the field reference being replaced.
        // private means these fields can only be used inside MainForm.
        private readonly ExcelManager excelManager = new ExcelManager();
        private readonly StudentService studentService;
        private readonly GradeService gradeService;
        private readonly List<Student> students;

        // These are Windows Forms controls. DataGridView displays table data,
        // TextBox accepts text, and Label displays text to the user.
        private readonly DataGridView studentGrid = new DataGridView();
        private readonly TextBox searchTextBox = new TextBox();
        private readonly Label totalLabel = new Label();

        // CONCEPT: constructor and object initialization
        // The constructor runs when Program creates new MainForm(). It sets up
        // the window, loads data, and builds the controls.
        public MainForm()
        {
            Text = "Student Management System (Azher)";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(850, 500);
            Size = new Size(980, 620);

            // CONCEPT: 3-tier architecture
            // MainForm is the presentation layer. StudentService and GradeService
            // are the business layer. ExcelManager is the data-access layer.
            // The form talks to services instead of directly changing Excel data.
            studentService = new StudentService(excelManager);
            gradeService = new GradeService(excelManager);

            // CONCEPT: try/catch and Exception
            // Code inside try may fail, for example if the workbook is locked
            // or damaged. catch handles the Exception instead of crashing the UI.
            try
            {
                students = studentService.LoadStudents();
            }
            catch (Exception ex)
            {
                // ex is the Exception object containing error information.
                students = new List<Student>();
                MessageBox.Show("The Excel file could not be loaded.\n\n" + ex.Message,
                    "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            BuildLayout();
            RefreshStudentGrid();
        }

        private void BuildLayout()
        {
            // CONCEPT: object initializer
            // An object initializer creates a control and sets several properties
            // in one readable block.
            var title = new Label
            {
                Text = "Student Management System (Azher Kindi)",
                Dock = DockStyle.Top,
                Height = 48,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Padding = new Padding(12, 8, 0, 0)
            };

            // FlowLayoutPanel is a Windows Forms control that arranges child
            // controls from left to right, wrapping when necessary.
            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5),
                WrapContents = false
            };
            // CONCEPT: event handler and delegate
            // CreateButton receives AddStudent as a method reference. The method
            // will run later when that button's Click event occurs.
            actionPanel.Controls.Add(CreateButton("Add Student", AddStudent));
            actionPanel.Controls.Add(CreateButton("Edit Student", EditStudent));
            actionPanel.Controls.Add(CreateButton("Delete Student", DeleteStudent));
            actionPanel.Controls.Add(CreateButton("Grades", OpenGrades));

            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(10, 4, 10, 4),
                WrapContents = false
            };
            searchPanel.Controls.Add(new Label { Text = "Search:", AutoSize = true, Margin = new Padding(0, 6, 6, 0) });
            searchTextBox.Width = 300;
            // CONCEPT: event subscription
            // += subscribes a method to an event. When the TextBox raises KeyDown,
            // SearchTextBox_KeyDown is called.
            searchTextBox.KeyDown += SearchTextBox_KeyDown;
            searchPanel.Controls.Add(searchTextBox);

            // CONCEPT: lambda expression
            // (_, _) => RefreshStudentGrid() is a short event-handler method.
            // The underscores mean we do not need the sender and event arguments.
            searchPanel.Controls.Add(CreateButton("Search", (_, _) => RefreshStudentGrid(), 85));

            // DataGridView is a Windows Forms control for displaying rows and
            // columns of data. We use it to display Student objects.
            studentGrid.Dock = DockStyle.Fill;
            studentGrid.AllowUserToAddRows = false;
            studentGrid.AllowUserToDeleteRows = false;
            studentGrid.AutoGenerateColumns = false;
            studentGrid.ReadOnly = true;
            studentGrid.MultiSelect = false;
            studentGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            studentGrid.RowHeadersVisible = false;
            studentGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // DataGridViewTextBoxColumn displays one text property in a column.
            // DataPropertyName connects the column to a Student property.
            studentGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Student ID", DataPropertyName = "StudentId", FillWeight = 18 });
            studentGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", DataPropertyName = "FullName", FillWeight = 27 });
            studentGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Major", DataPropertyName = "Major", FillWeight = 22 });
            studentGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", FillWeight = 28 });
            studentGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Average", DataPropertyName = "Average", FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00" } });

            totalLabel.Text = "Total Students: 0";
            totalLabel.Dock = DockStyle.Bottom;
            totalLabel.Height = 35;
            totalLabel.Padding = new Padding(12, 8, 0, 0);

            Controls.Add(studentGrid);
            Controls.Add(totalLabel);
            Controls.Add(searchPanel);
            Controls.Add(actionPanel);
            Controls.Add(title);
        }

        // CONCEPT: method parameter with default value
        // width is optional; when the caller omits it, 120 is used.
        private static Button CreateButton(string text, EventHandler click, int width = 120)
        {
            // Button is a Windows Forms control that the user can click.
            var button = new Button { Text = text, Width = width, Height = 30, Margin = new Padding(3) };

            // CONCEPT: delegate and event subscription
            // EventHandler is a delegate type used by many Windows Forms events.
            // += connects the supplied method to the Button's Click event.
            button.Click += click;
            return button;
        }

        private void RefreshStudentGrid()
        {
            // Trim removes extra spaces from the beginning and end of the search.
            var search = searchTextBox.Text.Trim();

            // CONCEPT: conditional operator and LINQ
            // If search is empty, use the complete list. Otherwise Where filters
            // the list. ToList creates a new List<Student> for the grid.
            var visibleStudents = string.IsNullOrEmpty(search)
                ? students
                : students.Where(student =>
                    student.StudentId.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    student.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    student.Major.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            // DataSource tells the DataGridView which collection supplies its rows.
            // Clearing it first makes the grid reload the current values.
            studentGrid.DataSource = null;
            studentGrid.DataSource = visibleStudents;
            totalLabel.Text = $"Total Students: {students.Count}";
        }

        // CONCEPT: nullable reference type
        // Student? means this method may return a Student object or null when no
        // row is selected. The caller must check for null before using it.
        private Student? GetSelectedStudent()
        {
            if (studentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a student first.", "Student Management System", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            // ?. is the null-conditional operator. It avoids calling ToString if
            // the cell value is null.
            var id = studentGrid.SelectedRows[0].Cells[0].Value?.ToString();

            // FirstOrDefault is a LINQ search. It returns the first matching object
            // or null if no object matches the condition.
            return students.FirstOrDefault(student => student.StudentId == id);
        }

        // CONCEPT: event handler
        // This method runs when the Add Student button raises its Click event.
        // object? sender is the control that raised the event; EventArgs e contains
        // event information. They are not needed here, so their values are unused.
        private void AddStudent(object? sender, EventArgs e)
        {
            // using var disposes the child form after the method finishes.
            using var form = new StudentForm();
            if (form.ShowDialog(this) != DialogResult.OK || form.Student == null)
            {
                return;
            }

            // The service owns the unique-ID business rule. The form only
            // collects input and displays any error returned by the service.
            try
            {
                studentService.AddStudent(students, form.Student);
                RefreshStudentGrid();
            }
            catch (Exception ex)
            {
                ShowServiceError(ex);
            }
        }

        // Another event handler, this time for editing the selected student.
        private void EditStudent(object? sender, EventArgs e)
        {
            var selected = GetSelectedStudent();
            if (selected == null) return;

            using var form = new StudentForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK && form.Student != null)
            {
                try
                {
                    studentService.UpdateStudent(students, form.Student);
                    RefreshStudentGrid();
                }
                catch (Exception ex)
                {
                    ShowServiceError(ex);
                }
            }
        }

        // This event handler removes the selected student after confirmation.
        private void DeleteStudent(object? sender, EventArgs e)
        {
            var selected = GetSelectedStudent();
            if (selected == null) return;

            // MessageBox is a simple Windows Forms dialog for showing a message
            // and receiving a DialogResult such as Yes or No.
            var answer = MessageBox.Show("Are you sure you want to delete this student?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
            {
                try
                {
                    studentService.DeleteStudent(students, selected);
                    RefreshStudentGrid();
                }
                catch (Exception ex)
                {
                    ShowServiceError(ex);
                }
            }
        }

        // This event handler opens another Form for the selected student's grades.
        private void OpenGrades(object? sender, EventArgs e)
        {
            var selected = GetSelectedStudent();
            if (selected == null) return;

            using var form = new GradesForm(selected, students, gradeService);
            form.ShowDialog(this);
            RefreshStudentGrid();
        }

        // The service performs the operation and may throw an exception when a
        // business rule or data-access operation fails. This presentation-layer
        // helper converts that exception into a user-friendly MessageBox.
        private static void ShowServiceError(Exception ex)
        {
            var title = ex is ArgumentException ? "Validation Error" : "Save Error";
            MessageBox.Show(ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // CONCEPT: event handler and enum
        // KeyEventArgs describes a keyboard event. Keys.Enter is an enum value
        // representing the Enter key.
        private void SearchTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RefreshStudentGrid();
                e.SuppressKeyPress = true;
            }
        }
    }
}
