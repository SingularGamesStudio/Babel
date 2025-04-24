using System.Collections.Generic;
using UnityEngine;

public class Movement: MonoBehaviour
{
	public Leg LLeg;
	public Leg RLeg;
	public Rigidbody2D Body;
	public Rigidbody2D StaticBody;
	public Rigidbody2D Eye;

	bool grounded = false;

	Leg ToUnlock = null;
	float CurAimCooldown = 0;

	public float StepForce = 100f;
	public float EyeForce = 100f;
	public float AirMoveForce = 20f;
	public float AimForce = 500f;
	public float ShootForce = 10000f;

	public float AimCooldown = 1f;
	public float StretchCooldown = 2f;
	public float StretchScale = 10f;

	public class ReducedFreq
	{
		public ReducedFreq(float TTL, float original)
		{
			this.TTL = TTL;
			this.original = original;
		}
		public float TTL;
		public float original;
	}
	Dictionary<SpringJoint2D, ReducedFreq> reducedFreqs = new Dictionary<SpringJoint2D, ReducedFreq>();

	void Start()
	{

	}

	private void Update()
	{
		SwitchLegs();


		LRMovement();
		Aim();
		Grapple();
		CollapseSprings();
		StaticBody.position = Body.position;
	}

	void Aim()
	{
		Leg AimingLeg = null;
		if (CurAimCooldown > 0) {
			CurAimCooldown -= Time.deltaTime;
			return;
		}
		if ((!Input.GetMouseButton(0) && !Input.GetMouseButton(1)) || !grounded) {
			return;
		}
		if (LLeg.collided && RLeg.collided) {
			if (Input.GetMouseButton(0)) {
				AimingLeg = LLeg;
			}
			if (Input.GetMouseButton(1)) {
				AimingLeg = RLeg;
			}
		} else {
			if (LLeg.collided) {
				AimingLeg = RLeg;
			} else {
				AimingLeg = LLeg;
			}
		}
		if (AimingLeg != null) {
			Vector2 force = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - AimingLeg.rb.position).normalized;

			AimingLeg.Unlock();
			AimingLeg.rb.AddForce(force * AimForce * Time.deltaTime);
			Eye.AddForce(force * EyeForce * Time.deltaTime);
		}
	}

	void Grapple()
	{
		Leg AimingLeg = null;
		if (LLeg.collided) {
			AimingLeg = RLeg;
		} else {
			AimingLeg = LLeg;
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			Vector2 force = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - Body.position).normalized;

			AimingLeg.rb.AddForce(force * ShootForce * Time.deltaTime);
			CurAimCooldown = AimCooldown;
			ToUnlock = Opposite(AimingLeg);

			for (int i = 0; i < AimingLeg.transform.parent.childCount; i++) {
				var springs = AimingLeg.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
				foreach (SpringJoint2D spring in springs) {
					if (reducedFreqs.ContainsKey(spring)) {
						spring.frequency = reducedFreqs[spring].original;
					}
					reducedFreqs[spring] = new ReducedFreq(StretchCooldown, spring.frequency);
					spring.frequency /= StretchScale;
				}
			}
		}
	}

	void CollapseSprings()
	{
		foreach (SpringJoint2D spring in reducedFreqs.Keys) {
			float dt = StretchCooldown / (StretchScale - 1);
			ReducedFreq r = reducedFreqs[spring];
			r.TTL -= Time.deltaTime;
			if (r.TTL <= 0) {
				spring.frequency = r.original;
			} else {
				spring.frequency = r.original / StretchScale * (1 + (StretchCooldown - r.TTL) / dt);
			}
		}
	}


	void SwitchLegs()
	{
		if (LLeg.collided && RLeg.collided) {
			if (Vector2.SignedAngle(RLeg.rb.position - Body.position, LLeg.rb.position - Body.position) > 0) {
				var temp = RLeg;
				RLeg = LLeg;
				LLeg = temp;
			}
			if (ToUnlock != null) {
				ToUnlock.Unlock(0.4f);
				ToUnlock = null;
			}
		}
		grounded = LLeg.collided || RLeg.collided;
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
		
		if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) {
			return;
		}
		Leg ActiveLeg = null;
		if(LLeg.collided && RLeg.collided) {
			if (Input.GetKey(KeyCode.D)) 
				ActiveLeg = LLeg;
			if (Input.GetKey(KeyCode.A)) 
				ActiveLeg = RLeg;
		} else if(LLeg.collided || RLeg.collided) {
			if (LLeg.collided)
				ActiveLeg = LLeg;
			if (RLeg.collided)
				ActiveLeg = RLeg;
		} else {
			if (Input.GetKey(KeyCode.D))
				Body.AddForce(Vector2.right * AirMoveForce * Time.deltaTime);
			if (Input.GetKey(KeyCode.A))
				Body.AddForce(Vector2.left * AirMoveForce * Time.deltaTime);
		}

		
		if (Input.GetKey(KeyCode.D)) {
			Vector2 Realign = Vector2.right * MoveForce;
			Vector2 Ltemp = (LLeg.rb.position - Body.position);
			Ltemp = new Vector2(Ltemp.y, -Ltemp.x);

			LLeg.Unlock();
			LLeg.rb.AddForce(Ltemp.normalized * StepForce * Time.deltaTime);
			Eye.AddForce(Vector2.right * EyeForce * Time.deltaTime);
			if (grounded)
				Body.AddForce(Realign * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.A)) {
			Vector2 Realign = Vector2.left * MoveForce;
			Vector2 Rtemp = (RLeg.rb.position - Body.position);
			Rtemp = new Vector2(-Rtemp.y, Rtemp.x);

			RLeg.Unlock();
			RLeg.rb.AddForce(Rtemp.normalized * StepForce * Time.deltaTime);
			Eye.AddForce(Vector2.left * EyeForce * Time.deltaTime);
			if (grounded)
				Body.AddForce(Realign * Time.deltaTime);
		}
	}

	Leg Opposite(Leg l)
	{
		if (l == null) return null;
		if (l == LLeg) return RLeg;
		return LLeg;
	}
}
