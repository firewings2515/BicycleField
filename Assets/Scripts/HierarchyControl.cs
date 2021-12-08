using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyControl
{
    public HierarchyDomination[,] heirarchy_master;
    public int split_x;
    public int split_y;
    bool[,] check_mask;
    int at_x = 0;
    int at_y = 0;
    float max_x = 101;
    float max_y = 101;

    public void setup(int x, int y, float max_x_set, float max_y_set)
    {
        split_x = x;
        split_y = y;
        heirarchy_master = new HierarchyDomination[split_x + 1, split_y + 1];
        for (int i = 0; i < split_x + 1; i++)
        {
            for (int j = 0; j < split_y + 1; j++)
            {
                heirarchy_master[i, j] = new HierarchyDomination();
                heirarchy_master[i, j].id = "heir_" + i + "_" + j;
            }
        }

        check_mask = new bool[5, 5];
        for (int i = 0; i < 5; i++)
        {
            if (i == 0 || i == 4)
            {
                for (int j = 0; j < 5; j++)
                {
                    check_mask[i, j] = false;
                    check_mask[j, i] = false;
                }
            }
            else
            {
                for (int j = 1; j < 4; j++)
                {
                    check_mask[i, j] = true;
                }
            }
        }

        max_x = max_x_set;
        max_y = max_y_set;
    }

    public void beginHierarchy()
    {
        for (int i = 0; i < split_x + 1; i++)
        {
            for (int j = 0; j < split_y + 1; j++)
            {
                heirarchy_master[i, j].toggle(false);
            }
        }
    }

    public void calcLocation(float x, float y, ref int hier_x, ref int hier_y)
    {
        at_x = (int)(x / max_x * split_x);
        at_y = (int)(y / max_y * split_y);

        hier_x = at_x;
        hier_y = at_y;
    }

    public void lookHierarchy()
    {
        // false first
        for (int i = -2; i <= 2; i++)
        {
            if (at_x + i < 0 || at_x + i > split_x) continue;
            for (int j = -2; j <= 2; j++)
            {
                if (at_y + j < 0 || at_y + j > split_y) continue;

                if (check_mask[i + 2, j + 2]) continue;

                heirarchy_master[at_x + i, at_y + j].toggle(check_mask[i + 2, j + 2]);
            }
        }

        // turn on true
        for (int i = -1; i <= 1; i++)
        {
            if (at_x + i < 0 || at_x + i > split_x) continue;
            for (int j = -1; j <= 1; j++)
            {
                if (at_y + j < 0 || at_y + j > split_y) continue;

                heirarchy_master[at_x + i, at_y + j].toggle(true);
            }
        }
    }

    public List<string> getHousesInArea(int x, int y)
    {
        List<string> house_ids = new List<string>();

        List<GameObject> view_instances = heirarchy_master[x, y].objects;
        for (int view_instances_index = 0; view_instances_index < view_instances.Count; view_instances_index++)
        {
            ViewInstance view_instance;
            if (view_instances[view_instances_index].TryGetComponent<ViewInstance>(out view_instance))
            {
                if (view_instance.is_house)
                {
                    house_ids.Add(view_instance.house_id);
                }
            }
        }

        return house_ids;
    }
}