using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPosition : MonoBehaviour {

	public Vector3[] Positions;
	public Vector3[] Rotations;

	// Use this for initialization
	void Start () {
		int index = Random.Range(0, Positions.Length);
		transform.localPosition = Positions[index]; 
		index = Random.Range(0, Rotations.Length);
		transform.localRotation = Quaternion.Euler(Rotations[index]);
	}

}
