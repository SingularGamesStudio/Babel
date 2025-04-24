using UnityEngine;

public class Leg: MonoBehaviour
{
	public Movement parent;
	public Rigidbody2D rb;
	public bool collided;
	public float slipTTL = 0;
	private void Awake()
	{
		rb = this.GetComponent<Rigidbody2D>();
	}
	private void Update()
	{
		slipTTL -= Time.deltaTime;
	}
	private void OnCollisionStay2D(Collision2D collision)
	{
		if (slipTTL <= 0) {
			collided = true;
			rb.constraints = RigidbodyConstraints2D.FreezePosition;
		}
	}

	public void Unlock(float slipTime = 0)
	{
		collided = false;
		rb.constraints = RigidbodyConstraints2D.None;
		slipTTL = slipTime;
	}
}
