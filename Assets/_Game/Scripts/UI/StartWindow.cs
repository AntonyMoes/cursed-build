using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class StartWindow : UIElement {
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _exitButton;

        private Action _startGame;
        private Action _quit;

        private void Awake() {
            _startGameButton.onClick.AddListener(OnStart);
            _exitButton.onClick.AddListener(OnExit);
        }

        public void Load(Action startGame, Action quit) {
            _startGame = startGame;
            _quit = quit;
        }

        private void OnStart() {
            Hide(_startGame);
        }

        private void OnExit() {
            _quit?.Invoke();
        }
    }
}