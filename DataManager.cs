using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;

/// <summary>
/// Data Manager class. Use to store and load json files. Works on Android.
/// </summary>
public static class DataManager
{
    //String that will store de json data from the object.
    private static string dataString;

    //String that will store the persistent data path of the device with a folder.
    private static string persistentDataPath;
    //String that will store the streaming assets path of the device.
    private static string streamingAssetsPath;

    //Hash that will be use to encrypt/decrypt. Change it as you want.
    private static string hash = "124578963";

    /// <summary>
    /// Sets the streaming assets path and the persistent data path of the currently
    /// device.
    /// </summary>
    /// <param name="folderName">Name of the folder that will store the files.</param>
    public static void SetFilesPath(string folderName)
    {
        try
        {
            streamingAssetsPath = Application.streamingAssetsPath;
            persistentDataPath = string.Concat(Application.persistentDataPath, "/", folderName);
            if (!Directory.Exists(persistentDataPath))
                Directory.CreateDirectory(persistentDataPath);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Converts an object into a json string and save it into a file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="obj">Object that will be saved. The class must be serialized.</param>
    /// <param name="streamingAssets">Will be saved in the Streaming Assets folder?</param>
    /// <param name="encrypt">Will be encrypted?</param>
    public static void SaveData(string fileName, object obj, bool streamingAssets = false, bool encrypt = false)
    {
        try
        {
            //Gets the full path of the file.
            string path = streamingAssets ? path = string.Concat(streamingAssetsPath, "/", fileName)
                    : path = string.Concat(persistentDataPath, "/", fileName);

            if (!File.Exists(path))
                File.Create(path).Close();

            //Checks if the data will be encrypted and 
             //generate a json string of the object.
            dataString = encrypt ? dataString = Encrypt(JsonUtility.ToJson(obj))
                : dataString = JsonUtility.ToJson(obj);

            //Creates the file.
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(dataString);
            writer.Close();

            Debug.Log("Data saved successfully.");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Loads a json string and returns an object with the data.
    /// </summary>
    /// <typeparam name="T">Class of the object that will be returned.</typeparam>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="streamingAssets">Will be loaded from the Streaming Assets folder?</param>
    /// <param name="decrypt">Will be decrypted?</param>
    /// <returns>Object of the json string.</returns>
    public static object LoadData<T>(string fileName, bool streamingAssets = false, bool decrypt = false)
    {
        try
        {
            //Gets the full path of the file.
            string path = streamingAssets ? path = string.Concat(streamingAssetsPath, "/", fileName)
                    : path = string.Concat(persistentDataPath, "/", fileName);

            //Change the way to load the file depending of the plataform.
            if (Application.platform == RuntimePlatform.Android)
            {
                //Must use WWW class to load Streaming Assets on Android.
                if (streamingAssets)
                {
                    WWW www = new WWW(path);
                    while (!www.isDone) { }
                    dataString = decrypt ? dataString = Decrypt(www.text)
                        : dataString = www.text;
                    www.Dispose();
                }
                else
                {
                    //Use StreamReader to load files of the persistent data path.
                    StreamReader reader = new StreamReader(path);
                    dataString = decrypt ? dataString = Decrypt(reader.ReadToEnd())
                        : dataString = reader.ReadToEnd();
                    reader.Close();
                }
            }
            //If it is not on Android
            else
            {
                StreamReader reader = new StreamReader(path);
                dataString = decrypt ? dataString = Decrypt(reader.ReadToEnd())
                        : dataString = reader.ReadToEnd();
                reader.Close();
            }

            Debug.Log("Data Loaded Successfully!");
            //Returns the object of the json string.
            return JsonUtility.FromJson<T>(dataString.Trim());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Encrypts a string.
    /// </summary>
    /// <param name="input">string that will be encrypted.</param>
    /// <returns>String encrypted.</returns>
    public static string Encrypt(string input)
    {
        byte[] data = UTF8Encoding.UTF8.GetBytes(input);
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
            using (TripleDESCryptoServiceProvider trip =
                new TripleDESCryptoServiceProvider()
                {
                    Key = key,
                    Mode =
                CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                })
            {
                ICryptoTransform tr = trip.CreateEncryptor();
                byte[] result = tr.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(result, 0, result.Length);
            }
        }
    }

    /// <summary>
    /// Decrypts a string.
    /// </summary>
    /// <param name="input">string that will be decrypted.</param>
    /// <returns>String decrypted.</returns>
    public static string Decrypt(string input)
    {
        byte[] data = Convert.FromBase64String(input);
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
            using (TripleDESCryptoServiceProvider trip =
                new TripleDESCryptoServiceProvider()
                {
                    Key = key,
                    Mode =
                CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                })
            {
                ICryptoTransform tr = trip.CreateDecryptor();
                byte[] result = tr.TransformFinalBlock(data, 0, data.Length);
                return UTF8Encoding.UTF8.GetString(result);
            }
        }
    }

    //Both Encrypt and Decrypt methods were taken from N3K EN Channel.
    //Video: Unity Mobile Game - Encryption - 24 - Android & iOS [C#][Tutorial]
    //Link: https://www.youtube.com/watch?v=ORfN77KdfKE
}
