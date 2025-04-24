using UnityEngine;
using static Movement;

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
			if (!collided) {
				for (int i = 0; i < gameObject.transform.parent.childCount; i++) {
					var springs = gameObject.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
					foreach (SpringJoint2D spring in springs) {
						if (spring.connectedBody.name == "Static") {
							spring.connectedBody = parent.Body;
						}
					}
				}
			}
			collided = true;
			rb.constraints = RigidbodyConstraints2D.FreezePosition;
		}

	}

	public void Unlock(float slipTime = 0)
	{
		if(collided) {
			for(int i = 0; i<gameObject.transform.parent.childCount; i++) {
				var springs = gameObject.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
				foreach (SpringJoint2D spring in springs) {
					if(spring.connectedBody.name=="Body") {
						spring.connectedBody = parent.StaticBody;
					}
				}
			}
		}
		collided = false;
		rb.constraints = RigidbodyConstraints2D.None;
		slipTTL = slipTime;
	}
}
