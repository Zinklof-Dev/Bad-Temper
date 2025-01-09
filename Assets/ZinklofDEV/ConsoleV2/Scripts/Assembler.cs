using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using ZinklofDev.ConsoleV2;
using NUnit.Framework;
using System.Collections.Generic;

public static class Assembler
{
    public static bool HasInitialized = false;
    public static Assembly[] assemblyArray = new Assembly[1];

    public static void Initialize()
    {
        if (HasInitialized)
        {
            return;
        }

        assemblyArray = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly a in assemblyArray)
        {
            foreach (Type t in a.GetTypes())
            {
                foreach (MethodInfo m in t.GetMethods())
                {
                    if (m.GetCustomAttributes(typeof(Command), false).Length > 0)
                    {
                        string commandName = "";

                        Command commandAttribute = m.GetCustomAttribute<Command>();

                        if (commandAttribute.callName == string.Empty)
                            commandName = t.Name + "." + m.Name;
                        else
                            commandName = commandAttribute.callName;

                        ParameterInfo[] parameters = m.GetParameters();

                        List<GenericCommandVariable> vars = new List<GenericCommandVariable>();

                        foreach (ParameterInfo p in parameters)
                        {
                            GenericCommandVariable var = new GenericCommandVariable(p);
                            vars.Add(var);
                        }

                        ShellCommandClass shellCommand = new ShellCommandClass(commandName, commandAttribute.helpDescription, commandAttribute.cheat, vars.ToArray(), m);

                        Debug.Log(shellCommand.callName);

                        Shell.registeredCommands.Add(shellCommand);

                        ZinklofDev.ConsoleV2.Console.CommandsRegistered++;
                    }
                }
            }
        }

        ZinklofDev.ConsoleV2.Console.Log("Succsesfully built " + ZinklofDev.ConsoleV2.Console.CommandsRegistered + " Command(s)", "Assembler");
        ZinklofDev.ConsoleV2.Shell.PokeShell(ZinklofDev.ConsoleV2.Console.CommandsRegistered);
        HasInitialized = true;
    }
}
