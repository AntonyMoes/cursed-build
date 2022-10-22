using UnityEngine;

namespace _Game.Scripts {
    public static class InputHelper {
        public static bool GetTouch(out Vector2 position) {
            if (Input.GetMouseButton(0)) {
                position = Input.mousePosition;
                return true;
            }

            if (Input.touchCount > 0) {
                position = Input.GetTouch(0).position;
                return true;
            }

            position = Vector2.zero;
            return false;
        }
    }
}