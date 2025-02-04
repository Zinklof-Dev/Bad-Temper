using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZinklofDev.ConsoleV2
{
    public class Command : System.Attribute
    {
        public string callName = string.Empty;
        public bool cheat;
        public string helpDescription;

        public Command(string helpDescription, bool cheat = false, string callName = "")
        {
            if (callName == "")
            {
                callName = string.Empty;
            }

            this.callName = callName;
            this.cheat = cheat;
            this.helpDescription = helpDescription;
        }
    }

    public class ShellCommandClass
    {
        public string callName;
        public string helpDescription;
        public bool cheat;

        public GenericCommandVariable[] genericCommandVariables;
        MethodInfo methodInfo;

        public ShellCommandClass(string callName, string helpDescription, bool cheat, GenericCommandVariable[] genericCommandVariables, MethodInfo methodInfo)
        {
            this.callName = callName;
            this.helpDescription = helpDescription;
            this.cheat = cheat;
            this.genericCommandVariables = genericCommandVariables;
            this.methodInfo = methodInfo;
        }

        public void Invoke(object[] vars)
        {
            if (vars.Length == 0)
                methodInfo.Invoke(null, null);
            else 
                methodInfo.Invoke(null, vars);
        }
    }

    public class GenericCommandVariable
    {
        public string value;
        public byte type;
        
        /*
         * 0 = string
         * 1 = Int | Int32
         * 2 = Float | Single
         * 4 = Bool
         * 5 = Double
         * 6 = Long | Int64
         * 7 = Short | Int16
         * 8 = UInt | UInt32
         * 9 = ULong | UInt64
         * 10 = UShort | UInt16
         * 11 = Byte
         * 12 = Char
         * 255 = Error
         */

        public GenericCommandVariable(ParameterInfo p, string value = "")
        {
            this.value = value;

            Type t = p.ParameterType;

            string typeName = t.ToString();

            switch (typeName)
            {
                case "System.String":
                    type = 0;
                    break;
                case "System.Int32":
                    type = 1;
                    break;
                case "System.Single":
                    type = 2;
                    break;
                case "System.Boolean":
                    type = 3;
                    break;
                case "System.Double":
                    type = 4;
                    break;
                case "System.Int64":
                    type = 5;
                    break;
                case "System.Int16":
                    type = 6;
                    break;
                case "System.UInt32":
                    type = 7;
                    break;
                case "System.UInt64":
                    type = 8;
                    break;
                case "System.UInt16":
                    type = 9;
                    break;
                case "System.Byte":
                    type = 10;
                    break;
                case "System.Char":
                    type = 11;
                    break;
                default:
                    type = 255;
                    break;
            }
        }

        public object ParseAsObject(string value)
        {
            switch (type)
            {
                case 0:
                    return value;
                case 1:
                    return Int32.Parse(value);
                case 2:
                    return Single.Parse(value);
                case 3:
                    return Boolean.Parse(value.ToLower());
                case 4:
                    return Double.Parse(value);
                case 5:
                    return Int64.Parse(value);
                case 6:
                    return Int16.Parse(value);
                case 7:
                    return UInt32.Parse(value);
                case 8:
                    return UInt64.Parse(value);
                case 9:
                    return UInt16.Parse(value);
                case 10:
                    return Byte.Parse(value);
                case 11:
                    return Char.Parse(value);
                default:
                    throw new ArgumentException("ZinklofDev.ConsoleV2.GenericCommandVariable.ParseAsObject(), Arguement falls out of range exception");
            }
        }

        public override string ToString()
        {
            switch (type)
            {
                case 0:
                    return "String";
                case 1:
                    return "Int32";
                case 2:
                    return "Float";
                case 3:
                    return "Bool";
                case 4:
                    return "Double";
                case 5:
                    return "Int64";
                case 6:
                    return "Int16";
                case 7:
                    return "UInt32";
                case 8:
                    return "UInt64";
                case 9:
                    return "UInt16";
                case 10:
                    return "Byte";
                case 11:
                    return "Char";
                default:
                    throw new ArgumentException("ZinklofDev.ConsoleV2.GenericCommandVariable.ParseAsObject(), Arguement falls out of range exception");
            }
        }
    }

    public static class Shell
    {
        public static List<ShellCommandClass> registeredCommands = new List<ShellCommandClass>();

        public static void PokeShell(int expectedCommands)
        {
            if (expectedCommands != registeredCommands.Count)
            {
                Console.Log("Shell is live with" + registeredCommands.Count + " commands, which does not match the asembler and console expected numbers", "Shell", "ff534a", true, 22);
                return;
            }
            else
            Console.Log("Shell is live with " + registeredCommands.Count + " commands which matches the assembler and console expected number", "Shell");
        }

        [Command("The Help Command", false, "Help")]
        public static void Help()
        {
            string result = "";

            foreach (ShellCommandClass cmd in registeredCommands)
            {                
                string variables = "";

                if (cmd.genericCommandVariables.Length > 0)
                {
                    int i = 0;
                    foreach (GenericCommandVariable variable in cmd.genericCommandVariables)
                    {
                        if (i != cmd.genericCommandVariables.Length-1)
                            variables += variable.ToString() + ",";
                        else
                            variables += variable.ToString();
                        i++;
                    }
                }
                
                result += "<b>" + cmd.callName + "(" + variables + ")</b>" + "\n";
                result += cmd.helpDescription + "\n";
            }

            ZinklofDev.ConsoleV2.Console.Log(result, "Help");
        }

        public static string[] SearchForCommands(string querry)
        {
            List<string> commands = new List<string>();

            string[] splitQuerry = querry.Split("(");

            foreach (ShellCommandClass command in registeredCommands)
            {
                if (command.callName.ToLower().Contains(splitQuerry[0].ToLower()))
                {
                    string variables = "";

                    if (command.genericCommandVariables.Length > 0)
                    {
                        int i = 0;
                        foreach (GenericCommandVariable variable in command.genericCommandVariables)
                        {
                            if (i != command.genericCommandVariables.Length-1)
                                variables += variable.ToString() + ",";
                            else
                                variables += variable.ToString();
                            i++;
                        }
                    }

                    commands.Add(command.callName + "(" + variables + ")");
                }
            }

            return commands.ToArray();
        }

        public static void CallCommand(string input)
        {
            Console.Log("> " + input, "", "ffffff");

            // Figure out what command the user is trying to call
            string callName = input.Split("(")[0];

            // Generate a temp local command
            ShellCommandClass command = null;

            // Check our list of registered commands for one with the same CallName
            foreach (ShellCommandClass cmd in registeredCommands)
            {
                if (cmd.callName.ToLower() == callName.ToLower())
                {
                    command = cmd; // Now set that temp generated one to this
                    break;
                }
                else
                {
                    continue;
                }
            }

            if (command == null)
            {
                Console.Log("Couldn't find a command of the name: " + callName, "Shell", "ff534a");
                return;
            }

            // Split the input string seperating the call name from the params
            string[] firstSplit = input.Split("(");

            string[] splitVariables = firstSplit[1].Split(",");

            for (int i = 0; i < splitVariables.Length; i++) //should leave with strings like (1,6,12) -> "1" "6" "12"
            {
                splitVariables[i] = splitVariables[i].Replace(")", "");
            }

            foreach (string variable in splitVariables)
            {
                Console.Log(variable, "Shell");
            }

            List<object> args = new List<object>();

            if (command.genericCommandVariables.Length != 0)
            {
                for (int i = 0; i < command.genericCommandVariables.Length; i++)
                {
                    string variable = splitVariables[i];

                    args.Add(command.genericCommandVariables[i].ParseAsObject(variable));
                }
                Console.Log(args.Count + " Parameters being passed, " + command.genericCommandVariables.Length + " Expected", "Shell");

                command.Invoke(args.ToArray());
            }
            else
            {
                command.Invoke(args.ToArray());
            }
        }
    }
}
