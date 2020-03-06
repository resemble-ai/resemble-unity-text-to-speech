using UnityEngine;
using System.Collections.Generic;
using Resemble.GUIEditor;

namespace Resemble
{
    /// <summary> Contains and displays all current requests to Resemble API. </summary>
    public static class Pool
    {
        public static List<Task> tasks = new List<Task>();

        /// <summary> Add a task to the pool. Make this task visible for the user.
        /// /!\ This is a pool for visibility only, no execution. </summary>
        public static void AddTask(Task task)
        {
            tasks.Add(task);
            Resemble_Window.RefreshPoolList();
        }

        /// <summary> Return the task linked to the object. If there is no task linked, return null. </summary>
        public static Task GetPendingTask(Object link)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                Task task = tasks[i];
                if (task.status == Task.Status.Completed)
                    continue;
                if (task.link == link)
                    return task;
            }
            return null;
        }
    }
}
