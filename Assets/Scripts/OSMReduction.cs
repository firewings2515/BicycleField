using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class OSMReduction : MonoBehaviour
{
    public string file_name = "mapNTUST.osm";
    public string output_name = "mapNTUSTSimple.osm";
    public bool load_osm_file = false;
    public bool leave_visible = false;
    public bool leave_version = false;
    public bool leave_changeset = false;
    public bool leave_timestamp = false;
    public bool leave_user = false;
    public bool leave_uid = false;
    public bool leave_relation = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (load_osm_file)
        {
            load_osm_file = false;
            loadOSMFile();
        }
    }

    void loadOSMFile()
    {
        string file_path = Application.streamingAssetsPath + "//" + file_name;
        string[] input_s;
        List<string> output_s = new List<string>();
        using (StreamReader sr = new StreamReader(file_path))
        {
            input_s = sr.ReadToEnd().Split(' ', '\n', '\t');
            int xml_mode = 0; // 1 is node
            bool quotes = false;
            int quote_index = 0;
            for (int s_index = 0; s_index < input_s.Length; s_index++)
            {
                int f_q = input_s[s_index].IndexOf("\"");
                int s_q = input_s[s_index].IndexOf("\"", f_q + 1);
                if (!quotes && f_q != -1 && s_q == -1)
                {
                    quotes = true;
                    quote_index = s_index;
                    continue;
                }
                else if (quotes)
                {
                    if (f_q != -1 && s_q == -1)
                    {
                        quotes = false;
                        input_s[s_index] = input_s[quote_index] + input_s[s_index];
                    }
                    else
                    {
                        input_s[quote_index] += input_s[s_index];
                        continue;
                    }
                }

                if (input_s[s_index].IndexOf("<node") == 0)
                {
                    xml_mode = 1;
                }
                else if (input_s[s_index].IndexOf("<way") == 0)
                {
                    xml_mode = 2;
                }
                else if (input_s[s_index].IndexOf("<relation") == 0)
                {
                    xml_mode = 3;
                }

                if (xml_mode == 1) // node
                {
                    if (input_s[s_index].IndexOf("visible") == 0 && !leave_visible)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("version") == 0 && !leave_version)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("changeset") == 0 && !leave_changeset)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("timestamp") == 0 && !leave_timestamp)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("user") == 0 && !leave_user)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("uid") == 0 && !leave_uid)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("</node>") == 0)
                    {
                        xml_mode = 0;
                    }
                }
                else if (xml_mode == 2) // way
                {
                    if (input_s[s_index].IndexOf("visible") == 0 && !leave_visible)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("version") == 0 && !leave_version)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("changeset") == 0 && !leave_changeset)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("timestamp") == 0 && !leave_timestamp)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("user") == 0 && !leave_user)
                    {
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("uid") == 0 && !leave_uid)
                    {
                        if (input_s[s_index].IndexOf(">") != -1)
                        {
                            output_s[output_s.Count - 1] += ">";
                        }
                        continue;
                    }
                    else if (input_s[s_index].IndexOf("</way>") == 0)
                    {
                        xml_mode = 0;
                    }
                }
                else if (xml_mode == 3) // relation
                {
                    if (input_s[s_index].IndexOf("</relation>") == 0)
                    {
                        xml_mode = 0;
                    }

                    if (!leave_relation)
                    {
                        while (string.IsNullOrWhiteSpace(output_s[output_s.Count - 1]))
                        {
                            output_s.RemoveAt(output_s.Count - 1);
                        }
                        continue;
                    }
                }

                output_s.Add(input_s[s_index]);
            }
        }

        file_path = Application.streamingAssetsPath + "//" + output_name;
        using (StreamWriter sw = new StreamWriter(file_path))
        {
            foreach (string s in output_s)
            {
                sw.Write(s);

                if (s.Length > 0 && s.IndexOf(">") == s.Length - 1)
                {
                    sw.Write("\n");
                }
                else
                {
                    sw.Write(" ");
                }
            }
        }

        Debug.Log("OSM Reduction Success!");
    }
}