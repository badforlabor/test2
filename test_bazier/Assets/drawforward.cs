using UnityEngine;
using System.Collections;

public class drawforward : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {        
        UnityEditor.Handles.ArrowCap(0, this.transform.position, this.transform.rotation, 3);
    }
}
