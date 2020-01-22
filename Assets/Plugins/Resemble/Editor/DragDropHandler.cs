//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[InitializeOnLoad]
//public class DragDropHandler
//{

//    private static bool haveResembleItems
//    {
//        get
//        {
//            return _haveResembleItems;
//        }
//        set
//        {
//            if (_haveResembleItems != value)
//            {
//                _haveResembleItems = value;
//                if (value)
//                    EditorApplication.update += ApplicationUpdate;
//                else
//                    EditorApplication.update -= ApplicationUpdate;
//            }
//        }
//    }
//    private static bool _haveResembleItems;
//    private static bool inDrag;

//    static DragDropHandler()
//    {
//        Selection.selectionChanged += SelectionChange;
//        SelectionChange();
//    }

//    static void SelectionChange()
//    {
//        Debug.Log("Selection change");
//        int count = Selection.objects.Length;
//        if (count == 0)
//        {
//            haveResembleItems = false;
//            return;
//        }

//        for (int i = 0; i < count; i++)
//        {
//            System.Type type = Selection.objects[i].GetType();
//            if (type == typeof(Pod))
//            {
//                haveResembleItems = true;
//                return;
//            }
//        }
//        haveResembleItems = false;
//    }

//    static void ApplicationUpdate()
//    {
//        if (!haveResembleItems)
//            EditorApplication.update -= ApplicationUpdate;

//        if (!inDrag && DragAndDrop.paths.Length > 0)
//            BeginDrag();
//        else if (inDrag && DragAndDrop.paths.Length == 0)
//            EndDrag();
//    }

//    static void BeginDrag()
//    {
//        inDrag = true;
//        Debug.Log("Begin drag");
//    }

//    static void EndDrag()
//    {
//        inDrag = false;
//        Debug.Log("end drag");
//    }



//}
