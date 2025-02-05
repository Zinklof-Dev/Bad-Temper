using UnityEngine;

public class Item
{
    public string name { get; set; }
    public string description { get; set; }
    public ushort itemID { get; set; }
    public string imageFilePath { get; set; }
}

public static class ItemManager
{
    static public Item wood = new Item()
    {
        name = "Wood",
        description = "Let's stay mature here, its the corpse of a tree.",
        itemID = 0,
        imageFilePath = null
    };
    static public Item stone = new Item()
    {
        name = "Stone",
        description = "Rock? Unga Bunga!",
        itemID = 1,
        imageFilePath = null
    };
    static public Item coal = new Item()
    {
        name = "Coal",
        description = "They say this is the remains of ancient creatures the size of mountains",
        itemID = 2,
        imageFilePath = null
    };
    static public Item copper = new Item()
    {
        name = "Copper",
        description = "Shiny, solid too, but sorta brittle... Gotta be something better than this.", 
        itemID = 3,
        imageFilePath = null
    }
}
