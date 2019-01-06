﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableObject {

    private string type;
    private string name;
    /* For Primitive Data Types, value == value, otherwise value holds ToString() */
    private string value;

    VariableObject[] fields;

    public VariableObject (string type) {
        init (type, "", "");
    }
    public VariableObject (string type, string name) {
        init (type, name, "");
    }
    public VariableObject (string type, string name, string value) {
        init (type, name, value);
    }
    public void init (string type, string name, string value) {
        this.type = type;
        this.name = name;
        this.value = value;

        switch (type) {
            case "Vector2":
                fields = new VariableObject[] {
                    new VariableObject ("float", "x", value.Split (',') [0]),
                    new VariableObject ("float", "y", value.Split (',') [1])
                };
                break;
            default:
                fields = null;
                break;
        }
    }

}
public class Interpreter {

    const byte BOOLEAN = 0,
        INTEGER = 1,
        FLOAT = 2,
        STRING = 3;
    private const IReadOnlyList<string> VARIABLE_TYPES = {
        "bool",
        "int",
        "float",
        "string",
        "Vector2"
    };

    private enum OPERATORS {
        EQUALS = "=",
        INCREMENT = "++",
        DECREMENT = "--",
        ADDITION = "+=",
        SUBTRACTION = "-=",
        MULTIPLICATION = "*=",
        DIVISION = "/=",
        MODULO = "%=",
        EQUAL_TO = "==",
        NOT_EQUAL = "!=",
        GREATER_THAN = ">",
        GREATER_THAN_EQUAL = ">=",
        LESS_THAN = "<",
        LESS_THAN_EQUAL = "<=",
        AND = "&&",
        OR = "||",
        MODULUS = "%",
        TIMES = "*",
        DIVIDE = "/",
        ADD = "+",
        SUBTRACT = "-"
        };

        private const IReadOnlyList<string> ARITHMETIC_OPERATORS = { "%", "*", "/", "+", "-" };
        private GameObject obj;

        private List<VariableObject> variables;
        private string[] script;

        private short pointer; //tracks what line is being processed

        int left_int, right_int;
        float left_float, right_float;
        bool left_bool, right_bool;

        // Stack<short> backlog; 

        public Interpreter (string[] script, GameObject obj) {
        this.script = script;
        this.obj = obj;

        pointer = 0;
        variables = new List<VariableObject> ();
    }

    /* Parsing each line of text into code (a.k.a. where the magic happens) */
    public void interpretLine () {
        string line = script[pointer];
        string[] line_parts = line.Split (' ');

        switch (line_parts[0]) {
            case "{": //this shouldn't be possible if linting to "if () {" lines
            case "}":
                //scope control:
                //update visible variables?
                pointer = pointer + 1; //find new pointer position, might be going back to loop, skip else, etc
                interpretLine (); //does not cost anything in interpreter, recalls
                break;
            case "if":
            case "for":
            case "while":
                /* e.g. "for (int i = 0; i < 10; i++) {" */
                string parameter;
                for (int i = 1; i < line_parts.Length - 1; i++) {
                    parameter += line_parts[i];
                }
                /* e.g. "(int i = 0; i < 10; i++)" */

                break;
            case "break":
            case "continue":
                //scope-flow control?
                //break => go to current loop's } 
                //continue => go to current loop's { [where ++ and conditionals are also checked]
                break;
            case "bool":
            case "int":
            case "float":
            case "string":
            case "Vector2":
                string variable_type = line_parts[0];
                string variable_name = line_parts[1];
                if (line_parts.Length == 2) {
                    /* e.g. "int i;" */
                    variables.Add (new VariableObject (variable_type, variable_name, ""));
                } else {
                    /* e.g. "int i = 122;" */
                    string variable_value = "";
                    for (int i = 3; i < line_parts.Length; i++) {
                        variable_value += line_parts[i];
                    }
                    /* e.g. "122;" */
                    variable_value = parse (variable_type, variable_value);
                    variables.Add (new VariableObject (variable_type, variable_name, value));
                }
                break;
            default:

                break;
        }

    }
    private string parse (string input) {

        List<string> parts = input.Split (' ');

        /* EVALUATE PARATHESIS AND FUNCTIONS RECURSIVELY */
        /* e.g. "12 + function(2) * 4" ==> "12 + 4 * 4" */
        for (int part = 0; part < parts.Count; part++) {
            if (parts[part].Contains ("(")) {

                string parts_to_be_condensed = parts[part];
                while (parts[part].Contains (")") == false) {
                    part++;
                    parts_to_be_condensed += " " + parts[part];
                }
                if (parts[part].IndexOf ("(") == 0) {
                    parts[parts] = parse (parts_to_be_condensed);
                } else {
                    //to support user-made functions, or functions that require interpreted lines of code to be executed first, will require logic here to allow for putting this parse in a stack to be popped on the "return" of said function
                    parts[parts] = evaluateFunction (parse (parts_to_be_condensed));
                }
            }
            //todo: remove ")" (once scanned) and ";"s (useless)
        }

        /* PEMDAS REST OF OPERATIONS */
        /* e.g. ["12", "+", 4, "*", "4"] ==> ["12", "+", "16"] ==> ["28"]*/
        if (parts.Count > 1) {
            for (int operation = 0; operation < ARITHMETIC_OPERATORS.Length; operation++) {
                string current_operation = ARITHMETIC_OPERATORS[operation];
                for (int part = 1; part < parts.Count - 1; part++) {

                    if (parts[part] == current_operation) {
                        parts[part - 1] = evaluateOperation (parts[part - 1], parts[part], parts[part + 1]);
                        parts.RemoveRange (part, 2);
                        part -= 2;
                    }
                }
            }
        }
        //RETURN FULLY SIMPLIFIED VALUE
        return parts[0];
    }

