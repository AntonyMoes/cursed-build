using System;
using GeneralUtils;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class SimpleButton : MonoBehaviour {
        [SerializeField] private Button _button;

        private readonly Action<SimpleButton> _onClick;
        public readonly Event<SimpleButton> OnClick;

        public bool Interactable {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        public SimpleButton() {
            OnClick = new Event<SimpleButton>(out _onClick);
        }

        private void Awake() {
            _button.onClick.AddListener(() => {
                AudioController.Instance.PlaySound("click");
                _onClick(this);
            });
        }
    }
}