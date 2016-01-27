using UnityEngine;
using System.Collections;

namespace Edit.Backend {
	public class BlockUtilities : MonoBehaviour {

		public static float? CalculateAnchorHeight (GameObject model) {

			// Instanitate a instance of the model so shit will work
			GameObject modelInstance = (GameObject)GameObject.Instantiate(model, Vector3.zero, Quaternion.identity);

			// Find all renderers in object to consider
			Renderer[] boundsToConsider = modelInstance.GetComponentsInChildren<Renderer>();
			
			if (boundsToConsider.Length == 0)
				return null;
			
			Bounds theBound = boundsToConsider[0].bounds;

			// Mesh all the bounds into 1 bound
			if (boundsToConsider.Length > 0) {
				for (int i = 1; i < boundsToConsider.Length; i++) {
					theBound.Encapsulate(boundsToConsider[i].bounds);
				}
			}
			
			Vector3 bottomFacePos = theBound.center;
			bottomFacePos.y -= (theBound.size.y / 2.0f);
			
			Vector3 anchorPosition = modelInstance.transform.position;
			anchorPosition.y = bottomFacePos.y;
			
			float distance = Vector3.Distance(modelInstance.transform.position, anchorPosition);

			// Round to nearest 4th decimal place
			distance *= 10000.0f;
			distance = Mathf.Round(distance);
			distance /= 10000.0f;

			DestroyImmediate(modelInstance);
			
			return distance;
		}
	}
}
