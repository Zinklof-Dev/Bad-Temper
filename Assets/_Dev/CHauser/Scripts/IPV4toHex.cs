using UnityEditor;
using UnityEngine;
using ZinklofDev.Console;

public class IPV4toHex : MonoBehaviour
{
   
    private void Awake()
    {
        // All commands made must be registered with the Shell
        Shell.RegisterCommand(IP_TO_HEX);
        Shell.RegisterCommand(HEX_TO_IP);
    }

    // Function Converts IPV4 To a Hexadecimal Number
    public static void IPV4ToHexadecimal(string IP)
    {
        string[] strings = IP.Split(".");
        string IPConverted = "";

        foreach (string s in strings) 
        { 
            int decimalNumber = int.Parse(s);
            // Converts an int decimal number into a string hex number
            string hexValue = decimalNumber.ToString("X");

            if (hexValue.Length == 1)
            {
                hexValue = "0" + hexValue;
            }

            IPConverted += hexValue;
        }

        Log.LogResponse(IPConverted);
    }

    // Function converts the Hexadecimal format IP into standard format
    public static void HexadecimalToIPV4(string IP)
    {
        bool onSecondLetter = false;
        int section = 0;
        string tempSplitString = "";
        string IPConverted = "";

        foreach (char s in IP)
        {
            if (onSecondLetter == false)
            {
                onSecondLetter = true;
                tempSplitString = "";
                tempSplitString += s;
            }
            else
            {
                onSecondLetter = false;
                tempSplitString += s;

                // Converts every two digits in the Hex format IP into regular Binary format
                int converted = int.Parse(tempSplitString, System.Globalization.NumberStyles.HexNumber);

                IPConverted += converted;

                // Puts periods back into standard format IP
                if (section < 3)
                {
                    IPConverted += ".";
                }

                section++;
            }
        }

        Log.LogResponse(IPConverted); 
    }

    // Command To Run Functions 
    public static Command<string> IP_TO_HEX = new Command<string>(/*Command ID: first 4 letters 0001 for game command, first two letters developer ID (Cole Hauser is 15), last letters command number*/ "0001x1500000001", /* Command inputed into the consol*/ "ip_to_hex", /* Command Description*/ "Converts an IPV4 to Hexadecimal Format", /* Is it a cheat? */false, /*Variable that allows input of comand to be passed into method*/(t1) =>
    {
        IPV4ToHexadecimal(t1);
    });

    public static Command<string> HEX_TO_IP = new Command<string>("0001x1500000002", "hex_to_ip", "Converts an IPV4 from Hexadecimal Format back to Standard Format", false, (t1) =>
    {
        HexadecimalToIPV4(t1);
    });

}
