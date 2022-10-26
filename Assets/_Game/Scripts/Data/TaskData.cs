using System;
using Newtonsoft.Json;

namespace _Game.Scripts.Data {
    [Serializable]
    public class TaskData {
        public string title;
        public string id;
        [JsonProperty] private string text;
        public string Text => text.Replace("\\n", "\n");
        [JsonProperty] private string department;
        public Department Department => department.ToDepartment();
    }
}