using UnityEngine;

public class WinZone: MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == 6) {
			Game.Inst.GameOver(true);
		}
	}
}
