Shader "Exosphir/Mask" {
    SubShader {
		Tags {"Queue" = "Geometry+10" }
 
		// Don't draw in the RGBA channels; just the depth buffer
 		
 		//Cull Front
		ColorMask 0
		ZWrite On
 
		// Do nothing specific in the pass:
 
		Pass {}
    }
}
