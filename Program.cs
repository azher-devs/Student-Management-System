// CONCEPT: using directive
// A using directive lets us refer to types without writing their full namespace.
using System;
using System.Windows.Forms;

// CONCEPT: namespace
// A namespace groups related types and helps prevent name conflicts.
namespace StudentManagementSystem
{
    // CONCEPT: static class
    // No Program object is needed. The operating system calls Main directly.
    internal static class Program
    {
        // CONCEPT: [STAThread] attribute
        // Windows Forms uses a single-threaded apartment for its user-interface controls.
        [STAThread]
        // CONCEPT: method
        // Main is the first method that runs when the application starts.
        private static void Main()
        {
            // Prepare common Windows Forms settings such as visual styles and DPI support.
            ApplicationConfiguration.Initialize();

            // CONCEPT: object creation and argument
            // new MainForm() creates an object of the MainForm class.
            // That object is passed as an argument to Application.Run.
            Application.Run(new MainForm());
        }
    }
}
