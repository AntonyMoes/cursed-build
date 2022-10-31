using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Data;
using _Game.Scripts.Drag;
using _Game.Scripts.UI;
using DG.Tweening;
using GeneralUtils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Scripts {
    public class GameRunner : UIElement {
        [Header("UI")]
        [SerializeField] private ProgressBar _deathBar;
        [SerializeField] private TextMeshProUGUI _taskCounter;
        [SerializeField] private TaskWindow _taskWindow;
        [SerializeField] private CanvasGroup _group;
        
        [Header("Objects")]
        [SerializeField] private DropComponent[] _taskSlots;
        [SerializeField] private DepartmentSlot[] _departmentSlots;
        [SerializeField] private Task _taskPrefab;
        [FormerlySerializedAs("_spawnRoot")] [SerializeField] private Transform _moveRoot;
        [SerializeField] private RectTransform _buildPoint;
        [SerializeField] private TutorialController _tutorialController;

        [Header("Settings")]
        [SerializeField] private float _pointsToDie;
        [SerializeField] private float _pointsPerFail;
        [SerializeField] private float _pointsPerSuccess;
        [SerializeField] private float _pointsPerSecondCantSpawnTasks;
        [SerializeField] private float _spawnDelay;
        [SerializeField] private float _spawnDelayChange;

        private float _deathPoints;
        private float DeathPoints {
            get => _deathPoints;
            set {
                _deathPoints = Mathf.Clamp(value, 0f, _pointsToDie);
                _deathBarTween?.Kill();
                _deathBarTween = DOVirtual.Float(_deathBar.CurrentValue, _deathPoints, DeathBarAnimationDuration, 
                    val => _deathBar.CurrentValue = val);

                if (_deathPoints >= _pointsToDie) {
                    EndGame(false);
                }
            }
        }

        private int _finishedTasks;
        private int FinishedTasks {
            get => _finishedTasks;
            set {
                _finishedTasks = value;
                _taskCounter.text = _finishedTasks.ToString();
            }
        }

        private Action<bool, int> _onEnd;
        private Build _build;
        private bool _gameInProgress;
        private Rng _rng;
        private bool _cantSpawnTasks;
        private bool _spawnPaused;
        private Coroutine _spawnTasksCoroutine;
        private float _currentSpawnDelay;
        private readonly List<Task> _tasks = new List<Task>();
        private const float DeathBarAnimationDuration = 0.3f;
        private Tween _deathBarTween;
        private const float TaskDespawnDuration = 0.3f;

        public void StartGame(DataStorage dataStorage, Action<bool, int> onEnd, Build build, bool showTutorial) {
            _onEnd = onEnd;
            _build = build;

            _tasks.ToArray().ForEach(DestroyTask);
            
            _deathBar.Load(0f, _pointsToDie);
            _deathBar.CurrentValue = 0f;
            _deathPoints = 0f;
            FinishedTasks = 0;

            Show(() => PerformStartGame(dataStorage, showTutorial));
        }

        private void PerformStartGame(DataStorage dataStorage, bool showTutorial) {
            var tasks = dataStorage.Tasks;
            _rng = new Rng(Rng.RandomSeed);
            var shuffledTasks = _rng.NextShuffle(tasks);

            _departmentSlots.ForEach(slot => slot.slot.OnSetObject.Subscribe(OnTaskAssigned));
            _cantSpawnTasks = false;
            _gameInProgress = true;

            
            if (showTutorial) {
                AudioController.Instance.SetMusicVolume(0.3f);
                AudioController.Instance.PlayMusic();
                _spawnPaused = true;
                _tutorialController.StartGameTutorial(() => {
                    _spawnTasksCoroutine = StartCoroutine(SpawnTasks(shuffledTasks));
                }, () => {
                    AudioController.Instance.SetMusicVolume(1f, false);
                    _spawnPaused = false;
                }, onDone => ShowTaskWindow(_tasks.First().Data, onDone), _taskWindow.Hide);
            } else {
                AudioController.Instance.SetMusicVolume(1f);
                AudioController.Instance.PlayMusic();
                _spawnPaused = false;
                _spawnTasksCoroutine = StartCoroutine(SpawnTasks(shuffledTasks));
            }
        }

        private void EndGame(bool win) {
            _departmentSlots.ForEach(slot => slot.slot.OnSetObject.Unsubscribe(OnTaskAssigned));
            _gameInProgress = false;

            if (_spawnTasksCoroutine != null) {
                StopCoroutine(_spawnTasksCoroutine);
                _spawnTasksCoroutine = null;
            }

            Hide(() => {
                AudioController.Instance.SetMusicVolume(0.3f);
                _onEnd?.Invoke(win, FinishedTasks);
            });
        }

        private void OnTaskAssigned(DropComponent slot) {
            var taskObject = slot.HeldObject;
            if (taskObject == null) {
                return;
            }

            var task = taskObject.GetComponent<Task>();
            var department = _departmentSlots.First(departmentSlot => departmentSlot.slot == slot).department;

            if (task.Data.Department == department) {
                AudioController.Instance.PlaySound("correct");
                FinishedTasks++;
                DeathPoints -= _pointsPerSuccess;
            } else {
                AudioController.Instance.PlaySound("wrong");
                DeathPoints += _pointsPerFail;
            }

            task.DragComponent.Container = null;
            task.transform
                .DOScale(0, TaskDespawnDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => DestroyTask(task));
        }

        private void Update() {
            if (!_gameInProgress) {
                return;
            }

            if (_cantSpawnTasks) {
                DeathPoints += _pointsPerSecondCantSpawnTasks * Time.deltaTime;
            }
        }

        private IEnumerator SpawnTasks(IEnumerable<TaskData> tasks) {
            _currentSpawnDelay = _spawnDelay;
            
            foreach (var taskData in tasks) {
                yield return WaitForFreeSlot();

                var freeSlots = GetFreeSlots().ToArray();
                var slot = _rng.NextChoice(freeSlots);
                var task = Instantiate(_taskPrefab);
                _tasks.Add(task);
                task.Load(taskData);
                task.DragComponent.OnClick.Subscribe(_ => {
                    AudioController.Instance.PlaySound("click");
                    ShowTaskWindow(taskData);
                });

                _build.AnimateSpawnTask(task, _moveRoot, slot);

                yield return new WaitWhile(() => _spawnPaused);
                yield return new WaitForSeconds(_currentSpawnDelay);
                _currentSpawnDelay -= _spawnDelayChange;
            }

            yield return new WaitUntil(() => _tasks.Count == 0);
            EndGame(true);
        }

        private void ShowTaskWindow(TaskData data, Action onDone = null) {
            _taskWindow.Load(data);
            _taskWindow.Show(onDone);
        }

        private void DestroyTask(Task task) {
            _tasks.Remove(task);
            Destroy(task.gameObject);
        }

        private IEnumerator WaitForFreeSlot() {
            if (GetFreeSlots().Any()) {
                yield break;
            }

            _cantSpawnTasks = true;
            yield return new WaitUntil(() => GetFreeSlots().Any());
            _cantSpawnTasks = false;
        }

        private IEnumerable<DropComponent> GetFreeSlots() {
            return _taskSlots.Where(slot => slot.HeldObject == null);
        }

        protected override void PerformShow(Action onDone = null) {
            _group.alpha = 0f;
            _group.interactable = false;

            const float duration = 0.5f;
            DOTween.Sequence()
                .Append(_build.transform.DOMove(_buildPoint.position, duration).SetEase(Ease.InOutSine))
                .Append(_group.DOFade(1f, duration))
                .OnComplete(() => {
                    _group.interactable = true;
                    onDone?.Invoke();
                });
        }

        protected override void PerformHide(Action onDone = null) {
            _group.interactable = false;

            const float duration = 0.5f;
            _group.DOFade(0f, duration)
                .OnComplete(() => onDone?.Invoke());
        }

        [Serializable]
        private struct DepartmentSlot {
            public Department department;
            public DropComponent slot;
        }
    }
}