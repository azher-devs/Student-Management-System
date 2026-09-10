using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudentManagementSystem
{
    // GradesForm is a Windows Forms window dedicated to one student's grades.
    public class GradesForm : Form
    {
        // These fields keep references to the selected Student, the complete
        // student list, the Excel manager, and the form controls.
        private readonly Student student;
        private readonly List<Student> allStudents;
        private readonly GradeService gradeService;
        private readonly Label averageLabel = new Label();
        private readonly DataGridView gradeGrid = new DataGridView();

        // CONCEPT: constructor parameters
        // The parent form passes the selected student, shared data, and business
        // service into this form. The form does not talk directly to Excel.
        public GradesForm(Student student, List<Student> allStudents, GradeService gradeService)
        {
            this.student = student;
            this.allStudents = allStudents;
            this.gradeService = gradeService;
            Text = "Grades";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(550, 400);
            Size = new Size(650, 460);
            BuildLayout();
            RefreshGrid();
        }

        private void BuildLayout()
        {
            // TableLayoutPanel arranges the student information in rows and columns.
            var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = 72, Padding = new Padding(12), ColumnCount = 2, RowCount = 2 };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.Controls.Add(new Label { Text = "Student ID:", AutoSize = true }, 0, 0);
            header.Controls.Add(new Label { Text = student.StudentId, AutoSize = true }, 1, 0);
            header.Controls.Add(new Label { Text = "Student Name:", AutoSize = true }, 0, 1);
            header.Controls.Add(new Label { Text = student.FullName, AutoSize = true }, 1, 1);

            // Label is a Windows Forms control used to display text such as the average.
            averageLabel.Dock = DockStyle.Top;
            averageLabel.Height = 35;
            averageLabel.Padding = new Padding(12, 5, 0, 0);
            averageLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // FlowLayoutPanel arranges the grade action buttons in one row.
            var actionPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(10, 5, 10, 5), WrapContents = false };
            actionPanel.Controls.Add(CreateButton("Add Grade", AddGrade));
            actionPanel.Controls.Add(CreateButton("Edit Grade", EditGrade));
            actionPanel.Controls.Add(CreateButton("Delete Grade", DeleteGrade));

            // DataGridView displays the student's grades in rows and columns.
            gradeGrid.Dock = DockStyle.Fill;
            gradeGrid.AllowUserToAddRows = false;
            gradeGrid.ReadOnly = true;
            gradeGrid.MultiSelect = false;
            gradeGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gradeGrid.RowHeadersVisible = false;
            gradeGrid.AutoGenerateColumns = false;
            gradeGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // DataPropertyName connects each column to a Grade property.
            gradeGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Course Name", DataPropertyName = "CourseName" });
            gradeGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Grade", DataPropertyName = "GradeValue", DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });

            Controls.Add(gradeGrid);
            Controls.Add(actionPanel);
            Controls.Add(averageLabel);
            Controls.Add(header);
        }

        // EventHandler is a delegate type that can point to a method receiving
        // (object sender, EventArgs e). Windows Forms uses this pattern for events.
        private static Button CreateButton(string text, EventHandler click)
        {
            var button = new Button { Text = text, Width = 105, Height = 30, Margin = new Padding(3) };
            // Subscribe the supplied event-handler delegate to the Click event.
            button.Click += click;
            return button;
        }

        private void RefreshGrid()
        {
            // String interpolation inserts the calculated value into the text.
            averageLabel.Text = $"Average: {student.Average:0.00}";

            // Binding means the DataGridView gets its rows from student.Grades.
            gradeGrid.DataSource = null;
            gradeGrid.DataSource = student.Grades;
        }

        // Nullable Grade? means no grade may be returned when no row is selected.
        private Grade? GetSelectedGrade()
        {
            if (gradeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a grade first.", "Grades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            // DataBoundItem is stored as object. The 'as Grade' cast safely tries
            // to treat it as a Grade and returns null if it is not a Grade.
            return gradeGrid.SelectedRows[0].DataBoundItem as Grade;
        }

        // Event handler for the Add Grade button.
        private void AddGrade(object? sender, EventArgs e)
        {
            using var form = new GradeForm();
            if (form.ShowDialog(this) == DialogResult.OK && form.Grade != null)
            {
                // The form returns a Grade object. We add the relationship to the
                // selected student before putting it in the student's grade list.
                try
                {
                    gradeService.AddGrade(allStudents, student, form.Grade);
                    RefreshGrid();
                }
                catch (Exception ex)
                {
                    ShowServiceError(ex);
                }
            }
        }

        // Event handler for editing the selected grade.
        private void EditGrade(object? sender, EventArgs e)
        {
            var selected = GetSelectedGrade();
            if (selected == null) return;

            using var form = new GradeForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK && form.Grade != null)
            {
                try
                {
                    gradeService.UpdateGrade(allStudents, student, selected, form.Grade);
                    RefreshGrid();
                }
                catch (Exception ex)
                {
                    ShowServiceError(ex);
                }
            }
        }

        // Event handler for deleting the selected grade.
        private void DeleteGrade(object? sender, EventArgs e)
        {
            var selected = GetSelectedGrade();
            if (selected == null) return;

            // MessageBox returns a DialogResult enum value such as Yes or No.
            var answer = MessageBox.Show("Are you sure you want to delete this grade?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
            {
                try
                {
                    gradeService.DeleteGrade(allStudents, student, selected);
                    RefreshGrid();
                }
                catch (Exception ex)
                {
                    ShowServiceError(ex);
                }
            }
        }

        // The service handles business rules and persistence. The form only
        // displays the error and keeps the user interface alive.
        private static void ShowServiceError(Exception ex)
        {
            var title = ex is ArgumentException ? "Validation Error" : "Save Error";
            MessageBox.Show(ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
