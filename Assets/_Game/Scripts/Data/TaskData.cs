using System;
using Unity.Plastic.Newtonsoft.Json;

namespace _Game.Scripts.Data {
    [Serializable]
    public class TaskData {
        public string title;
        public string text;
        public string id;
        [JsonProperty] private string department;
        public Department Department => department.ToDepartment();
    }
}