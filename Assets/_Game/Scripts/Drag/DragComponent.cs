using System;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts.Drag {
    public class DragComponent : MonoBehaviour {
        [SerializeField] private DropComponent _debugCurrentDrop;
        [SerializeField] private string _type;
        public string Type => _type;
        
        private readonly Action<DragComponent> _onStartDrag;
        public readonly Event<DragComponent> OnStartDrag;

        private readonly Action<DragComponent> _onEndDrag;
        public readonly Event<DragComponent> OnEndDrag;

        private DropComponent _dropComponent;

        public DropComponent Container {
            get => _dropComponent;
            set {
                if (_dropComponent != null) {
                    _dropComponent.HeldObject = null;
                }

                _dropComponent = value;
                _dropComponent.HeldObject = this;

                transform.SetParent(_dropComponent.transform);
                ((RectTransform) transform).anchoredPosition = Vector2.zero;
            }
        }

        private const float MaxDelta = 1f;

        private Vector2? _initialDragPosition;
        private Vector3 _dragDelta;
        private bool _startedDrag;
        private bool _justFinishedDrag;

        public DragComponent() {
            OnStartDrag = new Event<DragComponent>(out _onStartDrag);
            OnEndDrag = new Event<DragComponent>(out _onEndDrag);
        }

        private void Start() {
            Container = _debugCurrentDrop;
            DragController.Instance.Register(this);
        }

        private void OnDestroy() {
            DragController.Instance.Unregister(this);
        }
        
        private void OnMouseDrag() {
            if (_justFinishedDrag) {
                _justFinishedDrag = false;
                return;
            }

            if (!InputHelper.GetTouch(out var position)) {
                return;
            }

            var rectTransform = (RectTransform) transform;
            var parentTransform = (RectTransform) rectTransform.parent;
            // var positionInParent = (Vector2) parentTransform.TransformPoint(position);
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(parentTransform, position, Camera.main, out var positionInParent);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(parentTransform, position, Camera.main, out var dragPosition);

            if (_initialDragPosition == null) {
                _initialDragPosition = position;
                _dragDelta = rectTransform.position - dragPosition;
            }

            if ((position - _initialDragPosition.Value).magnitude <= MaxDelta && !_startedDrag) {
                return;
            }

            if (!_startedDrag) {
                _startedDrag = true;
                _onStartDrag(this);
            }

            rectTransform.position = dragPosition + _dragDelta;
        }

        private void EndDrag() {
            if (_initialDragPosition != null) {
                _initialDragPosition = null;

                _startedDrag = false;
                _justFinishedDrag = true;

                _onEndDrag(this);
            }
        }

        private void OnMouseUp() {
            EndDrag();
        }
    }
}