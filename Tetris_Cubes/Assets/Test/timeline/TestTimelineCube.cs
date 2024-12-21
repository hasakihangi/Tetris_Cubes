using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTimelineCube : MonoBehaviour
{
    // timeline是可以存储起来的, 只需要将elapsed重置为0就可以重复使用
    // 但是timelineManager中会自动重置, 是否需要一个自动回收的参数?
    // 里面的timelineNode也不能进行回收, 不然会导致?
    
    public Cube cube;

    public bool isPerforming = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private Vector3 startPos;
    private Color startColor;
    private Vector3 startScale;

    private const float interval = 3f;
    public float timelineRate = 1f;
    private Timeline t;
    
    // Update is called once per frame
    void Update()
    {
        if (!isPerforming)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isPerforming = true;
                startPos = cube.transform.localPosition;
                t = Timeline.Get(
                    (elapsed, rate) =>
                    {
                        cube.PosLerp(startPos, new Vector3(0,5,0), elapsed/interval);
                        if (elapsed > interval)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                );
                t.OnDone = () =>
                {
                    isPerforming = false;
                    cube.transform.localPosition = startPos;
                    t = null;
                };
                GetComponent<TimelineManager>().AddTimeline(t);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                isPerforming = true;
                startPos = cube.transform.localPosition;
                startColor = cube.spriteRenderer.color;
                startScale = cube.transform.localScale;
                t = Timeline.Get(
                    (elapsed, rate) =>
                    {
                        cube.PosLerp(startPos, new Vector3(0,1,0), elapsed/interval);
                        cube.ColorLerp(startColor, Color.black, elapsed/interval);
                        cube.Shake(1f, 10, elapsed/interval);
                        cube.ScaleLerp(startScale, new Vector3(0.1f,0.1f,1f), elapsed/interval);
                        
                        if (elapsed > interval)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                );
                t.OnDone = () =>
                {
                    isPerforming = false;
                    cube.transform.localPosition = startPos;
                    cube.spriteRenderer.color = startColor;
                    cube.transform.localScale = startScale;
                    t = null;
                };
                GetComponent<TimelineManager>().AddTimeline(t);
            }
        }

        if (t != null)
        {
            t.timelineRate = timelineRate;
        }
    }
}
