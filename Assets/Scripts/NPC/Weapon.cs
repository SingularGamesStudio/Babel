using UnityEngine;

public class Weapon: MonoBehaviour
{
	public Rigidbody2D body;
	SpriteRenderer spriteRenderer;
	Rigidbody2D rb;
	Color saved;
	float TTL = 0;
	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{
		rb.AddForce(body.transform.up * Time.deltaTime * 400);
		if (TTL > 0 && TTL - Time.deltaTime <= 0) {
			spriteRenderer.color = saved;
			gameObject.tag = "Untagged";
			TTL = 0;
		}
		TTL -= Time.deltaTime;
	}

	public void Activate()
	{
		gameObject.tag = "Danger";
		saved = spriteRenderer.color;
		spriteRenderer.color = Color.red;
		TTL = 1;
	}
}
