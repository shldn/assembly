using UnityEngine;
using System.Collections;
	
	[ExecuteInEditMode]
	[RequireComponent (typeof(Camera))]
	[AddComponentMenu ("Image Effects/Other/Screen Overlay")]

class ScreenOverlay : PostEffectsBase
{
	
	public enum OverlayBlendMode
	{
		Additive = 0,
		ScreenBlend = 1,
		Multiply = 2,
		Overlay = 3,
		AlphaBlend = 4,	
	}
	
	
	
	public OverlayBlendMode blendMode = OverlayBlendMode.Overlay;
	public float intensity = 1.0f;
	public Texture2D texture;
			
	public Shader overlayShader;
	private Material overlayMaterial = null;
	
	bool CheckResources ()
	{
		 CheckSupport (false);
		
		overlayMaterial = CheckShaderAndCreateMaterial (overlayShader, overlayMaterial);
		
		if 	(!isSupported)
			ReportAutoDisable ();
		return isSupported;
	}
	
	void  OnRenderImage ( RenderTexture source ,   RenderTexture destination  )
	{		
		if (CheckResources() == false)
		{
			Graphics.Blit (source, destination);
			return;
		}
		overlayMaterial.SetFloat ("_Intensity", intensity);
		overlayMaterial.SetTexture ("_Overlay", texture);
		Graphics.Blit (source, destination, overlayMaterial, (int)blendMode);
	}
}