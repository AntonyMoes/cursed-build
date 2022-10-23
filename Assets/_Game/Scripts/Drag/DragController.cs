using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts.Drag {
    public class DragController : SingletonBehaviour<DragController> {
        [SerializeField] private Transform _dragRoot;

        private readonly List<DragComponent> _dragComponents = new List<DragComponent>();
        private readonly List<DropComponent> _dropComponents = new List<DropComponent>();
        private bool _isDragging;
        private Transform _savedParent;
        private DropComponent _currentDrop;

        #region Registration

        public void Register(DragComponent dragComponent) {
            if (!_dragComponents.Contains(dragComponent)) {
                dragComponent.OnStartDrag.Subscribe(OnStartDrag);
                _dragComponents.Add(dragComponent);
            }
        }

        public void Unregister(DragComponent dragComponent) {
            dragComponent.OnStartDrag.Unsubscribe(OnStartDrag);
            dragComponent.OnEndDrag.Unsubscribe(OnEndDrag);
            _dragComponents.Remove(dragComponent);
        }

        public void Register(DropComponent dropComponent) {
            if (!_dropComponents.Contains(dropComponent)) {
                dropComponent.OnSelectDrop.Subscribe(OnSelectDrop);
                _dropComponents.Add(dropComponent);
            }
        }

        public void Unregister(DropComponent dropComponent) {
            dropComponent.OnSelectDrop.Unsubscribe(OnSelectDrop);
            _dropComponents.Remove(dropComponent);
        }

        #endregion

        private void OnStartDrag(DragComponent dragComponent) {
            if (_isDragging) {
                Debug.LogError("Already dragging other object");
                return;
            }

            // Debug.Log("Start drag");
            _isDragging = true;
            _savedParent = dragComponent.transform.parent;
            dragComponent.transform.SetParent(_dragRoot, true);
            dragComponent.OnEndDrag.Subscribe(OnEndDrag);
            dragComponent.Container.State.Value = DropComponent.DropState.StillHolding;

            foreach (var dropComponent in _dropComponents.Where(CanDrop)) {
                dropComponent.State.Value = DropComponent.DropState.Ready;
            }
        }

        private void OnEndDrag(DragComponent dragComponent) {
            // Debug.Log("End drag");

            dragComponent.transform.SetParent(_savedParent, true);
            dragComponent.Container = _currentDrop != null ? _currentDrop : dragComponent.Container;
            dragComponent.OnEndDrag.Unsubscribe(OnEndDrag);
            _isDragging = false;
            _currentDrop = null;

            foreach (var dropComponent in _dropComponents) {
                dropComponent.State.Value = DropComponent.DropState.Idle;
            }
        }

        private void OnSelectDrop(DropComponent dropComponent, bool selected) {
            if (!_isDragging || !CanDrop(dropComponent)) {
                return;
            }

            Debug.Log("OnSelectTarget");

            if (selected) {
                if (_currentDrop != null) {
                    _currentDrop.State.Value = DropComponent.DropState.Ready;
                }

                _currentDrop = dropComponent;
                _currentDrop.State.Value = DropComponent.DropState.CurrentTarget;
            } else if (_currentDrop == dropComponent) {
                _currentDrop.State.Value = DropComponent.DropState.Ready;
                _currentDrop = null;
            }
        }

        private static bool CanDrop(DropComponent drop) {
            return drop.HeldObject == null && drop.CanBeDroppedTo;
        }
    }
}