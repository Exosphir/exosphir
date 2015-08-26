using UnityEngine;
using System.Collections;

public class BlockControl : MonoBehaviour {

	private ConfigurableInput input;

	public float lerpFloorDamping = 2.0f;
	public float moveLerp = 2.0f;

	public float minScale = 0.4f;
	public float maxScale = 5.0f;
	private float currentScale = 1.0f;

	private float floor = 500;
	private float lerpedFloor = 500.0f;
	private Vector3 collisionPoint;

	public Transform visualizerObject;

	public float lerpRotationDamping = 4.0f;

	public float rotate90Time = 0.25f;
	private int rotate90ButtonCount = 0;
	private float oldRotate90Time;

	private Vector3 finalScale = Vector3.one;
	private Vector3 finalRotation = Vector3.zero;
	private float scrollwheelSteps;

	private EditCamera editCam;
	private bool disableCamZoom;
	private bool oldDisableCamZoom;
	private bool disablePlacement;

	private GameObject theSelectedBlock;
	private GameObject oldSelectedBlock;

	public AudioClip rotateSound;
	[Range(0.0f, 1.0f)]
	public float rotateSoundVolume = 1.0f;

	public AudioSource cubeSoundSource;

	[HideInInspector]
	public Rect catalogRect;

	void Start () {
		input = ConfigurableInput.GetInstance();
		editCam = GetComponent<EditCamera>();

		oldRotate90Time = rotate90Time;
	}

	void Update () {
		disableCamZoom = false;

		theSelectedBlock = BlockCatalog.SelectedBlock();

		// Update floor positions for stuff
		UpdateFloorPositions();

		// Find point of collision for a Raycast and the build plane using Plane.Raycast()
		CalculateCollisionPointOnGrid();

		// Reset scale and rotation on change of selected block
		if (oldSelectedBlock != theSelectedBlock) {
			finalScale = theSelectedBlock.transform.localScale;
			finalRotation = theSelectedBlock.transform.rotation.eulerAngles;
			currentScale = 1.0f;
		}
		
		Vector3 visualizerObjectPos = Input.GetKey(input.turnOffSnap)? Grid.SnapToGrid(collisionPoint, Vector3.one * 0.25f) : Grid.SnapToGrid(collisionPoint, Vector3.one * 2.5f);
		visualizerObjectPos.y = collisionPoint.y;
		visualizerObject.position = Vector3.Lerp(visualizerObject.position, visualizerObjectPos, Time.deltaTime * moveLerp);

		// Move the sound to the cube
		cubeSoundSource.transform.position = visualizerObject.position;

		if (visualizerObject.childCount > 0) {
			Vector3 finalLocal = new Vector3(0.0f, 0.0f, finalRotation.z);
			Vector3 finalGlobal = new Vector3(0.0f, finalRotation.y, 0.0f);

			visualizerObject.GetChild(0).localRotation = Quaternion.Lerp(visualizerObject.GetChild(0).rotation, Quaternion.Euler(finalRotation), Time.deltaTime * lerpRotationDamping);
			//visualizerObject.GetChild(0).rotation = Quaternion.Lerp(visualizerObject.GetChild(0).rotation, Quaternion.Euler(finalGlobal), Time.deltaTime * lerpRotationDamping);
			visualizerObject.GetChild(0).localScale = finalScale;
		}

		// Place block if user clicks on grid and is not orbiting and not in catalogRect
		if (catalogRect.Contains(Input.mousePosition)) {
			disablePlacement = true;
		}

		if (Input.GetMouseButton(0) && !Input.GetButton("Fire2") && !disablePlacement) {
			visualizerObjectPos.y = Grid.SnapToGrid(collisionPoint, Vector3.one * (Mathf.Approximately(floor, Mathf.RoundToInt(floor))? 2.5f : 0.25f)).y;
			BlockManagement.PlaceBlock (theSelectedBlock, visualizerObjectPos, Quaternion.Euler(finalRotation), finalScale);
		}

		// Delete block on right click
		if (Input.GetMouseButton(1) && !disablePlacement) {
			visualizerObjectPos.y = Grid.SnapToGrid(collisionPoint, Vector3.one * (Mathf.Approximately(floor, Mathf.RoundToInt(floor))? 2.5f : 0.25f)).y;
			BlockManagement.RemoveBlocksAtPosition(visualizerObjectPos, 0.7f);
		}

		BlockCatalog.Block selectedBlock = BlockCatalog.SelectedBlockAsBlock();

		// Rotate rotate-able blocks
		if (Input.GetKey(input.rotateKey) && selectedBlock.rotateable) {
			disableCamZoom = true;

			// Using this idea of "steps", allows users using a trackpad or Apple Magic Mouse to rotate with percision
			scrollwheelSteps = scrollwheelSteps + Mathf.Abs(Input.GetAxis("Mouse ScrollWheel"));
			if (scrollwheelSteps > 1.0f) {
				// Play the sound
				cubeSoundSource.volume = rotateSoundVolume;
				cubeSoundSource.clip = rotateSound;
				cubeSoundSource.Play();
				
				if (Input.GetKey (input.secondaryRotate)) {
					finalRotation.z += 15.0f * Mathf.Sign(Input.GetAxis("Mouse ScrollWheel"));
				} else {
					finalRotation.y += 15.0f * Mathf.Sign(Input.GetAxis("Mouse ScrollWheel"));
				}

				scrollwheelSteps = 0.0f;
			}

			if (Input.GetAxis("Mouse ScrollWheel") == 0.0f) {
				scrollwheelSteps = 1.0f;
			}
		} else {
			scrollwheelSteps = 1.0f;
		}

		// Rotate rotate-able blocks 90 degrees if rotate button tapped
		if (selectedBlock.rotateable) {
			if (Input.GetKeyDown(input.rotateKey) || Input.GetKeyUp(input.rotateKey)){
				if (rotate90Time > 0.0f && rotate90ButtonCount == 1){
					if (Input.GetKey (input.secondaryRotate)) {
						finalRotation.z += 90.0f;
					} else {
						finalRotation.y += 90.0f;
					}
				} else {
					rotate90Time = oldRotate90Time;
					rotate90ButtonCount += 1;
				}
			}
			
			if (rotate90Time > 0.0f) {
				rotate90Time -= 1.0f * Time.deltaTime;
			} else {
				rotate90ButtonCount = 0;
			}
		}

		// Scale scale-able blocks
		if (Input.GetKey(input.scaleKey) && selectedBlock.scaleable) {
			disableCamZoom = true;

			currentScale += Input.GetAxis("Mouse ScrollWheel");
			currentScale = Mathf.Clamp(currentScale, minScale, maxScale);

			finalScale = theSelectedBlock.transform.localScale * currentScale;
		}

		// Disable zoom on edit cam for when the blocks are being manipulated
		if (oldDisableCamZoom != disableCamZoom) {
			if (disableCamZoom) {
				editCam.DisableZoom();
			} else {
				editCam.EnableZoom();
			}
		}

		oldDisableCamZoom = disableCamZoom;
		oldSelectedBlock = theSelectedBlock;
	
		// Reset disablePlacement
		disablePlacement = false;
	}

