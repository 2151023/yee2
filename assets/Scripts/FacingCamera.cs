using UnityEngine;

public class FacingCamera : MonoBehaviour {
	private void Update(){
		transform.LookAt (Camera.main.transform);
	}
}
