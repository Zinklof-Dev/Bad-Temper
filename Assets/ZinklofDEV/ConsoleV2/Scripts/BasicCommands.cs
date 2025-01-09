using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using ZinklofDev.ConsoleV2;

public static class BasicCommands
{
    [Command("Prints Hello World to the console.")]
    public static void HelloWorld()
    {
        Console.Log("Hello World!", "HelloWorld");
    }

    [Command("Adds two variables together")]
    public static void Add(float a, float b)
    {
        Console.Log(a + b, "Add");
    }

    [Command("Try to guess what side a coin will land on! true = heads, false = tails!")]
    public static void CoinGamble(bool a)
    {
        int coin = Random.Range(0, 2);
        bool coinAsBool = false;
        string coinAsString = "Tails";

        if (coin == 1) 
        {
            coinAsBool = true;
            coinAsString = "Heads";
        }

        if (a = coinAsBool)
        {
            Console.Log(coinAsString + "! You guessed right!", "CoinGamble");
        }
        else
        {
            Console.Log(coinAsString + "! You guessed wrong :(", "CoinGamble");
        }
    }

    [Command("Echos what you input into the console")]
    public static void Echo(string message)
    {
        Console.Log(message, "Echo");
    }
}
