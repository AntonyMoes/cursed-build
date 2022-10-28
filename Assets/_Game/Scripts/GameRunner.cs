﻿using System;
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
        [SerializeField] private Transform _spawnRoot;
        [SerializeField] private RectTransform _buildPoint;

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
        private Build _build;
        private bool _gameInProgress;
        private Rng _rng;
        private bool _cantSpawnTasks;
        private Coroutine _spawnTasksCoroutine;
        private float _currentSpawnDelay;
        private readonly List<Task> _tasks = new List<Task>();

        public void StartGame(DataStorage dataStorage, Action<bool, int> onEnd, Build build) {
            _onEnd = onEnd;
            _build = build;

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

        protected override void PerformShow(Action onDone = null) {
            // _build.transform.position = _buildPoint.position;
            // _build.RectTransform.anchoredPosition += Vector2.up * Screen.height;
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