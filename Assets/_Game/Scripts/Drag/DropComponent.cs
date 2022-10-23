using System;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts.Drag {
    public class DropComponent : MonoBehaviour {
        [SerializeField] private bool _canBeDroppedTo;
        public bool CanBeDroppedTo => _canBeDroppedTo;

        private readonly Action<DropComponent, bool> _onSelectDrop;
        public readonly Event<DropComponent, bool> OnSelectDrop;

        private readonly Action<DropComponent> _onSetObject;
        public readonly Event<DropComponent> OnSetObject;

        private DragComponent _heldObject;
        public DragComponent HeldObject {
            get => _heldObject;
            set {
                _heldObject = value;
                _onSetObject(this);
            }
        }

        public readonly UpdatedValue<DropState> State = new UpdatedValue<DropState>(DropState.Idle);

        private bool _isSelected;

        public DropComponent() {
            OnSelectDrop = new Event<DropComponent, bool>(out _onSelectDrop);
            OnSetObject = new Event<DropComponent>(out _onSetObject);
        }

        private void Start() {
            DragController.Instance.Register(this);
        }

        private void OnDestroy() {
            DragController.Instance.Unregister(this);
        }

        // private void OnMouseEnter() {
        //     _onSelectDrop(this, true);
        // }
        //
        // private void OnMouseExit() {
        //     _onSelectDrop(this, false);
        // }

        private void Update() {
            if (!InputHelper.GetTouch(out var position)) {
                return;
            }

            var selected = RectTransformUtility.RectangleContainsScreenPoint((RectTransform) transform, position, Camera.main);
            if (selected && !_isSelected) {
                Debug.Log($"Drop {name} ON");
                _isSelected = true;
                _onSelectDrop(this, true);
            } else if (!selected && _isSelected) {
                Debug.Log($"Drop {name} OFF");
                _isSelected = false;
                _onSelectDrop(this, false);
            }
        }

        public enum DropState {
            Idle,
            Ready,
            CurrentTarget,
            StillHolding
        }
    }
}