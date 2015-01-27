using UnityEngine;
using System.Collections;

public class TouchCamera : MonoBehaviour {
	public Transform target;
	Vector3 f0Dir= Vector3.zero;
	float zoomDistance= 5;
	float theta= 0.0F;
	float fai= 0.0F;
	float dx= 0.0F;
	float dy= 0.0F;
	float loc_x= 0.0F;
	float loc_y= 0.0F;
	float delta= 0.0F;
	float deltaWeight= 0.05F;
	Vector2 curDist= Vector2.zero;
	Vector2 prevDist= Vector2.zero;
	Transform dm;
	
	Vector3 upVal= Vector3.zero;
	Vector3 pos= new Vector3(0, 0, 0);
	Vector3 rot= new Vector3(0, 0, 0);
	
	void Update() {
		if (Input.touchCount == 1) {
			Touch f0 = Input.GetTouch(0);
			Vector3 f0Delta2 = new Vector3(f0.deltaPosition.x, -f0.deltaPosition.y, 0);
			f0Dir= f0Delta2;
			loc_x= Mathf.Deg2Rad*f0Dir.x*1;
			loc_y= -Mathf.Deg2Rad*f0Dir.y*1;
			
		} else if( Input.touchCount== 2 ) {
			loc_x= 0.0F;
			loc_y= 0.0F;
			Touch f0= Input.GetTouch(1);
			Touch f1= Input.GetTouch(0);
			Vector3 f0Delta= new Vector3(-f0.deltaPosition.x, -f0.deltaPosition.y, 0);
			Vector3 f1Delta= new Vector3(-f1.deltaPosition.x, -f1.deltaPosition.y, 0);
			float toDirection= Vector3.Dot(f0Delta, f1Delta);
			Debug.Log(toDirection);
			
			if(f1.phase == TouchPhase.Moved && f0.phase == TouchPhase.Moved){
				if( toDirection > 0 ) {
					f0Dir= Vector3.zero;
					dy+= f0Delta.y*0.01F;
					dx+= f0Delta.x*0.01F;
					loc_x= f0Delta.x*0.01F;
					loc_y= f0Delta.y*0.01F;
				} else {
					curDist = f0.position - f1.position;
					prevDist = (f0.position - f0.deltaPosition) - (f1.position - f1.deltaPosition);
					float delta = curDist.magnitude - prevDist.magnitude;
					zoomDistance= zoomDistance+(-delta*deltaWeight);
				}
			}
		} else {
			f0Dir= Vector3.zero;
			loc_x= 0.0F;
			loc_y= 0.0F;
		}
		theta+= Mathf.Deg2Rad*f0Dir.x*1;
		fai+=   -Mathf.Deg2Rad*f0Dir.y*1;
		
		upVal.z= zoomDistance*Mathf.Cos(theta)*Mathf.Sin(fai+Mathf.PI/2);
		upVal.x= zoomDistance*Mathf.Sin(theta)*Mathf.Sin(fai+Mathf.PI/2);
		upVal.y= zoomDistance*Mathf.Cos(fai+Mathf.PI/2);
		
		transform.position= upVal;
		target.transform.Translate( Camera.main.transform.up*loc_y+Camera.main.transform.right*(loc_x), Space.World);
		transform.position+= target.position;
		Camera.main.transform.LookAt(target.position);
	}
}