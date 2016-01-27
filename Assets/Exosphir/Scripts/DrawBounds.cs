using UnityEngine;
using System.Collections;

public class DrawBounds : MonoBehaviour {

	void OnDrawGizmosSelected() {

		Renderer[] boundsToConsider = transform.GetComponentsInChildren<Renderer>();

		if (boundsToConsider.Length != 0) {
			Bounds theBound = boundsToConsider[0].bounds;

			for (int i = 1; i < boundsToConsider.Length; i++) {
				theBound.Encapsulate(boundsToConsider[i].bounds);
			}

			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(theBound.center, theBound.size);
			
			Vector3 bottomFacePos = theBound.center;
			bottomFacePos.y -= (theBound.size.y / 2.0f);
			
			Vector3 bottomFaceSize = theBound.size;
			bottomFaceSize.y = 0.0f;
			
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(bottomFacePos, bottomFaceSize);
			
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, 0.1f);
			
			Vector3 anchorPosition = transform.position;
			anchorPosition.y = bottomFacePos.y;
			
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(anchorPosition, 0.1f);
			
			Gizmos.DrawLine(transform.position, anchorPosition);
		}
	}
}
