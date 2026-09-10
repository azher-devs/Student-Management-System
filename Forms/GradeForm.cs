using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManagementSystem
{
    // GradeForm is a Windows Forms child window used to add or edit one Grade.
    public class GradeForm : Form
    {
        // TextBox lets the user enter course text.
        private readonly TextBox courseNameTextBox = new TextBox();

        // NumericUpDown is a Windows Forms control for numeric input.
        // It is useful here because it can enforce the 0-to-100 range.
        private readonly NumericUpDown gradeInput = new NumericUpDown();

        // The ? means the form may return a Grade object or null when cancelled.
        // private set means only this form assigns the result.
        public Grade? Grade { get; private set; }

        // The optional Grade parameter is null when adding and contains an object
        // when editing. This is one form reused for two related tasks.
        public GradeForm(Grade? grade = null)
        {
            // The conditional operator chooses the window title based on the mode.
            Text = grade == null ? "Add Grade" : "Edit Grade";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(350, 155);

            // A nullable parameter must be checked before reading its properties.
            if (grade != null)
            {
                courseNameTextBox.Text = grade.CourseName;
                gradeInput.Value = (decimal)grade.GradeValue;
            }

            // These properties restrict valid numeric input to the requested range.
            gradeInput.Minimum = 0;
            gradeInput.Maximum = 100;
            gradeInput.DecimalPlaces = 0;
            gradeInput.Dock = DockStyle.Fill;
            BuildLayout();
        }

        private void BuildLayout()
        {
            // TableLayoutPanel arranges labels, input controls, and buttons in rows.
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            layout.Controls.Add(new Label { Text = "Course Name", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0) }, 0, 0);
            courseNameTextBox.Dock = DockStyle.Fill;
            layout.Controls.Add(courseNameTextBox, 1, 0);
            layout.Controls.Add(new Label { Text = "Grade", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0) }, 0, 1);
            layout.Controls.Add(gradeInput, 1, 1);

            // FlowLayoutPanel arranges the Save and Cancel buttons.
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var saveButton = new Button { Text = "Save", Width = 80 };

            // Subscribe the event handler to the Button's Click event.
            saveButton.Click += SaveButton_Click;
            var cancelButton = new Button { Text = "Cancel", Width = 80, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(saveButton);
            layout.Controls.Add(buttons, 1, 2);

            Controls.Add(layout);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        // Event handler: Windows Forms calls this method when Save is clicked.
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // This local variable contains the cleaned user input.
            var courseName = courseNameTextBox.Text.Trim();

            // Validation uses if. return stops the method when the input is invalid.
            if (string.IsNullOrWhiteSpace(courseName))
            {
                MessageBox.Show("Course Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // CONCEPT: casting
            // NumericUpDown.Value is decimal. The explicit (double) cast converts
            // it to the double type used by GradeValue.
            Grade = new Grade { CourseName = courseName, GradeValue = (double)gradeInput.Value };

            // Tell the parent form that this dialog completed successfully.
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
