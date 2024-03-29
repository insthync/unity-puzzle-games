using UnityEngine;
using System.Collections;

public class PhysicNodeActivator : MonoBehaviour {
	private Collider mCollider;
	private Collider2D mCollider2D;
	private Rigidbody mRigidBody;
	private Rigidbody2D mRigidBody2D;
	void Awake()
	{
		mCollider = GetComponent<Collider> ();
		mCollider2D = GetComponent<Collider2D> ();
		mRigidBody = GetComponent<Rigidbody> ();
		mRigidBody2D = GetComponent<Rigidbody2D> ();
		if (mCollider != null)
		{
			mCollider.isTrigger = true;
		}
		if (mCollider2D != null)
		{
			mCollider2D.isTrigger = true;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject == CeilAndFloorControls.instance.ceilObject)
		{
			mCollider.isTrigger = false;
			CeilAndFloorControls.instance.AddActivatedNode(this);
		}
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject == CeilAndFloorControls.instance.ceilObject)
		{
			mCollider2D.isTrigger = false;
			CeilAndFloorControls.instance.AddActivatedNode(this);
		}
	}

	public void Blow(Vector3 blowPoint, float blowForce)
	{
        var force = (transform.position - blowPoint).normalized * blowForce;

        if (mRigidBody != null)
		{
			mRigidBody.AddForce(force, ForceMode.Impulse);
		}
		if (mRigidBody2D != null)
		{
			mRigidBody2D.AddForce(force, ForceMode2D.Impulse);
		}
	}
}
