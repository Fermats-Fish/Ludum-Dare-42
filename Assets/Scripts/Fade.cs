using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour {

	SpriteRenderer sr;
	Text te;

	void Start () {
		sr = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (sr.color.a - Time.deltaTime <= 0f){
			Destroy (gameObject);
		}
		sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, sr.color.a - Time.deltaTime );
	}
}
