using _Game.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts {
    public class App : MonoBehaviour {
        [SerializeField] private DataStorage _dataStorage;
        [SerializeField] private StartWindow _startWindow;
        [SerializeField] private RestartWindow _restartWindow;
        [SerializeField] private GameRunner _gameRunner;
        [SerializeField] private TutorialController _tutorialController;
        [SerializeField] private Build _build;

        private const int HadValue = 100;
        private const int DidNotHaveValue = -66;
        private const string TutorialKey = "TUTORIAL";
        private const string IntroKey = "INTRO";

        private void Start() {
#if UNITY_EDITOR
            PlayerPrefs.DeleteAll();
#endif

            _dataStorage.Init();

            var showIntro = PlayerPrefs.GetInt(IntroKey, DidNotHaveValue) != HadValue;
            if (showIntro) {
                _tutorialController.StartIntro(() => {
                    PlayerPrefs.SetInt(IntroKey, HadValue);
                    _startWindow.Load(StartGame, OnQuit, _build);
                    _startWindow.Show();
                });
            } else {
                _startWindow.Load(StartGame, OnQuit, _build);
                _startWindow.Show();
            }

        }

        private void StartGame() {
            var showTutorial = PlayerPrefs.GetInt(TutorialKey, DidNotHaveValue) != HadValue;
            _gameRunner.StartGame(_dataStorage, OnEnd, _build, showTutorial);
            PlayerPrefs.SetInt(TutorialKey, HadValue);
        }

        private void OnEnd(bool win, int score) {
            _restartWindow.Load(win, score, StartGame, OnQuit, _build);
            _restartWindow.Show();
        }

        private void OnQuit() {
            Application.Quit();
        }
    }
}