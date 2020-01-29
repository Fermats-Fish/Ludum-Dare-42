using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRocketButton : MonoBehaviour {

	public GameObject rocketLauchUI;

	public void OnPress(){
		if (rocketLauchUI.activeInHierarchy){
			rocketLauchUI.SetActive (false);
		} else {
			rocketLauchUI.SetActive (true);
		}
	}

}
