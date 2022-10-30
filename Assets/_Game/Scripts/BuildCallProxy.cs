using System;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class BuildCallProxy : MonoBehaviour {
        private readonly Action _onReadyToMoveTask;
        public readonly GeneralUtils.Event OnReadyToMoveTask;

        public BuildCallProxy() {
            OnReadyToMoveTask = new GeneralUtils.Event(out _onReadyToMoveTask);
        }

        private void ReadyToMoveTask() {
            _onReadyToMoveTask();
        }
    }
}