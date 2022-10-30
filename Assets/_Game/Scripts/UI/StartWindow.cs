﻿using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class StartWindow : UIElement {
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private RectTransform _buildPoint;
        [SerializeField] private CanvasGroup _group;

        private Action _startGame;
        private Action _quit;
        private Build _build;

        private void Awake() {
            _startGameButton.onClick.AddListener(OnStart);
            _exitButton.onClick.AddListener(OnExit);
        }

        public void Load(Action startGame, Action quit, Build build) {
            _startGame = startGame;
            _quit = quit;
            _build = build;
        }

        protected override void PerformShow(Action onDone = null) {
            _build.transform.position = _buildPoint.position;
            _build.RectTransform.anchoredPosition += Vector2.up * Screen.height;
            _group.alpha = 0f;
            _group.interactable = false;

            const float duration = 0.5f;
            DOTween.Sequence()
                .Append(_group.DOFade(1f, duration))
                .Append(_build.transform.DOMove(_buildPoint.position, duration).SetEase(Ease.OutBack))
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

        private void OnStart() {
            Hide(_startGame);
        }

        private void OnExit() {
            _quit?.Invoke();
        }
    }
}