﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interpreter {

    private GameObject obj;
    private string[] script;

    private ScopeHandler scope;
    private ListenerHandler listener_handler;

    string variable_type, variable_name, variable_value, variable_modifier, variable_initialization, parameter, condition, debugger;

    public Interpreter (string[] script, GameObject obj) {
        if (script == null) script = new string[] { };
        this.script = script;
        this.obj = obj;

        scope = new ScopeHandler ();
        listener_handler = new ListenerHandler ();
    }

    /* Parsing each line of text into code (a.k.a. where the magic happens) */
    public bool interpretLine () {
        if (script.Length > 0 && scope.getPointer () >= 0 && scope.getPointer () < script.Length) {
            debugger += "LINE: " + script[scope.getPointer ()] + "\n";
            string line = script[scope.getPointer ()];
            string[] line_parts = line.Split (' ');
            int pointer_saved = scope.getPointer ();

            switch (line_parts[0]) {
                case Operators.EMPTY:
                    break;
                case Keywords.LIBRARY_IMPORT:
                    listener_handler.addListener (line_parts);
                    break;
                case Keywords.BREAK:
                case Operators.CLOSING_BRACKET:
                    scope.pop ();
                    break;
                case Keywords.CONTINUE:
                    scope.back ();
                    break;
                case Keywords.IF:
                case Keywords.WHILE:
                    /* Add to scope */
                    scope.push (getMatchingEndBracket (getPointer ()));
                    /* e.g. "while (i < 10) {" */
                    condition = Operators.EMPTY;
                    for (int i = 1; i < line_parts.Length - 1; i++) condition += line_parts[i] + " ";

                    /* e.g. "(i < 10)" */
                    evaluateCondition (condition.Substring (1, condition.Length - 3), line_parts[0]);
                    break;
                case Keywords.FOR:
                    scope.push (getMatchingEndBracket (getPointer ()));
                    /* e.g. "for (int i = 0; i < 10; i++) {" */
                    parameter = Operators.EMPTY;
                    for (int i = 1; i < line_parts.Length - 1; i++) parameter += line_parts[i] + " ";
                    /* e.g. "(int i = 0; i < 10; i++)" */
                    parameter = parameter.Substring (1, parameter.Length - 3);
                    /* e.g. "int i = 0; i < 10; i++" */
                    string[] parameters = parameter.Split (Operators.END_LINE_CHAR);
                    variable_initialization = parameters[0];
                    condition = parameters[1].Substring (1);
                    variable_modifier = parameters[2].Substring (1);
                    /* e.g. ["int i = 0", "i < 10", "i++"] */
                    if (scope.isVariableInScope (variable_initialization.Split (' ') [1])) {
                        scope.setVariableInScope (variable_modifier); /* Is not the first time for loop has run, e.g. "i" exists*/
                    } else {
                        scope.declareVariableInScope (variable_initialization); /* Run first part of for loop for first iteration, e.g. "i" needs to be initialized*/
                    }
                    evaluateCondition (condition, line_parts[0]);
                    break;
                case Variables.BOOLEAN:
                case Variables.INTEGER:
                case Variables.FLOAT:
                case Variables.STRING:
                    scope.declareVariableInScope (line);
                    break;
                default:
                    if (scope.isVariableInScope (line_parts[0])) {
                        /* CHECK IF LINE REFERS TO A VARIABLE, e.g. "i = 10;" */
                        scope.setVariableInScope (line);
                    } else {
                        if (line_parts[0].Contains (".")) {
                            /* e.g. "Console.WriteLine("test")" */
                            string class_name = line_parts[0].Split ('.') [0];
                            string function_name = line_parts[0].Split ('.') [1].Split ('(') [0];
                            string function_parameters = line.Substring (line.IndexOf ("(") + 1);
                            function_parameters = function_parameters.Substring (0, function_parameters.Length - 2);
                            function_parameters = scope.parseInScope (function_parameters);

                            /* e.g. "Console", "WriteLine" */
                            switch (class_name) {
                                case Classes.CONSOLE:
                                    Referencer.consoleManager.execute (function_name, function_parameters, obj);
                                    break;
                                case "Application":
                                    break;
                                case "Mathf":
                                    break;
                            }
                        }
                        /* CHECK IF LINE REFERS TO A FUNCTION, e.g. "Console.WriteLine("Test");" */
                        /*
                            switch className
                         */
                        //how to handle the many various "action" functions people could call, e.g. Console.log, this.rotate(), etc.
                    }
                    break;
            }
            scope.step ();
            // if (pointer == pointer_saved) pointer++; //step line
            // if (pointer >= script.Length) return true; //script done/
            listener_handler.updateListeners(getPointer());

            return false;
        } else return true;
    }

    private void evaluateCondition (string input, string type) {
        input = Evaluator.cast (scope.parseInScope (input), Variables.BOOLEAN);

        if (bool.Parse (input) == true) {
            /* e.g. "true", execute within brackets */
            if (type == Keywords.WHILE || type == Keywords.FOR) {
                //  scope_tracker.Push (pointer);
            } else if (type == Keywords.IF) {
                // scope_tracker.Push (getPointerTo (pointer, Operators.CLOSING_BRACKET));
            }
        } else { scope.skipScope (); }

    }

    private int getMatchingEndBracket (int start_line) {
        int bracket_count = 1;
        while (bracket_count != 0) {
            start_line++;
            if (script[start_line].Contains (Operators.OPENING_PARENTHESIS)) {
                bracket_count++;
            } else if (script[start_line] == Operators.CLOSING_PARENTHESIS) {
                bracket_count--;
            }
        }
        return start_line;
    }

    public int getPointer () {
        return scope.getPointer ();
    }
    public override string ToString () {
        string output = debugger + "\n\n";
        debugger = Operators.EMPTY;
        // for (int i = 0; i < variables.Count; i++) {
        //     output += "-> Variable(" + variables[i].ToString () + ")\n";
        // }
        return output;
    }
}