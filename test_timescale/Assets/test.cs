using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

    float UpdateTime = 0;
    float FixedUpdateTime = 0;
    float GUITime = 0;
    float CoroutineTime = 0;

    int UpdateCounter = 0;
    int FixedUpdateCounter = 0;
    int GUICounter = 0;
    int CoroutineCounter = 0;

    float StartTime = 0;

	// Use this for initialization
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 100;

        Time.timeScale = 0.1f;
        StartTime = Time.realtimeSinceStartup;

        StartCoroutine(Func());
	}
	
	// Update is called once per frame
	void Update () {
        UpdateTime += Time.unscaledDeltaTime;
        UpdateCounter++;
	}
    void FixedUpdate()
    {
        FixedUpdateTime += Time.deltaTime;// Time.unscaledDeltaTime;
        FixedUpdateCounter++;
    }
    void OnGUI()
    {
        GUITime += Time.deltaTime;//Time.unscaledDeltaTime;
        GUICounter++;

        GUI.Label(new Rect(10, 10, 1000, 30), "update time=" + UpdateTime + ", counter=" + UpdateCounter);
        GUI.Label(new Rect(10, 40, 1000, 30), "fixed time=" + FixedUpdateTime + ", counter=" + FixedUpdateCounter);
        GUI.Label(new Rect(10, 70, 1000, 30), "gui time=" + GUITime + ", counter=" + GUICounter);
        GUI.Label(new Rect(10, 100, 1000, 30), "coroutine time=" + CoroutineTime + ", counter=" + CoroutineCounter);
        GUI.Label(new Rect(10, 130, 1000, 30), "real time=" + (Time.realtimeSinceStartup - StartTime));
    }
    IEnumerator Func()
    {
        while (true)
        {
            CoroutineTime += Time.deltaTime;//Time.unscaledDeltaTime;
            CoroutineCounter++;
            //yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();
        }
    }
}
