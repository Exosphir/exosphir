using UnityEngine;
using System.Collections;

public class SkyboxControl : MonoBehaviour {

	public Transform rigParent;
	public Transform sunParent;

	public ReflectionProbe skyboxReflection;

	public Material skybox;
	public Rect area = new Rect(10, 10, 200, 400);

	private Material skyboxInstance;

	void Start () {
		// Copy the skybox material to a new instance so the original skybox material is not modified and saved by accident
		skyboxInstance = new Material(skybox.shader);
		skyboxInstance.CopyPropertiesFromMaterial(skybox);
	}

	void OnGUI () {
		GUILayout.BeginArea(area);
		GUILayout.BeginVertical("box");


		Vector3 sunRotation = sunParent.localRotation.eulerAngles;
		Vector3 rigRotation = rigParent.rotation.eulerAngles;

		if (sunRotation.x > 90.0f) {
			sunRotation.x -= 360.0f;
		}

		GUILayout.Label("Sun Height");
		sunRotation.x = GUILayout.HorizontalSlider(sunRotation.x, -90.0f, 90.0f);
		GUILayout.Label("Sun Rotation");
		rigRotation.y = GUILayout.HorizontalSlider(rigRotation.y, 0.0f, 360.0f);

		sunParent.localRotation = Quaternion.Euler(sunRotation);
		rigParent.rotation = Quaternion.Euler(rigRotation);

		GUILayout.Label("Atmosphere Thickness");
		skyboxInstance.SetFloat ("_AtmosphereThickness", GUILayout.HorizontalSlider(skyboxInstance.GetFloat ("_AtmosphereThickness"), 0.0f, 5.0f));

		GUILayout.Label("Exposure");
		skyboxInstance.SetFloat ("_Exposure", GUILayout.HorizontalSlider(skyboxInstance.GetFloat ("_Exposure"), 0.0f, 8.0f));

		GUILayout.EndVertical();
		GUILayout.EndArea();

		if (GUI.changed) {
			skyboxReflection.RenderProbe();
		}

		RenderSettings.skybox = skyboxInstance;
	}
}
