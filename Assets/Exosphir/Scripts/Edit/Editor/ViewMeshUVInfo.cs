using UnityEngine;
using UnityEditor;
using System.Collections;

//This is going to be an EditorWindow so we need to make sure that we extend from the EditorWindow class
public class ViewMeshUVInfo :  EditorWindow
{
	//We are going to be getting all of the displayed data from a MeshFilter, so lets store it here
	private MeshFilter m_viewedMesh;
	//We also want the window to be scrollable is there is a lot of data that is being displayed
	private Vector2 m_scrollPosition;
	//Since we are displaying mesh data, we can also show the last triangle that the user clicked on, that way we can show them the vertex indices if they need them, so we would store the displayed string here
	private string lastSelectedTri = "";
	//We also want to limit the maximum size of the handles being drawn, so we will store a float here for the maximum size and then set that value when the window is initialized
	private static float m_maxHandlesSize;
	//The UVs will also be drawn to the right of the data being displayed, so we need to have a base offset in the X direction
	private static float m_handlesXOffset;
	
	//Lastly since we are displaying all of the mesh data, lets store it in a string here
	string meshInfoString = "";
	
	//To access this lets create a function called Init() and give it a MenuItem attribute
	[MenuItem("Mesh Info/View UV info")]
	static void Init()
	{  
		//Now to make things easier with drawing etc, lets grab the window and make sure that it can't be scaled
		ViewMeshUVInfo m_window = EditorWindow.GetWindow<ViewMeshUVInfo>();
		
		//setting the min and max size of the window will make it so that scaling the window will have no effect 
		m_window.minSize = new Vector2(1180, 700);
		m_window.maxSize = new Vector2(1181, 701);  
		
		//Now we set those offset values since we have initialized the window
		m_maxHandlesSize = m_window.maxSize.y - 30;
		m_handlesXOffset = m_window.maxSize.x * 0.418f;
	}
	
	//During OnGUI there will be a point where we check to see if the mouse is within one of the triangles displayed in the UVs. This function will return true if it is and false if it isn't. This is just a basic triangle point test.
	bool isMousePointWithinTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 vect1 = p2 - p1, vect2 = p3 - p1;
		Vector3 mouseFromP1 = (Vector3 )Event.current.mousePosition - p1;
		
		float u = (Vector3.Dot(vect2, vect2) * Vector3.Dot(mouseFromP1, vect1) - Vector3.Dot(vect2, vect1) * Vector3.Dot(mouseFromP1, vect2)) / (Vector3.Dot(vect1, vect1) * Vector3.Dot(vect2, vect2) - Vector3.Dot(vect1, vect2) * Vector3.Dot(vect2, vect1));
		
		float v = (Vector3.Dot(vect1, vect1) * Vector3.Dot(mouseFromP1, vect2) - Vector3.Dot(vect1, vect2) * Vector3.Dot(mouseFromP1, vect1)) / (Vector3.Dot(vect1, vect1) * Vector3.Dot(vect2, vect2) - Vector3.Dot(vect1, vect2) * Vector3.Dot(vect2, vect1));
		
