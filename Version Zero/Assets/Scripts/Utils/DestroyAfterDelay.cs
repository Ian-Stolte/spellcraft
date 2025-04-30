using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestroyAfterDelay : MonoBehaviour
{
    public float delay;
    private float delayTimer;

    public bool fadeOut;
    private Color origColor;


    void Start()
    {
        if (GetComponent<Image>() != null)
            origColor = GetComponent<Image>().color;
        else if (GetComponent<SpriteRenderer>() != null)
            origColor = GetComponent<SpriteRenderer>().color;
        else
            fadeOut = false;
    }

    void Update()
    {
        delayTimer += Time.deltaTime;

        if (fadeOut)
        {
            float pct = delayTimer/delay;
            Color newColor = new Color(Mathf.Lerp(origColor.r, 0, pct), Mathf.Lerp(origColor.g, 0, pct), Mathf.Lerp(origColor.b, 0, pct));
            if (GetComponent<Image>() != null)
                GetComponent<Image>().color = newColor;
            else if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().color = newColor;
        }

        if (delayTimer >= delay)
        {
            Destroy(gameObject);
        }
    }
}
