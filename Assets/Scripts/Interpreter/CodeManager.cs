﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeManager : MonoBehaviour {
    /*     
    public List<string> integers_within_editting_scope = new List<string>();
    public List<string> doubles_within_editting_scope = new List<string>();
    public List<string> booleans_within_editting_scope = new List<string>();
    public List<string> strings_within_editting_scope = new List<string>();
    string[] available_variable_types;
    public List<string> lines;
    string[] list_of_PDTs = { "boolean", "char", "double", "int", "String" };
    string[] list_of_evaluations = { "==", ">", "<", "<=", ">=", "!=" };
    string[] list_of_operations = { " + ", " - ", " * ", " / ", " % "};
    string[] list_of_condensed_operations = { "=", "++", "--", "+=", "-=", "*=", "/=", "%=" };
    public List<VariableObject<bool>> booleans = new List<VariableObject<bool>>();
    public List<VariableObject<int>> integers = new List<VariableObject<int>>();
    public List<VariableObject<double>> doubles = new List<VariableObject<double>>();
    public List<VariableObject<string>> strings = new List<VariableObject<string>>();
    */

    public void Compiler_compile(List<string> lines_in)
    {
       //for (current_line = 0; current_line < lines_in.Count; current_line++)
        {
            //Introduce delay for a "penalty" of large codes, increase speed of code by level of computer
           // Compiler_compile(lines_in[current_line]);
        }
    }
    /* Parsing each line of text into code (a.k.a. where the magic happens) */
    public void Compiler_compile(string line)
    {
        //Master Switch
        switch (line)
        {
            case "{":
            case "}":
                //scope control
                break;
            case "if":
            case "else":
            case "for":
            case "while":
                //flow control
                break;
            case "break":
            case "continue":
                //scope-flow control?
                break;
            default:
                switch (line)
                {
                    case "isFunction":
                        break;
                    case "isVariable":
                        break;                    
                    //function call?
                    //variable modification?
                }
                break;
        }


        // print(line);
       /* if (line.IndexOf("while (") == 0)
        {
            string condition = ("(" + Parser_splitStringBetween(line, "(", ")") + ")");
            //  print(condition);
            if (Compiler_booleanEvaluation(condition))
            {
                tasks.Push(new TaskObject(current_line, Compiler_findMatchingBracket(current_line), condition, ""));
            }
            else
            {
                current_line = Compiler_findNextInstanceOf(current_line, "}");
            }
        }
        if (line.IndexOf("for (") == 0)
        {
            Compiler_addVariable(Parser_splitStringBetween(line, "(", ";") + ";");
            string condition = ("(" + Parser_splitStringBetween(line, "; ", ";") + ")");
            if (Compiler_booleanEvaluation(condition))
            {
                tasks.Push(new TaskObject(current_line, Compiler_findMatchingBracket(current_line), condition, (Parser_splitStringBetween(line, Parser_splitStringBetween(line, "; ", ";"), ")") + ";").Substring(2)));
            }
            else
            {
                current_line = Compiler_findNextInstanceOf(current_line, "}");
            }
        }
        if (tasks.Count != 0)
        {
            if (current_line == tasks.Peek().ending_line)
            {
                current_line = Compiler_doEndOfTask(tasks.Peek());
                if (current_line == tasks.Peek().ending_line) tasks.Pop();
            }
        }

        if (line.IndexOf("if (") == 0)
        {
            next_else = false; //Used to know if an "else" is linked to an if/will be executed           
            if (!Compiler_booleanEvaluation(line))
            {
                //Jump past If Statement
                current_line = Compiler_findMatchingBracket(current_line);//Compiler_findNextInstanceOf(current_line, "}");
                //Enter next else if available
                next_else = true;
            }
        }
        if (line.IndexOf("else") == 0)
        {
            if (next_else == false)
            {
                current_line = Compiler_findMatchingBracket(current_line);
            }

        }

        //Variable statements
        for (int types_of_PDTs = 0; types_of_PDTs < list_of_PDTs.Length; types_of_PDTs++)
        {
            //DECLARING A NEW VARIABLE OF TYPE (list_of_PDTs[types_of_PDTs])
            if (line.IndexOf(list_of_PDTs[types_of_PDTs] + " ") == 0)
            {
                Compiler_addVariable(line);
            }
        }
        //Looks for a simple statement that is modifying variables
        Compiler_modifyVariable(line);

        if (line.IndexOf("System.out.print") == 0)
        {
            string output = Parser_splitStringBetween(line, "(", ")");
            print(output);
            output = Compiler_evaluateExpression(output);
            print(output);
            if (line.IndexOf("System.out.println") == 0)
            {
                Display_pushConsoleText(output + "\n");
            }
            else Display_pushConsoleText(output);
        }
        if (line.IndexOf("Iterate(") == 0)
        {
            //Iterate(Phone.Vibrate);
            if (line.Contains("Vibrate"))
            {
                if (LevelManager.vibrating) Vibration.Vibrate(10);
            }
            else if (line.Contains("Position"))
            {

                string type = Parser_splitStringBetween(line, "(", ",");
                type = type.Substring(1, type.Length - 2);
                string pos = Parser_splitStringBetween(line, ", new Position(", ")");

                string x = Compiler_evaluateExpression(pos.Remove(pos.IndexOf(",")));
                string y = Compiler_evaluateExpression(pos.Substring(pos.IndexOf(",") + 1));

                Vector2 position = new Vector2(float.Parse(x), float.Parse(y));
                position /= 6;
                GameObject obj = Instantiate(Resources.Load(type), position, this.transform.rotation) as GameObject;
                obj.transform.SetParent(this.transform);

                //obj.GetComponent<SpriteRenderer>().color = new Color(Random.value, Random.value, Random.value);
            }
            else
            {
                string output = Parser_splitStringBetween(line, "(", ")");
                output = output.Substring(1, output.Length - 2);
                Instantiate(Resources.Load(output), this.transform.position, this.transform.rotation);
            }
        }
        if (line.IndexOf("ClearIterations();") == 0)
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Destroy(this.transform.GetChild(i).gameObject);
            }
        }
        if (line.IndexOf("IterateArduino(new Color(") == 0)
        {//lines.Add("IterateArduino(new Color(1,1,1));");

            string message = Parser_splitStringBetween(line, "(new Color(", ")");//1,1,1
            string[] values = message.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                int val = int.Parse(Compiler_evaluateExpression(values[i]));
                if (val > 255) val = 255;
                if (val < 0) val = 0;
                bluetooth_module.message = byte.Parse(val.ToString());
                bluetooth_module.send();
            }
        }*/
    }


    void Start () {
		
	}


	void Update () {
		
	}
}
