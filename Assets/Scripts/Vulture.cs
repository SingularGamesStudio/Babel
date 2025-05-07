using UnityEngine;

public class Vulture: MonoBehaviour
{
	public string state = "waitL";
	public NPC target;
	Rigidbody2D rb;
	public Rigidbody2D LWing;
	public Rigidbody2D RWing;

	public GameObject LPoint;
	public GameObject RPoint;

	public GameObject staticBody;

	public float flyForce;
	public float wingForce;
	public float sidewaysForce;
	public float lungeForce;
	public float realignForce;

	public float minDist;
	float t = 0;
	float TTL = 0;
	// Start is called before the first frame update
	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	public void Update()
	{
		staticBody.transform.position = gameObject.transform.position;
		staticBody.transform.rotation = gameObject.transform.rotation;
		if (state == "dead") {
			return;
		}

		TTL -= Time.deltaTime;
		switch (state) {
			case "waitL":
				rb.AddForce(Vector2.left * Time.deltaTime * sidewaysForce);
				if (transform.position.x < LPoint.transform.position.x) {
					state = "waitR";
				}
				break;
			case "waitR":
				rb.AddForce(Vector2.right * Time.deltaTime * sidewaysForce);
				if (transform.position.x > RPoint.transform.position.x) {
					state = "waitL";
				}
				break;
			case "lunge":
				rb.AddForce((target.transform.position - rb.transform.position).normalized * Time.deltaTime * lungeForce);
				if ((target.transform.position - rb.transform.position).magnitude < minDist || TTL < 0) {
					if (UnityEngine.Random.Range(0, 2) == 0) {
						state = "waitL";
					} else {
						state = "waitR";
					}
					TTL = UnityEngine.Random.Range(8f, 20f);
				}
				break;
		}
		if (state == "waitL" || state == "waitR") {
			if (transform.position.y < LPoint.transform.position.y) {
				rb.AddForce(Vector2.up * Time.deltaTime * flyForce);
				LWing.AddForce(transform.up * Time.deltaTime * wingForce * Mathf.Sin(t * 5));
				RWing.AddForce(transform.up * Time.deltaTime * wingForce * Mathf.Sin(t * 5));

			} else {
				rb.AddForce(Vector2.up * Time.deltaTime / 3 * flyForce);
				LWing.AddForce(transform.up * Time.deltaTime / 3 * wingForce * Mathf.Sin(t * 5));
				RWing.AddForce(transform.up * Time.deltaTime / 3 * wingForce * Mathf.Sin(t * 5));
				t += Time.deltaTime / 3;
			}
			if (TTL < 0) {
				state = "lunge";
				TTL = 8f;
			}
		}
		Vector2 dir = Vector2.down;
		if (state == "lunge") {
			dir = (target.transform.position - rb.transform.position).normalized;
		}
		rb.AddTorque(-Vector2.SignedAngle(transform.up * -1, dir) * realignForce * Time.deltaTime);
	}
}