    private string evaluateFunction (string function, string parameter) {
        if (function.IndexOf ("Mathf") == 0) {
            return evaluateMathf (function, parameter);
        }
        //...

        return "";
    }
    private string evaluateMathf (string function, string parameter) {
        switch (function) {
            case "Mathf.Abs":
                return Mathf.Abs (float.Parse (parameter)) + "";
                //...
        }
        return "";
    }
    private string evaluateOperation (string left, string arithmetic_operator, string right) {
        /* e.g. ["12", "*", "4"] ==> ["48"] */
        string left_type = getVariableType (left, true), right_type = getVariableType (right, false);
        if (left_type == right_type) {
            switch (left_type) {
                case VARIABLE_TYPES.BOOLEAN:
                    return evaulateBooleans (left_bool, arithmetic_operator, right_bool) + "";
                case VARIABLE_TYPES.INTEGER:
                    return evaluateIntegers (left_int, arithmetic_operator, right_int) + "";
                case VARIABLE_TYPES.FLOAT:
                    return evaluateFloats (left_float, arithmetic_operator, right_float) + "";
                    //...
                case VARIABLE_TYPES.STRING:
                    return evaulateString (left_bool, arithmetic_operator, right_bool) + "";
                default:
                    return "";
            }
        }
    }
    private bool evaulateBooleans (bool left, string arithmetic_operator, bool right) {
        switch (arithmetic_operator) {
            case OPERATORS.EQUAL_TO:
                return left == right;
            case OPERATORS.AND:
                return left && right;
            case OPERATORS.OR:
                return left || right;
            default:
                return "";
        }
    }
    private int evaluateIntegers (int left, string arithmetic_operator, int right) {
        switch (arithmetic_operator) {
            case OPERATORS.MODULUS:
                return left % right;
            case OPERATORS.TIMES:
                return left * right;
            case OPERATORS.DIVIDE:
                return left / right;
            case OPERATORS.ADD:
                return left + right;
            case OPERATORS.SUBTRACT:
                return left - right;
            default:
                return false;

        }
    }
    private float evaluateFloats (float left, string arithmetic_operator, float right) {
        switch (arithmetic_operator) {
            case OPERATORS.MODULUS:
                return left % right;
            case OPERATORS.TIMES:
                return left * right;
            case OPERATORS.DIVIDE:
                return left / right;
            case OPERATORS.ADD:
                return left + right;
            case OPERATORS.SUBTRACT:
                return left - right;
            default:
                return 0;

        }
    }
    private string evaulateString (string left, string arithmetic_operator, string right) {
        switch (arithmetic_operator) {
            case OPERATORS.ADD:
                return left + right;
            default:
                return 0;

        }
    }

    private string getVariableType (string input) {
        getVariableType (input, true);
    }
    private string getVariableType (string input, bool left) {
        if (left) {
            if (bool.TryParse (input, out left_bool)) return VARIABLE_TYPES[BOOLEAN];
            if (int.TryParse (input, out left_int)) return VARIABLE_TYPES[INTEGER];
            if (float.TryParse (input, out left_float)) return VARIABLE_TYPES[FLOAT];
            //...
        } else {
            if (bool.TryParse (input, out right_bool)) return VARIABLE_TYPES[BOOLEAN];
            if (int.TryParse (input, out right_int)) return VARIABLE_TYPES[INTEGER];
            if (float.TryParse (input, out right_float)) return VARIABLE_TYPES[FLOAT];
            //...
        }
        return VARIABLE_TYPES[STRING];
    }
}
// string[] parameter;
// switch (function) {
//     case "Abs":
//         return Mathf.Abs(parameter[0]);
//     case "Round":
//         return Mathf.Round(parameter[0]);
//     case "Floor":
//         return Mathf.Floor(parameter[0]);
//     case "Ceil":
//         return Mathf.Ceil(parameter[0]);
// }
// Fire();
// Fire(10);
// Subroutines...
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