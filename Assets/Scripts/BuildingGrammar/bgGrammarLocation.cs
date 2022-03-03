using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class bgGrammarLocation
{
    public string file_path;
    public int start_line;
    public int end_line;
    public bgGrammarLocation(string _file_path,int _start_line,int _end_line) {
        file_path = _file_path;
        start_line = _start_line;
        end_line = _end_line;
    }
    public List<string> get_lines() {
        List<string> all_lines = File.ReadAllLines(file_path).ToList().GetRange(start_line,end_line-start_line+1);
        return all_lines;
    }
}
