using System;
using DG.Tweening;
using GeneralUtils;
using GeneralUtils.Processes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts {
    public class TutorialController : MonoBehaviour {
        [SerializeField] private CanvasGroup _tutorialGroup;
        [SerializeField] private RectTransform _mask;
        [SerializeField] private Button _nextStepButton;
        [SerializeField] private TextMeshProUGUI _stepText;
        [SerializeField] private RectTransform _stepTextTransform;
        [SerializeField] private GameObject _nextStepText;
        [SerializeField] private TutorialStep[] _steps;
        [SerializeField] private TutorialStep[] _introSteps;

        private TutorialStep[] _currentSteps;

        private const float AnimationDuration = 0.4f;
        private int _currentStep;
        private Action _onDone;
        private Action _spawnAndWait;
        private Action<Action> _openTaskWindow;
        private Action<Action> _closeTaskWindow;

        private void Awake() {
            _nextStepButton.onClick.AddListener(NextStep);
            _tutorialGroup.gameObject.SetActive(false);
        }

        public void StartGameTutorial(Action spawnAndWait, Action continueSpawning, Action<Action> openTaskWindow, Action<Action> closeTaskWindow) {
            _spawnAndWait = spawnAndWait;
            _openTaskWindow = openTaskWindow;
            _closeTaskWindow = closeTaskWindow;

            StartTutorial(_steps, continueSpawning);
        }
        
        public void StartIntro(Action onDone) {
            StartTutorial(_introSteps, onDone);
        }

        private void StartTutorial(TutorialStep[] steps, Action onDone = null) {
            _currentSteps = steps;
            _currentStep = -1;
            _onDone = onDone;
            _tutorialGroup.alpha = 0f;
            _stepText.color = _stepText.color.WithAlpha(1f);
            NextStep();
        }

        private void NextStep() {
            if (++_currentStep >= _currentSteps.Length) {
                HideHider(_onDone);
                return;
            }

            _nextStepButton.enabled = false;
            _nextStepText.SetActive(false);

            var stepProcess = new SerialProcess();
            stepProcess.Add(AsyncProcess.From(ShowHider, _currentStep == 0));
            stepProcess.Add(new LazyProcess(() => {
                return _currentSteps[_currentStep].action switch {
                    TutorialAction.None => new DummyProcess(),
                    TutorialAction.SpawnAndWait => new SyncProcess(_spawnAndWait),
                    TutorialAction.OpenTaskWindow => new AsyncProcess(_openTaskWindow),
                    TutorialAction.CloseTaskWindow => new AsyncProcess(_closeTaskWindow),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }));
            stepProcess.Add(new SyncProcess(() => {
                _nextStepText.SetActive(true);
                _nextStepButton.enabled = true;
            }));
            stepProcess.Run();
        }

        private void ShowHider(bool initial, Action onDone) {
            var currentStep = _currentSteps[_currentStep];
            SetPositionSettings(_stepTextTransform, currentStep.position);

            if (initial) {
                _mask.position = currentStep.mask.position;
                _mask.sizeDelta = currentStep.mask.sizeDelta;
                _stepText.text = currentStep.text;
                _stepTextTransform.anchoredPosition = Vector2.zero;
                _tutorialGroup.gameObject.SetActive(true);
                _tutorialGroup.DOFade(1f, AnimationDuration).OnComplete(() => onDone?.Invoke());
                return;
            }

            DOTween.Sequence()
                .Insert(0f, _mask.DOMove(currentStep.mask.position, AnimationDuration))
                .Insert(0f, _mask.DOSizeDelta(currentStep.mask.sizeDelta, AnimationDuration))
                .Insert(0f, _stepTextTransform.DOAnchorPos(Vector2.zero, AnimationDuration))
                .Insert(0f, _stepText.DOFade(0f, AnimationDuration / 2f))
                .InsertCallback(AnimationDuration / 2f, () => _stepText.text = currentStep.text)
                .Insert(AnimationDuration / 2f, _stepText.DOFade(1f, AnimationDuration / 2f))
                .OnComplete(() => onDone?.Invoke());
        }

        private void HideHider(Action onDone) {
            _tutorialGroup.DOFade(0f, AnimationDuration).OnComplete(() => {
                _tutorialGroup.gameObject.SetActive(false);
                onDone?.Invoke();
            });
        }

        private static void SetPositionSettings(RectTransform textTransform, TextPosition position) {
            const float center = 0.5f;
            const float pivotDelta = 0.6f;

            switch (position) {
                case TextPosition.Down:
                    textTransform.pivot = new Vector2(center, center + pivotDelta);
                    textTransform.anchorMax = textTransform.anchorMin = new Vector2(center, 0f);
                    break;
                case TextPosition.Up:
                    textTransform.pivot = new Vector2(center, center - pivotDelta);
                    textTransform.anchorMax = textTransform.anchorMin = new Vector2(center, 1f);
                    break;
                case TextPosition.Left:
                    textTransform.pivot = new Vector2(center + pivotDelta, center);
                    textTransform.anchorMax = textTransform.anchorMin = new Vector2(0f, center);
                    break;
                case TextPosition.Right:
                    textTransform.pivot = new Vector2(center - pivotDelta, center);
                    textTransform.anchorMax = textTransform.anchorMin = new Vector2(1f, center);
                    break;
                case TextPosition.Center:
                    textTransform.pivot = new Vector2(center, center);
                    textTransform.anchorMax = textTransform.anchorMin = new Vector2(center, center);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
        }
        

        [Serializable]
        private struct TutorialStep {
            public RectTransform mask;
            [TextArea] public string text;
            public TutorialAction action;
            public TextPosition position;
        }

        private enum TutorialAction {
            None,
            SpawnAndWait,
            OpenTaskWindow,
            CloseTaskWindow
        }
        
        private enum TextPosition {
            Down,
            Up,
            Left,
            Right,
            Center
        }
    }
}