	void UpdateFloorPositions () {
		if (Input.GetKeyDown(input.upFloor)) {
			floor += (Input.GetKey (input.fastMovementKey)? 10 : 1);
			floor = Mathf.Round(floor);
		}
		if (Input.GetKeyDown(input.downFloor)) {
			floor -= (Input.GetKey (input.fastMovementKey)? 10 : 1);
			floor = Mathf.Round(floor);
		}

		// Fine floor moving
		if (Input.GetKey(input.fineFloor)) {
			disableCamZoom = true;

			// Using this idea of "steps", allows users using a trackpad or Apple Magic Mouse to rotate with percision
			scrollwheelSteps = scrollwheelSteps + Mathf.Abs(Input.GetAxis("Mouse ScrollWheel"));
			if (scrollwheelSteps > 1.0f) {
				floor += 0.1f * Mathf.Sign(Input.GetAxis("Mouse ScrollWheel"));
				
				scrollwheelSteps = 0.0f;
			}
			
			if (Input.GetAxis("Mouse ScrollWheel") == 0.0f) {
				scrollwheelSteps = 1.0f;
			}
		}

		lerpedFloor = Mathf.Lerp(lerpedFloor, floor, Time.deltaTime * lerpFloorDamping);
	}

	void CalculateCollisionPointOnGrid () {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane hPlane = new Plane(Vector3.up, new Vector3(0.0f, this.GetLerpedFloorInWorld(), 0.0f));
		
		float distance = 0; 
		
		if (hPlane.Raycast(ray, out distance)){
			collisionPoint = ray.GetPoint(distance);
		}
	}

	//////////////////////////////////////////
	// Helper functions

	Vector3 AddVector3ToFloat (Vector3 a, float b) {
		Vector3 final = a;

		final.x = final.x + b;
		final.y = final.y + b;
		final.z = final.z + b;

		return final;
	}

	//////////////////////////////////////////
	// Return functions for other scripts

	public static BlockControl GetInstance () {
		BlockControl[] bC = (BlockControl[])GameObject.FindObjectsOfType(typeof(BlockControl));
		if (bC != null && bC.Length == 1) {
			return bC[0];
		} else if (bC.Length > 1) {
			Debug.LogError("Too many BlockControls found");
			return null;
		} else {
			Debug.LogError("No type BlockControl found");
			return null;
		}
	}

	public Vector3 GetCollisionPoint () {
		return collisionPoint;
	}

	public float GetFloor () {
		return floor;
	}

	public float GetLerpedFloor () {
		return lerpedFloor;
	}

	public float GetFloorInWorld () {
		return (floor * 2.5f) - 1250.0f;
	}

	public float GetLerpedFloorInWorld () {
		return (lerpedFloor * 2.5f) - 1250.0f;
	}
}
