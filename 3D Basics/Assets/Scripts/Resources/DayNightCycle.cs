using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public float tickTime;

    public int days;
    [SerializeField] private TextMeshProUGUI dayTxt;

    public int hours;
    public int minutes;
    [SerializeField] private TextMeshProUGUI timeTxt;
    public bool pm;
    [SerializeField] private TextMeshProUGUI amPmTxt;


    void Start()
    {
        StartCoroutine(TrackTime());
    }


    private IEnumerator TrackTime()
    {
        while (true)
        {
            minutes += 10;
            if (minutes == 60)
            {
                minutes = 0;
                hours++;
            }
            if (hours == 13)
            {
                hours = 1;
                pm = !pm;
                if (!pm)
                    days++;
            }

            string minutesTxt = (minutes == 0) ? "00" : "" + minutes;
            timeTxt.text = hours + ":" + minutesTxt;
            amPmTxt.text = pm ? "hE" : "za";
            amPmTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-760 + timeTxt.preferredWidth*2, -450);
            dayTxt.text = "" + days;

            Quaternion startRot = transform.rotation;
            Quaternion newRot;
            if (transform.rotation.eulerAngles.x > 180)
                newRot = startRot * Quaternion.Euler((180f/50f), 0, 0);
            else
                newRot = startRot * Quaternion.Euler((180/94f), 0, 0);
            float delay = 0;
            while (delay < tickTime)
            {
                transform.rotation = Quaternion.Slerp(startRot, newRot, delay/tickTime);
                delay += Time.deltaTime;
                yield return null;
            }
            transform.rotation = newRot;
        }
    }
}
