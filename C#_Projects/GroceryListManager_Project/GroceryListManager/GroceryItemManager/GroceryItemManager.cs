using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GroceryListManager
{
    /// <summary>
    /// Manages a collection of <see cref="GroceryItem"/> and provides methods 
    /// for manipulating the grocery items.
    /// Also allows for reading and saving grocery items from/to files.
    /// </summary>
    class GroceryItemManager
    {
        private ObservableCollection<GroceryItem> groceryItems;
        private float totalCost;
        private string token = "GroceryItemManager 2024";

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryItemManager"/> class 
        /// with an empty collection of grocery items.
        /// </summary>
        public GroceryItemManager()
        {
            groceryItems = new ObservableCollection<GroceryItem>();
        }

        /// <summary>
        /// Gets the collection of grocery items.
        /// </summary>
        public ObservableCollection<GroceryItem> GroceryItems
        {
            get { return groceryItems; }
        }

        /// <summary>
        /// Clears all items from the grocery list.
        /// </summary>
        public void ClearGroceryItems()
        {
            groceryItems.Clear();
        }

        /// <summary>
        /// Calculates and returns the total cost of all items in the grocery list.
        /// </summary>
        /// <returns>The total cost of all items in the grocery list.</returns>
        public float GetTotalCost()
        {
            totalCost = 0;
            foreach (GroceryItem item in groceryItems)
            {
                totalCost += item.Cost * item.NoOfUnits;
            }
            return totalCost;
        }

        /// <summary>
        /// Adds a new <see cref="GroceryItem"/> to the grocery list with the specified description, cost, and number of units.
        /// </summary>
        /// <param name="description">The description of the grocery item.</param>
        /// <param name="cost">The cost of the grocery item.</param>
        /// <param name="noOfUnits">The number of units of the grocery item.</param>
        public void AddGroceryItem(string description, float cost, float noOfUnits)
        {
            GroceryItem newGroceryItem = new GroceryItem(description, cost, noOfUnits);
            groceryItems.Add(newGroceryItem);
        }

        /// <summary>
        /// Removes the <see cref="GroceryItem"/> at the specified index from the grocery list.
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        public void RemoveGroceryItem(int index)
        {
            if (index >= 0 && index < groceryItems.Count)
            {
                groceryItems.RemoveAt(index);
            }
        }
        /// <summary>
        /// Loads a grocery list from a specified file path.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        /// <param name="filePath">The file path of the grocery list file.</param>
        private void LoadGroceryListHelper(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                List<GroceryItem>? tempGroceryItems = new List<GroceryItem>();
                // Ensure that the file was created by this program by reading the token
                string? line = sr.ReadLine();
                if (line == null || !line.Equals(token))
                {
                    throw new FileLoadException("The file is not readable by this program.");
                }
                // Read the rest of the file as json
                string jsonContent = sr.ReadToEnd();
                // Deserialize the grocery items. This might throw an exception if the file is corrupt
                tempGroceryItems = JsonSerializer.Deserialize<List<GroceryItem>>(jsonContent);
                // Reset list of groceries
                ClearGroceryItems();
                // Continue to add groceries to groceryItems list
                if (tempGroceryItems != null)
                {
                    foreach (GroceryItem groceryItem in tempGroceryItems)
                    {
                        groceryItems.Add(groceryItem);
                    }
                }
            }
        }
        /// <summary>
        /// Loads a grocery list from the cached file path.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        public void LoadCachedGroceryList()
        {
            LoadGroceryListHelper(GroceryManagerSettings.GroceryListFilePath);
        }

        /// <summary>
        /// Loads the default grocery list from the default file path 
        /// or recreates the file if missing or corrupted.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        public void LoadDefaultGroceryList()
        {
            // Reset the file path to known good values
            string filePath = GroceryManagerSettings.Default_GroceryListFilePath;
            GroceryManagerSettings.GroceryListFilePath = filePath;
            try
            {
                // Attempt to load the groceries from the default file
                LoadGroceryListHelper(filePath);
            }
            catch (Exception ignored)
            {
                // The default file is missing or has been corrupted, recreate it
                File.WriteAllText(filePath, token + "\n" + "[]");
                // Reload default grocery list
                LoadGroceryListHelper(filePath);
            }
        }
        /// <summary>
        /// Saves the grocery list to the specified file path in JSON format.
        /// </summary>
        /// <remarks>
        /// Can throw exceptions.
        /// </remarks>
        /// <param name="filePath">The file path where the grocery list will be saved.</param>
        public void SaveGroceryList(string filePath)
        {
            // Ensure that directoryPath is valid
            if (filePath == null) throw new FileLoadException("Null reference detected on filePath");
            // Add new line character to end of each item to make the file more readable
            string json = '[' + string.Join($"{Environment.NewLine},", 
                groceryItems.Select(groceryItem => JsonSerializer.Serialize(groceryItem))) + ']';
            // Append the token to the start of the file
            string fileContent = $"{token}\n{json}";
            // Write file
            File.WriteAllText(filePath, fileContent);
        }
    }
}
