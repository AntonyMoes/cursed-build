using _Game.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts {
    public class App : MonoBehaviour {
        [SerializeField] private DataStorage _dataStorage;
        [SerializeField] private StartWindow _startWindow;
        [SerializeField] private RestartWindow _restartWindow;
        [SerializeField] private GameRunner _gameRunner;

        private void Start() {
            _dataStorage.Init();

            _startWindow.Load(StartGame, OnQuit);
            _startWindow.Show();
        }

        private void StartGame() {
            _gameRunner.StartGame(_dataStorage, OnEnd);
            // (restart, exit) => _restartWindow.Load(restart, exit)
        }

        private void OnEnd(bool win, int score) {
            _restartWindow.Load(win, score, StartGame, OnQuit);
            _restartWindow.Show();
        }

        private void OnQuit() {
            Application.Quit();
        }
    }
}