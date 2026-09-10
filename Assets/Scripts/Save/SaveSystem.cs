using System.IO;
using UnityEngine;

namespace IdleGame.Save
{
    public class SaveSystem
    {
        private readonly string _filePath;

        public SaveSystem(string filePath)
        {
            _filePath = filePath;
        }

        public void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_filePath, json);
        }

        public SaveData Load()
        {
            if (!File.Exists(_filePath)) return null;
            string json = File.ReadAllText(_filePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}
