using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ReadWriteWorld : MonoBehaviour {

	public int headerValue;

	private const float positionResolution = 10.0f;

	void OnGUI () {
		Rect guiRect = new Rect(Screen.width - 200.0f, 10.0f, 190.0f, 500.0f);
		GUILayout.BeginArea (guiRect);

		if (GUILayout.Button("Save World")) {
			WriteWorld();
		}

		if (GUILayout.Button("Read World")) {
			// Remove everything in the world before reading the saved world
			ClearWorld();
			ReadWorld();
		}

		if (GUILayout.Button("New World")) {
			ClearWorld();
		}

		GUILayout.EndArea();
	}

	void ClearWorld () {
		GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("ActiveBlock");
		int oldLength = objectsToDestroy.Length;
		
		for (int i = 0; i < oldLength; i++) {
			Destroy(objectsToDestroy[i].gameObject);
		}
	}

	//////////////////////////////////////
	/// Write World Stuff
	//////////////////////////////////////

	public void WriteWorld () {
		GameObject[] objectsToWrite = GameObject.FindGameObjectsWithTag("ActiveBlock");

		StreamWriter sw = new StreamWriter("TestHexValues.txt", false, Encoding.UTF8);

		string finalString = "";
		string blockHeader = char.ConvertFromUtf32(headerValue);

		for (int i = 0; i < objectsToWrite.Length; i++) {
			string transformString = EncodeTransform(objectsToWrite[i].transform);

			BlockCatalog.Block blockToEncode = BlockCatalog.FindBlock(objectsToWrite[i].name);
			if (blockToEncode == null) {
				Debug.LogError("Error finding corresponding block");
			}

			string idString = EncodeID(blockToEncode);

			finalString = finalString + blockHeader + idString + transformString;
		}

		sw.WriteLine(finalString);

		Debug.Log ("Finished Saving");

		sw.Close();
	}

	string EncodeID (BlockCatalog.Block block) {
		string finalString = char.ConvertFromUtf32(block.id + 33);

		return finalString;
	}

	string EncodeTransform (Transform trans) {
		string finalString = "";

		Vector3 tempPosition = new Vector3(trans.position.x + 1250.0f, trans.position.y + 1250.0f, trans.position.z + 1250.0f);
		Vector3 position = (tempPosition / 2.5f) * positionResolution;

		Vector3 rotation = trans.rotation.eulerAngles / 15.0f;

		int scale = (int)Mathf.Round(trans.localScale.x * 100.0f);

		List<int> decValues = new List<int>();
		decValues.Add((int)position.x);
		decValues.Add((int)position.y);
		decValues.Add((int)position.z);

		if (rotation.x == 0.0f && rotation.y == 0.0f && rotation.z == 0.0f) {
			// Put a placeholder value instead of saving 3 unnessisary values
			decValues.Add(24); // 24 = 360 / 15.  Rotation cannot be >= 360 because interpolation
		} else {
			decValues.Add((int)rotation.x);
			decValues.Add((int)rotation.y);
			decValues.Add((int)rotation.z);
		}

		decValues.Add(scale);

		for (int i = 0; i < decValues.Count; i++) {
			string stringValue = char.ConvertFromUtf32(decValues[i] + 33);  // All values will be added 33 because characters before character 33 are unreadable by the file reader
			
			finalString = finalString + stringValue;
		}

		return finalString;
	}

	//////////////////////////////////////
	/// Read World Stuff
	//////////////////////////////////////

	public void ReadWorld () {
		string[] encodedBlocks = GetEncodedBlocks("TestHexValues.txt");

		// Remove useless first element from string array
		List<string> list = new List<string>(encodedBlocks);
		list.RemoveAt(0);
		encodedBlocks = list.ToArray ();

		// Go through each block string
		foreach (string blockString in encodedBlocks) {
			// Get the block type
			int id = GetBlockID(blockString);
			BlockCatalog.Block theBlock = BlockCatalog.GetBlockFromID(id);

			// Get the position
			Vector3 pos = GetBlockPosition(blockString);

			// Get the rotation
			Vector3 rot = GetBlockRotation(blockString);

			// Get the scale
			float scale = GetBlockScale(blockString);

			// Spawn the block
			BlockManagement.PlaceBlockNoPooling(theBlock.model, pos, Quaternion.Euler(rot), Vector3.one * scale);
		}

		Debug.Log ("Loaded " + list.Count + " Blocks");
	}

	string[] GetEncodedBlocks (string path) {
		string text = File.ReadAllText(path, Encoding.Unicode);

		char header = char.ConvertFromUtf32(headerValue)[0];
		return text.Split(header);
	}

	int GetBlockID (string blockString) {
		return blockString[0] - 33;
	}

	Vector3 GetBlockPosition (string blockString) {
		Vector3 pos = new Vector3((((blockString[1] - 33.0f) / positionResolution) * 2.5f) - 1250.0f, (((blockString[2]  - 33.0f) / positionResolution) * 2.5f) - 1250.0f, (((blockString[3] - 33.0f) / positionResolution) * 2.5f) - 1250.0f);
		return pos;
	}

	Vector3 GetBlockRotation (string blockString) {
		Vector3 rot = Vector3.zero;
		if (blockString[4] - 33 != 24) {
			rot = new Vector3((blockString[4] - 33.0f) * 15.0f, (blockString[5] - 33.0f) * 15.0f, (blockString[6] - 33.0f) * 15.0f);
		}
		return rot;
	}

	float GetBlockScale (string blockString) {
		float scale = 0.0f;
		if (blockString[4] - 33 == 24) {
			scale = (blockString[5] - 33.0f) / 100.0f;
		} else {
			scale = (blockString[7] - 33.0f) / 100.0f;
		}
		return scale;
	}
}
