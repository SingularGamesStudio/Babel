using System.Collections.Generic;
using UnityEngine;

public class Game: MonoBehaviour
{
	public List<GameObject> levels = new List<GameObject>();
	public Movement player;

	// Update is called once per frame
	public void Update()
	{
		int minAt = 0;
		float min = 1000000000;
		for (int i = 0; i < levels.Count; i++) {
			if ((player.transform.position - levels[i].transform.position).sqrMagnitude < min) {
				min = (player.transform.position - levels[i].transform.position).sqrMagnitude;
				minAt = i;
			}
		}
		player.stage = minAt;
		Camera.main.transform.position = levels[minAt].transform.position;
	}
}
