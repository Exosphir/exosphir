using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UndoRedo))]
public class UndoRedoInspector : Editor {

	UndoRedo myTarget;

	void OnEnable () {
		myTarget = (UndoRedo)target;
	}
	
	public override void OnInspectorGUI () {

	}
}
