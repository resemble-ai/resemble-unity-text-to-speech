using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resemble.Structs;

namespace Resemble
{
    public static class Pool
    {
        private static List<APIBridge.Task> tasks = new List<APIBridge.Task>();

        public static void DrawGUI()
        {

        }

        public static void AddTask(APIBridge.Task task)
        {
            tasks.Add(task);
        }
    }
}
