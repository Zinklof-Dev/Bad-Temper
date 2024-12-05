using System;
using UnityEngine;
using ZinklofDev.Utils;
using Newtonsoft.Json;

public class Profile
{
    string profVers { get; set; }
    FixedString32Byte username { get; set; }

}

public static class ProfileSystem
{
    public static Profile profile = null;
    public static string saveLoc = Application.persistantDataPath + "/profile.zdf"
    
    public static Profile FetchProfile()
    {
        if (profile = null)
        {
            FileStream fs = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            string contents;
            using(var sr = new StreamReader(fs))
            {
                contents = sr.ReadToEnd();
            }

            if (contents == null || contents = "") // make new file
            {
                debug.log("no saved profile found, making new file");
                string contents = NewProfileJson();

                Encoding unicode = Encoding.unicode;
                Bytes[] contentAsBytes = unicode.GetBytes(contents);
                
                using (StreamWriter sw = new StreamWriter(fs, false))
                {
                    sr.write(contentAsBytes);
                }
            }

            
          
            JsonConvert.DeserializeObject<profile>(contents)
        }
    }

    private static string NewProfileJson()
}
