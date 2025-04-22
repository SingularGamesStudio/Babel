using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Leg LLeg;
    public Leg RLeg;
    public Rigidbody2D Body;
	public Rigidbody2D Eye;

	[HideInInspector]
	public int[] collided = new int[2];
	bool grounded = false;
	Leg AimingLeg = null;
	float CurAimCooldown = 0;

    public float StepForce = 100f;
	public float EyeForce = 100f;
	public float MoveForce = 20f;
	public float AimForce = 500f;

	public float AimCooldown = 1f;
	public float StretchCooldown = 2f;
	public float StretchScale = 10f;

	public class ReducedFreq
	{
		public ReducedFreq(SpringJoint2D spring, float TTL)
		{
			this.spring = spring;
			this.TTL = TTL;
		}
		public SpringJoint2D spring;
		public float TTL;
	}
	List<ReducedFreq> reducedFreqs = new List<ReducedFreq>();

    void Start()
    {
        
    }

    void FixedUpdate()
    {
		SwitchLegs();


		LRMovement();
		Aim();
		Grapple();


		AimingLeg = null;
	}

	void Aim()
	{
		if(CurAimCooldown>0) {
			CurAimCooldown -= Time.fixedDeltaTime;
			return;
		}
		if(Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
			return;
		}
		if (Input.GetMouseButton(0)) {
			AimingLeg = LLeg;
		}
		if (Input.GetMouseButton(1)) {
			AimingLeg = RLeg;
		}
		if(AimingLeg != null && grounded) {
			Vector2 force = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - AimingLeg.rb.position).normalized;

			AimingLeg.Unlock();
			AimingLeg.rb.AddForce(force*AimForce);
			Eye.AddForce(force * EyeForce);
		}
	}

	void Grapple()
	{
		if (AimingLeg!=null && grounded && Input.GetKeyDown(KeyCode.Space)) {
			Vector2 force = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - AimingLeg.rb.position).normalized;
			AimingLeg.rb.AddForce(force * 10000);
			CurAimCooldown = AimCooldown;

			for(int i = 0; i< AimingLeg.transform.parent.childCount; i++) {
				var springs = AimingLeg.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
				foreach (SpringJoint2D spring in springs) {
					spring.frequency /= StretchScale;
					reducedFreqs.Add(new ReducedFreq(spring, StretchCooldown));
				}

			}
		}
	}

	void CollapseSprings()
	{
		foreach(ReducedFreq r in reducedFreqs) {
			
		}
	}


	void SwitchLegs()
	{
		if (collided[0] >0 && collided[1] > 0 && Vector2.SignedAngle(RLeg.rb.position - Body.position, LLeg.rb.position - Body.position) > 0) {
			var temp = RLeg;
			RLeg = LLeg;
			LLeg = temp;
		}
		if (collided[0] > 0 || collided[1] > 0) {
			grounded = true;
		} else {
			grounded = false;
		}
		/*if(collided[0]>0) {
			collided[0]--;
			if (collided[0] == 0) {
				if(LLeg.name=="0") {
					LLeg.constraints = RigidbodyConstraints2D.None;
					Debug.Log("Unlock");
				} else {
					RLeg.constraints = RigidbodyConstraints2D.None;
					Debug.Log("Unlock");
				}
			}
		}

		if (collided[1] > 0) {
			collided[1]--;
			if (collided[1] == 0) {
				if (LLeg.name == "1") {
					LLeg.constraints = RigidbodyConstraints2D.None;
				} else {
					RLeg.constraints = RigidbodyConstraints2D.None;
				}
			}
		}*/
	}

	void LRMovement()
	{
		if (Input.GetKey(KeyCode.D)) {
			Vector2 Realign = Vector2.right * MoveForce;
			Vector2 Ltemp = (LLeg.rb.position - Body.position);
			Ltemp = new Vector2(Ltemp.y, -Ltemp.x);

			LLeg.Unlock();
			LLeg.rb.AddForce(Ltemp.normalized * StepForce);
			Eye.AddForce(Vector2.right * EyeForce);
			if(grounded)
				Body.AddForce(Realign);
		}
		if (Input.GetKey(KeyCode.A)) {
			Vector2 Realign = Vector2.left * MoveForce;
			Vector2 Rtemp = (RLeg.rb.position - Body.position);
			Rtemp = new Vector2(-Rtemp.y, Rtemp.x);

			RLeg.Unlock();
			RLeg.rb.AddForce(Rtemp.normalized * StepForce);
			Eye.AddForce(Vector2.left * EyeForce);
			if (grounded)
				Body.AddForce(Realign);
		}
	}
}
