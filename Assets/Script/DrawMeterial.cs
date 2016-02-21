using UnityEngine;
using System.Collections;

public class DrawMeterial : MonoBehaviour {
	[SerializeField] SpriteRenderer render;
	[SerializeField] Texture2D baseTex;
	[SerializeField] Texture2D newTex;
	[SerializeField] Vector2 initPos;
	Color[] baseColor;
	Color[] colors;
	// Use this for initialization
	void Awake () {
		newTex = new Texture2D (baseTex.width, baseTex.height, TextureFormat.ARGB32, false);

		baseColor = baseTex.GetPixels();
		colors = new Color[baseTex.width * baseTex.height];
		for(int i = 0 ; i < baseTex.width; ++ i )
			for ( int j = 0 ; j < baseTex.height; ++ j )
		{
			colors[i*baseTex.height+j].a = 1 - baseColor[i*baseTex.height+j].a;
		}
		newTex.SetPixels (colors);
		newTex.Apply();
		Vector2 size = new Vector2(newTex.width,newTex.height);
		render.sprite = Sprite.Create(newTex, new Rect(initPos,size) , new Vector2(0.5f, 0.5f));

		//set up screen size
		Camera.main.orthographicSize =  (float)Screen.height / 100f / 2f;
	}

	// Update is called once per frame
	void Update () {

		SetPixelFromWorld (colors, Input.mousePosition, Color.white);
		newTex.SetPixels (colors);
		newTex.Apply ();
	}

	void SetPixelFromWorld(Color[] colors, Vector3 pos , Color col )
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint(render.gameObject.transform.position);
		int Y = ((int)pos.x - (int)screenPos.x) + newTex.width / 2;
		int X = ((int)pos.y - (int)screenPos.y) + newTex.height / 2;
		X = Mathf.Clamp (X, 0, newTex.width-1);
		Y = Mathf.Clamp (Y, 0, newTex.height-1);
		colors [X * newTex.width + Y] = col;
	}
}
