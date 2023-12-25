using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoReminder
{
    /// <summary>
    /// Allows user to create, save, and load tasks to be done.
    /// </summary>
    public partial class MainForm : Form
    {
        private TaskManager taskManager;
        // Set program directory as save location for saving and opening files 
        private string saveLocation = Application.StartupPath;
        private bool hasUnsavedChanges; // Current file has unsaved changes or not
        private string token = "ToDoReminder.txt";
        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            Text += "Martin Engström";
            // Add all PriorityType priorities to comboBox
            foreach (string priority in Enum.GetNames(typeof(PriorityType)))
            {
                cmbBoxPriority.Items.Add(priority.Replace("_", " "));
            }
            // Set the CustomFormat string of DateTimePicker.
            dateTimePicker.CustomFormat = "dd-MM-yyyy      HH:mm";
            dateTimePicker.Format = DateTimePickerFormat.Custom;
            // Initialize GUI
            InitializeGUI();

        }

        /// <summary>
        /// Initializes the graphical user interface elements and sets up the initial state.
        /// </summary>
        private void InitializeGUI()
        {
            // Create new list of tasks
            taskManager = new TaskManager();
            // Select first priority item of comboBox
            cmbBoxPriority.SelectedIndex = 0;
            // Set minimum date to now
            dateTimePicker.MinDate = DateTime.Now;
            // Set default save file path and open file path
            saveFileDialog1.InitialDirectory = saveLocation;
            openFileDialog1.InitialDirectory = saveLocation;
            UpdateDataGridView();
            // Mark that the current form has no unsaved changes
            hasUnsavedChanges = false;
        }

        #region Helper Functions
        /// <summary>
        /// Displays an error message using a MessageBox.
        /// </summary>
        /// <param name="msg">The error message to display.</param>
        private void DisplayError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Updates the DataGridView with the current list of tasks.
        /// </summary>
        private void UpdateDataGridView()
        {
            dataGridView1.Rows.Clear();
            List<Task> tasks = taskManager.Tasks;

            foreach (Task task in tasks)
            {
                // Add a new row to the DataGridView
                int rowIndex = dataGridView1.Rows.Add();
                DataGridViewRow row = dataGridView1.Rows[rowIndex];

                // Populate the DataGridView cells
                row.Cells["DateColumn"].Value = $"{task.Deadline.Day}-{task.Deadline.Month}-{task.Deadline.Year}";
                row.Cells["TimeColumn"].Value = task.Deadline.ToString("HH:mm");
                row.Cells["PriorityColumn"].Value = task.Priority.ToString().Replace("_", " ");
                row.Cells["DescriptionColumn"].Value = task.Description;

                // Mark task as late with red text if the deadline has passed
                if (task.Deadline < DateTime.Now)
                {
                    row.DefaultCellStyle.ForeColor = Color.Red;
                }
            }

            // Mark that the current form has unsaved changes
            hasUnsavedChanges = true;
        }

        /// <summary>
        /// Updates the form with the details of the provided task.
        /// </summary>
        /// <param name="task">The task whose details are used to update the form.</param>
        private void UpdateFormWithTask(Task task)
        {
            dateTimePicker.Value = task.Deadline;
            cmbBoxPriority.SelectedIndex = (int)task.Priority;
            txtBoxDescription.Text = task.Description;
        }

        /// <summary>
        /// Clear form details to default values.
        /// </summary>
        private void ClearForm()
        {
            dateTimePicker.Value = DateTime.Now;
            cmbBoxPriority.SelectedIndex = 0;
            txtBoxDescription.Text = string.Empty;
        }



        /// <summary>
        /// Gets the index of the selected task in the DataGridView.
        /// </summary>
        /// <returns>The index of the selected task; otherwise, -1.</returns>
        private int GetSelectedTaskIndex()
        {
            // Check if any row is selected
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the index of the selected row
                int selectedRowIndex = dataGridView1.SelectedRows[0].Index;

                // Ensure that the row index is valid
                if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView1.Rows.Count)
                {
                    return selectedRowIndex;
                }
            }
            return -1;
        }



        /// <summary>
        /// Reads the date and time from the DateTimePicker control.
        /// </summary>
        /// <param name="deadline">DateTime value for deadline of task.</param>
        /// <returns>True if the deadline is later than the current time; otherwise, false.</returns>
        private bool ReadDateTime(out DateTime deadline)
        {
            deadline = dateTimePicker.Value;
            // Verify that the deadline is later than the current time
            if (DateTime.Now < deadline)
                return true;
            // Invalid input
            DisplayError("The date and time of the task precedes the current date and time.");
            return false;
        }

        /// <summary>
        /// Reads the priority from the ComboBox control.
        /// </summary>
        /// <param name="priority">Priority value of task.</param>
        /// <returns>True if the selected priority is of PriorityType; otherwise, false.</returns>
        private bool ReadPriority(out PriorityType priority)
        {
            string prioritySelectionText = cmbBoxPriority.Text.Replace(" ", "_");
            // Verify that the selected priority is of type enum PriorityType
            if (PriorityType.TryParse(prioritySelectionText, out priority))
                return true;
            // Invalid input
            DisplayError("Invalid priority value.");
            return false;
        }

        /// <summary>
        /// Reads the description of task from description TextBox.
        /// </summary>
        /// <param name="description">Description of task.</param>
        /// <returns>True if the description is not empty or null; otherwise, false.</returns>
        private bool ReadDescription(out string description)
        {
            description = txtBoxDescription.Text;
            // Verify that the description is not empty or null
            if (!string.IsNullOrEmpty(description))
                return true;
            // Invalid input
            DisplayError("The description is empty; a task needs a description.");
            return false;
        }
        #endregion

        #region File Operations
        /// <summary>
        /// Shows the SaveFileDialog and attempts to save the list of tasks to a file.
        /// </summary>
        /// <returns>
        /// DialogResult.OK if the data is saved successfully, DialogResult.Cancel if the user cancels the operation.
        /// </returns>
        private DialogResult SaveToFile()
        {
            // Show SaveFileDialog
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Proceed to save file
                string filePath = saveFileDialog1.FileName;
                // Update save location 
                saveLocation = Path.GetDirectoryName(filePath);
                saveFileDialog1.InitialDirectory = saveLocation;
                // Continue to try to write to file
                try
                {
                    // Create a StreamWriter to write to the file
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        // Write the token to the file
                        sw.WriteLine(token);
                        // Write each task's details to the file (Deadline, priority, description)
                        foreach (Task task in taskManager.Tasks)
                        {
                            sw.WriteLine($"{task.Deadline},{task.Priority},{task.Description}");
                        }
                    }
                    MessageBox.Show("Data saved successfully.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    hasUnsavedChanges = false;
                    return DialogResult.OK;
                }
                catch (Exception ex)
                {
                    // An error occured
                    DisplayError($"Error saving data: {ex.Message}");
                    return DialogResult.Cancel;
                }
            }
            else
            {
                // User clicked Cancel in SaveFileDialog
                return DialogResult.Cancel;
            }
        }


        /// <summary>
        /// Shows the OpenFileDialog and attempts to read list of tasks from file.
        /// If the read was successful, the file is opened; otherwise, display that file could not be read.
        /// </summary>
        /// <returns>DialogResult.OK if successful; otherwise, DialogResult.Cancel</returns>
        private DialogResult OpenFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                try
                {
                    // Initialize new taskmanager
                    TaskManager newTaskManager = new TaskManager();
                    // Create a StreamReader to read from the file
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        // Read token to make sure the file was created by ToDoReminder
                        string line = sr.ReadLine();
                        if (!line.Equals(token)) throw new FileLoadException("The file is not readable by this program");
                        // Continue to read file
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] variables = line.Split(',');
                            DateTime deadline;
                            PriorityType priority;

                            // Verify that 3 variables exist in the read line
                            if (variables.Length != 3)
                            {
                                throw new FileLoadException($"File is corrupt - Incorrect number of variables: {variables.Length}; Expected 3.");
                            }
                            // Try to parse deadline
                            if (!DateTime.TryParse(variables[0], out deadline))
                            {
                                throw new FileLoadException("File is corrupt - DateTime can not be parsed.");
                            }
                            // Try to parse priority
                            if (!PriorityType.TryParse(variables[1], out priority))
                            {
                                throw new FileLoadException("File is corrupt - Priority can not be parsed.");
                            }
                            // Set description
                            string description = variables[2];
                            // Add task to new taskManager
                            newTaskManager.AddTask(deadline, priority, description);
                        }
                    }
                    // Replace old taskManager
                    taskManager = newTaskManager;
                    return DialogResult.OK;
                }
                catch (Exception ex)
                {
                    DisplayError($"The file could not be read: {ex.Message}");
                }
            }
            return DialogResult.Cancel;
        }

        // Returns true if process should cancel
        /// <summary>
        /// Prompts user to save current tasks before proceeding with execution of the calling function.
        /// </summary>
        /// <param name="prompt">Caption of MessageBox</param>
        /// <returns>True if operation was successful; otherwise if canceled, false.</returns>
        private bool PromptUserToSaveDataToFile(string prompt)
        {
            DialogResult yesNoCancel = MessageBox.Show("Do you want to save first?", prompt, MessageBoxButtons.YesNoCancel);
            if (yesNoCancel == DialogResult.Yes)
            {
                // If save was canceled, propagate cancel
                if (SaveToFile() == DialogResult.Cancel) return false;
                // File was saved, continue with operation
                return true;
            }
            else if (yesNoCancel == DialogResult.No)
            {
                // File was not saved, continue with operation
                return true;
            }
            // Operation was canceled 
            return false;
        }
        #endregion

        #region Button events
        /// <summary>
        /// Handles the Click event of the "Delete" button, 
        /// removing the selected task from the taskManager, then updates listView.
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Start deleting task
            int selectedIndex = GetSelectedTaskIndex();
            if (selectedIndex >= 0)
            {
                DialogResult result = MessageBox.Show("Do you want to delete the task?", "Delete Task", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    // Proceed to remove task
                    taskManager.RemoveTask(selectedIndex);
                    // Reset listview selection
                    UpdateDataGridView();
                }
                
            }
            else if (selectedIndex >= 0)
            {
                // A bug was detected with the selection and deleting of tasks
                DisplayError("Selected task doesn't exist. This shouldn't happen, please report it to IT.");
            }
        }

        /// <summary>
        /// Handles the Click event of the "Edit" button.
        /// Adds selected task to form and deletes task from taskManager and listview.
        /// </summary>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            // Start editing customer
            int selectedIndex = GetSelectedTaskIndex();
            if (selectedIndex >= 0)
            {
                // Get old task details
                Task oldTask = taskManager.GetTask(selectedIndex);
                // Update form with task details
                UpdateFormWithTask(oldTask);
                // Delete old task
                taskManager.RemoveTask(selectedIndex);
                // Reset listview selection
                UpdateDataGridView();
            }
        }

        /// <summary>
        /// Handles the Click event of the "Add" button.
        /// Adds task to taskmanager if form-data is valid; otherwise, display error.
        /// </summary>
        /// <param name="sender"> Ignored</param>
        /// <param name="e"> Ignored</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DateTime deadline;
            PriorityType priority;
            string description;

            // Read data from form; display error if form-data is invalid
            bool deadlineOk = ReadDateTime(out deadline);
            bool priorityOk = ReadPriority(out priority);
            bool descriptionOk = ReadDescription(out description);

            // Verify that all data was read correctly
            if(deadlineOk && priorityOk && descriptionOk)
            {
                // Continue to add task
                taskManager.AddTask(deadline, priority, description);
                ClearForm();
                UpdateDataGridView();
            }
        }
        #endregion

        #region Application Menu events
        /// <summary>
        /// Resets task form; Prompts user to save file if there are unsaved changes.
        /// If user cancels operation, nothing is changed.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void toolStripMenuItemNew_Click(object sender, EventArgs e)
        {
            // Ask to save file and make sure the operation was not canceled
            if (hasUnsavedChanges && !PromptUserToSaveDataToFile("Create New")) return;
            // Reset task form
            InitializeGUI();
            // Mark that there are no unsaved changes
            hasUnsavedChanges = false;
        }

        /// <summary>
        /// Opens new file to import tasks; Prompts user to save file first if there are unsaved changes.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            // Ask to save file and make sure the operation was not canceled
            if (hasUnsavedChanges && !PromptUserToSaveDataToFile("Open File")) return;
            // Check if open file was successful
            if(OpenFile() == DialogResult.OK)
            {
                UpdateDataGridView();
                // Mark that there are no unsaved changes
                hasUnsavedChanges = false;
            }
        }

        /// <summary>
        /// Saves current tasks to file.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void toolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        /// <summary>
        /// Handles the Click event of the "Exit" field in the menu. 
        /// Prompts user to save file if there are unsaved changes before exiting.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            // Ask to save file and make sure the operation was not canceled
            if (hasUnsavedChanges && !PromptUserToSaveDataToFile("Exit Program")) return;
            // Verify that the user really meant to exit
            DialogResult result = MessageBox.Show("Do you really want to exit the program?", "Exit", MessageBoxButtons.OKCancel);
            if(result == DialogResult.OK)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the "About" field in menu. 
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }
        #endregion

        #region Control events
        /// <summary>
        /// Handles tick event of timer. 
        /// Updates labelClock to current time.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void timerTick(object sender, EventArgs e)
        {
            labelClock.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion

    }
}
