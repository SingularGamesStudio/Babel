using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leg : MonoBehaviour
{
	public Movement parent;
	public Rigidbody2D rb;
	public int id;
	private void Awake()
	{
		rb = this.GetComponent<Rigidbody2D>();
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		parent.collided[id] = 1;
		rb.constraints = RigidbodyConstraints2D.FreezePosition;
	}
	
	public void Unlock()
	{
		parent.collided[id] = 0;
		rb.constraints = RigidbodyConstraints2D.None;
	}
}
