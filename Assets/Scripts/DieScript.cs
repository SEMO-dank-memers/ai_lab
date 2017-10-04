using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieScript : MonoBehaviour
{
	void OnCollisionEnter2D()
	{
		Destroy(gameObject);
	}
}
