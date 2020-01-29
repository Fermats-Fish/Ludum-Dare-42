using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	static float minZoom = 1f;
	Vector3 startDragCoord;
	float scrollSpeed = 2f;

	public static CameraController instance;

	bool lockDragging = false;
	bool dragging = false;

	void Start(){

		if (instance != null){
			Debug.LogError ("There are two camera controllers for some reason!");
		} else {
			instance = this;
		}
	}

	void Update(){

		// When the mouse button is pressed down, store where it was pressed.
		if (Input.GetMouseButtonDown (2) || (Input.GetMouseButtonDown (1) && lockDragging == false)) {
			StartDrag ();
		}

		// When the mouse is scrolled, change the zoom (aka size).
		float scroll = Input.GetAxis ("Scroll");
		if (scroll != 0){
			Camera.main.orthographicSize /= Mathf.Pow (2f, scroll * scrollSpeed);
			if (Camera.main.orthographicSize <= minZoom){
				Camera.main.orthographicSize = minZoom;
			}
		}

		// When the mouse is dragged move the camera to make sure the mouse is still pointing to the same world coordinate.
		if (dragging == true){
			ContinueDrag ();
		}

		if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)){
			StopDrag ();
		}
	}

	void StartDrag(){
		dragging = true;
		startDragCoord = Camera.main.ScreenToWorldPoint (Input.mousePosition);
	}

	void ContinueDrag(){
		Vector3 currentCoord = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		transform.Translate (startDragCoord - currentCoord);
	}

	void StopDrag(){
		dragging = false;
	}

	public void LockDragging(){
		lockDragging = true;
	}

	public void UnlockDragging(){
		lockDragging = false;
	}


}

