using UnityEngine;
using UnityEngine.EventSystems;

namespace Extensions {
    public static class MonoBehaviourExtensions {
        public static bool PointerOnWorld(this MonoBehaviour self) {
            //note that "GameObject" in this method's name means
            //a EventSystem enabled object (a UI object).
            //So if its not over a UI object, it's on the world!
            return !EventSystem.current.IsPointerOverGameObject();
        }
    }
}
