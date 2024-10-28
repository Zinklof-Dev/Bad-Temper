using UnityEditor;
using UnityEngine;
using ZinklofDev.Console;
using ZinklofDev.Utils.Testing;

public class IPV4toHex : MonoBehaviour
{
    private void Awake()
    {
        // All commands made must be registered with the Shell
        Shell.RegisterCommand(IP_TO_HEX);
        Shell.RegisterCommand(HEX_TO_IP);
    }

    // Function Converts IPV4 To a Hexadecimal Number
    // 
    public static string IPV4ToHexadecimal(string IP)
    {
        // Splits the IP string into the four octets and discards the periods
        string[] strings = IP.Split(".");
        string IPConverted = "";

        foreach (string s in strings) 
        {
            int decimalNumber = int.Parse(s); //Cameron | So i found out that string format error comes from here, not sure why? it doesn't break anything so like... probably ignorable?? we'll see down the line KEK. // Cole | Fixed. You were passing this function into itself...

            // Converts an int decimal number into a string hex number
            // When converting an int into a string, there are diffrent formats you can use. The format, "X", is hexadecimal

            string hexValue = decimalNumber.ToString("X");

            if (hexValue.Length == 1)
            {
                hexValue = "0" + hexValue;
            }

            IPConverted += hexValue;
        }

        Log.LogResponse(IPConverted);
        return IPConverted;
    }

    // Function converts the Hexadecimal format IP into standard format
    public static string HexadecimalToIPV4(string IP)
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
        return IPConverted;
    }

    public static Test IPtoHex = new Test("IPV4toHex.cs", () =>
    {
        string x = IPV4ToHexadecimal("255.124.74.8");

        IPtoHex.Expect(x, "FF7C4A08");
    });

    public static Test HexToIp = new Test("IPV4toHex.cs", () =>
    {
        string x = HexadecimalToIPV4("FF000FA4");

        HexToIp.Expect(x, "255.0.15.164");
    });

    // LegacyCommand To Run Functions 
    public static LegacyCommand<string> IP_TO_HEX = new LegacyCommand<string>(/*LegacyCommand ID: first 4 letters 0001 for game command, first two letters developer ID (Cole Hauser is 15), last letters command number*/ "0001x1500000001", /* LegacyCommand inputed into the consol*/ "ip_to_hex", /* LegacyCommand Description*/ "Converts an IPV4 to Hexadecimal Format", /* Is it a cheat? */false, /*Variable that allows input of comand to be passed into method*/(t1) =>
    {
        IPV4ToHexadecimal(t1);
    });

    public static LegacyCommand<string> HEX_TO_IP = new LegacyCommand<string>("0001x1500000002", "hex_to_ip", "Converts an IPV4 from Hexadecimal Format back to Standard Format", false, (t1) =>
    {
        HexadecimalToIPV4(t1);
    });

}
