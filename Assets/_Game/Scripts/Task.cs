using _Game.Scripts.Data;
using _Game.Scripts.Drag;
using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class Task : MonoBehaviour {
        [SerializeField] private DragComponent _dragComponent;
        public DragComponent DragComponent => _dragComponent;

        [Header("Presentation")]
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _id;

        public TaskData Data { get; private set; }

        public bool Enabled {
            set => _dragComponent.enabled = value;
        }

        public void Load(TaskData data) {
            Data = data;

            _title.text = Data.title;
            _id.text = Data.id;
        }
    }
}