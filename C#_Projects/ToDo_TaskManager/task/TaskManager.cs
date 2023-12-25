using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoReminder
{
    /// <summary>
    /// Manages a list of tasks to be done, and allows for adding, removing, and editing tasks.
    /// </summary>
    internal class TaskManager
    {
        private List<Task> tasks; // List of tasks to be done

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskManager"/> class.
        /// </summary>
        public TaskManager()
        {
            tasks = new List<Task>();
        }

        /// <summary>
        /// Get a task at the specified index from the list of tasks.
        /// </summary>
        /// <param name="index">The index of the task to retrieve.</param>
        /// <returns>Task that corresponds to the provided index.</returns>
        public Task GetTask(int index)
        {
            // Ensure the index is within the valid range
            if (index >= 0 && index < tasks.Count)
            {
                return tasks[index];
            }

            return null;
        }

        /// <summary>
        /// Adds a new task to the list in the order of earliest deadline to latest deadline.
        /// </summary>
        /// <param name="deadline">The deadline for the task.</param>
        /// <param name="priority">The priority of the task.</param>
        /// <param name="description">The description of the task.</param>
        public void AddTask(DateTime deadline, PriorityType priority, string description)
        {
            // Add task to the list
            Task newTask = new Task(deadline, priority, description);
            // Find the index where the new task should be inserted based on the deadline
            int indexToInsert = tasks.FindIndex(task => task.Deadline > deadline);
            // If no task with a later deadline is found, append it to the list
            if (indexToInsert == -1)
            {
                // Append task
                tasks.Add(newTask);
            }
            else
            {
                // Insert task
                tasks.Insert(indexToInsert, newTask);
            }
        }

        /// <summary>
        /// Removes a task at the specified index from the list.
        /// </summary>
        /// <param name="index">The index of the task to be removed.</param>
        public void RemoveTask(int index)
        {
            // Ensure the index is within the valid range
            if (index >= 0 && index < tasks.Count)
            {
                tasks.RemoveAt(index);
            }
        }

        /// <summary>
        /// Edits the details of a task at the specified index.
        /// </summary>
        /// <param name="index">The index of the task to be edited.</param>
        /// <param name="updatedTask">The updated details for the task.</param>
        public void EditTask(int index, Task updatedTask)
        {
            // Ensure the index is within the valid range
            if (index >= 0 && index < tasks.Count)
            {
                tasks[index] = updatedTask;
            }
        }

        /// <summary>
        /// Gets the list of tasks.
        /// </summary>
        /// <returns>A list of tasks.</returns>
        public List<Task> Tasks
        {
            get { return tasks; }
        }
    }

}
