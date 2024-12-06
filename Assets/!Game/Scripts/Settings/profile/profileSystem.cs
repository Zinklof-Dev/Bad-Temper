using System;
using System.IO;
using System.Text;
using UnityEngine;
using Unity.Collections;
using ZinklofDev.Utils;
using Newtonsoft.Json;

public class Profile
{
    public string profVers { get; set; }
    public FixedString32Bytes username { get; set; }
}

public static class ProfileSystem
{
    private static Profile profile = null;
    private static string saveLoc = Application.persistentDataPath + "/profile.zdf";
    
    public static Profile FetchProfile()
    {
        if (profile == null)
        {
            FileStream fs = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            string content;
            Byte[] contentAsBytes;
            contentAsBytes = System.IO.File.ReadAllBytes(saveLoc);
            Encoding unicode = Encoding.Unicode;
            content = unicode.GetString(contentAsBytes);

            if (content == null || content == "") // make new file
            {
                Debug.Log("no saved profile found, making new file");
                content = NewProfileJson();

                contentAsBytes = unicode.GetBytes(content);
                
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(contentAsBytes);
                }
            }
            
            Profile profile = JsonConvert.DeserializeObject<Profile>(content);

            return profile;
        }
        return null;
    }

    public static void SaveProfileChanges(Profile newProfile)
    {
        FileStream fs = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        string content = JsonConvert.SerializeObject(newProfile);

        Encoding unicode = Encoding.Unicode;
        Byte[] contentAsBytes = unicode.GetBytes(content);
        
        using (StreamWriter sw = new StreamWriter(fs))
        {
            sw.Write(contentAsBytes);
        }
    }

    private static string NewProfileJson()
    {
        Profile returnProfile = new Profile();
        returnProfile.profVers = "0.1";
        returnProfile.username = (FixedString32Bytes)"New Player";

        string json = JsonConvert.SerializeObject(returnProfile);

        return json;
    }
}
