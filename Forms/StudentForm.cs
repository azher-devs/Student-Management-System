using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManagementSystem
{
    // StudentForm is another Windows Forms Form. It is a child window used
    // for both adding a new student and editing an existing student.
    public class StudentForm : Form
    {
        // CONCEPT: field and readonly
        // These fields store the TextBox controls while the form is open.
        // readonly means the field cannot point to a different TextBox later.
        private readonly TextBox studentIdTextBox = new TextBox();
        private readonly TextBox fullNameTextBox = new TextBox();
        private readonly TextBox majorTextBox = new TextBox();
        private readonly TextBox emailTextBox = new TextBox();
        // bool is a data type with only true or false values.
        private readonly bool isEdit;

        // The ? means this property may contain a Student object or null.
        // private set means other classes can read it, but only this form can set it.
        public Student? Student { get; private set; }

        // CONCEPT: constructor parameter with a default value
        // The caller may pass an existing Student for editing, or omit the
        // argument to create a new student form.
        public StudentForm(Student? student = null)
        {
            // != means "not equal". This creates a true/false value for the form title.
            isEdit = student != null;
            Text = isEdit ? "Edit Student" : "Add Student";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(380, 250);

            // A nullable value must be checked before using it.
            if (student != null)
            {
                studentIdTextBox.Text = student.StudentId;
                fullNameTextBox.Text = student.FullName;
                majorTextBox.Text = student.Major;
                emailTextBox.Text = student.Email;
                studentIdTextBox.ReadOnly = true;
            }

            BuildLayout();
        }

        private void BuildLayout()
        {
            // TableLayoutPanel is a Windows Forms control that arranges controls
            // in rows and columns, similar to a small table.
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 5
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // for is a loop. It repeats the statement while i is less than 4.
            for (var i = 0; i < 4; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            AddField(layout, 0, "Student ID", studentIdTextBox);
            AddField(layout, 1, "Full Name", fullNameTextBox);
            AddField(layout, 2, "Major", majorTextBox);
            AddField(layout, 3, "Email", emailTextBox);

            // FlowLayoutPanel arranges the Save and Cancel Button controls.
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var saveButton = new Button { Text = "Save", Width = 80, DialogResult = DialogResult.None };

            // Subscribe the SaveButton_Click method to the Button's Click event.
            saveButton.Click += SaveButton_Click;
            var cancelButton = new Button { Text = "Cancel", Width = 80, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(saveButton);
            layout.Controls.Add(buttons, 1, 4);

            Controls.Add(layout);
            // Pressing Enter acts like clicking the AcceptButton.
            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        // This helper method receives a TableLayoutPanel, a row number, a label,
        // and a Control. Control is a base type shared by TextBox and other controls.
        private static void AddField(TableLayoutPanel layout, int row, string labelText, Control input)
        {
            layout.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0) }, 0, row);
            input.Dock = DockStyle.Fill;
            layout.Controls.Add(input, 1, row);
        }

        // CONCEPT: event handler
        // This method runs when the Save button's Click event occurs.
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // Local variables exist only while this method runs.
            // Trim removes accidental spaces from user input.
            var id = studentIdTextBox.Text.Trim();
            var fullName = fullNameTextBox.Text.Trim();
            var major = majorTextBox.Text.Trim();
            var email = emailTextBox.Text.Trim();

            // CONCEPT: validation and if statements
            // Each if checks one rule. return stops the method when the input is invalid.
            if (string.IsNullOrWhiteSpace(id))
            {
                ShowValidation("Student ID cannot be empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowValidation("Full Name cannot be empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(major))
            {
                ShowValidation("Major cannot be empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowValidation("Email cannot be empty.");
                return;
            }

            // CONCEPT: object creation and object initializer
            // This creates one Student object and fills its properties from the form.
            Student = new Student { StudentId = id, FullName = fullName, Major = major, Email = email };

            // DialogResult tells the parent form whether Save or Cancel was chosen.
            DialogResult = DialogResult.OK;
            Close();
        }

        // static means this helper does not need a StudentForm object field.
        private static void ShowValidation(string message) =>
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
