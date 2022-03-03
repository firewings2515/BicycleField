using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgBuilder : MonoBehaviour
{
    public string build_component;
    public string[] grammar_files_path;

    List<bgAsset> assets;
    List<bgWall> walls;
    List<bgFacade> facades;
    List<bgBase> bases;
    List<bgBuilding> buildings;

    List<bgComponent> components;
    bgParser parser;
    private GameObject building;
    // Start is called before the first frame update
    void Start()
    {
        parser = new bgParser();
        components = new List<bgComponent>();

        parser.parse(grammar_files_path);

        components.AddRange(parser.assets);
        components.AddRange(parser.walls);
        components.AddRange(parser.facades);
        components.AddRange(parser.bases);
        components.AddRange(parser.buildings);

        link_component();

        building = build();
    }
    void link_component() {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].builder = this;
        }
    }

    GameObject build() {
        Debug.Log("-----------------build------------");
        for (int i = 0; i < components.Count; i++) {
            if (components[i].name == build_component) {
                return components[i].build();
            }
        }
        return null;
    }

    public bgComponent get_component(string name) {
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].name == build_component)
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
