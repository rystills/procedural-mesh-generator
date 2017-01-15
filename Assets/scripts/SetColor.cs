using UnityEngine;
using System.Collections;

public class SetColor : MonoBehaviour {

	public float r;
	public float g;
	public float b;

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(r, g, b));
	}
}