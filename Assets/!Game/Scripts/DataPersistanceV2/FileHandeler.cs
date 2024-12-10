using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/*namespace ZinklofDev.DataPersistanceV2
{
    public static class FileHandeler
    {
        public static dynamic GetFileAsClass<T>(string path, string filename, bool encryption)
        {
            string fullpath = Path.combine(path, filename);
            T returnClass = null;

            if (File.Exists(fullpath))
            {
                try
                {
                    string data = "";
                    using (FileStream fs = new FileStream(fullpath, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            data = sr.ReadToEnd();
                        }
                    }

                    if (encryption)
                    {
                        data = EncryptDecrypt(data);
                    }

                    returnClass = JsonUtility.FromJson<T>(data);
                }
                catch (Exception e)
                {
                    Debug.LogException("Error fetching file: " + fullpath + "\n" + e);
                }
            }
			else
		  	{
				Debug.LogError("No File is present at " + fullpath + "\n, use the ovverride to function to create one, or use the ovverride of this function to create one if one cannot be found.");
		  	}
		  	return returnClass;
        }

        public static dynamic GetFileAsClass<T>(string path, string filename, bool encryption, bool create)
        {
            string fullpath = Path.Combine(path, filename);
            T returnClass = (T)Activator.CreateInstance(typeof(T));

            if (File.Exists(fullpath))
            {
                try
                {
                    string data = "";
                    using (FileStream fs = new FileStream(fullpath, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            data = sr.ReadToEnd();
                        }
                    }

                    if (encryption)
                    {
                        data = EncryptDecrypt(data);
                    }

                    returnClass = JsonUtility.FromJson<T>(data);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error fetching file: " + fullpath + "\n" + e);
                }
            }
			else if (create)
			{
				try
				{
					T defaultClass = returnClass.Default();
					
					Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

					string data = JsonUtility.ToJson(defaultClass, true);

					if (encryption)
					{
						data = EncryptDecrypt(dataToStore);
					}

					using (FileStream fs = new FileStream(fullPath, FileMode.Create))
					{
						using (StreamWriter sw = new StreamWriter(fs))
						{
							sw.write(data);
						}
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Error trying to create file " + fullpath + "\n" + e);
				}
				try
                {
                    string data = "";
                    using (FileStream fs = new FileStream(fullpath, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            data = sr.ReadToEnd();
                        }
                    }

                    if (encryption)
                    {
                        data = EncryptDecrypt(data);
                    }

                    returnClass = JsonUtility.FromJson<T>(data);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error fetching file: " + fullpath + "\n" + e);
                }
			}
			else
		  	{
				Debug.LogError("No File is present at " + fullpath + "\n, use the ovverride to function to create one, or use the ovverride of this function to create one if one cannot be found.");
				return null;
		  	}
		  	return returnClass;
        }

		public static void CreateFile<T>(string path, string file, bool encryption)
		{
            string fullpath = Path.Combine(path, filename);

            try
			{
				T defaultClass = T.Default();
					
				Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

				string data = JsonUtility.ToJson(defaultClass, true);

				if (encryption)
				{
					data = EncryptDecrypt(dataToStore);
				}

				using (FileStream fs = new FileStream(fullPath, FileMode.Create))
				{
					using (StreamWriter sw = new StreamWriter(fs))
					{
						sw.write(data);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogException("Error trying to create file " + fullpath + "\n" + e);
			}
		}

        private static string EncryptDecrypt(string data)
        {
            string modifiedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char) (data[i] ^ encryptionKey[i % encryptionKey.Length]);
            }
            return modifiedData;
        }
    }
}
*/