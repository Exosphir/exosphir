using UnityEngine;
using System.Collections;

namespace Extensions {
	public static class GameObjectExtensions {

		// PerserveNameOnInstantiate is used so if we want to spawn something but dont want that stupid ass "(Clone)" tag at the end of the name upon Instantiate
		public static GameObject PerserveNameOnInstantiate (Object original, Vector3 position, Quaternion rotation) {
			GameObject theObject = (GameObject)GameObject.Instantiate(original, position, rotation);
			theObject.name = original.name;

			return theObject;
		}
	}
}
