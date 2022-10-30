using _Game.Scripts.Drag;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts {
    public class Build : MonoBehaviour {
        [SerializeField] private Transform _taskRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private BuildCallProxy _callProxy;
        public Animator Animator => _animator;

        public RectTransform RectTransform => (RectTransform) transform;

        private Task _task;
        private DropComponent _slot;
        private Transform _moveRoot;
        private static readonly int SpawnTrigger = Animator.StringToHash("spawn");

        private void Awake() {
            _callProxy.OnReadyToMoveTask.Subscribe(OnReadyToMoveTask);
        }

        public void AnimateSpawnTask(Task task, Transform moveRoot, DropComponent slot) {
            var taskTransform = task.transform;
            taskTransform.SetParent(_taskRoot);
            taskTransform.localPosition = Vector3.zero;
            taskTransform.localScale = Vector3.one * 0.5f;
            task.Enabled = false;

            _task = task;
            _slot = slot;
            _moveRoot = moveRoot;

            _animator.SetTrigger(SpawnTrigger);
        }

        private void OnReadyToMoveTask() {
            const float duration = 0.5f;

            var taskTransform = _task.transform;

            DOTween.Sequence()
                .Insert(0f, taskTransform.DOMove(_slot.transform.position, duration))
                .Insert(0f, taskTransform.DOScale(Vector3.one, duration))
                .InsertCallback(duration * 0.06f, () => taskTransform.SetParent(_moveRoot, true))
                .OnComplete(() => {
                    _task.DragComponent.Container = _slot;
                    _task.Enabled = true;
                });
        }
    }
}