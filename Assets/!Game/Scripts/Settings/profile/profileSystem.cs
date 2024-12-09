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
            FileStream fs = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite); // prevents no file issues, always ensure files exists even if blank
            fs.Close();

            string content;

            using (StreamReader sr = new StreamReader(saveLoc))
            {
                content = sr.ReadToEnd();
                sr.Close();
            }

            if (content == null || content == "") // make new file
            {
                fs = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);

                Debug.Log("no saved profile found, making new file");
                content = NewProfileJson();

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(content);
                    sw.Close();
                    fs.Close();
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
        
        using (StreamWriter sw = new StreamWriter(fs))
        {
            sw.Write(content);
            sw.Close();
            fs.Close();
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
