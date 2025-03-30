using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    public void FadeIn(float n)
    {
        StartCoroutine(FadeInCor(n));
    }

    private IEnumerator FadeInCor(float n)
    {
        float elapsed = 0f;
        while (elapsed < n)
        {
            elapsed += Time.deltaTime;
            GetComponent<CanvasGroup>().alpha = elapsed/n;
            yield return null;
        }
        GetComponent<CanvasGroup>().alpha = 1;
    }

    public void FadeOut(float n)
    {
        StartCoroutine(FadeOutCor(n));
    }

    private IEnumerator FadeOutCor(float n)
    {
        float elapsed = n;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            GetComponent<CanvasGroup>().alpha = elapsed/n;
            yield return null;
        }
        GetComponent<CanvasGroup>().alpha = 0;
    }

    public void FadeInOut(float n1, float n2)
    {
        StartCoroutine(FadeInOutCor(n1, n2));
    }

    private IEnumerator FadeInOutCor(float n1, float n2)
    {
        float elapsed = 0f;
        while (elapsed < n1)
        {
            elapsed += Time.deltaTime;
            GetComponent<CanvasGroup>().alpha = elapsed/n1;
            yield return null;
        }

        elapsed = n2;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            GetComponent<CanvasGroup>().alpha = elapsed/n2;
            yield return null;
        }
        GetComponent<CanvasGroup>().alpha = 0;
    }
}
