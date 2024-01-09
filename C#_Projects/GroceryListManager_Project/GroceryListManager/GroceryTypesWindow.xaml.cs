using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;

namespace GroceryListManager
{
    /// <summary>
    /// Window for editing the grocery types that can be used for creating grocery lists.
    /// </summary>
    public partial class GroceryTypesWindow : Window
    {
        private GroceryTypeManager groceryTypeManager; // Manager for grocery types

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryTypesWindow"/> class.
        /// </summary>
        public GroceryTypesWindow()
        {
            InitializeComponent();
            this.groceryTypeManager = new GroceryTypeManager();
            // Set the data source for datagrid to the grocery types in the GroceryTypeManager
            dataGrid.ItemsSource = groceryTypeManager.GroceryTypes;
            // Load groceries
            LoadCachedgroceryTypes();
        }

        /// <summary>
        /// Displays an error message using a MessageBox.
        /// </summary>
        /// <param name="msg">The error message to display.</param>
        private void DisplayError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK);
        }

        /// <summary>
        /// Sets the position of GroceryWindow relative to the display.
        /// </summary>
        /// <param name="left">Margin from the left border.</param>
        /// <param name="top">Margin from the top border.</param>
        public void SetWindowPosition(double left, double top)
        {
            this.Left = left;
            this.Top = top;
        }

        /// <summary>
        /// Attempts to load new groceryTypes file as the new source of grocery types.
        /// </summary>
        /// <returns>True if the file loaded properly; otherwise, false.</returns>
        private bool TryLoadgroceryTypes(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Temporarly save current used file path
                string tmpFilePath = GroceryManagerSettings.GroceryTypesFilePath;
                // Update the groceryManagerSettings path variable
                GroceryManagerSettings.GroceryTypesFilePath = filePath;
                try
                {
                    // Try to load groceries from new file
                    groceryTypeManager.LoadCachedGroceryTypes();
                    labelFileLoadNotice.Content = "Grocery types loaded successfully.";
                    return true;
                }
                catch (Exception ex)
                {
                    // Handle improperly formatted file and reset the filePath
                    GroceryManagerSettings.GroceryTypesFilePath = tmpFilePath;
                }
            }
            // The load failed
            return false;
        }

        /// <summary>
        /// Loads grocery types from the cached file path or falls back to the default path if load fails.
        /// </summary>
        private void LoadCachedgroceryTypes()
        {
            // Try to load the cached file
            string filePath = GroceryManagerSettings.GroceryTypesFilePath;
            if (!TryLoadgroceryTypes(filePath))
            {
                // The file could not be loaded
                DisplayError($"The file located at {filePath} couldn't be read, " +
                    "the default groceryTypes.txt file will be opened instead.");
                // Reset the path variable in settings to a known good value
                GroceryManagerSettings.GroceryTypesFilePath = GroceryManagerSettings.Default_groceryTypesFilePath;
                // Load default groceries
                try
                {
                    groceryTypeManager.LoadDefaultGroceryTypes();
                }
                catch (Exception ex)
                {
                    // Loading default groceries failed. This should never happen!
                    DisplayError($"The default groceries could not be loaded. " +
                        $"Please contact IT with the following report: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Attempts to load new groceryTypes file as the new source of groceries repeatedly
        /// while the file fails to load and the user wants to try again.
        /// </summary>
        /// <returns>True if a file loaded properly; otherwise, false.</returns>
        private bool OpenNewgroceryTypes()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Set filter for file extension and default file extension
            openFileDialog.DefaultExt = ".json";
            openFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            openFileDialog.InitialDirectory = Path.GetDirectoryName(GroceryManagerSettings.GroceryTypesFilePath);
            // Display OpenFileDialog
            bool? result = openFileDialog.ShowDialog();
            // Process open file dialog box results
            if (result == true)
            {
                string filePath = openFileDialog.FileName;

                // Attempt to load groceries from file 
                if (TryLoadgroceryTypes(filePath))
                {
                    // Load success
                    return true;
                }
                else
                {
                    // Handle failed loading of file and ask if user wants to try again
                    MessageBoxResult tryAgainResult = MessageBox.Show("The file is not readable by the application.\n" +
                        "Would you like to try a different file?", "Error", MessageBoxButton.YesNo);
                    // Loop the function until the user clicks on No
                    if (tryAgainResult == MessageBoxResult.Yes)
                    {
                        OpenNewgroceryTypes();
                    }
                }
            }
            // The user cancelled the operation
            return false;
        }



        /// <summary>
        /// Checks if there are any invalid grocery types in the datagrid.
        /// </summary>
        /// <returns>True if there are no invalid input; otherwise, false.</returns>
        private bool AreAnyItemsInvalid()
        {
            foreach(var item in dataGrid.Items)
            {
                // Check if there are errors for any items
                if (Validation.GetHasError(dataGrid.ItemContainerGenerator.ContainerFromItem(item)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempt to save the grocery types to a specified file.
        /// </summary>
        /// <param name="filePath">The file path that the grocery types will be saved to.</param>
        /// <returns></returns>
        private bool TrySavegroceryTypes(string filePath)
        {
            try
            {
                // Try to save groceries to file
                groceryTypeManager.SaveGroceryTypes(filePath);
                return true;
            }
            catch (Exception ex)
            {
                // Handle the case where save fails (should never happen)
                DisplayError("The file could not be saved. This should not have happened! " +
                    $"Please contact IT with this report: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Event handler for removing a selected grocery type in the datagrid.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void btnRemoveGrocery_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid.SelectedIndex;
            // Ensure that the selected item exists
            if (selectedIndex >= 0 && selectedIndex < dataGrid.Items.Count)
            {
                // Remove selected grocery
                groceryTypeManager.RemoveGroceryType(selectedIndex);
            }
        }

        /// <summary>
        /// Event handler allowing for saving the current groceryTypeManager to the file it was loaded from.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Verify that all user input is valid
            if (AreAnyItemsInvalid())
            {
                // Handle items are invalid
                DisplayError("One or more inputs are invalid, check the value of any fields with red borders.");
                return;
            }
            if (!TrySavegroceryTypes(GroceryManagerSettings.GroceryTypesFilePath))
            {
                // The save failed
                return;
            }
            // The file was successfully saved
            MessageBox.Show("The data was saved successfully.\nThis window will now close.",
                "Saving Available Groceries", MessageBoxButton.OK);
            // Hide window
            DialogResult = true;
            this.Hide();
        }

        /// <summary>
        /// Even handler to Cancel all changes made to the loaded file.
        /// </summary>
        /// <param name="sender">Ignore</param>
        /// <param name="e"></Ignoreparam>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Ask if the user really wants to cancel.
            MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel?", "Cancel", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // Close window.
                DialogResult = false;
                this.Close();
            }
        }

        /// <summary>
        /// Event handler allowing user to select location and name for a new empty grocery types source file.
        /// User is asked if they want to save their current file before proceeding.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void menuNew_Click(object sender, RoutedEventArgs e)
        {
            // Ask the user if they want to save first
            MessageBoxResult saveFirstResult = MessageBox.Show("Do you want to quick-save before creating New?",
                                                                "Create New File", MessageBoxButton.YesNoCancel);
            if (saveFirstResult == MessageBoxResult.Yes)
            {
                // Try to quick-save
                if (!TrySavegroceryTypes(GroceryManagerSettings.GroceryTypesFilePath))
                {
                    return;
                }
            }
            else if (saveFirstResult != MessageBoxResult.No)
            {
                // Cancel the operation
                return;
            }
            // Create a temporary GroceryTypeManager for the new file
            GroceryTypeManager tmpGM = new GroceryTypeManager();
            // Let the user pick a filename and location
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".json";
            saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(GroceryManagerSettings.GroceryTypesFilePath);
            bool? result = saveFileDialog.ShowDialog();
            // Handle user's choice for the SaveFileDialog
            if (result == true)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    // Save an empty file
                    tmpGM.SaveGroceryTypes(filePath);
                }
                catch (Exception ex)
                {
                    // Handle the case where saving the new file fails (should never happen)
                    DisplayError("The file could not be saved. This should not have happened! " +
                        $"Please contact IT with this report: {ex.Message}");
                    return;
                }

                // Update cached file path
                GroceryManagerSettings.GroceryTypesFilePath = filePath;
                // Clear groceries from groceryTypeManager to reflect the changes
                groceryTypeManager.ClearGroceryTypes();
            }
        }

        /// <summary>
        /// Event handler allowing user to select a new file to load as the new source for grocery types.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            bool result = OpenNewgroceryTypes();
            if (result == true)
            {
                MessageBox.Show("The file has been opened.", "Open File", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Event handler allowing for saving the current groceryTypeManager to a file chosen by the user.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            // Verify that all user input is valid
            if (AreAnyItemsInvalid())
            {
                // Handle input is invalid
                DisplayError("One or more inputs are invalid, check the value of any fields with red borders.");
                return;
            }
            // Let the user pick a filename and location
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".json";
            saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(GroceryManagerSettings.GroceryTypesFilePath);
            bool? result = saveFileDialog.ShowDialog();
            // Handle user's choice for the SaveFileDialog
            if (result == true)
            {
                string filePath = saveFileDialog.FileName;
                if (!TrySavegroceryTypes(filePath))
                {
                    return;
                }
                // Update cached file path
                GroceryManagerSettings.GroceryTypesFilePath = filePath;
                // The file was successfully saved
                MessageBox.Show("The data was saved successfully.\nThis window will now close.", 
                    "Saving Available Groceries", MessageBoxButton.OK);
                // Hide window
                DialogResult = true;
                this.Hide();
            }
        }

        /// <summary>
        /// Event handler for displaying a helpful message that describes the program and
        /// how to use it.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Use this program to keep track of your grocery lists!\n\nHow to use:" +
                "\nEdit the grocery types you can choose from in this window." +
                "\n\nNote: You can add new items by clicking on the empty bottom row. " +
                "After making an edit, finish it by pressing enter.",
                "About Grocery List Manager", MessageBoxButton.OK);
        }

        /// <summary>
        /// Gets a list of groceries from groceryTypeManager.
        /// </summary>
        /// <returns>Grocery types.</returns>
        internal List<GroceryType> GetGroceries()
        {
            List<GroceryType> tmpGroceryList = new List<GroceryType>();
            foreach(GroceryType grocery in groceryTypeManager.GroceryTypes)
            {
                tmpGroceryList.Add(grocery);
            }
            return tmpGroceryList;
        }
    }
}
