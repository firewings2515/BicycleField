using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgBuilder
{

    List<bgAsset> assets;
    List<bgWall> walls;
    List<bgFacade> facades;
    List<bgBase> bases;
    List<bgRoof> roofs;
    List<bgBalcony> balconys;
    List<bgBuilding> buildings;

    List<bgComponent> components;
    bgParser parser;

    public bgBuilder(){
        parser = new bgParser();
        components = new List<bgComponent>();
    }
    public bgBuilder(string[] grammar_files_path)
    {
        parser = new bgParser();
        components = new List<bgComponent>();
        compile_code(grammar_files_path);
    }

    // Start is called before the first frame update
    public void compile_code(string[] grammar_files_path)
    {


        parser.parse(grammar_files_path);

        components.AddRange(parser.assets);
        components.AddRange(parser.walls);
        components.AddRange(parser.facades);
        components.AddRange(parser.bases);
        components.AddRange(parser.roofs);
        components.AddRange(parser.balconys);
        components.AddRange(parser.buildings);

        link_component();

        //building = build();
    }
    void link_component() {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].builder = this;
        }
    }

    public GameObject build(string name) {
        //Debug.Log("-----------------build------------");
        for (int i = 0; i < components.Count; i++) {
            if (components[i].name == name) {
                return components[i].build();
            }
        }
        return null;
    }
    public Mesh build_mesh(string name)
    {
        //Debug.Log("-----------------build------------");
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].name == name)
            {
                return components[i].build_mesh();
            }
        }
        return null;
    }

    public bgComponent get_component(string name) {
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].name == name)
            {
                return components[i];
            }
        }
        return null;
    }

    public bgAsset get_asset(string name)
    {
        for (int i = 0; i < parser.assets.Count; i++)
        {
            if (parser.assets[i].name == name)
            {
                return parser.assets[i];
            }
        }
        return null;
    }

    public bgWall get_wall(string name)
    {
        for (int i = 0; i < parser.walls.Count; i++)
        {
            if (parser.walls[i].name == name)
            {
                return parser.walls[i];
            }
        }
        return null;
    }

    public bgFacade get_facade(string name)
    {
        for (int i = 0; i < parser.facades.Count; i++)
        {
            if (parser.facades[i].name == name)
            {
                return parser.facades[i];
            }
        }
        return null;
    }

    public bgBase get_base(string name)
    {
        for (int i = 0; i < parser.bases.Count; i++)
        {
            if (parser.bases[i].name == name)
            {
                return parser.bases[i];
            }
        }
        return null;
    }

    public bgRoof get_roof(string name)
    {
        for (int i = 0; i < parser.roofs.Count; i++)
        {
            if (parser.roofs[i].name == name)
            {
                return parser.roofs[i];
            }
        }
        return null;
    }


    public bgBalcony get_balcony(string name)
    {
        for (int i = 0; i < parser.balconys.Count; i++)
        {
            if (parser.balconys[i].name == name)
            {
                return parser.balconys[i];
            }
        }
        return null;
    }


    public bgBuilding get_building(string name)
    {
        for (int i = 0; i < parser.buildings.Count; i++)
        {
            if (parser.buildings[i].name == name)
            {
                return parser.buildings[i];
            }
        }
        return null;
    }

    
    public void clear() {
        parser.clear();
        components.Clear();
    }


    public void load_base_coords(string name, List<Vector3> vertexs) {
        for (int i = 0; i < parser.bases.Count; i++)
        {
            if (parser.bases[i].name == name)
            {
                parser.bases[i].vertexs = vertexs;
            }
        }
    }

    public void load_base_facades(string name, List<List<string>> component_names)
    {
        for (int i = 0; i < parser.bases.Count; i++)
        {
            if (parser.bases[i].name == name)
            {
                parser.bases[i].facades = new List<List<bgFacade>>();
                for (int j = 0; j < component_names.Count; j++) {
                    parser.bases[i].facades.Add(new List<bgFacade>());
                    for (int k = 0; k < component_names[j].Count; k++)
                    {
                        bgFacade facade = get_facade(component_names[j][k]);
                        parser.bases[i].facades[j].Add(facade);
                    }
                }
            }
        }
    }
}
