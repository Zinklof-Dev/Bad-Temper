using System;
using System.IO;
using System.Text;
using UnityEngine;
using Unity.Collections;
using ZinklofDev.Utils;
using Newtonsoft.Json;

public class Profile
{
    string profVers { get; set; }
    FixedString32Bytes username { get; set; }

}

public static class ProfileSystem
{
    public static Profile profile = null;
    public static string saveLoc = Application.persistentDataPath + "/profile.zdf";
    
    public static Profile FetchProfile()
    {
        if (profile == null)
        {
            FileStream fs = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            string contents;
            using(var sr = new StreamReader(fs))
            {
                contents = sr.ReadToEnd();
            }

            if (contents == null || contents == "") // make new file
            {
                Debug.Log("no saved profile found, making new file");
                contents = NewProfileJson();

                Encoding unicode = Encoding.Unicode;
                Byte[] contentAsBytes = unicode.GetBytes(contents);
                
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(contentAsBytes);
                }
            }



            JsonConvert.DeserializeObject<Profile>(contents);

            return profile;
        }
        return null;
    }

    private static string NewProfileJson()
    {
        return null;
    }
}
