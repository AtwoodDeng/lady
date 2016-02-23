using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Controller : MonoBehaviour {

	[SerializeField] float[] startTime;
	[SerializeField] float[] endTime;
	[SerializeField] Vector3 floatVelocity;
	[SerializeField] float spinVelocity;
	[SerializeField] float scaleChange;
	[SerializeField] string[] text;

	[SerializeField] Color[] color;
	[SerializeField] bool[] isShowed;

	[SerializeField] float colorShowTime = 8f;
	[SerializeField] Color oriColor;
	[SerializeField] Color showColor;


	[SerializeField] bool isUseAnimation = false;
	[SerializeField] bool[] isAutoPrint;

	int index;
	DrawMeterial draw;
	SpriteRenderer sprite;
	bool isOn =false;

	Vector3 oriPos;
	Vector3 oriScale;
	Vector3 oriEular;

	[SerializeField] float checkInterval = 1f;

	void Awake()
	{
		draw = GetComponent<DrawMeterial>();
		if (draw != null )
			draw.enabled = false;
		sprite = GetComponent<SpriteRenderer>();
		if (sprite != null) {
			Color col = oriColor;
			col.a = sprite.color.a;
			sprite.color = col;
		}

		oriPos = transform.position;
		oriScale = transform.localScale;
		oriEular = transform.eulerAngles;

	}
	
	float checkTime = 0 ;
	// Update is called once per frame
	void Update () {
		if (index >= startTime.Length)
			return;
		if ( Time.time - LogicManager.startTime - startTime[index] > 0 && !isOn)
		{
			Begin();
		}

		if ( Time.time - LogicManager.startTime - endTime[index] > 0 && isOn)
		{
			End();
			index++;
		}
		
		if (isOn && !isShowed[index])
		{
			if ( Time.time > checkTime)
			{
				checkTime = Time.time + checkInterval;
				if ( draw.checkDraw() )
				{
					Show();
				}
			}
		}
	}

	void Show()
	{
		isShowed[index] = true;
		for(int i = 0 ; i <= index ; ++ i )
			ShowText(i);
		ShowSprite();
	}

	void ShowSprite()
	{
		if (sprite == null)
		return;
		sprite.DOColor(showColor, colorShowTime);
	}

	void ShowText(int i )
	{
		if ( i >= text.Length || i < 0 )
			return;

		Message msg = new Message();

		msg.AddMessage("word", text[i]);
		msg.AddMessage("color" , color[i]);
		msg.AddMessage ("pos", transform.position + i * Vector3.down * 0.6f);

		EventManager.Instance.PostEvent(EventDefine.showText, msg, this);

	}

	void Begin()
	{
		isOn = true;
		if (draw != null)
		draw.enabled = true;
		float time = endTime[index]-startTime[index];

		if (sprite != null)
		{
			sprite.DOFade(1, 0);
			sprite.DOColor(oriColor, 0f);
		}


		transform.position = oriPos;
		transform.localScale = oriScale;
		transform.eulerAngles = oriEular;

		if (isUseAnimation)
		{
//			transform.DOMove(floatVelocity*time, time);
			transform.DORotate(new Vector3(0,0,spinVelocity*time), time , RotateMode.LocalAxisAdd);
			transform.DOScale(Mathf.Pow(scaleChange, time), time);
		}

		if (isAutoPrint[index]) {
			Show();
		}

		VelocityManipulator[] vm = GetComponentsInChildren<VelocityManipulator> ();
		foreach (VelocityManipulator v in vm) {
			v.enabled = true;
		}
		ParticlesAreaManipulator[] pam = GetComponentsInChildren<ParticlesAreaManipulator> ();
		foreach (ParticlesAreaManipulator v in pam) {
			v.enabled = true;
		}
	}

	void End()
	{
		isOn = false;
		
		if (sprite != null)
			sprite.DOFade(0, 2f).OnComplete(DisableDraw);

		VelocityManipulator[] vm = GetComponentsInChildren<VelocityManipulator> ();
		foreach (VelocityManipulator v in vm) {
			v.enabled = false;
		}
		ParticlesAreaManipulator[] pam = GetComponentsInChildren<ParticlesAreaManipulator> ();
		foreach (ParticlesAreaManipulator v in pam) {
			v.enabled = false;
		}

	}

	void DisableDraw()
	{
		if (draw != null) {
			draw.enabled = false;
			draw.Reset ();
		}
	}
}
