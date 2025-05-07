using UnityEngine;

public class Stalactite: MonoBehaviour
{
	public bool active = false;
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!active || (collision.collider.attachedRigidbody != null && Mathf.Abs((collision.otherCollider.attachedRigidbody.velocity - collision.collider.attachedRigidbody.velocity).y) < 8))
			return;
		if (collision.collider.gameObject.layer >= 8 && collision.collider.gameObject.layer <= 10) {
			if (collision.collider.transform.parent != null && collision.collider.transform.parent.childCount > 0) {
				Transform child = collision.collider.transform.parent.GetChild(0);
				if (child.GetComponent<Vulture>() != null) {
					if (child.GetComponent<Vulture>().state != "dead")
						Mood.Inst.rep += 100;
					child.GetComponent<Vulture>().state = "dead";

				}
			}
		}
		if (collision.collider.gameObject.layer == 7) {
			if (collision.collider.transform.parent != null && collision.collider.transform.parent.childCount > 0) {
				Transform child = collision.collider.transform.parent.GetChild(0);
				if (child.GetComponent<NPC>() != null) {
					if (child.GetComponent<NPC>().state != "dead")
						Mood.Inst.rep -= 100;
					child.GetComponent<NPC>().state = "dead";
				}
			}
		}
	}
}
