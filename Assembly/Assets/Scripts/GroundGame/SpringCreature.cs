using UnityEngine;
using System.Collections;

public class SpringCreature : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        Vector3 normal = Vector3.up;
        AttachmentHelpers.AttachSpringToObject(gameObject, AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal), normal);
        AttachmentHelpers.AttachSpringToObject(gameObject, AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal), normal);
        AttachmentHelpers.AttachSpringToObject(gameObject, AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal), normal);
        AttachmentHelpers.AttachSpringToObject(gameObject, AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal), normal);
        AttachmentHelpers.AttachSpringToObject(gameObject, AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal), normal);
	}
	

}
