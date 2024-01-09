using System.ComponentModel;

namespace GroceryListManager
{
    /// <summary>
    /// Represents a grocery item with description, cost, number of units, and total cost.
    /// Implements the <see cref="INotifyPropertyChanged"/> interface for WPF data binding; extends <see cref="GroceryType"/>.
    /// </summary>
    class GroceryItem : GroceryType, INotifyPropertyChanged
    {
        private float noOfUnits; // The number of units of the grocery to be bought
        private float totalCost; // The total cost of all the units

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>
        /// (This event has to be public to implement INotifyPropertyChanged and
        /// let the datagrid update its values automatically in WPF)
        /// </remarks>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryItem"/> class with default values.
        /// </summary>
        public GroceryItem() : base()
        {
            NoOfUnits = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryItem"/> class with a specified description.
        /// </summary>
        /// <param name="description">The description of the grocery item.</param>
        public GroceryItem(string description) : base(description)
        {
            NoOfUnits = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryItem"/> class with a specified description and cost.
        /// </summary>
        /// <param name="description">The description of the grocery item.</param>
        /// <param name="cost">The cost of the grocery item.</param>
        public GroceryItem(string description, float cost) : base(description, cost)
        {
            NoOfUnits = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroceryItem"/> class with a specified description, cost, and number of units.
        /// </summary>
        /// <param name="description">The description of the grocery item.</param>
        /// <param name="cost">The cost of the grocery item.</param>
        /// <param name="noOfUnits">The number of units of the grocery item.</param>
        public GroceryItem(string description, float cost, float noOfUnits) : base(description, cost)
        {
            NoOfUnits = noOfUnits;
        }

        /// <summary>
        /// Updates the total cost based on the current cost and number of units.
        /// </summary>
        public void UpdateTotalCost()
        {
            TotalCost = Cost * NoOfUnits;
        }

        /// <summary>
        /// Gets or sets the number of units of the grocery item.
        /// </summary>
        /// <remarks>
        /// If the provided value is less than zero,
        /// sets the number of units to zero.
        /// </remarks>
        public float NoOfUnits
        {
            get { return noOfUnits; }
            set
            {
                if (value >= 0)
                {
                    noOfUnits = value;
                }
                else
                {
                    noOfUnits = 0;
                }
                // Update total cost when number of units changes
                UpdateTotalCost();
            }
        }

        /// <summary>
        /// Gets or sets the total cost of the grocery item and 
        /// notifies property changed to update total cost. 
        /// </summary>
        /// <remarks>
        /// If the provided value is less than zero,
        /// sets the total cost to zero.
        /// </remarks>
        public float TotalCost
        {
            get { return totalCost; }
            set
            {
                if (value >= 0)
                {
                    totalCost = value;
                }
                else
                {
                    totalCost = 0;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalCost"));
            }
        }
    }

}
