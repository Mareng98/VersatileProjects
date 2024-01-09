using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using System.Collections.Generic;

namespace GroceryListManager
{
    /// <summary>
    /// Main Window for creating grocery lists made up of grocery items that are based on grocery types.
    /// </summary>
    public partial class MainWindow : Window
    {
        private GroceryItemManager groceryItemManager; // Manager for grocery items
        private List<GroceryType> groceryTypes; // Current list of grocery types that can be chosen from
        private GroceryTypesWindow groceryWindow; // Window to edit available grocery types

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //groceryTypeManager = new GroceryTypeManager();
            groceryItemManager = new GroceryItemManager();
            groceryWindow = new GroceryTypesWindow();
            groceryTypes = groceryWindow.GetGroceries();
            // Initialize the collection and set it as the ItemsSource for the DataGrid
            dataGrid.ItemsSource = groceryItemManager.GroceryItems;
            // Set update total cost on cell edit
            dataGrid.CellEditEnding += CellEditEnding_Handler;
            // Load grocery items
            LoadCachedGroceryList();
            // Populate available grocery types
            CmbBoxUpdateGroceryTypes();
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
        /// Attempts to load new grocery list file as the new source of grocery items.
        /// </summary>
        /// <returns>True if the file loaded properly; otherwise, false.</returns>
        private bool TryLoadGroceryList(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Temporarly save current used file path
                string tmpFilePath = GroceryManagerSettings.GroceryListFilePath;
                // Update the groceryManagerSettings path variable
                GroceryManagerSettings.GroceryListFilePath = filePath;
                try
                {
                    // Load grocery list
                    groceryItemManager.LoadCachedGroceryList();
                    UpdateTotalCost();
                    // If no exception occurs, the load succeded
                    return true;
                }
                catch (Exception ex)
                {
                    // The file chosen is not properly formatted, reset the filePath
                    GroceryManagerSettings.GroceryListFilePath = tmpFilePath;
                }
            }
            // The load failed
            return false;
        }

        /// <summary>
        /// Loads grocery items from the cached file path or falls back to the default path if load fails.
        /// </summary>
        private void LoadCachedGroceryList()
        {
            // Try to load the cached file
            string filePath = GroceryManagerSettings.GroceryListFilePath;
            if (!TryLoadGroceryList(filePath))
            {
                // The file could not be loaded
                DisplayError($"The file located at {filePath} couldn't be read, " +
                    "the default file will be opened instead.");
                // Reset the path variable in settings to a known good value
                GroceryManagerSettings.GroceryListFilePath = GroceryManagerSettings.Default_GroceryListFilePath;
                // Proceed to load default file
                try
                {
                    // Load default grocery list file
                    groceryItemManager.LoadDefaultGroceryList();
                    UpdateTotalCost();
                }
                catch (Exception ex)
                {
                    // Loading default grocery list failed. This should never happen!
                    DisplayError($"The default grocery list could not be loaded. " +
                        $"Please contact IT with the following report: {ex.Message}");
                }
            }
        }


