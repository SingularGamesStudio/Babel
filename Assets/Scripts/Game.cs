using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game: MonoBehaviour
{
	public bool finished = false;
	public GameObject GameOverScreen;
	public GameObject VictoryScreen;
	public List<GameObject> levels = new List<GameObject>();
	public Movement player;
	public static Game Inst;


	private void Awake()
	{
		Inst = this;
	}
	// Update is called once per frame
	public void Update()
	{
		if (finished) {
			if (Input.anyKeyDown) {
				SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
			}
		}
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
	public void GameOver(bool victory)
	{
		if (finished) {
			return;
		}
		if (victory) {
			VictoryScreen.SetActive(true);
		} else {
			GameOverScreen.SetActive(true);
		}
		finished = true;
	}
}
