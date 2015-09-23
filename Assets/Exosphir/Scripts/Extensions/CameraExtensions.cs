using UnityEngine;
using System.Collections;

namespace Extensions {
	public class CameraExtensions : MonoBehaviour {

		void Update () {
			PointOfConvergence(GetComponent<Camera>());
		}

		public static Vector3 PointOfConvergence (Camera camera) {
			Ray cornerRay = camera.ScreenPointToRay(new Vector3(0, 0, 0));
			Ray centerRay = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

			// Calculate angles of the corner and center ray for the trig that is about to happen

			float xzCornerAngle = Vector2.Angle(new Vector2(centerRay.direction.x, centerRay.direction.z), new Vector2(cornerRay.direction.x, cornerRay.direction.z));
			float zyCornerAngle = Vector2.Angle(new Vector2(centerRay.direction.z, centerRay.direction.y), new Vector2(cornerRay.direction.z, cornerRay.direction.y));

			// Angle "i"
			float xzCenterAngle = Vector2.Angle(new Vector2(camera.transform.right.x, camera.transform.right.z), new Vector2(centerRay.direction.x, centerRay.direction.z));
			float zyCenterAngle = Vector2.Angle(new Vector2(camera.transform.right.z, camera.transform.right.y), new Vector2(centerRay.direction.z, centerRay.direction.y));

			// Angle "o"
			float xzOuterAngle = 180.0f - (xzCornerAngle + 90.0f);
			float zyOuterAngle = 180.0f - (zyCornerAngle + 90.0f);

			// Angle "v"
			float xzPointAngle = 180.0f - (xzOuterAngle + xzCenterAngle);
			float zyPointAngle = 180.0f - (zyOuterAngle + zyCenterAngle);

			// Get the distance from center point to corner point on both planes

			Vector2 halfScreen = new Vector2(Vector3.Distance(centerRay.origin, new Vector3(cornerRay.origin.x, centerRay.origin.y, cornerRay.origin.z)), Vector3.Distance(cornerRay.origin, new Vector3(cornerRay.origin.x, centerRay.origin.y, cornerRay.origin.z)));

			// Calculate the hypotenuse of the PoC calculation triangle

			float xzHypotenuse = (Mathf.Sin(xzCenterAngle * Mathf.Deg2Rad) * halfScreen.x) / Mathf.Sin(xzPointAngle * Mathf.Deg2Rad);
			float zyHypotenuse = (Mathf.Sin(zyCenterAngle * Mathf.Deg2Rad) * halfScreen.y) / Mathf.Sin(zyPointAngle * Mathf.Deg2Rad);

			// Calculate the final PoC by calculating the PoC on both planes

			Vector3 inverseCorner = -cornerRay.direction;

			Vector2 xzPoC = new Vector2(inverseCorner.x, inverseCorner.z) * xzHypotenuse;
			Vector2 zyPoC = new Vector2(inverseCorner.z, inverseCorner.y) * zyHypotenuse;  // zyPoc.x = xzPoc.y and therefore isnt used

			Vector3 pointOfConvergence = cornerRay.origin + new Vector3(xzPoC.x, zyPoC.y, xzPoC.y);

			// DEBUG

			//Debug.DrawRay(centerRay.origin, centerRay.direction, Color.yellow);

			Debug.Log ("cA " + xzCornerAngle + " o: " + xzOuterAngle + " i: " + xzCenterAngle + " v: " + xzPointAngle + " halfScreen: " + halfScreen.x + " hyp: " + xzHypotenuse);

			Debug.DrawRay(cornerRay.origin, inverseCorner * 2.0f, Color.green);
			Debug.DrawLine(cornerRay.origin, pointOfConvergence, Color.yellow);

			return pointOfConvergence;
		}
	}
}
