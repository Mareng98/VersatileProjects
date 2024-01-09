using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;


namespace GroceryListManager
{
    /// <summary>
    /// Manages a collection of <see cref="GroceryType"/> and provides 
    /// methods for manipulating the grocery types.
    /// Also allows for reading and saving grocery types from/to files.
    /// </summary>
    class GroceryTypeManager
    {
        private ObservableCollection<GroceryType> groceryTypes;
        private string token = "GroceryTypeManager 2024";

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryTypeManager"/> class 
        /// with an empty collection of grocery types.
        /// </summary>
        public GroceryTypeManager()
        {
            groceryTypes = new ObservableCollection<GroceryType>();
        }

        /// <summary>
        /// Clears all grocery types from the list.
        /// </summary>
        public void ClearGroceryTypes()
        {
            groceryTypes.Clear();
        }

        /// <summary>
        /// Loads a grocery types from a specified file path.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        /// <param name="filePath">The file path of the grocery types file.</param>
        private void LoadGroceryTypesHelper(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                List<GroceryType>? tempGroceryList = new List<GroceryType>();
                // Ensure that the file was created by this program by reading the token
                string? line = sr.ReadLine();
                if (line == null || !line.Equals(token))
                {
                    throw new FileLoadException("The file is not readable by this program.");
                }
                // Read the rest of the file as json
                string jsonContent = sr.ReadToEnd();
                // Deserialize the groceries. This might throw an exception if the file is corrupt
                tempGroceryList = JsonSerializer.Deserialize<List<GroceryType>>(jsonContent);
                // Reset list of grocery types
                ClearGroceryTypes();
                // Continue to add groceries to groceryTypes list
                if (tempGroceryList != null)
                {
                    foreach (GroceryType grocery in tempGroceryList)
                    {
                        groceryTypes.Add(grocery);
                    }
                }
                
            }
        }

        /// <summary>
        /// Loads grocery types from the cached file path.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        public void LoadCachedGroceryTypes()
        {
            LoadGroceryTypesHelper(GroceryManagerSettings.GroceryTypesFilePath);
        }

        /// <summary>
        /// Creates and loads default grocery types based on a template.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        private void CreateDefaultGroceryTypes()
        {
            // List of grocery types with values taken from a popular swedish grocery store
            List<GroceryType> templateGroceryTypes = new List<GroceryType>{
                    new GroceryType { Description = "Milk, 1.5L", Cost = 17.5F },
                    new GroceryType { Description = "Cream, 5dl", Cost = 27.5F },
                    new GroceryType { Description = "Eggs, 10p", Cost = 34.95F },
                    new GroceryType { Description = "Salted Butter, 500g", Cost = 54.95F },
                    new GroceryType { Description = "Household Cheese, 1.1kg", Cost = 126.5F },
                    new GroceryType { Description = "Bananas, 200g", Cost = 5.99F },
                    new GroceryType { Description = "Potatoes, 1kg", Cost = 16.95F },
                };
            // Write templateGroceryTypes to groceryTypes.txt file
            // Add new line character to end of each item to make the file more readable
            string json = '[' + string.Join($"{Environment.NewLine},", templateGroceryTypes.Select(grocery => JsonSerializer.Serialize(grocery))) + ']';
            // Append the token to the start of the file
            string fileContent = $"{token}\n{json}";
            // Write file
            File.WriteAllText(GroceryManagerSettings.Default_groceryTypesFilePath, fileContent);

            // Load template grocery types
            foreach (GroceryType grocery in templateGroceryTypes)
            {
                groceryTypes.Add(grocery);
            }
        }

        /// <summary>
        /// Loads the default grocery types from the default file path 
        /// or recreates the file if missing or corrupted.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        public void LoadDefaultGroceryTypes()
        {
            // Reset the file path to known good values
            string filePath = GroceryManagerSettings.Default_groceryTypesFilePath;
            GroceryManagerSettings.GroceryTypesFilePath = filePath;
            try
            {
                // Attempt to load the groceries from the default file
                LoadGroceryTypesHelper(filePath);
            }
            catch(Exception ex)
            {
                // The default file is missing or has been corrupted, recreate it and load again
                CreateDefaultGroceryTypes();
                LoadGroceryTypesHelper(filePath);
            }
        }

        /// <summary>
        /// Saves the grocery types collection to the specified file path in JSON format.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        /// <param name="filePath">The file path where the grocery types will be saved.</param>
        public void SaveGroceryTypes(string filePath)
        {
            // Ensure that filePath is valid
            if (filePath == null) throw new FileLoadException("Null reference detected on filepath.");
            // Add new line character to end of each item to make the file more readable
            string json = '[' + string.Join($"{Environment.NewLine},", groceryTypes.Select(grocery => JsonSerializer.Serialize(grocery))) + ']';
            // Append the token to the start of the file
            string fileContent = $"{token}\n{json}";
            // Write file
            File.WriteAllText(filePath, fileContent);
        }

        /// <summary>
        /// Gets the collection of grocery types.
        /// </summary>
        public ObservableCollection<GroceryType> GroceryTypes
        {
            get { return groceryTypes; }
        }

        /// <summary>
        /// Removes the <see cref="GroceryType"/> at the specified index from the collection.
        /// </summary>
        /// <param name="index">The index of the grocery to be removed.</param>
        public void RemoveGroceryType(int index)
        {
            // Verify that the item grocery type exists
            if (index >= 0 && index < groceryTypes.Count)
            {
                groceryTypes.RemoveAt(index);
            }
        }
    }
}
