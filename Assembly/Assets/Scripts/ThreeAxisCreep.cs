using UnityEngine;
using System.Collections;

// This script interperets data from AI Navigation and controls a Mech with it.
public class ThreeAxisCreep : MonoBehaviour{

	public float maxCreep = 1f;
	public float relaxedness = 1f;

	public Vector3 creep;
	Vector3 creepTarget;
	Vector3 creepVel;
	Vector3 creepCooldown;

	void Update(){
		float creepMinRapidity = relaxedness * 0.7f;

		creepCooldown -= Vector3.one * Time.deltaTime;

		if(creepCooldown.x <= 0)
		{
			creepCooldown.x = Random.Range(0f, creepMinRapidity);
			creepTarget.x = Random.Range(-maxCreep, maxCreep);
		}
		creep.x = Mathf.SmoothDamp(creep.x, creepTarget.x, ref creepVel.x, relaxedness);

		if(creepCooldown.y <= 0)
		{
			creepCooldown.y = Random.Range(0f, creepMinRapidity);
			creepTarget.y = Random.Range(-maxCreep, maxCreep);
		}
		creep.y = Mathf.SmoothDamp(creep.y, creepTarget.y, ref creepVel.y, relaxedness);

		if(creepCooldown.z <= 0)
		{
			creepCooldown.z = Random.Range(0f, creepMinRapidity);
			creepTarget.z = Random.Range(-maxCreep, maxCreep);
		}
		creep.z = Mathf.SmoothDamp(creep.z, creepTarget.z, ref creepVel.z, relaxedness);

	} // End of Update().

} // End of TwoAxisCreep.