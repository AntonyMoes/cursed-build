using UnityEngine;

namespace _Game.Scripts {
    public class Build : MonoBehaviour {
        [SerializeField] private Animator _animator;
        public Animator Animator => _animator;

        public RectTransform RectTransform => (RectTransform) transform;
    }
}