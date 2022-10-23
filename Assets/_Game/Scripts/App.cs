using UnityEngine;

namespace _Game.Scripts {
    public class App : MonoBehaviour {
        [SerializeField] private DataStorage _dataStorage;
        [SerializeField] private GameRunner _gameRunner;

        private void Start() {
            _dataStorage.Init();
            _gameRunner.StartGame(_dataStorage);
        }
    }
}