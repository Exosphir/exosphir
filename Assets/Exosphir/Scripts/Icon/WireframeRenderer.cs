using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WireframeRenderer : MonoBehaviour {

	//public LineRenderer baseLine;

	public float lineWidth = 0.1f;
	public Material lineMaterial;

	private MeshFilter theFilter;
	private Mesh theMesh;

	private Vector3 oldPosition;
	private Quaternion oldRotation;

	private GameObject[] lineRendererGMs;

	void Start () {
		theFilter = GetComponent<MeshFilter>();
		theMesh = theFilter.mesh;

		lineRendererGMs = new GameObject[theMesh.triangles.Length];
		for (int i = 0; i < lineRendererGMs.Length; i++) {
			lineRendererGMs[i] = new GameObject("Line " + i);

			lineRendererGMs[i].transform.parent = transform;
			lineRendererGMs[i].transform.localPosition = Vector3.zero;
			lineRendererGMs[i].transform.localRotation = Quaternion.identity;

			lineRendererGMs[i].AddComponent<LineRenderer>();
			lineRendererGMs[i].GetComponent<LineRenderer>().SetVertexCount(3);
			lineRendererGMs[i].GetComponent<LineRenderer>().useWorldSpace = false;
			lineRendererGMs[i].GetComponent<LineRenderer>().SetWidth(lineWidth, lineWidth);
			lineRendererGMs[i].GetComponent<LineRenderer>().material = lineMaterial;
		}

		UpdateChange ();

		oldPosition = transform.position;
		oldRotation = transform.rotation;
	}

	void Update () {
		if (oldPosition != transform.position || oldRotation != transform.rotation) {
			UpdateChange ();
		}

		oldPosition = transform.position;
		oldRotation = transform.rotation;
	}

	void UpdateChange () {
		for (int t = 0; t < theMesh.triangles.Length; t++) {
			lineRendererGMs[t].GetComponent<LineRenderer>().SetPosition(0, theMesh.vertices[theMesh.triangles[t]]);
			lineRendererGMs[t].GetComponent<LineRenderer>().SetPosition(1, theMesh.vertices[theMesh.triangles[t+1]]);
			lineRendererGMs[t].GetComponent<LineRenderer>().SetPosition(2, theMesh.vertices[theMesh.triangles[t+2]]);
		}
	}
}
