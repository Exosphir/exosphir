using UnityEngine;
using System.Collections;

namespace Extensions {

	public static class MatrixExtensions {

		// All the angles should be in degrees
		public static Vector3 RotatePoint (Vector3 objectPosition, Vector3 rotations) {
			// Make all the matrices identity matrices so that we dont have to worry about the 1's in the final rotation matrix
			Matrix4x4 x = Matrix4x4.identity;
			Matrix4x4 y = Matrix4x4.identity;
			Matrix4x4 z = Matrix4x4.identity;
			
			// Get rid of that one 1 in the corner that nobody likes
			x[3, 3] = 0;
			y[3, 3] = 0;
			z[3, 3] = 0;
			
			// https://en.wikipedia.org/wiki/Rotation_matrix#In_three_dimensions
			
			/* Set the x rotation matrix
		 * | 1       0       0   |
		 * | 0    cos(x) -sin(x) |
		 * | 0    sin(x)  cos(x) |
		*/
			
			x[1, 1] = Mathf.Cos (rotations.x * Mathf.Deg2Rad);
			x[2, 1] = Mathf.Sin (rotations.x * Mathf.Deg2Rad);
			x[1, 2] = -Mathf.Sin (rotations.x * Mathf.Deg2Rad);
			x[2, 2] = Mathf.Cos(rotations.x * Mathf.Deg2Rad);
			
			/* Set the y rotation matrix
		 * | cos(y)  0    sin(y) |
		 * | 0       1      0    |
		 * | -sin(y) 0    cos(y) |
		*/
			
			y[0, 0] = Mathf.Cos (rotations.y * Mathf.Deg2Rad);
			y[0, 2] = Mathf.Sin (rotations.y * Mathf.Deg2Rad);
			y[2, 0] = -Mathf.Sin (rotations.y * Mathf.Deg2Rad);
			y[2, 2] = Mathf.Cos (rotations.y * Mathf.Deg2Rad);
			
			/* Set the y rotation matrix
		 * | cos(z) -sin(z)  0   |
		 * | sin(z) cos(z)   0   |
		 * | 0        0      1   |
		*/
			
			z[0, 0] = Mathf.Cos (rotations.z * Mathf.Deg2Rad);
			z[0, 1] = -Mathf.Sin (rotations.z * Mathf.Deg2Rad);
			z[1, 0] = Mathf.Sin (rotations.z * Mathf.Deg2Rad);
			z[1, 1] = Mathf.Cos (rotations.z * Mathf.Deg2Rad);
			
			// Multiply all the matrices
			Matrix4x4 finalRotation = x * y * z;
			
			// Rotate position around center
			Vector3 finalPosition = finalRotation.MultiplyPoint3x4(objectPosition);
			
			return finalPosition;
		}
	}
}
