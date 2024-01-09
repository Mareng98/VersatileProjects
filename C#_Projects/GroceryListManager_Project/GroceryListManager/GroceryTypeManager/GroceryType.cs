

namespace GroceryListManager
{
    /// <summary>
    /// Represents a grocery type with a description and cost.
    /// </summary>
    class GroceryType
    {
        private string description; // A description of the type of grocery
        private float cost; // The cost of the grocery type

        /// <summary>
        /// Default constructor, chaincalls constructor with one parameter.
        /// </summary>
        public GroceryType() : this(string.Empty)
        { }

        /// <summary>
        /// Constructor that chaincalls a constructor with two parameters.
        /// </summary>
        /// <param name="description">The description of the grocery item.</param>
        public GroceryType(string description) : this(description, 0)
        { }

        /// <summary>
        /// Chain-called constructor that creates a Grocery with description and cost. 
        /// Creates a Grocery instance with the specified description and cost.
        /// </summary>
        /// <param name="description">The description that describes the grocery item.</param>
        /// <param name="cost">The cost of the grocery item.</param>
        public GroceryType(string description, float cost)
        {
            // Set the description and cost using the provided parameters
            Description = description;
            Cost = cost;
        }

        /// <summary>
        /// Gets or sets the description of the grocery description. 
        /// </summary>
        /// <remarks>
        /// If the provided value is null or empty,
        /// sets the description to "No Description" as a default.
        /// </remarks>
        public string Description
        {
            get { return description; }
            set
            {
                // The description must have a value.
                if (!string.IsNullOrEmpty(value))
                {
                    description = value;
                }
                else
                {
                    description = "No Description";
                }
            }
        }

        /// <summary>
        /// Gets or sets the cost of the grocery item.
        /// </summary>
        /// <remarks>
        /// If the provided value is less than zero,
        /// sets the cost to zero as a default.
        /// </remarks>
        public float Cost
        {
            get { return cost; }
            set
            {
                // Negative cost is not allowed
                if (value >= 0)
                {
                    cost = value;
                }
                else
                {
                    cost = 0;
                }
            }
        }
    }
}

