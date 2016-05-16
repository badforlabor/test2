using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StarInfo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Quaternion q = LibreMath.ToQuaternion(transform.forward);
        if (this.transform.rotation != q)
        {
            Debug.LogError("xxxxxxxxx 方向不对！" + q + ", r=" + transform.rotation);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.ArrowCap(0, transform.position, transform.rotation, 1.2f);
#endif
    }
}
