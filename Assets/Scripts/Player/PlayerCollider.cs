using UnityEngine;

public class PlayerCollider: MonoBehaviour
{
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.gameObject.tag == "Danger") {
			Game.Inst.GameOver(false);
		}
		if (collision.collider.gameObject.layer == 11) {
			collision.collider.gameObject.GetComponent<Stalactite>().active = true;
			collision.collider.attachedRigidbody.constraints = RigidbodyConstraints2D.None;
		}
	}
}
