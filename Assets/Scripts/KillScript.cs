using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillScript : MonoBehaviour
{
	public GameObject winText;
	public Collider2D thisCollider;

	void OnCollisionEnter2D()
	{
		winText.SetActive(true);
		Time.timeScale = 0;
	}
}
