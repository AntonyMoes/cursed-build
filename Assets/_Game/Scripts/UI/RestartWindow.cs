using System;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class RestartWindow : UIElement {
        [SerializeField] private TextMeshProUGUI _score;
        [SerializeField] private Button _restartGameButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private RectTransform _buildPoint;
        [SerializeField] private CanvasGroup _group;

        private Action _restartGame;
        private Action _quit;
        private Build _build;

        private void Awake() {
#if UNITY_WEBGL
            _exitButton.gameObject.SetActive(false);
#endif

            _restartGameButton.onClick.AddListener(OnRestart);
            _exitButton.onClick.AddListener(OnExit);
        }

        public void Load(bool win, int score, Action restartGame, Action quit, Build build) {
            _restartGame = restartGame;
            _quit = quit;
            _build = build;

            _score.text = Regex.Replace(_score.text, "[0-9]+", score.ToString());
        }

        protected override void PerformShow(Action onDone = null) {
            _group.alpha = 0f;
            _group.interactable = false;

            const float duration = 0.5f;
            DOTween.Sequence()
                .Insert(0f, _group.DOFade(1f, duration))
                .Insert(0f, _build.transform.DOMove(_buildPoint.position, duration).SetEase(Ease.InOutSine))
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

        private void OnRestart() {
            Hide(_restartGame);
        }

        private void OnExit() {
            _quit?.Invoke();
        }
    }
}