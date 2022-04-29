using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House
{
    public List<string> ref_node = new List<string>();
    public string id = string.Empty;
    public string name = string.Empty;
    public List<string> tag_k = new List<string>();
    public List<string> tag_v = new List<string>();

    public string writeWay()
    {
        string output = string.Empty;
        output += $"  <way id=\"{id}\"";
        if (tag_k.Count > 0)
        {
            output += ">\n";
            for (int nd_index = 0; nd_index < ref_node.Count; nd_index++)
            {
                output += $"    <nd ref=\"{ref_node[nd_index]}\"/>\n";
            }
            for (int tag_index = 0; tag_index < tag_k.Count; tag_index++)
            {
                output += $"    <tag k=\"{tag_k[tag_index]}\" v=\"{tag_v[tag_index]}\"/>\n";
            }
            output += "  </way>\n";
        }
        else
        {
            output += "/>\n";
        }
        return output;
    }
}