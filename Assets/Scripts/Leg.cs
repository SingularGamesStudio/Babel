using UnityEngine;

public class Leg: MonoBehaviour
{
	[HideInInspector]
	public Rigidbody2D rb;
	public bool collided;
	public float slipTTL = 0;
	public float disrespectTTL = 0;
	public FixedJoint2D joint = null;

	private void Awake()
	{
		rb = this.GetComponent<Rigidbody2D>();
	}
	private void Update()
	{
		slipTTL -= Time.deltaTime;
		disrespectTTL -= Time.deltaTime;
		if (rb.gameObject.layer == 6 && joint != null && joint.connectedBody.gameObject.layer == 7 && disrespectTTL <= 0) {
			Mood.Inst.fear += Mood.Inst.TouchFear;
			Mood.Inst.rep -= Mood.Inst.TouchRespect;
			disrespectTTL = 1f;
		}
	}
	public void OnCollisionStay2D(Collision2D collision)
	{
		if (slipTTL <= 0) {
			collided = true;
			//rb.constraints = RigidbodyConstraints2D.FreezePosition;
			if (joint == null && collision.rigidbody.gameObject.layer != 6) {
				joint = gameObject.AddComponent<FixedJoint2D>();
				joint.connectedBody = collision.rigidbody;

			}

		}
	}

	public void Unlock(float slipTime = 0)
	{
		collided = false;
		//rb.constraints = RigidbodyConstraints2D.None;
		slipTTL = slipTime;
		Destroy(joint);
		joint = null;
	}
}
