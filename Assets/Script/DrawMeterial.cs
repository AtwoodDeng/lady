﻿using UnityEngine;
using System.Collections;

public class DrawMeterial : MonoBehaviour {
	[SerializeField] FluidSimulator fluid;
	[SerializeField] SpriteRenderer render;
	[SerializeField] Texture2D baseTex;
	[SerializeField] Texture2D newTex;
	[SerializeField] Texture2D cameraTex;
	[SerializeField] Vector2 initPos;
	Color[] baseColor;
	Color[] colors;

	[SerializeField] int pixelCheckRange = 5;
	[SerializeField] AnimationCurve alphaChangeCurve;
	[SerializeField] float alphaChangeRate = 0.02f;

	Vector3 focusScreenPos;
	// Use this for initialization
	void Awake () {
		newTex = new Texture2D (baseTex.width, baseTex.height, TextureFormat.ARGB32, false);
		newTex.name = "MyNewTex";
		baseColor = baseTex.GetPixels();
		colors = new Color[baseTex.width * baseTex.height];
		for(int i = 0 ; i < newTex.width; ++ i )
			for ( int j = 0 ; j < newTex.height; ++ j )
		{
			colors[i*baseTex.height+j] = baseColor[i*baseTex.height+j];
			colors[i*baseTex.height+j].a = 0.005f;

		}
		newTex.SetPixels (colors);
		newTex.Apply();
		Vector2 size = new Vector2(newTex.width,newTex.height);
		render.sprite = Sprite.Create(newTex, new Rect(initPos,size) , new Vector2(0.5f, 0.5f));

		//set up screen size
		Camera.main.orthographicSize =  (float)Screen.height / 100f / 2f;
		cameraTex = new Texture2D (pixelCheckRange * 2 +1 , pixelCheckRange * 2 +1 );
		StartCoroutine (captureImage ());
	}

	// Update is called once per frame
	void Update () {
		focusScreenPos = Input.mousePosition;
		showImage ( (int)focusScreenPos.x , (int) focusScreenPos.y );
		Color[] newColor = cameraTex.GetPixels ();
		newTex.SetPixels (colors);
		newTex.Apply ();

	}
	void LateUpdate() {
	}

	IEnumerator captureImage()
	{
		while (true) {
			
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			
			RenderTexture target = Camera.main.targetTexture;
			
			if( target == null )
				yield return null;
			yield return new WaitForEndOfFrame();
			
			cameraTex.ReadPixels (new Rect ((int)focusScreenPos.x - pixelCheckRange
			                                ,(int)focusScreenPos.y - pixelCheckRange
			                                , (int)focusScreenPos.x + pixelCheckRange
			                                , (int)focusScreenPos.x + pixelCheckRange), 0, 0);
			cameraTex.Apply ();
		}
	}

	void RTImage(Camera cam) {
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = cam.targetTexture;
		cam.Render();
		if (cameraTex == null )
			cameraTex = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
		cameraTex.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
		cameraTex.Apply();
		RenderTexture.active = currentRT;
	}
	
	Vector2 getFluidVelocity( Vector3 position )
	{
		Ray ray = Camera.main.ScreenPointToRay (position);
		RaycastHit hitInfo = new RaycastHit();
		if (fluid.GetComponent<Collider>().Raycast(ray, out hitInfo, 10))
		{
			Vector2 simSize = new Vector2(fluid.GetWidth(), fluid.GetHeight());
			Vector2 posInSimSpace = new Vector2(hitInfo.textureCoord.x * simSize.x, hitInfo.textureCoord.y * simSize.y);
			Vector2 velInSimSpace = fluid.GetVelocity((int)posInSimSpace.x, (int)posInSimSpace.y) * Time.deltaTime;
			return velInSimSpace;
		}
		return Vector2.zero;
	}

	void showImage( int posX , int posY  )
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint(render.gameObject.transform.position);
		int Y = ((int)posX - (int)screenPos.x) + newTex.width / 2;
		int X = ((int)posY - (int)screenPos.y) + newTex.height / 2;
		Color[] camCol = cameraTex.GetPixels ();
		for ( int xx = X - pixelCheckRange; xx < X + pixelCheckRange; xx++ )
			for (int yy = Y - pixelCheckRange; yy < Y + pixelCheckRange; yy++ ) {
			if ( ( ( xx - X ) * ( xx - X) + (yy - Y ) * ( yy - Y ) ) > pixelCheckRange * pixelCheckRange )
				continue;
			float smooth =  alphaChangeCurve.Evaluate( (1.0f * ( (xx-X) * (xx-X) + (yy - Y) * (yy - Y) ) / pixelCheckRange / pixelCheckRange / 2f) );
			float gray = color2Gray( camCol[( ( xx - X )+pixelCheckRange ) * (pixelCheckRange * 2 + 1 ) + ( yy - Y ) + pixelCheckRange] );
			//float velRelate = Vector2.Dot( Vector2.up 
			//   , getFluidVelocity( render.gameObject.transform.position + new Vector3( (xx - X ) * 0.01f , ( yy - Y ) * 0.01f , 0 ))) ;
			ChangeAlphaFromWorld(xx , yy , smooth * gray * alphaChangeRate);
		}
	}

	float color2Gray(Color col)
	{
		return Mathf.Min (Mathf.Min (col.r, col.g), col.b);
	}

	void ChangeAlphaFromWorld(int x , int y , float alphaChange )
	{
		if (x <= 0 || x >= newTex.width)
			return;
		if (y <= 0 || y >= newTex.height)
			return;

		if (baseColor [x * newTex.width + y].a <= 0f)
			return;
		else {
			colors [x * newTex.width + y].a = Mathf.Clamp( colors [x * newTex.width + y].a + alphaChange , 0 , baseColor [ x * newTex.width + y].a );
		}

	}

	void SetPixelFromWorld(Vector3 pos , Color col )
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint(render.gameObject.transform.position);
		int Y = ((int)pos.x - (int)screenPos.x) + newTex.width / 2;
		int X = ((int)pos.y - (int)screenPos.y) + newTex.height / 2;
		X = Mathf.Clamp (X, 0, newTex.width-1);
		Y = Mathf.Clamp (Y, 0, newTex.height-1);
		col.a = baseColor [X * newTex.width + Y].a;
		colors [X * newTex.width + Y] = col;

	}
}