        /// <summary>
        /// Attempts to load new grocery list file as the new source of grocery items repeatedly
        /// while the file fails to load and the user wants to try again.
        /// </summary>
        /// <returns>True if a file loaded properly; otherwise, false.</returns>
        private void OpenNewGroceryList()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Set filter for file extension and default file extension
            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text Files | *.txt";
            openFileDialog.InitialDirectory = Path.GetDirectoryName(GroceryManagerSettings.GroceryListFilePath);
            // Display OpenFileDialog
            bool? result = openFileDialog.ShowDialog();
            // Process open file dialog box results
            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                // Attempt to load groceries from file 
                if (!TryLoadGroceryList(filePath))
                {
                    // Load failed, ask if user wants to try again
                    MessageBoxResult tryAgainResult = MessageBox.Show("The file is not readable by the application.\n" +
                        "Would you like to try a different file?", "File Not Readable", MessageBoxButton.YesNo);
                    if (tryAgainResult == MessageBoxResult.Yes)
                    {
                        // Loop the function while the user says Yes
                        OpenNewGroceryList();
                    }
                }
            }
        }

        /// <summary>
        /// Checks if there are any invalid grocery items in the datagrid.
        /// </summary>
        /// <returns>True if there are no invalid input; otherwise, false.</returns>
        private bool AreAnyItemsInvalid()
        {
            foreach (var groceryItem in dataGrid.Items)
            {
                if (Validation.GetHasError(dataGrid.ItemContainerGenerator.ContainerFromItem(groceryItem)))
                {
                    // Validation error found for this item
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Updates the total cost label based on the total cost retrieved from the GroceryItemManager.
        /// </summary>
        private void UpdateTotalCost()
        {
            labelTotalCost.Content = $"{groceryItemManager.GetTotalCost().ToString("N2")} kr";
        }

        /// <summary>
        /// Updates the grocery types in the combo box.
        /// </summary>
        private void CmbBoxUpdateGroceryTypes()
        {
            // Reset cmbBox
            cmbBoxGroceryType.Items.Clear();
            // Add groceries to cmbBox
            foreach(GroceryType grocery in groceryTypes)
            {
                cmbBoxGroceryType.Items.Add($"{grocery.Description,-20} ({grocery.Cost:F2} kr)");
            }
        }

        /// <summary>
        /// Reads the number of units from the text box and validates that it is a value >= 0.
        /// </summary>
        /// <param name="noOfUnits">The parsed number of units if successful.</param>
        /// <returns>True if successfully parsed and non-negative, otherwise false.</returns>
        private bool ReadNoOfUnits(out float noOfUnits)
        {
            // Try to parse noOfUnits to float
            if(float.TryParse(txtBoxNoOfUnits.Text, out noOfUnits))
            {
                // Make sure that noOfUnits is not negative
                if(noOfUnits >= 0)
                {
                    return true;
                }
            }
            DisplayError("The value of 'No. of Units' is invalid. Make sure that you input a positive number.");
            return false;
        }

        /// <summary>
        /// Reads the selected grocery type from the combo box and validates its selection.
        /// </summary>
        /// <param name="grocery">The selected GroceryType if successful, otherwise Null.</param>
        /// <returns>True if a valid grocery type is selected, otherwise false.</returns>
        private bool ReadGroceryType(out GroceryType? grocery)
        {
            int selectedIndex = cmbBoxGroceryType.SelectedIndex;
            // Make sure that the selected index is valid.
            if (selectedIndex >= 0 && 
                selectedIndex < cmbBoxGroceryType.Items.Count
                && selectedIndex < groceryTypes.Count)
            {
                // Read the type of the grocery
                grocery = groceryTypes[selectedIndex];
                if(grocery != null)
                {
                    return true;
                }
                else
                {
                    // The dataGrid is not in sync with the GroceryItemManager, this should never happen
                    DisplayError("Selected grocery was not found! This shouldn't happen, please report it to IT.");
                    return false;
                }
            }
            DisplayError("The value of 'Type' is invalid. Make sure that you have selected a valid grocery.");
            grocery = null;
            return false;
        }


        /// <summary>
        /// Event handler for adding a selected grocery item in the datagrid.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void btnAddGrocery_Click(object sender, RoutedEventArgs e)
        {
            GroceryType? grocery;
            bool groceryTypeOk = ReadGroceryType(out grocery);
            float noOfUnits;
            bool noOfUnitsOk = ReadNoOfUnits(out noOfUnits);
            // Make sure that grocery and noOfUnits have valid values
            if (groceryTypeOk && noOfUnitsOk)
            {
                // Add groceryItem
                groceryItemManager.AddGroceryItem(grocery.Description, grocery.Cost, noOfUnits);
                UpdateTotalCost();
            }
        }

        /// <summary>
        /// Event handler for opening GroceryTypesWindow to allow for editing 
        /// available grocery types.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void btnEditgroceryTypes_Click(object sender, RoutedEventArgs e)
        {
            // Swap from this window to the next one
            this.Hide();
            // Edit grocery types
            groceryWindow.SetWindowPosition(this.Left, this.Top);
            groceryWindow.ShowDialog();
            // Create new grocery window that reloads groceries from file
            groceryWindow = new GroceryTypesWindow();
            // Update list of groceries
            groceryTypes = groceryWindow.GetGroceries();
            // update cmbBox
            CmbBoxUpdateGroceryTypes();

            // Show this window in the same position as groceryWindow
            this.Top = groceryWindow.Top;
            this.Left = groceryWindow.Left;
            this.Show();
        }

        /// <summary>
        /// Event handler for removing a selected grocery item in the datagrid.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void btnRemoveSelectedGrocery_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid.SelectedIndex;
            if(selectedIndex >= 0 && selectedIndex < dataGrid.Items.Count)
            {
                groceryItemManager.RemoveGroceryItem(selectedIndex);
            }
        }

        /// <summary>
        /// Event handler allowing user to select location and name for a new empty grocery items source file.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void menuNew_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult saveFirstResult = MessageBox.Show(
                "Any changes will be lost, do you want to continue to create New?",
                "Create New", MessageBoxButton.YesNo);
            if (saveFirstResult == MessageBoxResult.Yes)
            {
                groceryItemManager.ClearGroceryItems();
            }
        }

        /// <summary>
        /// Event handler allowing user to select a new file to load as the new source for grocery items.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenNewGroceryList();
        }

        /// <summary>
        /// Event handler allowing for saving the current groceryItemManager to a file chosen by the user.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (AreAnyItemsInvalid())
            {
                // Handle items are invalid
                DisplayError("One or more inputs are invalid, check the value of any fields with red borders.");
                return;
            }
            else
            {
                // Let the user pick a filename and location
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = ".txt";
                saveFileDialog.Filter = "Text Files | *.txt";
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(GroceryManagerSettings.GroceryListFilePath);
                bool? result = saveFileDialog.ShowDialog();
                // Handle user's choice for the SaveFileDialog
                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;
                    try
                    {
                        // Save the file
                        groceryItemManager.SaveGroceryList(filePath);
                    }
                    catch (Exception ex)
                    {
                        // Handle the case where save fails (should never happen)
                        DisplayError("The file could not be saved. This should not have happened! " +
                            $"Please contact IT with this report: {ex.Message}");
                        return;
                    }
                    // Update cached file path
                    GroceryManagerSettings.GroceryListFilePath = filePath;
                    // Let the user know the file was saved
                    MessageBox.Show("The data was saved successfully.", "Saving Grocery List", MessageBoxButton.OK);
                }
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
                "\nEdit your grocery list in this window, and edit the grocery types you can choose from by clicking" +
                "on the 'Edit Available Groceries' button. You can save and open both grocery lists and grocery types." +
                "\n\nNote: You can edit the number of units column by pressing enter after you're done editing.", 
                "About Grocery List Manager",MessageBoxButton.OK);
        }

        /// <summary>
        /// Event handle to update total cost when a cell edit is ending.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void CellEditEnding_Handler(object? sender, DataGridCellEditEndingEventArgs e)
        {
            UpdateTotalCost();
        }
    }
}
