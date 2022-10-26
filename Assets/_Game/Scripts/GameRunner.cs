using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Data;
using _Game.Scripts.Drag;
using _Game.Scripts.UI;
using GeneralUtils;
using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class GameRunner : UIElement {
        [Header("UI")]
        [SerializeField] private ProgressBar _deathBar;
        [SerializeField] private TextMeshProUGUI _taskCounter;
        [SerializeField] private TaskWindow _taskWindow;
        
        [Header("Objects")]
        [SerializeField] private DropComponent[] _taskSlots;
        [SerializeField] private DepartmentSlot[] _departmentSlots;
        [SerializeField] private Task _taskPrefab;
        [SerializeField] private Transform _spawnRoot;

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
                _deathPoints = Mathf.Clamp(value, 0f, _pointsToDie);;
                _deathBar.CurrentValue = _deathPoints;

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
        private bool _gameInProgress;
        private Rng _rng;
        private bool _cantSpawnTasks;
        private Coroutine _spawnTasksCoroutine;
        private float _currentSpawnDelay;
        private readonly List<Task> _tasks = new List<Task>();

        public void StartGame(DataStorage dataStorage, Action<bool, int> onEnd) {
            _onEnd = onEnd;

            _tasks.ToArray().ForEach(DestroyTask);

            InitUI();
            DeathPoints = 0f;
            FinishedTasks = 0;

            Show(() => PerformStartGame(dataStorage));
        }

        private void PerformStartGame(DataStorage dataStorage) {
            var tasks = dataStorage.Tasks;
            _rng = new Rng(Rng.RandomSeed);
            var shuffledTasks = _rng.NextShuffle(tasks);

            _departmentSlots.ForEach(slot => slot.slot.OnSetObject.Subscribe(OnTaskAssigned));
            _cantSpawnTasks = false;
            _gameInProgress = true;

            _spawnTasksCoroutine = StartCoroutine(SpawnTasks(shuffledTasks));
        }

        private void EndGame(bool win) {
            _departmentSlots.ForEach(slot => slot.slot.OnSetObject.Unsubscribe(OnTaskAssigned));
            _gameInProgress = false;

            StopCoroutine(_spawnTasksCoroutine);

            Hide(() => _onEnd?.Invoke(win, FinishedTasks));
        }

        private void InitUI() {
            _deathBar.Load(0f, _pointsToDie);
        }

        private void OnTaskAssigned(DropComponent slot) {
            var taskObject = slot.HeldObject;
            if (taskObject == null) {
                return;
            }

            var task = taskObject.GetComponent<Task>();
            var department = _departmentSlots.First(departmentSlot => departmentSlot.slot == slot).department;

            if (task.Data.Department == department) {
                FinishedTasks++;
                DeathPoints -= _pointsPerSuccess;
            } else {
                DeathPoints += _pointsPerFail;
            }

            DestroyTask(task);
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
                var task = Instantiate(_taskPrefab, _spawnRoot);
                _tasks.Add(task);
                task.Load(taskData);
                task.DragComponent.Container = slot;
                task.DragComponent.OnClick.Subscribe(_ => {
                    _taskWindow.Load(taskData);
                    _taskWindow.Show();
                });

                yield return new WaitForSeconds(_currentSpawnDelay);
                _currentSpawnDelay -= _spawnDelayChange;
            }
            
            yield return new WaitUntil(() => _tasks.Count == 0);
            EndGame(true);
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

        [Serializable]
        private struct DepartmentSlot {
            public Department department;
            public DropComponent slot;
        }
    }
}