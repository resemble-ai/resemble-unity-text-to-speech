using System.Collections.Generic;
using Resemble.GUIEditor;

namespace Resemble
{
    /// <summary> Contains and displays all current requests to Resemble API. </summary>
    public static class Pool
    {
        public static List<Task> tasks = new List<Task>();
        private static int lastID;

        /// <summary> Add a task to the pool. Make this task visible for the user.
        /// /!\ This is a pool for visibility only, no execution. </summary>
        public static void AddTask(Task task)
        {
            task.poolID = lastID;
            lastID++;
            tasks.Add(task);
            Resemble_Window.RefreshPoolList();
        }
    }
}
