using System.Collections.Generic;
using UnityEngine;

public class Movement: MonoBehaviour
{
	public int stage = 0;

	public Leg LLeg;
	public Leg RLeg;
	public Rigidbody2D Body;
	public Rigidbody2D StaticBody;
	public Rigidbody2D Eye;

	public bool aiming = false;
	public bool grounded = false;
	Leg AimingLeg = null;
	Leg ToUnlock = null;
	float CurAimCooldown = 0;

	public float AimCooldown = 1f;

	public float LegMovementLimit = 0.1f;

	[Header("Forces")]
	public float StepForceLeg = 1000f;
	public float StepForceBody = 2200f;
	public float StepRealignForce = 300f;

	public float EyeForce = 30f;
	public float AirMoveForce = 500f;
	public float AimForce = 5000f;
	public float ShootForce = 400000f;




	[Header("Stretch")]
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
		LegStatus();
		CollapseSprings();

		ChooseAimingLeg();
		Aim();
		Grapple();


		UnlockControl();
		LRMovement();

		Eye.position += (Body.position - StaticBody.position);
		StaticBody.position = Body.position;
	}

	void UnlockControl()
	{
		if (Input.GetMouseButton(1)) {
			LLeg.Unlock();
			RLeg.Unlock();
		}
	}

	void ChooseAimingLeg()
	{
		Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if (LLeg.collided == RLeg.collided) {
			if ((LLeg.rb.position - mousePos).sqrMagnitude > (RLeg.rb.position - mousePos).sqrMagnitude) {
				AimingLeg = (LLeg.collided) ? LLeg : RLeg;
			} else {
				AimingLeg = (LLeg.collided) ? RLeg : LLeg;
			}
		} else {
			if (LLeg.collided) {
				AimingLeg = RLeg;
			} else {
				AimingLeg = LLeg;
			}
		}
	}

	void Aim()
	{
		if (CurAimCooldown > 0) {
			CurAimCooldown -= Time.deltaTime;
			aiming = false;
			return;
		}
		if (!Input.GetMouseButton(0)) {
			aiming = false;
			return;
		}
		if (AimingLeg != null) {
			aiming = true;
			Vector2 force = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - AimingLeg.rb.position).normalized;

			AimingLeg.Unlock();
			AimingLeg.rb.AddForce(force * AimForce * Time.deltaTime);
			Eye.AddForce(force * EyeForce * Time.deltaTime);
		}
	}

	void Grapple()
	{
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


	void LegStatus()
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
		bool newGrounded = LLeg.collided || RLeg.collided;


		if (!grounded && newGrounded) {
			for (int i = 0; i < LLeg.transform.parent.childCount; i++) {
				var springs = LLeg.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
				foreach (SpringJoint2D spring in springs) {
					if (spring.connectedBody.name == "Static") {
						spring.connectedBody = Body;
					}
				}
			}
			for (int i = 0; i < RLeg.transform.parent.childCount; i++) {
				var springs = RLeg.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
				foreach (SpringJoint2D spring in springs) {
					if (spring.connectedBody.name == "Static") {
						spring.connectedBody = Body;
					}
				}
			}
		}
		if (grounded && !newGrounded) {
			for (int i = 0; i < LLeg.transform.parent.childCount; i++) {
				var springs = LLeg.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
				foreach (SpringJoint2D spring in springs) {
					if (spring.connectedBody.name == "Body") {
						spring.connectedBody = StaticBody;
					}
				}
			}
			for (int i = 0; i < RLeg.transform.parent.childCount; i++) {
				var springs = RLeg.transform.parent.GetChild(i).GetComponents<SpringJoint2D>();
				foreach (SpringJoint2D spring in springs) {
					if (spring.connectedBody.name == "Body") {
						spring.connectedBody = StaticBody;
					}
				}
			}
		}

		grounded = newGrounded;
	}


	void LRMovement()
	{

		if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) {
			return;
		}
		Leg ActiveLeg = null;
		if (LLeg.collided && RLeg.collided) {
			if (Input.GetKey(KeyCode.D))
				ActiveLeg = LLeg;
			if (Input.GetKey(KeyCode.A))
				ActiveLeg = RLeg;
		} else if (LLeg.collided || RLeg.collided) {
			if (LLeg.collided)
				ActiveLeg = RLeg;
			if (RLeg.collided)
				ActiveLeg = LLeg;
		} else {
			if (Input.GetKey(KeyCode.D)) {
				Body.AddForce(Vector2.right * AirMoveForce * Time.deltaTime);
				Eye.AddForce(Vector2.right * EyeForce * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.A)) {
				Body.AddForce(Vector2.left * AirMoveForce * Time.deltaTime);
				Eye.AddForce(Vector2.left * EyeForce * Time.deltaTime);
			}
			return;
		}
		Leg OppositeLeg = Opposite(ActiveLeg);

		Vector2 LegDir = (ActiveLeg.rb.position - Body.position);
		Vector2 BodyDir = (Body.position - OppositeLeg.rb.position);

		if (Input.GetKey(KeyCode.D)) {
			LegDir = new Vector2(LegDir.y, -LegDir.x);
			BodyDir = new Vector2(BodyDir.y, -BodyDir.x);
			Eye.AddForce(Vector2.right * EyeForce * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.A)) {
			LegDir = new Vector2(-LegDir.y, LegDir.x);
			BodyDir = new Vector2(-BodyDir.y, BodyDir.x);
			Eye.AddForce(Vector2.left * EyeForce * Time.deltaTime);
		}
		ActiveLeg.Unlock();

		if ((LLeg.rb.position - RLeg.rb.position).sqrMagnitude >= LegMovementLimit * LegMovementLimit || (ActiveLeg.rb.position + LegDir.normalized - OppositeLeg.rb.position).sqrMagnitude > (LLeg.rb.position - RLeg.rb.position).sqrMagnitude) {
			ActiveLeg.rb.AddForce(LegDir.normalized * StepForceLeg * Time.deltaTime);
		} else {
			Vector2 speedProj = Vector2.Dot(ActiveLeg.rb.velocity, LegDir) / Vector2.Dot(LegDir, LegDir) * LegDir;
			if (Vector2.Dot(speedProj, LegDir) > 0)
				ActiveLeg.rb.AddForce(-LegDir.normalized * speedProj.magnitude * StepRealignForce * Time.deltaTime);
		}
		Body.AddForce(BodyDir.normalized * StepForceBody * Time.deltaTime);
	}

	Leg Opposite(Leg l)
	{
		if (l == null) return null;
		if (l == LLeg) return RLeg;
		return LLeg;
	}
}
