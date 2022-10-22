using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Drag {
    public class DropFrame : MonoBehaviour {
        [SerializeField] private DropComponent _dropComponent;
        [SerializeField] private Image _frame;
        [SerializeField] private StateColor[] _colors;

        private void Awake() {
            _dropComponent.State.Subscribe(OnStateChange, true);
        }

        private void OnStateChange(DropComponent.DropState state) {
            _frame.color = _colors.First(c => c.state == state).color;
        }

        [Serializable]
        private struct StateColor {
            public DropComponent.DropState state;
            public Color color;
        }
    }
}