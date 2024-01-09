using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GroceryListManager
{
    /// <summary>
    /// Class to control that file paths for the program GroceryListManager 
    /// are properly configured and to carry data across sessions.
    /// </summary>
    internal static class GroceryManagerSettings
    {
        // Set base directory as main directory for default files
        private static string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string settingsFilePath = baseDirectory + "GroceryManagerSettings.txt";
        private static readonly string token = "GroceryManager Settings 2024";
        // File path variables for Grocery Types
        private static readonly string default_groceryTypesFilePath = baseDirectory + "groceryTypes.json";
        private static string groceryTypesFilePath = default_groceryTypesFilePath;
        // File path variables for Grocery Items
        private static readonly string default_groceryListFilePath = baseDirectory + "groceryList.txt";
        private static string groceryListFilePath = default_groceryListFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryManagerSettings"/> class.
        /// </summary>
        /// <remarks>
        /// Verifies all file path variables on load and sets file path variables to cached values.
        /// </remarks>
        static GroceryManagerSettings()
        {
            LoadSettings();
        }

        /// <summary>
        /// Verifies all cached paths and sets path variables.
        /// </summary>
        public static void LoadSettings()
        {
            // Ensure that the settings file can be used
            VerifySettingsFile();
            // Try to load settings
            try
            {
                string[] lines = File.ReadAllLines(settingsFilePath);
                groceryTypesFilePath = lines[1];
                groceryListFilePath = lines[2];
            }
            catch (Exception ex)
            {
                // Write the exception in console (this should never happen)
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Verifies that all cached paths in the settings file are still pointing 
        /// to existing files, as well as that the GroceryManagerSettings is properly formatted.
        /// </summary>
        /// <remarks>If the file is corrupt, it recreates it with default values.</remarks>
        private static void VerifySettingsFile()
        {
            if (File.Exists(settingsFilePath))
            {
                bool fileOk = true;
                // Verify that the settings file is valid
                try { 
                    using (StreamReader sr = new StreamReader(settingsFilePath))
                    {
                        string? tokenLine = sr.ReadLine();
                        string? groceryTypesLine = sr.ReadLine();
                        string? groceriesListLine = sr.ReadLine();

                        // Verify the content
                        if (tokenLine == null || !tokenLine.Equals(token))
                        {
                            // The first line should be equal to the token
                            fileOk = false;
                        }else if (string.IsNullOrEmpty(groceryTypesLine) || 
                            (!File.Exists(groceryTypesLine) && !groceryTypesLine.Equals(default_groceryTypesFilePath)))
                        {
                            // The second line should be a valid path to a file or the default file path
                            fileOk = false;
                        }else if (string.IsNullOrEmpty(groceriesListLine) || 
                            (!File.Exists(groceriesListLine) && !groceriesListLine.Equals(default_groceryListFilePath)))
                        {
                            // The third line should be a valid path to a file or the default file path
                            fileOk = false;
                        }
                        if (fileOk)
                        {
                            return;
                        }
                    }
                }
                catch(Exception ex)
                {
                    // Write the exception in console
                    Console.WriteLine("Verifying the settings file failed: " + ex);
                }
            }
            // Repair settingsFile by setting it to known good values
            File.WriteAllText(settingsFilePath,
                    token + "\n" +
                    default_groceryTypesFilePath + "\n" +
                    default_groceryListFilePath);
            // Reset variables
            groceryTypesFilePath = default_groceryTypesFilePath;
            groceryListFilePath = default_groceryListFilePath;
        }

        /// <summary>
        /// Edits row 'index' in the settings file to the specified filePath.
        /// </summary>
        /// <remarks>
        /// Row 1: GroceryTypes file path.
        /// Row 2: GroceryList file path.
        /// </remarks>
        /// <param name="index">The row in the settings file that is edited (1 or 2).</param>
        /// <param name="filePath">The value that will be written to the indexed row.</param>
        private static void EditLineInSettingsFile(int index, string filePath)
        {
            // Verify that the settings file is valid
            VerifySettingsFile();
            // Read all lines
            string[] lines = File.ReadAllLines(settingsFilePath);
            // Update line at position of index
            if (index == 1)
            {
                // Update line 1
                lines[1] = filePath;
            }else if(index == 2)
            {
                // Update line 2
                lines[2] = filePath;
            }
            try
            {
                // Write the contents back to the file
                using (StreamWriter sw = new StreamWriter(settingsFilePath))
                {
                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch(Exception ex)
            {
                // Write the exception in console
                Console.WriteLine("Editing the settings file failed: " + ex);
            }
        }

        /// <summary>
        /// Gets or sets the file path for the grocery types data.
        /// </summary>
        /// <remarks>
        /// Setting the value ensures that it points to an existing file, 
        /// and updates the associated settings in the configuration file.
        /// </remarks>
        public static string GroceryTypesFilePath
        {
            get { return groceryTypesFilePath; }
            set
            {
                // Ensure that value points to an existing file
                if (!File.Exists(value)) return;
                // Edit line 1 in the settings file to value
                EditLineInSettingsFile(1, value);
                // Update variable
                groceryTypesFilePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the file path for the grocery list data.
        /// </summary>
        /// <remarks>
        /// Setting the value ensures that it points to an existing file, 
        /// and updates the associated settings in the configuration file.
        /// </remarks>
        public static string GroceryListFilePath
        {
            get { return groceryListFilePath; }
            set
            {
                // Ensure that value points to an existing file
                if (!File.Exists(value)) return;
                // Edit line 2 in the settings file to value
                EditLineInSettingsFile(2, value);
                // Update variable
                groceryListFilePath = value;
            }
        }

        /// <summary>
        /// Gets the default file path for the grocery types data.
        /// </summary>
        public static string Default_groceryTypesFilePath
        {
            get { return default_groceryTypesFilePath; }
        }

        /// <summary>
        /// Gets the default file path for the grocery list data.
        /// </summary>
        public static string Default_GroceryListFilePath
        {
            get { return default_groceryListFilePath; }
        }
    }
}
