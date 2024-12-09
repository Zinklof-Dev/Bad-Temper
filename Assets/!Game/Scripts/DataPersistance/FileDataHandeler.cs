using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandeler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private bool useEncription = false;
    private readonly string encryptionKey = "ColeIsGay";

    public FileDataHandeler(string dataDirPath, string dataFileName, bool useEncription)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncription = useEncription;
    }

    public ProfileData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        ProfileData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using(StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (useEncription)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                loadedData = JsonUtility.FromJson<ProfileData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(ProfileData gameData) 
    { 
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(gameData, true);

            if (useEncription)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            using (FileStream file = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char) (data[i] ^ encryptionKey[i % encryptionKey.Length]);
        }
        return modifiedData;
    }
}
