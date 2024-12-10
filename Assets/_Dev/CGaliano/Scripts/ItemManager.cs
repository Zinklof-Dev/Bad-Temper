using UnityEngine;

public class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public short ItemID { get; set; }
    public string ImageFilePath { get; set; }
}

public static class ItemManager
{
    static Item wood = new Item()
    {
        Name = "Wood",
        Description = "Let's stay mature here, its the corpse of a tree.",
        ItemID = 0,
        ImageFilePath = null
    };
}
