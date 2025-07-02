using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphBar : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float smooth;
    private float target;
    private float prevTarget;
    private float progress;
    private float waitTimer;


    void Start()
    {
        target = Random.Range(0.5f, 1f);
        prevTarget = GetComponent<Image>().fillAmount;
        waitTimer = Random.Range(0, 0.3f);
    }
    
    void Update()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer < 0)
        {
            float totalDist = Mathf.Abs(target - prevTarget);
            float currDist = Mathf.Abs(GetComponent<Image>().fillAmount - prevTarget);
            progress = totalDist > 0 ? Mathf.Clamp01(currDist / totalDist) : 0f;

            float t = (1 - smooth + smooth * Mathf.Sin(progress * Mathf.PI)) * speed * Time.deltaTime;
            GetComponent<Image>().fillAmount += (target < GetComponent<Image>().fillAmount) ? -t : t;

            if (Mathf.Abs(GetComponent<Image>().fillAmount - target) < 0.01f)
            {
                prevTarget = target;
                target = Random.Range(0.2f, 1f);
                progress = 0f;
                waitTimer = Random.Range(0f, 0.5f);
            }
        }
    }
}
