using System;
using System.IO;
using UnityEngine;

namespace vpet
{

    //public class Settings<T> :Settings
    //{
    //    public T get = (T) Activator.CreateInstance(typeof(T));

    //    internal void Save(string path)
    //    {
    //        string filepath = Path.Combine(path, this.GetType().ToString() + ".cfg");
    //        File.WriteAllText(filepath, JsonUtility.ToJson(get));
    //        Helpers.Log("Settings saved to: " + filepath);
    //    }

    //    internal void Load(string path)
    //    {
    //        string filepath = Path.Combine(path, this.GetType().ToString() + ".cfg");
    //        get = (T)JsonUtility.FromJson<T>(File.ReadAllText(filepath));
    //    }
    //}

    public class Settings
    { }

}
