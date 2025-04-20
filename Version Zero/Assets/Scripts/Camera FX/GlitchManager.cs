using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchManager : MonoBehaviour
{
    private Glitch g;
    private float origStr;
    [HideInInspector] public bool showingGlitch;

    void Start()
    {
        g = GetComponent<Glitch>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ShowGlitch(2);
        }
    }

    public void ShowGlitch(float duration = 0.5f, float strength = 1)
    {
        showingGlitch = true;
        origStr = g.glitch;
        g.glitch = strength;
        StartCoroutine(EndGlitch(duration));
        //AudioManager.Instance.PlayLoop(SFXNAME.Glitch);
        //AudioManager.Instance.SetSFXVolume(SFXNAME.Glitch, 0.3f);
    }

    private void EndGlitch()
    {
        showingGlitch = false;
        g.glitch = origStr;
        //AudioManager.Instance.StopLoop(SFXNAME.Glitch);
    }

    private IEnumerator EndGlitch(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndGlitch();
    }
}
