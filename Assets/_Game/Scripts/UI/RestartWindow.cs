using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class RestartWindow : UIElement {
        [SerializeField] private TextMeshProUGUI _score;
        [SerializeField] private Button _restartGameButton;
        [SerializeField] private Button _exitButton;

        private Action _restartGame;
        private Action _quit;

        private void Awake() {
            _restartGameButton.onClick.AddListener(OnRestart);
            _exitButton.onClick.AddListener(OnExit);
        }

        public void Load(bool win, int score, Action restartGame, Action quit) {
            _restartGame = restartGame;
            _quit = quit;

            _score.text = Regex.Replace(_score.text, "[0-9]+", score.ToString());
        }

        private void OnRestart() {
            Hide(_restartGame);
        }

        private void OnExit() {
            _quit?.Invoke();
        }
    }
}