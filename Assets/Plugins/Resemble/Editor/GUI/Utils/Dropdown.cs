using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble;

namespace Resemble.GUIEditor
{
    public struct Dropdown
    {
        private Rect rect;
        public struct Item
        {
            public string label;
            public bool separator;
            public bool enable;
            public bool check;
            public GenericMenu.MenuFunction methode;

            public Item(string label, GenericMenu.MenuFunction methode)
            {
                this.label = label;
                this.methode = methode;
                separator = false;
                enable = true;
                check = false;
            }

            public Item(string label, bool enable, GenericMenu.MenuFunction methode)
            {
                this.label = label;
                this.methode = methode;
                this.enable = enable;
                separator = false;
                check = false;
            }


            public Item(string label, bool enable, bool check, GenericMenu.MenuFunction methode)
            {
                this.label = label;
                this.methode = methode;
                this.enable = enable;
                this.check = check;
                separator = false;
            }

            public Item(string label)
            {
                this.label = label;
                methode = null;
                enable = true;
                check = false;
                separator = true;
            }

        }

        public void DoLayout(GUIContent title, params Item[] methodes)
        {
            if (GUILayout.Button(title, EditorStyles.toolbarButton))
                DropDown(methodes);
            if (Event.current.type == EventType.Repaint)
                rect = GUILayoutUtility.GetLastRect().Drop();
        }

        public void DropDown(Item[] methodes)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < methodes.Length; i++)
            {
                Item m = methodes[i];

                //Separator
                if (m.separator)
                {
                    menu.AddSeparator(m.label);
                    continue;
                }

                //Disable item
                if (!m.enable)
                {
                    menu.AddDisabledItem(new GUIContent(m.label), m.check);
                    continue;
                }

                //Classic item
                menu.AddItem(new GUIContent(m.label), m.check, m.methode);
            }

            menu.DropDown(rect);
        }
    }
}