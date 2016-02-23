using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LogicManager : MonoBehaviour {

	public LogicManager() { s_Instance = this; }
	public static LogicManager Instance { get { return s_Instance; } }
	private static LogicManager s_Instance;


	[SerializeField] GameObject textPrefab;
	[SerializeField] Vector3 textCreatePosition = new Vector3(3f,2f,0);
	[SerializeField] float textFloatSpeed;
	[SerializeField] float fadeTime = 2f;
	[SerializeField] float showTimePreWord = 0.05f;
 	static public float startTime = 0 ;

	void OnEnable()
	{
		EventManager.Instance.RegistersEvent(EventDefine.showText, ShowText);
	}

	void OnDisable()
	{

		EventManager.Instance.UnregistersEvent(EventDefine.showText, ShowText);
	}

	void ShowText(Message msg)
	{
		GameObject text = Instantiate(textPrefab) as GameObject;
//		text.transform.localPosition = new Vector3(Random.Range(-textCreatePosition.x, textCreatePosition.x)
//			,Random.Range(-textCreatePosition.y, textCreatePosition.y),0);
		TextMesh mesh = text.GetComponent<TextMesh>();
		mesh.text = (string)msg.GetMessage("word");
		mesh.color = (Color)msg.GetMessage("color");
		mesh.transform.position = (Vector3)msg.GetMessage ("pos");

		StartCoroutine(textAnimation(mesh));

	}

	IEnumerator textAnimation(TextMesh mesh)
	{
		float timer = 0;
		float duration = mesh.text.Length * showTimePreWord;
		Vector3 moveDirection = new Vector3 (Random.Range (-1f, 1f), 0, 0);

		while(true)
		{
			timer += Time.deltaTime;
			Color col = mesh.color;
			if (timer < fadeTime)
				col.a = timer/fadeTime;
			else if ( timer < fadeTime + duration)
				col.a = 1f;
			else if ( timer < fadeTime * 2 + duration)
				col.a = 1f - (timer - fadeTime - duration) / fadeTime;
			else 
				break;

			mesh.color = col;

			mesh.transform.position += moveDirection * textFloatSpeed * Time.deltaTime;

			yield return null;

		}

		Destroy(mesh.gameObject);

	}

	void OnGUI()
	{
		GUILayout.TextField(Time.time.ToString());
	}
}
