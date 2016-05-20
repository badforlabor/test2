using UnityEngine;
using System.Collections;

public class MoonInfo : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Quaternion q = LibreMath.ToQuaternion(transform.forward);
        if (this.transform.rotation != q)
        {
            Debug.LogError("xxxxxxxxx 方向不对！" + q + ", r=" + transform.rotation);
        }

        // 公式：forward = rotation * Vectro3.forward
        // 那么 localrotation 对应的localforward是？

        Debug.Log("角度：" + LibreMath.ToAngle(this.transform.localRotation));
        if (this.transform.localEulerAngles != LibreMath.ToAngle(this.transform.localRotation))
        {
            Debug.LogError("xxxxxxxxx 角度！" + this.transform.localEulerAngles + ", angle=" + LibreMath.ToAngle(this.transform.localRotation));
        }


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
