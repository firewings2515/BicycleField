using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Xml;

public class OSMReader
{
    List<string> points_id = new List<string>();
    public Dictionary<string, Node> points_lib = new Dictionary<string, Node>();
    public List<Way> pathes = new List<Way>();
    public List<List<string>> houses = new List<List<string>>();
    public List<string> houses_id = new List<string>();
    public Vector2 OSM_size;
    public Vector2 boundary_min = Vector2.zero;
    public Vector2 boundary_max = Vector2.zero;
    float near_distance = 0.0f;
    List<List<string>> avenue_links_ref = new List<List<string>>();
    List<string> avenue_links_to = new List<string>();
    bool debug_mode = false;
    float x_length = 1.0f;
    float y_length = 1.0f;

    public void toUnityLocation(float lon, float lat, out float x, out float z)
    {
        x = (lon - boundary_min.x) / x_length * OSM_size.x;
        z = (lat - boundary_min.y) / y_length * OSM_size.y;
    }

    public void readOSM(string file_path, Vector2 _OSM_size)
    {
        OSM_size = _OSM_size;
        XmlReader reader = XmlReader.Create(file_path);
        reader.MoveToContent();
        List<Node> points = new List<Node>();
        List<string> current_points = new List<string>();
        Dictionary<string, List<string>> connect_points = new Dictionary<string, List<string>>();

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "bounds")
                {
                    boundary_min = new Vector2(float.Parse(reader.GetAttribute("minlon")), float.Parse(reader.GetAttribute("minlat")));
                    boundary_max = new Vector2(float.Parse(reader.GetAttribute("maxlon")), float.Parse(reader.GetAttribute("maxlat")));
                }
                else if (reader.Name == "node")
                {
                    boundary_min.x = Mathf.Min(boundary_min.x, float.Parse(reader.GetAttribute("lon")));
                    boundary_min.y = Mathf.Min(boundary_min.y, float.Parse(reader.GetAttribute("lat")));
                    boundary_max.x = Mathf.Max(boundary_max.x, float.Parse(reader.GetAttribute("lon")));
                    boundary_max.y = Mathf.Max(boundary_max.y, float.Parse(reader.GetAttribute("lat")));

                    Node point = new Node();
                    point.position = new Vector3(float.Parse(reader.GetAttribute("lon")), 0.5f, float.Parse(reader.GetAttribute("lat"))); // 0.5f is altitude
                    points.Add(point);
                    string id = reader.GetAttribute("id");
                    points_id.Add(id);

                    connect_points[id] = new List<string>();
                }
                else if (reader.Name == "way")
                {
                    current_points.Clear();
                    bool fetch_road = false;
                    bool fetch_house = false;
                    string id = reader.GetAttribute("id");
                    string road_name = string.Empty;
                    int ref_index = 0;
                    string road_head = string.Empty;
                    string road_tail = string.Empty;
                    Highway highway = Highway.None;
                    int layer = 0;
                    
                    while (reader.Read())
                    {
                        if (reader.Name == "nd")
                        {
                            string way_ref = reader.GetAttribute("ref");
                            current_points.Add(way_ref); // point id
                            if (ref_index == 0)
                            {
                                road_head = way_ref;
                            }
                            road_tail = way_ref;
                            ref_index++;
                        }
                        else if (reader.Name == "tag")
                        {
                            if (reader.GetAttribute("v") == "motorway") // road
                            {
                                fetch_road = true;
                                highway = Highway.Motorway;
                            }
                            else if (reader.GetAttribute("v") == "trunk") // road
                            {
                                fetch_road = true;
                                highway = Highway.Trunk;
                            }
                            else if (reader.GetAttribute("v") == "primary") // road
                            {
                                fetch_road = true;
                                highway = Highway.Primary;
                            }
                            else if (reader.GetAttribute("v") == "secondary") // road
                            {
                                fetch_road = true;
                                highway = Highway.Secondary;
                            }
                            else if (reader.GetAttribute("v") == "tertiary") // road
                            {
                                fetch_road = true;
                                highway = Highway.Tertiary;
                            }
                            else if (reader.GetAttribute("v") == "unclassified") // road
                            {
                                fetch_road = true;
                                highway = Highway.Unclassified;
                            }
                            else if (reader.GetAttribute("v") == "residential") // road
                            {
                                fetch_road = true;
                                highway = Highway.Residential;
                            }
                            else if (reader.GetAttribute("k") == "name") // get road name
                            {
                                road_name = reader.GetAttribute("v");
                            }
                            else if (reader.GetAttribute("k").IndexOf("addr") == 0 || reader.GetAttribute("k").IndexOf("building") == 0) // house
                            {
                                fetch_house = true;
                            }
                            else if (reader.GetAttribute("k") == "layer") // road height
                            {
                                layer = int.Parse(reader.GetAttribute("v"));
                            }
                        }
                        else if (reader.Name == "way")
                        {
                            break;
                        }
                    }
                    if (fetch_road)
                    {
                        Way way = new Way();
                        way.ref_node = new List<string>(current_points);
                        way.id = id;
                        way.name = road_name;
                        way.head_node = road_head;
                        way.tail_node = road_tail;
                        way.highway = highway;
                        way.layer = layer;
                        pathes.Add(way);

                        for (int current_points_index = 0; current_points_index < current_points.Count; current_points_index++)
                        {
                            if (!connect_points[current_points[current_points_index]].Contains(id))
                            {
                                connect_points[current_points[current_points_index]].Add(id);
                            }
                        }
                    }
                    else if (fetch_house)
                    {
                        houses.Add(new List<string>(current_points));
                        houses_id.Add(id);
                    }
                }
            }
        }

        // normalize points
        x_length = boundary_max.x - boundary_min.x;
        y_length = boundary_max.y - boundary_min.y;
        near_distance = 0.0002f / x_length * OSM_size.x;
        for (int point_index = 0; point_index < points.Count; point_index++)
        {
            float unity_x, unity_z;
            toUnityLocation(points[point_index].position.x, points[point_index].position.z, out unity_x, out unity_z);
            points[point_index].position = new Vector3(unity_x, points[point_index].position.y, unity_z);
            points[point_index].connect_way = new List<string>(connect_points[points_id[point_index]]);
            points_lib.Add(points_id[point_index], points[point_index]);
        }

        for (int pathes_index = 0; pathes_index < pathes.Count; pathes_index++)
        {
            pathes[pathes_index].updateOrient(points_lib);
        }

        mergeRoad();
    }

    private void mergeRoad()
    {
        List<string> delete_pathes = new List<string>();

        for (int road_i = 0; road_i < pathes.Count - 1; road_i++)
        {
            if (pathes[road_i].is_merged || pathes[road_i].highway == Highway.CombineLink) break;

            for (int road_j = road_i + 1; road_j < pathes.Count; road_j++)
            {
                if (pathes[road_j].is_merged || pathes[road_j].highway == Highway.CombineLink) break;

                if (pathes[road_i].name == pathes[road_j].name && pathes[road_i].highway == pathes[road_j].highway && pathes[road_i].layer == pathes[road_j].layer && (pathes[road_i].head_node != pathes[road_j].head_node && pathes[road_i].head_node != pathes[road_j].tail_node && pathes[road_i].tail_node != pathes[road_j].head_node && pathes[road_i].tail_node != pathes[road_j].tail_node)) // same road name but no be connect same way
                {
                    List<string> connect_i_h = points_lib[pathes[road_i].head_node].connect_way;
                    List<string> connect_i_t = points_lib[pathes[road_i].tail_node].connect_way;
                    List<string> connect_j_h = points_lib[pathes[road_j].head_node].connect_way;
                    List<string> connect_j_t = points_lib[pathes[road_j].tail_node].connect_way;
                    Vector3 pos_i_h = points_lib[pathes[road_i].head_node].position;
                    Vector3 pos_i_t = points_lib[pathes[road_i].tail_node].position;
                    Vector3 pos_j_h = points_lib[pathes[road_j].head_node].position;
                    Vector3 pos_j_t = points_lib[pathes[road_j].tail_node].position;
                    bool can_merge = false;
                    float merged_road_width = 6;
                    if ((checkConnectRoad(connect_i_h, connect_j_h) || checkConnectRoad(connect_i_t, connect_j_t)) && isNearedPoint(pos_i_h, pos_j_h) && isNearedPoint(pos_i_t, pos_j_t))
                    {
                        can_merge = true;
                        merged_road_width = Vector2.Distance(pos_i_h, pos_j_h);
                    }
                    else if ((checkConnectRoad(connect_i_h, connect_j_t) || checkConnectRoad(connect_i_t, connect_j_h)) && isNearedPoint(pos_i_h, pos_j_t) && isNearedPoint(pos_i_t, pos_j_h))
                    {
                        pathes[road_j].ref_node.Reverse();
                        pathes[road_j].updateOrient(points_lib);
                        can_merge = true;
                        merged_road_width = Vector2.Distance(pos_i_h, pos_j_t);
                    }

                    if (can_merge)
                    {
                        float proportion = (pathes[road_i].ref_node.Count - 1) / (float)(pathes[road_j].ref_node.Count - 1);
                        float current_t = proportion;
                        float current_p;
                        int current_i = (int)proportion;
                        List<Vector3> avenue = new List<Vector3>();
                        avenue.Add(getMiddle(points_lib[pathes[road_i].ref_node[0]].position, points_lib[pathes[road_j].ref_node[0]].position));

                        for (int current_j = 1; current_j < pathes[road_j].ref_node.Count - 1; current_j++)
                        {
                            current_i = (int)current_t;

                            current_p = current_t - current_i; // get float under point
                            Vector3 new_road_i_point = new Vector3(points_lib[pathes[road_i].ref_node[current_i]].position.x * current_p + points_lib[pathes[road_i].ref_node[current_i + 1]].position.x * (1 - current_p), 0.5f, points_lib[pathes[road_i].ref_node[current_i]].position.z * current_p + points_lib[pathes[road_i].ref_node[current_i + 1]].position.z * (1 - current_p));
                            Vector3 new_road_point = getMiddle(new_road_i_point, points_lib[pathes[road_j].ref_node[current_j]].position);
                            avenue.Add(new_road_point);

                            current_t += proportion;
                        }
                        avenue.Add(getMiddle(points_lib[pathes[road_i].ref_node[pathes[road_i].ref_node.Count - 1]].position, points_lib[pathes[road_j].ref_node[pathes[road_j].ref_node.Count - 1]].position));

                        string avenue_id = pathes[road_i].id + "_" + pathes[road_j].id;

                        List<string> avenue_newref = new List<string>();
                        // create new nodes to point_lib
                        //===================================================================
                        for (int avenue_point_index = 0; avenue_point_index < avenue.Count; avenue_point_index++)
                        {
                            bool need_add_linkref = false;
                       
                            if (avenue_point_index == 0)
                            {
                                int link_ref_h_index = linkRefIndexOf(pathes[road_i].ref_node[0], pathes[road_j].ref_node[0]);
                                if (link_ref_h_index == -1)
                                {
                                    need_add_linkref = true;
                                }
                                else
                                {
                                    avenue_newref.Add(avenue_links_to[link_ref_h_index]);
                                    points_lib[avenue_links_to[link_ref_h_index]].connect_way.Add(avenue_id);
                                    points_lib[avenue_links_ref[link_ref_h_index][0]].connect_way.Remove(pathes[road_i].id);
                                    points_lib[avenue_links_ref[link_ref_h_index][1]].connect_way.Remove(pathes[road_i].id);
                                    points_lib[avenue_links_ref[link_ref_h_index][0]].connect_way.Remove(pathes[road_j].id);
                                    points_lib[avenue_links_ref[link_ref_h_index][1]].connect_way.Remove(pathes[road_j].id);
                                }
                            }
                            else if (avenue_point_index == avenue.Count - 1)
                            {
                                int link_ref_t_index = linkRefIndexOf(pathes[road_i].ref_node[pathes[road_i].ref_node.Count - 1], pathes[road_j].ref_node[pathes[road_j].ref_node.Count - 1]);
                                if (link_ref_t_index == -1)
                                {
                                    need_add_linkref = true;
                                }
                                else
                                {
                                    avenue_newref.Add(avenue_links_to[link_ref_t_index]);
                                    points_lib[avenue_links_to[link_ref_t_index]].connect_way.Add(avenue_id);
                                    points_lib[avenue_links_ref[link_ref_t_index][0]].connect_way.Remove(pathes[road_i].id);
                                    points_lib[avenue_links_ref[link_ref_t_index][1]].connect_way.Remove(pathes[road_i].id);
                                    points_lib[avenue_links_ref[link_ref_t_index][0]].connect_way.Remove(pathes[road_j].id);
                                    points_lib[avenue_links_ref[link_ref_t_index][1]].connect_way.Remove(pathes[road_j].id);
                                }
                            }
                            else
                            {
                                string newnode_id = avenue_id + "+" + avenue_point_index;
                                Node node = new Node();
                                node.position = avenue[avenue_point_index];
                                node.connect_way.Add(avenue_id); // need to check all roads is without fork
                                avenue_newref.Add(newnode_id);
                                points_lib.Add(newnode_id, node);
                            }

                            if (need_add_linkref)
                            {
                                string newnode_id = avenue_id + "+" + avenue_point_index;
                                Node node = new Node();
                                node.position = avenue[avenue_point_index];
                                node.connect_way.Add(avenue_id); // need to check all roads is without fork
                                avenue_newref.Add(newnode_id);
                                points_lib.Add(newnode_id, node);

                                List<string> points_links_ref = new List<string>();
                                if (avenue_point_index == 0)
                                {
                                    points_links_ref.Add(pathes[road_i].ref_node[0]);
                                    points_links_ref.Add(pathes[road_j].ref_node[0]);
                                }
                                else
                                {
                                    points_links_ref.Add(pathes[road_i].ref_node[pathes[road_i].ref_node.Count - 1]);
                                    points_links_ref.Add(pathes[road_j].ref_node[pathes[road_j].ref_node.Count - 1]);
                                }
                                points_lib[points_links_ref[0]].connect_way.Remove(pathes[road_i].id);
                                points_lib[points_links_ref[1]].connect_way.Remove(pathes[road_i].id);
                                points_lib[points_links_ref[0]].connect_way.Remove(pathes[road_j].id);
                                points_lib[points_links_ref[1]].connect_way.Remove(pathes[road_j].id);

                                avenue_links_ref.Add(new List<string>(points_links_ref));
                                points_links_ref.Clear();
                                avenue_links_to.Add(newnode_id);
                            }
                        }
                        
                        // calculate intersection points
                        for (current_i = 1; current_i < pathes[road_i].ref_node.Count - 1; current_i++)
                        {
                            //=current_i=====================================================
                            List<string> relation_ways = new List<string>();
                            if (points_lib[pathes[road_i].ref_node[current_i]].includeAnotherWay(pathes[road_i].id, out relation_ways))
                            {
                                bool find_intersection = false;
                                int way_search = Way.findWayIndex(pathes, relation_ways[0]);
                                Vector3 r_w_s = Vector2.zero;
                                Vector3 r_w_e = Vector2.zero;

                                int contact_index = pathes[way_search].ref_node.IndexOf(pathes[road_i].ref_node[current_i]);
                                if (contact_index + 1 < pathes[way_search].ref_node.Count && points_lib[pathes[way_search].ref_node[contact_index + 1]].includePathID(pathes[road_j].id))
                                {
                                    r_w_s = points_lib[pathes[way_search].ref_node[contact_index]].position;
                                    r_w_e = points_lib[pathes[way_search].ref_node[contact_index + 1]].position;

                                    for (int avenue_point_index = 0; avenue_point_index < avenue.Count - 1; avenue_point_index++)
                                    {
                                        Vector3 a_p_s = points_lib[avenue_newref[avenue_point_index]].position;
                                        Vector3 a_p_e = points_lib[avenue_newref[avenue_point_index + 1]].position;
                                        if (isIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z)))
                                        {
                                            Vector2 inter_point2D = getIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z));

                                            Node node = new Node();
                                            node.position = new Vector3(inter_point2D.x, 0.5f, inter_point2D.y); // 0.5f is altitude
                                            node.connect_way.Add(avenue_id);
                                            for (int r_w_i = 0; r_w_i < relation_ways.Count; r_w_i++)
                                            {
                                                if (relation_ways[r_w_i] == pathes[road_i].id || relation_ways[r_w_i] == pathes[road_j].id)
                                                    continue;
                                                node.connect_way.Add(relation_ways[r_w_i]); // maybe put afterward avenue_links_ref combination
                                            }
                                            string node_id = avenue_id + "^" + pathes[road_i].ref_node[current_i]; // show the suggest point
                                            points_lib.Add(node_id, node);
                                            avenue_newref.Insert(avenue_point_index + 1, node_id);

                                            if (debug_mode)
                                                Debug.Log("great~: " + pathes[way_search].id + " X " + avenue_id);
                                            if (contact_index + 1 == pathes[way_search].ref_node.Count - 1 && points_lib[pathes[way_search].ref_node[contact_index + 1]].connect_way.Count <= 2) // need update tail
                                            {
                                                points_lib.Remove(pathes[way_search].ref_node[contact_index + 1]);
                                                pathes[way_search].ref_node[contact_index + 1] = node_id;
                                            }
                                            else
                                            {
                                                pathes[way_search].ref_node.Insert(contact_index + 1, node_id);
                                            }
                                            pathes[way_search].updateOrient(points_lib);

                                            find_intersection = true;
                                            break;
                                        }
                                    }
                                }
                                else if (contact_index - 1 >= 0 && points_lib[pathes[way_search].ref_node[contact_index - 1]].includePathID(pathes[road_j].id))
                                {
                                    r_w_s = points_lib[pathes[way_search].ref_node[contact_index]].position;
                                    r_w_e = points_lib[pathes[way_search].ref_node[contact_index - 1]].position;

                                    for (int avenue_point_index = 0; avenue_point_index < avenue.Count - 1; avenue_point_index++)
                                    {
                                        Vector3 a_p_s = points_lib[avenue_newref[avenue_point_index]].position;
                                        Vector3 a_p_e = points_lib[avenue_newref[avenue_point_index + 1]].position;
                                        if (isIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z)))
                                        {
                                            Vector2 inter_point2D = getIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z));

                                            Node node = new Node();
                                            node.position = new Vector3(inter_point2D.x, 0.5f, inter_point2D.y); // 0.5f is altitude
                                            node.connect_way.Add(avenue_id);
                                            for (int r_w_i = 0; r_w_i < relation_ways.Count; r_w_i++)
                                            {
                                                if (relation_ways[r_w_i] == pathes[road_i].id || relation_ways[r_w_i] == pathes[road_j].id)
                                                    continue;
                                                node.connect_way.Add(relation_ways[r_w_i]); // maybe put afterward avenue_links_ref combination
                                            }
                                            string node_id = avenue_id + "v" + pathes[road_i].ref_node[current_i]; // show the suggest point
                                            points_lib.Add(node_id, node);
                                            avenue_newref.Insert(avenue_point_index + 1, node_id);

                                            if (debug_mode)
                                                Debug.Log("great~: " + pathes[way_search].id + " X " + avenue_id);
                                            if (contact_index - 1 == 0 && points_lib[pathes[way_search].ref_node[contact_index - 1]].connect_way.Count <= 2) // need update head
                                            {
                                                points_lib.Remove(pathes[way_search].ref_node[contact_index - 1]);
                                                pathes[way_search].ref_node[contact_index - 1] = node_id;
                                            }
                                            else
                                            {
                                                pathes[way_search].ref_node.Insert(contact_index, node_id);
                                            }
                                            pathes[way_search].updateOrient(points_lib);

                                            find_intersection = true;
                                            break;
                                        }
                                    }
                                }

                                if (find_intersection)
                                    continue;

                                bool is_road_head = true;
                                Vector3 relation_point_orient = pathes[way_search].getOrient(pathes[road_i].ref_node[current_i], out is_road_head); // initial

                                if (relation_ways.Count > 1)
                                {
                                    // calc combine orient
                                    for (int r_w_i = 1; r_w_i < relation_ways.Count; r_w_i++)
                                    {
                                        way_search = Way.findWayIndex(pathes, relation_ways[r_w_i]);
                                        relation_point_orient += pathes[way_search].getOrient(pathes[road_i].ref_node[current_i], out is_road_head);
                                    }
                                    relation_point_orient = relation_point_orient.normalized;
                                    r_w_s = points_lib[pathes[road_i].ref_node[current_i]].position;
                                    r_w_e = points_lib[pathes[road_i].ref_node[current_i]].position + relation_point_orient * near_distance * 2;
                                    //====================================
                                    if (debug_mode)
                                        Debug.Log("Warning! There are above two relation road: " + pathes[road_i].id + " at point " + pathes[road_i].ref_node[current_i]);
                                }
                                else
                                {
                                    r_w_s = points_lib[pathes[road_i].ref_node[current_i]].position;
                                    r_w_e = points_lib[pathes[road_i].ref_node[current_i]].position + relation_point_orient * 24; // 12 avenue width
                                    if (debug_mode)
                                        Debug.Log("relation == 1: " + relation_ways[0] + " ~ pid:" + pathes[road_i].ref_node[current_i] + " " + points_lib[pathes[road_i].ref_node[current_i]].position + " ++ " + relation_point_orient + " = " + r_w_e);
                                }

                                for (int avenue_point_index = 0; avenue_point_index < avenue.Count - 1; avenue_point_index++)
                                {
                                    Vector3 a_p_s = points_lib[avenue_newref[avenue_point_index]].position;
                                    Vector3 a_p_e = points_lib[avenue_newref[avenue_point_index + 1]].position;
                                    if (isIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z)))
                                    {
                                        Vector2 inter_point2D = getIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z));

                                        Node node = new Node();
                                        node.position = new Vector3(inter_point2D.x, 0.5f, inter_point2D.y); // 0.5f is altitude
                                        node.connect_way.Add(avenue_id);
                                        for (int r_w_i = 0; r_w_i < relation_ways.Count; r_w_i++)
                                        {

                                            node.connect_way.Add(relation_ways[r_w_i]); // maybe put afterward avenue_links_ref combination
                                        }
                                        string node_id = avenue_id + "x" + pathes[road_i].ref_node[current_i]; // show the suggest point
                                        points_lib.Add(node_id, node);
                                        avenue_newref.Insert(avenue_point_index + 1, node_id);

                                        Way way_link = new Way();
                                        way_link.ref_node.Add(pathes[road_i].ref_node[current_i]);
                                        way_link.ref_node.Add(node_id);
                                        way_link.id = avenue_id + "_" + pathes[road_i].id + "_combinelink";
                                        way_link.name = pathes[road_i].name + "_link";
                                        way_link.is_merged = true;
                                        way_link.highway = Highway.CombineLink;
                                        pathes.Add(way_link);
                                        if (debug_mode)
                                            Debug.Log("great!: " + way_link.id);
                                        find_intersection = true;

                                        points_lib[pathes[road_i].ref_node[current_i]].connect_way.Remove(pathes[road_i].id);
                                        break;
                                    }
                                }

                                if (debug_mode && !find_intersection)
                                    Debug.Log("Not Found!");
                            }
                            else
                            {
                                points_lib.Remove(pathes[road_i].ref_node[current_i]);
                            }
                            //=current_i=====================================================
                        }

                        for (int current_j = 1; current_j < pathes[road_j].ref_node.Count - 1; current_j++)
                        {
                            //=current_j=====================================================
                            List<string> relation_ways = new List<string>();
                            if (points_lib.ContainsKey(pathes[road_j].ref_node[current_j]) && points_lib[pathes[road_j].ref_node[current_j]].includeAnotherWay(pathes[road_j].id, out relation_ways))
                            {
                                bool find_intersection = false;
                                int[] way_searchs = new int[relation_ways.Count];
                                int way_search = Way.findWayIndex(pathes, relation_ways[0]);
                                Vector3 r_w_s = Vector2.zero;
                                Vector3 r_w_e = Vector2.zero;

                                int contact_index = pathes[way_search].ref_node.IndexOf(pathes[road_j].ref_node[current_j]);
                                if (contact_index + 1 < pathes[way_search].ref_node.Count && points_lib[pathes[way_search].ref_node[contact_index + 1]].includePathID(pathes[road_i].id))
                                {
                                    r_w_s = points_lib[pathes[way_search].ref_node[contact_index]].position;
                                    r_w_e = points_lib[pathes[way_search].ref_node[contact_index + 1]].position;

                                    for (int avenue_point_index = 0; avenue_point_index < avenue.Count - 1; avenue_point_index++)
                                    {
                                        Vector3 a_p_s = points_lib[avenue_newref[avenue_point_index]].position;
                                        Vector3 a_p_e = points_lib[avenue_newref[avenue_point_index + 1]].position;
                                        if (isIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z)))
                                        {
                                            Vector2 inter_point2D = getIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z));

                                            Node node = new Node();
                                            node.position = new Vector3(inter_point2D.x, 0.5f, inter_point2D.y); // 0.5f is altitude
                                            node.connect_way.Add(avenue_id);
                                            for (int r_w_i = 0; r_w_i < relation_ways.Count; r_w_i++)
                                            {
                                                if (relation_ways[r_w_i] == pathes[road_i].id || relation_ways[r_w_i] == pathes[road_j].id)
                                                    continue;
                                                node.connect_way.Add(relation_ways[r_w_i]); // maybe put afterward avenue_links_ref combination
                                            }
                                            string node_id = avenue_id + "^" + pathes[road_j].ref_node[current_j]; // show the suggest point
                                            points_lib.Add(node_id, node);
                                            avenue_newref.Insert(avenue_point_index + 1, node_id);

                                            if (debug_mode)
                                                Debug.Log("great~: " + pathes[way_search].id + " X " + avenue_id);
                                            if (contact_index + 1 == pathes[way_search].ref_node.Count - 1 && points_lib[pathes[way_search].ref_node[contact_index + 1]].connect_way.Count <= 2) // need update tail
                                            {
                                                points_lib.Remove(pathes[way_search].ref_node[contact_index + 1]);
                                                pathes[way_search].ref_node[contact_index + 1] = node_id;
                                            }
                                            else
                                            {
                                                pathes[way_search].ref_node.Insert(contact_index + 1, node_id);
                                            }
                                            pathes[way_search].updateOrient(points_lib);

                                            find_intersection = true;
                                            break;
                                        }
                                    }
                                }
                                else if (contact_index - 1 >= 0 && points_lib[pathes[way_search].ref_node[contact_index - 1]].includePathID(pathes[road_i].id))
                                {
                                    r_w_s = points_lib[pathes[way_search].ref_node[contact_index]].position;
                                    r_w_e = points_lib[pathes[way_search].ref_node[contact_index - 1]].position;

                                    for (int avenue_point_index = 0; avenue_point_index < avenue.Count - 1; avenue_point_index++)
                                    {
                                        Vector3 a_p_s = points_lib[avenue_newref[avenue_point_index]].position;
                                        Vector3 a_p_e = points_lib[avenue_newref[avenue_point_index + 1]].position;
                                        if (isIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z)))
                                        {
                                            Vector2 inter_point2D = getIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z));

                                            Node node = new Node();
                                            node.position = new Vector3(inter_point2D.x, 0.5f, inter_point2D.y); // 0.5f is altitude
                                            node.connect_way.Add(avenue_id);
                                            for (int r_w_i = 0; r_w_i < relation_ways.Count; r_w_i++)
                                            {
                                                if (relation_ways[r_w_i] == pathes[road_i].id || relation_ways[r_w_i] == pathes[road_j].id)
                                                    continue;
                                                node.connect_way.Add(relation_ways[r_w_i]); // maybe put afterward avenue_links_ref combination
                                            }
                                            string node_id = avenue_id + "v" + pathes[road_j].ref_node[current_j]; // show the suggest point
                                            points_lib.Add(node_id, node);
                                            avenue_newref.Insert(avenue_point_index + 1, node_id);

                                            if (debug_mode)
                                                Debug.Log("great~: " + pathes[way_search].id + " X " + avenue_id);
                                            if (contact_index - 1 == 0 && points_lib[pathes[way_search].ref_node[contact_index - 1]].connect_way.Count <= 2) // need update head
                                            {
                                                points_lib.Remove(pathes[way_search].ref_node[contact_index - 1]);
                                                pathes[way_search].ref_node[contact_index - 1] = node_id;
                                            }
                                            else
                                            {
                                                pathes[way_search].ref_node.Insert(contact_index, node_id);
                                            }
                                            pathes[way_search].updateOrient(points_lib);

                                            find_intersection = true;
                                            break;
                                        }
                                    }
                                }

                                if (find_intersection)
                                    continue;

                                bool is_road_head = true;
                                Vector3 relation_point_orient = pathes[way_search].getOrient(pathes[road_j].ref_node[current_j], out is_road_head); // initial

                                if (relation_ways.Count > 1)
                                {
                                    // need to calc combine orient
                                    for (int r_w_i = 1; r_w_i < relation_ways.Count; r_w_i++)
                                    {
                                        way_search = Way.findWayIndex(pathes, relation_ways[r_w_i]);
                                        relation_point_orient += pathes[way_search].getOrient(pathes[road_j].ref_node[current_j], out is_road_head);
                                    }
                                    relation_point_orient = relation_point_orient.normalized;
                                    r_w_s = points_lib[pathes[road_j].ref_node[current_j]].position;
                                    r_w_e = points_lib[pathes[road_j].ref_node[current_j]].position + relation_point_orient * near_distance * 2;
                                    //====================================
                                    if (debug_mode)
                                        Debug.Log("Warning! There are above two relation road: " + pathes[road_j].id + " at point " + pathes[road_j].ref_node[current_j]);
                                }
                                else
                                {
                                    r_w_s = points_lib[pathes[road_j].ref_node[current_j]].position;
                                    r_w_e = points_lib[pathes[road_j].ref_node[current_j]].position + relation_point_orient * 48; // 12 avenue width
                                    if (debug_mode)
                                        Debug.Log("relation == 1: " + relation_ways[0] + " ~ pid:" + pathes[road_j].ref_node[current_j] + " " + points_lib[pathes[road_j].ref_node[current_j]].position + " ++ " + relation_point_orient + " = " + r_w_e);
                                }

                                for (int avenue_point_index = 0; avenue_point_index < avenue.Count - 1; avenue_point_index++)
                                {
                                    Vector3 a_p_s = points_lib[avenue_newref[avenue_point_index]].position;
                                    Vector3 a_p_e = points_lib[avenue_newref[avenue_point_index + 1]].position;
                                    if (isIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z)))
                                    {
                                        Vector2 inter_point2D = getIntersection(new Vector2(a_p_s.x, a_p_s.z), new Vector2(a_p_e.x, a_p_e.z), new Vector2(r_w_s.x, r_w_s.z), new Vector2(r_w_e.x, r_w_e.z));

                                        Node node = new Node();
                                        node.position = new Vector3(inter_point2D.x, 0.5f, inter_point2D.y); // 0.5f is altitude
                                        node.connect_way.Add(avenue_id);
                                        for (int r_w_i = 0; r_w_i < relation_ways.Count; r_w_i++)
                                        {
                                            if (relation_ways[r_w_i] == pathes[road_i].id || relation_ways[r_w_i] == pathes[road_j].id)
                                                continue;
                                            node.connect_way.Add(relation_ways[r_w_i]); // maybe put afterward avenue_links_ref combination
                                        }
                                        string node_id = avenue_id + "x" + pathes[road_j].ref_node[current_j]; // show the suggest point
                                        points_lib.Add(node_id, node);
                                        avenue_newref.Insert(avenue_point_index + 1, node_id);

                                        Way way_link = new Way();
                                        way_link.ref_node.Add(pathes[road_j].ref_node[current_j]);
                                        way_link.ref_node.Add(node_id);
                                        way_link.id = avenue_id + "_" + pathes[road_j].id + "_combinelink";
                                        way_link.name = pathes[road_j].name + "_link";
                                        way_link.is_merged = true;
                                        way_link.highway = Highway.CombineLink;
                                        pathes.Add(way_link);
                                        if (debug_mode)
                                            Debug.Log("great!: " + way_link.id);
                                        find_intersection = true;

                                        points_lib[pathes[road_j].ref_node[current_j]].connect_way.Remove(pathes[road_j].id);
                                        break;
                                    }
                                }

                                if (debug_mode && !find_intersection)
                                    Debug.Log("Not Found!");
                            }
                            else
                            {
                                points_lib.Remove(pathes[road_j].ref_node[current_j]);
                            }
                            //=current_j=====================================================
                        }
                        //===================================================================

                        Way way = new Way();
                        way.ref_node = new List<string>(avenue_newref);
                        way.id = avenue_id;
                        way.name = pathes[road_i].name;
                        way.is_merged = true;
                        way.highway = pathes[road_i].highway;
                        way.road_width = merged_road_width;
                        pathes.Add(way);

                        delete_pathes.Add(pathes[road_i].id);
                        delete_pathes.Add(pathes[road_j].id);
                        // road_i < road_j
                        pathes.RemoveAt(road_j);
                        pathes.RemoveAt(road_i);
                    }
                }
            }
        }

        // Combine two points, and put thier info to new point
        for (int link_to_index = 0; link_to_index < avenue_links_to.Count; link_to_index++)
        {
            for (int merged_point_index = 0; merged_point_index < 2; merged_point_index++)
            {
                for (int connect_way_index = 0; connect_way_index < points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way.Count; connect_way_index++)
                {
                    if (delete_pathes.Contains(points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way[connect_way_index])) //在刪除路的名單內 continue
                        continue;
                    // not merge road and not merged road
                    if (!points_lib[avenue_links_to[link_to_index]].connect_way.Contains(points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way[connect_way_index]))
                        points_lib[avenue_links_to[link_to_index]].connect_way.Add(points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way[connect_way_index]);

                    int search_way = Way.findWayIndex(pathes, points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way[connect_way_index]);
                    int ref_0_index = pathes[search_way].ref_node.IndexOf(avenue_links_ref[link_to_index][0]);
                    int ref_1_index = pathes[search_way].ref_node.IndexOf(avenue_links_ref[link_to_index][1]);
                    if (!pathes[search_way].ref_node.Contains(avenue_links_to[link_to_index]) && ref_0_index != -1 && ref_1_index != -1 && Mathf.Abs(ref_0_index - ref_1_index) > 1) // merged road head combine case 2
                    {
                        // temporary code which need adjust ************************************************************************************************************************
                        int middle_index = ref_1_index - (ref_1_index - ref_0_index) / 2;
                        pathes[search_way].ref_node.Insert(middle_index, avenue_links_to[link_to_index]);
                        // *********************************************************************************************************************************************************
                    }
                    else // merged road head combine case 1
                    {
                        if (pathes[search_way].ref_node.Contains(avenue_links_to[link_to_index])) // do second in 2 points
                        {
                            pathes[search_way].ref_node.Remove(avenue_links_ref[link_to_index][merged_point_index]);
                            if (pathes[search_way].ref_node.Contains(avenue_links_ref[link_to_index][merged_point_index])) // remove circle road another same point
                                pathes[search_way].ref_node.Remove(avenue_links_ref[link_to_index][merged_point_index]);
                            while (points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way.Contains(pathes[search_way].id))
                            {
                                points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way.Remove(pathes[search_way].id);
                                connect_way_index--; // correct in reducing amount of points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way in for
                            }
                        }
                        else // do first in 2 points
                        {
                            int ref_node_index = pathes[search_way].ref_node.IndexOf(avenue_links_ref[link_to_index][merged_point_index]);
                            pathes[search_way].ref_node[ref_node_index] = avenue_links_to[link_to_index];
                            if (pathes[search_way].ref_node.Contains(avenue_links_ref[link_to_index][merged_point_index])) // remove circle road another same point
                                pathes[search_way].ref_node.Remove(avenue_links_ref[link_to_index][merged_point_index]);
                            while (points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way.Contains(pathes[search_way].id))
                            {
                                points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way.Remove(pathes[search_way].id);
                                connect_way_index--; // correct in reducing amount of points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way in for
                            }
                            pathes[search_way].updateOrient(points_lib);
                        }
                    }
                }
            }
        }

        for (int link_to_index = 0; link_to_index < avenue_links_to.Count; link_to_index++)
        {
            for (int merged_point_index = 0; merged_point_index < 2; merged_point_index++)
            {
                if (points_lib[avenue_links_ref[link_to_index][merged_point_index]].connect_way.Count == 0)
                {
                    points_lib.Remove(avenue_links_ref[link_to_index][merged_point_index]);
                }
            }
        }
    }

    private bool isNearedPoint(Vector3 pos_i, Vector3 pos_j)
    {
        return Vector3.Distance(pos_j, pos_i) < near_distance;
    }

    private Vector3 getMiddle(Vector3 pos_i, Vector3 pos_j)
    {
        return new Vector3((pos_i.x + pos_j.x) / 2, 0.5f, (pos_i.z + pos_j.z) / 2);
    }

    private bool checkConnectRoad(List<string> connect_i, List<string> connect_j)
    {
        for (int i = 0; i < connect_i.Count; i++)
        {
            for (int j = 0; j < connect_j.Count; j++)
            {
                if (connect_i[i] == connect_j[j]) // same way id
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<Vector2> getRoadPolygon(string road_id)
    {
        for (int road_index = 0; road_index < pathes.Count; road_index++)
        {
            if (pathes[road_index].id == road_id)
            {
                return getRoadPolygon(road_index);
            }
        }

        Debug.Log("Not found the road!");
        return new List<Vector2>();
    }

    public List<Vector2> getRoadPolygon(int road_index)
    {
        List<Vector2> vertex2D = new List<Vector2>();
        for (int point_index = 0; point_index < pathes[road_index].ref_node.Count; point_index++)
        {
            vertex2D.Add(new Vector2(points_lib[pathes[road_index].ref_node[point_index]].position.x, points_lib[pathes[road_index].ref_node[point_index]].position.z));
        }

        return vertex2D;
    }

    public List<Vector2> getHousePolygon(string house_id)
    {
        int house_index = houses_id.IndexOf(house_id);
        if (house_index == -1)
        {
            Debug.Log("Not found the house!");
            return new List<Vector2>();
        }

        return getHousePolygon(house_index);
    }

    public List<Vector2> getHousePolygon(int house_index)
    {
        List<Vector2> vertex2D = new List<Vector2>();
        for (int point_index = 0; point_index < houses[house_index].Count - 1; point_index++)
        {
            vertex2D.Add(new Vector2(points_lib[houses[house_index][point_index]].position.x, points_lib[houses[house_index][point_index]].position.z));
        }

        return vertex2D;
    }

    public List<Vector3> toPositions(List<string> ref_nodes)
    {
        List<Vector3> vectors = new List<Vector3>();
        for (int index = 0; index < ref_nodes.Count; index++)
        {
            vectors.Add(points_lib[ref_nodes[index]].position);
        }
        return vectors;
    }

    /// <summary>
    /// Gets the coordinates of the intersection point of two lines.
    /// </summary>
    /// <param name="A1">A point on the first line.</param>
    /// <param name="A2">Another point on the first line.</param>
    /// <param name="B1">A point on the second line.</param>
    /// <param name="B2">Another point on the second line.</param>
    /// <param name="found">Is set to false of there are no solution. true otherwise.</param>
    /// <returns>The intersection point coordinates. Returns Vector2.zero if there is no solution.</returns>
    public Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        if (tmp == 0)
        {
            // No solution!
            found = false;
            return Vector2.zero;
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        found = true;

        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }

    int Point_Side(Vector2 line_s, Vector2 line_e, Vector2 point)
    {
        // Compute the determinant: | xs ys 1 |
        //                          | xe ye 1 |
        //                          | x  y  1 |
        // Use its sign to get the answer.

        float det;

        det = line_s.x *
                (line_e.y - point.y) -
                line_s.y *
                (line_e.x - point.x) +
                line_e.x * point.y -
                line_e.y * point.x;

        if (det == 0.0)
            return 0; // ON
        else if (det > 0.0)
            return -1; // LEFT
        else
            return 1; // RIGHT
    }

    float cross_dir(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    float cross_dir(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    bool isIntersection(Vector2 a_s, Vector2 a_e, Vector2 b_s, Vector2 b_e)
    {
        float c1 = cross_dir(a_s, a_e, b_s);
        float c2 = cross_dir(a_s, a_e, b_e);
        float c3 = cross_dir(b_s, b_e, a_s);
        float c4 = cross_dir(b_s, b_e, a_e);

        // 端點不共線
        return (c1 * c2 < 0 && c3 * c4 < 0);
    }

    Vector2 getIntersection(Vector2 a_s, Vector2 a_e, Vector2 b_s, Vector2 b_e)
    {
        Vector2 a = a_e - a_s, b = b_e - b_s, s = b_s - a_s;

        // 兩線平行，交點不存在。
        // 兩線重疊，交點無限多。
        if (cross_dir(a, b) == 0) return Vector2.positiveInfinity;

        // 計算交點
        return a_s + a * cross_dir(s, b) / cross_dir(a, b);
    }

    private int linkRefIndexOf(string id1, string id2)
    {
        for (int index = 0; index < avenue_links_ref.Count; index++)
        {
            if ((avenue_links_ref[index][0] == id1 && avenue_links_ref[index][1] == id2) || (avenue_links_ref[index][0] == id2 && avenue_links_ref[index][1] == id1))
            {
                return index;
            }
        }
        return -1;
    }
}