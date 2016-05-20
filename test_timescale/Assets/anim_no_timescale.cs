using UnityEngine;
using System;
using System.Collections;


    public static class MLAnimationExt
	{	
        public static IEnumerator Play(this Animation animation, string clipName, bool useTimeScale, Action onComplete)
        {
            //We Don't want to use timeScale, so we have to animate by frame..
            if (!useTimeScale)
            {
                AnimationState _currState = animation[clipName];
                bool isPlaying = true;
                float _startTime = 0F;
                float _progressTime = 0F;
                float _timeAtLastFrame = 0F;
                float _timeAtCurrentFrame = 0F;
                float deltaTime = 0F;


                animation.Play(clipName);

                _timeAtLastFrame = Time.realtimeSinceStartup;
                while (isPlaying)
                {
                    _timeAtCurrentFrame = Time.realtimeSinceStartup;
                    deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
                    _timeAtLastFrame = _timeAtCurrentFrame;

                    _progressTime += deltaTime;
                    _currState.normalizedTime = _progressTime / _currState.length;
                    animation.Sample();

                    //Debug.Log(_progressTime);

                    if (_progressTime >= _currState.length)
                    {
                        //Debug.Log(&quot;Bam! Done animating&quot;);
                        if (_currState.wrapMode != WrapMode.Loop)
                        {
                            //Debug.Log(&quot;Animation is not a loop anim, kill it.&quot;);
                            //_currState.enabled = false;

                            _currState.normalizedTime = 0.99f;
                            animation.Sample();

                            isPlaying = false;
                        }
                        else
                        {
                            //Debug.Log(&quot;Loop anim, continue.&quot;);
                            _progressTime = 0.0f;
                        }
                    }

                    yield return new WaitForEndOfFrame();
                }
                yield return null;
                if (onComplete != null)
                {
                    onComplete();
                }
            }
            else
            {
                animation.Play(clipName);
            }
        }
		
	}

public class anim_no_timescale : MonoBehaviour {

	public AnimationClip anim;

	// Use this for initialization
	void Start () {
		Animation animation = this.gameObject.GetComponent<Animation>();
            StartCoroutine(animation.Play(anim.name, false, delegate()
            {
                Debug.Log("播放完毕");
            }));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	
}