		return(u >= 0) && (v >= 0) && (u + v < 1);
	}
	
	//Now everything else we do is in the OnGUI function. 
	void OnGUI()
	{
		//Lets start a check to see if anything changed
		EditorGUI.BeginChangeCheck();
		
		m_viewedMesh = EditorGUILayout.ObjectField("Mesh: ", m_viewedMesh, typeof(MeshFilter), true) as MeshFilter;
		
		//if the mesh changed we set up the information string to display all of the triangles, vertices and UVs
		if(EditorGUI.EndChangeCheck())
		{
			if(m_viewedMesh != null)
			{
				meshInfoString = "Triangles: \n";
				for(int i = 0; i < m_viewedMesh.sharedMesh.triangles.Length; i+= 3)
				{
					meshInfoString += "\n\nTriangle" + i / 3 + ": \n";
					meshInfoString += "Vertices: (" + m_viewedMesh.sharedMesh.vertices[m_viewedMesh.sharedMesh.triangles[i]].x + " , " + m_viewedMesh.sharedMesh.vertices[m_viewedMesh.sharedMesh.triangles[i]].y + "), (" +
						m_viewedMesh.sharedMesh.vertices[m_viewedMesh.sharedMesh.triangles[i + 1]].x + " , " + m_viewedMesh.sharedMesh.vertices[m_viewedMesh.sharedMesh.triangles[i + 1]].y + "), (" +
							m_viewedMesh.sharedMesh.vertices[m_viewedMesh.sharedMesh.triangles[i + 2]].x + " , " + m_viewedMesh.sharedMesh.vertices[m_viewedMesh.sharedMesh.triangles[i + 2]].y + ")\n";
					
					meshInfoString += "UVs: (" + m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].x + " , " + m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].y + "), (" +
						m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].x + " , " + m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].y + "), (" +
							m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].x + " , " + m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].y + ")";
					
				}
			}
		}   
		
		//if the mesh attached to this editor isn't null we display all of the data
		if(m_viewedMesh != null)
		{
			m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
			
			GUILayout.Label("The Mesh has: ");
			GUILayout.Label(m_viewedMesh.sharedMesh.vertexCount + " vertices");
			
			GUILayout.Label(m_viewedMesh.sharedMesh.uv.Length + " UVs");
			
			GUILayout.Label((m_viewedMesh.sharedMesh.triangles.Length / 3) + " triangles\n");
			
			GUILayout.Label("Legend: Blue = First Vertice, Red = Second Vertice, Yellow = Last Vertice\n");
			
			if(lastSelectedTri != "")
			{
				GUILayout.Label(lastSelectedTri + "\n");
			}
			
			GUILayout.Label(meshInfoString, GUILayout.ExpandHeight(true));
			
			//The first thing that we are going to draw is a grid where we want to display the UVs  
			Handles.color = Color.gray;
			
			for(int i = 1; i < 16; i++)
			{
				Handles.DrawLine(new Vector3 (m_handlesXOffset + m_maxHandlesSize * (i / 16f), m_scrollPosition.y), new  Vector3 (m_handlesXOffset + m_maxHandlesSize * (i / 16f), m_scrollPosition.y + m_maxHandlesSize));
				Handles.DrawLine(new Vector3 (m_handlesXOffset, m_scrollPosition.y + m_maxHandlesSize * (i / 16f)), new  Vector3 (m_handlesXOffset + m_maxHandlesSize, m_scrollPosition.y + m_maxHandlesSize * (i / 16f)));
				
			}
			bool clickedTri = false;
			
			//Next we will use handles to draw UV triangle
			for(int i = 0; i < m_viewedMesh.sharedMesh.triangles.Length; i+= 3)
			{
				Vector2 vert1= new Vector2 (m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].x * m_maxHandlesSize + m_handlesXOffset, (1 - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].y) * m_maxHandlesSize + m_scrollPosition.y); 
				Vector2 vert2 = new Vector2 (m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].x * m_maxHandlesSize + m_handlesXOffset, (1 - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].y) * m_maxHandlesSize + m_scrollPosition.y); 
				Vector2 vert3 = new Vector2 (m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].x * m_maxHandlesSize + m_handlesXOffset, (1 - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].y) * m_maxHandlesSize + m_scrollPosition.y);
				
				//while we are drawing it we will also check to see where the mouse is, and if the mouse is inside the displayed triangle
				bool thisTri = isMousePointWithinTriangle(vert1, vert2, vert3);
				
				if(thisTri)
				{ 
					//if the mouse was clicked, lets show the user the vertex indices of the triangle that was clicked on
					if(Event.current.type == EventType.mouseDown)
					{
						clickedTri = true;
						lastSelectedTri = "Vert indexes are: " + m_viewedMesh.sharedMesh.triangles[i] + ", " + m_viewedMesh.sharedMesh.triangles[i + 1] + ", " + m_viewedMesh.sharedMesh.triangles[i + 2];
					}
					
					//to show that the mouse is hovering over this triangle lets turn it green and highlight the vertices
					Handles.color = Color.green;
					//To draw the triangle, we simply call Handles.DrawPolyLine, which draws a line from one point to another in the order that they are passed
					Handles.DrawPolyLine(vert1, vert2, vert3, vert1);
					
					Handles.color = Color.blue;
					//To highlight the vertices of the selected triangle, what we are going to do is draw a very small line using a similar function as before, but this time we will use Handles.DrawAAPolyLine to allow us to set a width for the line
					Handles.DrawAAPolyLine(10.0f, new Vector3(m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].x * m_maxHandlesSize + (m_handlesXOffset * 0.99f), (1.0f - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].y) * m_maxHandlesSize + m_scrollPosition.y), new Vector3 (m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].x * m_maxHandlesSize + (m_handlesXOffset * 1.01f), (1.0f - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i]].y) * m_maxHandlesSize + m_scrollPosition.y));
					
					Handles.color = Color.red;
					Handles.DrawAAPolyLine(10.0f, new Vector3(m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].x * m_maxHandlesSize + (m_handlesXOffset * 0.99f), (1.0f - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].y) * m_maxHandlesSize + m_scrollPosition.y), new Vector3 (m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].x * m_maxHandlesSize + (m_handlesXOffset * 1.01f), (1.0f - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 1]].y) * m_maxHandlesSize + m_scrollPosition.y));
					
					Handles.color = Color.yellow;
					Handles.DrawAAPolyLine(10.0f, new Vector3(m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].x * m_maxHandlesSize + (m_handlesXOffset * 0.99f), (1.0f - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].y) * m_maxHandlesSize + m_scrollPosition.y), new Vector3 (m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].x * m_maxHandlesSize + (m_handlesXOffset * 1.01f), (1.0f - m_viewedMesh.sharedMesh.uv[m_viewedMesh.sharedMesh.triangles[i + 2]].y) * m_maxHandlesSize + m_scrollPosition.y));
				}
				else
				{
					//otherwise just draw the triangle in white
					Handles.color = Color.white;
					Handles.DrawPolyLine(vert1, vert2, vert3, vert1);
				}
			}
			
			if(Event.current.type == EventType.mouseDown && !clickedTri)
			{
				lastSelectedTri = "";
			}
			
			//Lastly we will draw a square outline around the UVs just to be fancy
			Handles.color = Color.white;
			Handles.DrawAAPolyLine(4.0f, new Vector3(m_handlesXOffset, m_scrollPosition.y),
			                       new Vector3(m_handlesXOffset + m_maxHandlesSize, m_scrollPosition.y),
			                       new Vector3 (m_handlesXOffset + m_maxHandlesSize, m_scrollPosition.y + m_maxHandlesSize),
			new Vector3(m_handlesXOffset, m_scrollPosition.y + m_maxHandlesSize),
			new Vector3(m_handlesXOffset, m_scrollPosition.y));
			
			GUILayout.EndScrollView();
			
			Repaint();
		} 
	}
}
