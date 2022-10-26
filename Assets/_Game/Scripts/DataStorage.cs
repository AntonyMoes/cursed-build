using System;
using _Game.Scripts.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace _Game.Scripts {
    public class DataStorage : MonoBehaviour {
        [SerializeField] private TextAsset _tasks;
        
        public TaskData[] Tasks { get; private set; }

        public void Init() {
            Tasks = LoadRecords<TaskData>(_tasks);
        }

        private static T[] LoadRecords<T>(TextAsset asset) {
            return JsonConvert.DeserializeObject<Records<T>>(asset.text)!.records;
        }

        [Serializable]
        private class Records<T> {
            public T[] records;
        }
    }
}