using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyDomination
{
    public string id;
    public List<GameObject> objects = new List<GameObject>();

    public void toggle(bool value)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].SetActive(value);
        }
    }
}