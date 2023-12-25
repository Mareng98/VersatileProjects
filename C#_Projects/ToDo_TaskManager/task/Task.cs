using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoReminder
{
    /// <summary>
    /// Represents a task to be done with a deadline, priority, and description.
    /// </summary>
    internal class Task
    {
        private DateTime deadline; // The date that the task has to be done
        private PriorityType priority; // Priority of task
        private string description; // Description of task

        /// <summary>
        /// Initializes a new instance of a <see cref="Task"/> with the specified parameters.
        /// </summary>
        /// <param name="time">The deadline for the task.</param>
        /// <param name="priority">The priority of the task.</param>
        /// <param name="description">The description of the task. 
        /// If the provided value is null or empty, a default description of "No description" is set.</param>
        public Task(DateTime time, PriorityType priority, string description)
        {
            Deadline = time;
            Priority = priority;
            Description = description;
        }

        /// <summary>
        /// Gets or sets the deadline for the task.
        /// </summary>
        public DateTime Deadline { get; set; }

        /// <summary>
        /// Gets or sets the priority of the task.
        /// </summary>
        public PriorityType Priority { get; set; }

        /// <summary>
        /// Gets or sets the description of the task.
        /// If the provided value is null or empty, a default description of "No description" is set.
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    description = value;
                }
                else
                {
                    description = "No description";
                }
            }
        }

    }
}
