using System;
using _Game.Scripts.Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class TaskWindow : UIElement {
        [SerializeField] private Button _closeButton;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextMeshProUGUI _id;

        private const float AnimationTime = 0.4f;

        private void Awake() {
            _closeButton.onClick.AddListener(() => Hide());
        }

        public void Load(TaskData task) {
            _title.text = task.title;
            _text.text = task.text;
            _id.text = task.id;
        }

        protected override void PerformShow(Action onDone = null) {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.DOFade(1f, AnimationTime).OnComplete(() => {
                _group.interactable = true;
                onDone?.Invoke();
            });
        }

        protected override void PerformHide(Action onDone = null) {
            _group.interactable = false;
            _group.DOFade(0f, AnimationTime).OnComplete(() => {
                _group.interactable = true;
                onDone?.Invoke();
            });
        }
    }
}