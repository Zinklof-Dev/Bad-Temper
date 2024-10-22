using JetBrains.Annotations;
using UnityEngine;
using ZinklofDev.Console;

 namespace ZinklofDev.Console
{
    public class BasicCommands : MonoBehaviour
    {
        public static LegacyCommand HELLOWORLD = new LegacyCommand("0000x0000000000", "HelloWorld", "Prints HelloWorld", false, () =>
        {
            ZinklofDev.Console.BasicCommands.HelloWorld();
        });

        public static void HelloWorld()
        {
            Log.LogMisc("Hello World!", "BasicCommands.cs Line(14)");
        }

        // 1 = on, 0 = off, everything else returns out of bounds
        public static LegacyCommand<byte> DEBUGCHEATS = new LegacyCommand<byte>("0000x0000000001", "zd_cheats", "Turns on Cheats", false, (t1) =>
        {
            DebugCheats(t1);
        });

        public static LegacyCommand<int, int> ADDITION = new LegacyCommand<int, int>("0000x0000000003", "zd_add", "Adds two values together (two ints)", false, (t1, t2) =>
        {
            Addition(t1, t2);
        });

        public static LegacyCommand EXIT = new LegacyCommand("0000x0000000004", "zd_exit", "Exits the program", false, () =>
        {
            Exit();
        }); 

        public static LegacyCommand<int> HEX = new LegacyCommand<int>("0000x0000000005", "zd_hex", "converts int to hex", false, (t1) => 
        {
            string hexValue = t1.ToString("X");

            Log.LogResponse(hexValue);
        });

        public static void DebugCheats(byte value)
        {
            if (value == 1)
            {
                Shell.CheatsOn = true;
                Log.LogResponse("Debug Cheats are enabled");
            }
            else if (value == 0)
            {
                Shell.CheatsOn = false;
                Log.LogResponse("Debug Cheats are disabled");
            }
            else
            {
                Log.LogError(value + " Is not a valid parameter/value for the command 'DebugCheats'. Use 1 or 0", "BasicCommands.cs(Line 49)");
            }
        }

        public static void Exit()
        {
            Application.Quit();
        }

        public static void Addition (int value, int value2)
        {
            Log.LogResponse(value + " + " +  value2 + " = " + (value + value2));
        }

        private void Awake()
        {
            Shell.RegisterCommand(HELLOWORLD);
            Shell.RegisterCommand(EXIT);
            Shell.RegisterCommand(DEBUGCHEATS);
            Shell.RegisterCommand(HEX);
            //Shell.RegisterCommand(ADDITION);
        }
    }
}
