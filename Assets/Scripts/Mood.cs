using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mood: MonoBehaviour
{
	public Text repText;
	public Text fearText;

	public static Mood Inst;
	public Movement player;
	[HideInInspector]
	public float rep;
	[HideInInspector]
	public float fear;

	public float fearLimit;
	public List<Vulture> carnivores = new List<Vulture>();

	public bool playerVibing = false;
	public float vibeTime = 0;
	[Header("Distance")]
	public float enemyDistance;
	public float meleeDistance;
	public float vibeDistance;

	[Header("Reputation coefs")]
	public float VelFear;
	public float FearDrop;
	public float TouchFear;
	public float TouchRespect;
	public float FearDisrespectTreshold;
	public float FearDisrespectCoef;
	public float VibeRespect;
	public float VibeFear;
	public float Loved;
	public float Liked;

	public float AmplitudeThreshold;
	Queue<float> amplitude = new Queue<float>();
	Vector3 lastPos = Vector3.zero;
	float ampSum = 0;

	// Update is called once per frame
	void Awake()
	{
		Inst = this;
	}

	public void Update()
	{
		if (player.grounded && player.aiming) {
			amplitude.Enqueue((Input.mousePosition - lastPos).magnitude);
			ampSum += (Input.mousePosition - lastPos).magnitude;
			if (amplitude.Count > 200) {
				ampSum -= amplitude.Dequeue();
			}
			lastPos = Input.mousePosition;
		} else {
			amplitude.Clear();
			ampSum = 0;
		}


		repText.text = "Reputation: " + rep.ToString();
		if (rep > Loved) {
			repText.color = Color.green;
		} else if (rep > Liked) {
			repText.color = Color.cyan;
		} else if (rep > -Liked) {
			repText.color = Color.grey;
		} else if (rep > -Loved) {
			repText.color = Color.yellow;
		} else {
			repText.color = Color.red;
		}
		fearText.text = "Fear: " + fear.ToString();
		if (fear > FearDisrespectTreshold) {
			fearText.color = Color.red;
		} else if (fear > fearLimit) {
			fearText.color = Color.yellow;
		} else {
			fearText.color = Color.grey;
		}

		fear += player.Body.velocity.sqrMagnitude * VelFear * Time.deltaTime;
		fear -= FearDrop * Time.deltaTime;
		if (fear < 0)
			fear = 0;
		if (fear > FearDisrespectTreshold && rep < Liked) {
			rep -= Time.deltaTime * FearDisrespectCoef;
		}
		vibeTime += Time.deltaTime;

		Debug.Log(ampSum / amplitude.Count);

		if (player.grounded && player.aiming && ampSum / amplitude.Count > AmplitudeThreshold) {
			playerVibing = true;//TODO: амплитуда
		} else {
			playerVibing = false;
			vibeTime = 0;
		}

		if (vibeTime > 1.5) {
			rep += Time.deltaTime * VibeRespect;
			fear -= Time.deltaTime * VibeFear;
		}
	}
}
