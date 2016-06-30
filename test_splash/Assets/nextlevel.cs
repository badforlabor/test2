using UnityEngine;
using System.Collections;

public class nextlevel : MonoBehaviour {

	public float LevelTime = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(LevelTime > 0)
		{
			LevelTime -= Time.deltaTime;	
			if(LevelTime <= 0)
			{				
                Application.LoadLevel(1);
			}
		}		
	}
}
