using System.Collections.Generic;
using UnityEngine;

public class NPC: MonoBehaviour
{
	public bool guard = false;
	public int stage = 0;
	public string state = "idle";
	public Weapon weapon;
	public List<Leg> legs = new List<Leg>();
	GameObject target;
	[HideInInspector]
	public Rigidbody2D rb;

	[Header("Forces")]
	public float HideForce;
	public float MoveForce;
	public float strikeForce = 100f;
	public float attackForce;
	public float realignTorque;

	public float vibeForceUp;
	public float vibeForceSide;

	float MeleeTTL = 2f;
	float WalkTTL = 0f;
	float StepTTL = 0f;
	float FlipTTL = 0f;
	bool flipped = true;

	[System.Serializable]
	public class step
	{
		public List<int> legs = new List<int>();
	}

	public List<step> steps = new List<step>();
	int stepID = 0;
	string walk = "";

	// Update is called once per frame

	public void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.centerOfMass = Vector2.down;
	}

	public void Update()
	{
		if (state == "dead") {
			foreach (Leg leg in legs) {
				leg.Unlock();
			}
			return;
		}
		if (stage != Mood.Inst.player.stage) {
			return;
		}
		WalkTTL -= Time.deltaTime;
		StepTTL -= Time.deltaTime;
		MeleeTTL -= Time.deltaTime;
		FlipTTL -= Time.deltaTime;
		if (guard) {
			walk = "s";
			WalkTTL = 1;
			Vector3 diff = (Mood.Inst.player.transform.position - gameObject.transform.position);
			if (MeleeTTL <= 0 && diff.magnitude < Mood.Inst.meleeDistance && Mood.Inst.rep < Mood.Inst.Loved) {
				weapon.gameObject.GetComponent<Rigidbody2D>().AddForce(diff.normalized * attackForce);
				weapon.Activate();
				MeleeTTL = 5;
			}
		} else {
			foreach (Vulture enemy in Mood.Inst.carnivores) {
				if (enemy.state != "dead" && (enemy.transform.position - gameObject.transform.position).magnitude < Mood.Inst.enemyDistance) {
					state = "fear";

					target = enemy.gameObject;
				}
			}
			if ((Mood.Inst.fear > Mood.Inst.fearLimit && Mood.Inst.rep < Mood.Inst.Liked) || Mood.Inst.rep < -Mood.Inst.Loved) {
				if ((Mood.Inst.player.transform.position - gameObject.transform.position).magnitude < Mood.Inst.enemyDistance) {
					state = "fear";
					target = Mood.Inst.player.gameObject;
				}
			}
			switch (state) {
				case "fear":
					rb.AddForce(Vector2.down * HideForce * Time.deltaTime);
					Vector3 diff = (target.transform.position - gameObject.transform.position);
					if (MeleeTTL <= 0 && diff.magnitude < Mood.Inst.meleeDistance) {
						weapon.gameObject.GetComponent<Rigidbody2D>().AddForce(diff.normalized * attackForce);
						weapon.Activate();
						MeleeTTL = 5;
					} else {
						if (diff.x > 0) {
							walk = "l";
						} else {
							walk = "r";
						}
						WalkTTL = 1;
					}
					state = "idle";
					break;
				case "idle":
					if (WalkTTL <= 0) {
						if (UnityEngine.Random.Range(0, 2) == 0) {
							walk = "l";
						} else {
							walk = "r";
						}
						WalkTTL = UnityEngine.Random.Range(1.5f, 8f);
					}
					if ((Mood.Inst.player.transform.position - gameObject.transform.position).magnitude < Mood.Inst.vibeDistance) {
						walk = "v";
						WalkTTL = UnityEngine.Random.Range(1.5f, 8f);
					}
					break;
			}
		}
		if (WalkTTL > 0) {
			if (walk == "l" || walk == "r") {
				if (StepTTL <= 0) {
					stepID = (stepID + 1) % steps.Count;
					StepTTL = 0.2f;
				}
			}
			flipped = true;
			foreach (Leg leg in legs) {
				if (leg.collided) {
					flipped = false;
					FlipTTL = 1;
					leg.GetComponent<SpringJoint2D>().frequency = 1;
				}
			}
			if (walk != "v") {
				foreach (int leg in steps[stepID].legs) {
					legs[leg].Unlock();
					legs[leg].GetComponent<SpringJoint2D>().frequency = 20;
				}
			}
			if (flipped) {
				if (MeleeTTL <= 0 && FlipTTL <= 0) {
					weapon.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.down * strikeForce);
					rb.AddTorque(realignTorque);
					MeleeTTL = 4;
				}
			} else {
				switch (walk) {
					case "l":
						rb.AddForce(Vector2.left * MoveForce * Time.deltaTime);
						break;
					case "r":
						rb.AddForce(Vector2.right * MoveForce * Time.deltaTime);
						break;
					case "v":
						weapon.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * vibeForceUp * Time.deltaTime);
						if (WalkTTL / 2 - (int)(WalkTTL / 2) > 0.5) {
							weapon.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.right * vibeForceSide * Time.deltaTime);
						} else {
							weapon.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * vibeForceSide * Time.deltaTime);
						}
						break;
				}
			}
		}
	}
}
