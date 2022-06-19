using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

public class bgParser
{
    public string[] grammar_files_path;
    List<int> file_start_line;

    public List<bgAsset> assets;
    public List<bgWall> walls;
    public List<bgFacade> facades;
    public List<bgBase> bases;
    public List<bgRoof> roofs;
    public List<bgBalcony> balconys;
    public List<bgBuilding> buildings;

    public bgParser() {
        assets = new List<bgAsset>();
        walls = new List<bgWall>();
        facades = new List<bgFacade>();
        bases = new List<bgBase>();
        roofs = new List<bgRoof>();
        balconys = new List<bgBalcony>();
        buildings = new List<bgBuilding>();
        file_start_line = new List<int>();
    }

    // Start is called before the first frame update
    public void parse(string[] _grammar_files_path)
    {
        grammar_files_path = _grammar_files_path;

        List<string> all_lines = new List<string>();

        List<bgGrammarLocation> locations = new List<bgGrammarLocation>();

        for (int i = 0; i < grammar_files_path.Length; i++)
        {
            file_start_line.Add(all_lines.Count);
            all_lines.AddRange(File.ReadAllLines(grammar_files_path[i]).ToList());            
        }

        int tmp_start_line = 0;

        bool get_component_name = false;
        bool get_input_parameter = false;
        bool get_comma = false;
        bool get_component_type = false;
        bool get_component_parameter = false;
        bool reading_parameters = false; // inside little bracket
        bool reading_component_code = false; // inside big bracket

        List<string> componet_names = new List<string>();
        List<string> componet_types = new List<string>();

        List<List<string>> input_parameters = new List<List<string>>();
        List<List<string>> componet_parameters = new List<List<string>>();

        List<List<string>> commands = new List<List<string>>();
        List<List<List<string>>> command_parameters = new List<List<List<string>>>();

        for (int i = 0; i < all_lines.Count; i++) {
            string line = all_lines[i];
            StringBuilder word = new StringBuilder();
            bool reading_word = false;
            bool pre_reading_word = false;

            bool get_command = false;

            for (int j = 0; j < line.Length; j++) {
                if (line[j] == ' ' || line[j] == '\t')
                {
                    reading_word = false;
                }
                else if (char.IsLetterOrDigit(line[j]) || line[j] == '_' || line[j] == '.' || line[j] == '-')
                {
                    reading_word = true;
                    word.Append(line[j]);
                }
                else if (line[j] == ':')
                {
                    get_comma = true;
                }
                else if (line[j] == '(')
                {
                    reading_word = false;
                    reading_parameters = true;

                    if (get_input_parameter == true && get_component_parameter == true)
                    {
                        command_parameters.Last().Add(new List<string>());

                    }
                }
                else if (line[j] == '"')
                {
                    j++;
                    while (line[j] != '"')
                    {
                        word.Append(line[j]);
                        j++;
                    }
                    //Debug.Log(word.ToString());
                    command_parameters.Last().Last().Add(word.ToString());
                    word.Clear();
                    continue;
                }
                else if (line[j] == ',')
                {
                    reading_word = false;
                }
                else if (line[j] == ')')
                {
                    reading_word = false;

                    

                    if (get_input_parameter == false)
                    {
                        get_input_parameter = true;
                        if (word.Length > 0)
                        {
                            input_parameters.Last().Add(word.ToString());
                            //Debug.Log("get parameter :" + word.ToString());
                        }

                    }
                    else if (get_component_parameter == false)
                    {
                        get_component_parameter = true;
                        if (word.Length > 0)
                        {
                            componet_parameters.Last().Add(word.ToString());
                            //Debug.Log("get parameter :" + word.ToString());
                        }

                    }
                    else
                    {
                        get_command = false;
                        if (word.Length > 0)
                        {
                            command_parameters.Last().Last().Add(word.ToString());
                            //Debug.Log("get parameter :" + word.ToString());
                        }

                    }
                    word.Clear();
                    reading_parameters = false;
                    continue;
                }
                else if (line[j] == '{')
                {
                    commands.Add(new List<string>());
                    command_parameters.Add(new List<List<string>>());
                    reading_word = false;
                    reading_parameters = false;
                    reading_component_code = true;

                    tmp_start_line = i + 1;
                    continue;
                }
                else if (line[j] == '}')
                {
                    reading_component_code = false;


                    //reset
                    get_component_name = false;
                    get_input_parameter = false;
                    get_comma = false;
                    get_component_type = false;
                    get_component_parameter = false;
                    reading_parameters = false;


                    locations.Add(get_location(tmp_start_line, i-1));
                    continue;
                }
                


                if (pre_reading_word == true && reading_word == false) {
                    if (get_component_name == false)
                    {
                        get_component_name = true;
                        componet_names.Add(word.ToString());
                        //Debug.Log("get component name :" + componet_names.Last());
                        input_parameters.Add(new List<string>());
                    }
                    else if (get_component_type == false)
                    {
                        get_component_type = true;
                        componet_types.Add(word.ToString());
                        //Debug.Log("get component type :" + componet_types.Last());
                        componet_parameters.Add(new List<string>());
                    }
                    else if (reading_component_code && get_command == false)
                    {
                        get_command = true;
                        //Debug.Log("get command :" + word.ToString());
                        commands.Last().Add(word.ToString());

                    }
                    else if (reading_parameters) {
                        //Debug.Log("get parameter :" + word.ToString());
                        if (get_input_parameter == false)
                        {
                            input_parameters.Last().Add(word.ToString());

                        }
                        else if (get_component_parameter == false)
                        {
                            componet_parameters.Last().Add(word.ToString());
                        }
                        else {
                            command_parameters.Last().Last().Add(word.ToString());

                        }
                    }


                    word.Clear();
                }

                
                pre_reading_word = reading_word;
            }
        }

        //Debug.Log("=========lexical finished=========");
        //components = new List<bgComponent>();

        for (int component_index = 0; component_index < componet_names.Count; component_index++)
        {
            //Debug.Log("name: " + componet_names[component_index]);
            for (int i = 0; i < input_parameters[component_index].Count; i++)
            {
                //Debug.Log("\tinput parameter:" + input_parameters[component_index][i]);
            }
            //Debug.Log("type: " + componet_types[component_index]);
            for (int i = 0; i < componet_parameters[component_index].Count; i++)
            {
                //Debug.Log("\tcomponet parameter:" + componet_parameters[component_index][i]);
            }

            //Debug.Log("commands:");
            for (int i = 0; i < commands[component_index].Count; i++)
            {
                //Debug.Log("\tcommand:" + commands[component_index][i]);
                for (int j = 0; j < command_parameters[component_index][i].Count; j++)
                {
                    //Debug.Log("\t\tcommand parameter:" + command_parameters[component_index][i][j]);
                }
            }
            bgComponent comp = new bgComponent();
            
            if (componet_types[component_index] == "Asset")
            {
                assets.Add(new bgAsset(input_parameters[component_index], componet_parameters[component_index], commands[component_index], command_parameters[component_index]));
                comp = assets.Last();
            }
            else if (componet_types[component_index] == "Wall")
            {
                walls.Add(new bgWall(input_parameters[component_index], componet_parameters[component_index], commands[component_index], command_parameters[component_index]));
                comp = walls.Last();
            }
            else if (componet_types[component_index] == "Facade")
            {
                facades.Add(new bgFacade(input_parameters[component_index], componet_parameters[component_index], commands[component_index], command_parameters[component_index]));
                comp = facades.Last();
            }
            else if (componet_types[component_index] == "Base")
            {
                bases.Add(new bgBase(input_parameters[component_index], componet_parameters[component_index], commands[component_index], command_parameters[component_index]));
                comp = bases.Last();
            }

            else if (componet_types[component_index] == "Roof")
            {
                roofs.Add(new bgRoof(input_parameters[component_index], componet_parameters[component_index], commands[component_index], command_parameters[component_index]));
                comp = roofs.Last();
            }
            else if (componet_types[component_index] == "Balcony")
            {
                balconys.Add(new bgBalcony(input_parameters[component_index], componet_parameters[component_index], commands[component_index], command_parameters[component_index]));
                comp = balconys.Last();
            }
            else if (componet_types[component_index] == "Building")
            {
                buildings.Add(new bgBuilding(input_parameters[component_index], componet_parameters[component_index], commands[component_index], command_parameters[component_index]));
                comp = buildings.Last();
            }
            

            comp.source_code = locations[component_index];
            comp.name = componet_names[component_index];
            comp.type = componet_types[component_index];
        }
        ////Debug.Log("=========check finished=========");
        /*
        for (int component_index = 0; component_index < componet_names.Count; component_index++)
        {
            if (componet_types[component_index] == "Asset")
            {
                ((bgAsset)components[component_index]).build();
            }
            else if (componet_types[component_index] == "Wall")
            {
                ((bgWall)components[component_index]).build();
            }
            else if (componet_types[component_index] == "Facade")
            {
                ((bgFacade)components[component_index]).build();
            }
            else if (componet_types[component_index] == "Base")
            {
                ((bgBase)components[component_index]).build();
            }
            else if (componet_types[component_index] == "Building")
            {
                ((bgBuilding)components[component_index]).build();
            }
        }
        */
    }




    bgGrammarLocation get_location(int start_line,int end_line) {
        int file_index = get_file_index(start_line);
        if (file_index != -1) {
            return new bgGrammarLocation(grammar_files_path[file_index], start_line - file_start_line[file_index], end_line - file_start_line[file_index]);
        }
        return null;
    }
    int get_file_index(int line) {
        for (int i = 0; i < file_start_line.Count; i++) {
            if (line >= file_start_line[i]) {
                return i;
            }
        }
        return -1;
    }

    public void clear() {
        file_start_line.Clear();
        assets.Clear();
        walls.Clear();
        facades.Clear();
        bases.Clear();
        roofs.Clear();
        balconys.Clear();
        buildings.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
