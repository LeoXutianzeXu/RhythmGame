using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class Countdown : MonoBehaviour
{
    [SerializeField] GameObject conductorObject;

    [SerializeField] float countdownBPM;

    TextMeshProUGUI countdownText;

    AudioSource metronomeMeasure;
    AudioSource metronomeBeat;

    private float countDownPosition;
    private float startDspTime;

    private int currentBeatInMeasure;
    private int lastBeatInMeasure = 0;

    private bool allowGo = false;

    // Start is called before the first frame update
    void Start()
    {
        startDspTime = (float)AudioSettings.dspTime;
        countdownText = GameObject.Find("Countdown Text").GetComponent<TextMeshProUGUI>();

        metronomeBeat = GameObject.Find("Metronome Beat").GetComponent<AudioSource>();
        metronomeMeasure = GameObject.Find("Metronome Measure").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Calculations similar to that of the ConductorScript variables.

        countDownPosition = (float)(AudioSettings.dspTime - startDspTime);
        currentBeatInMeasure = (Mathf.FloorToInt(countDownPosition * (countdownBPM / 60)) % 4) + 1;

        //When the beat changes, change the text of countdown, play metronome sounds and after 'go', enable the Conductor.
        if (lastBeatInMeasure != currentBeatInMeasure) 
        {
            if (currentBeatInMeasure == 2)
            {
                if (allowGo)
                {
                    conductorObject.SetActive(true);
                    Destroy(gameObject);
                }
                else
                {
                    countdownText.text = "3";

                    metronomeMeasure.enabled = true;
                }
            }
            else if (currentBeatInMeasure == 3)
            {
                countdownText.text = "2";

                metronomeMeasure.enabled = false;
                metronomeBeat.enabled = true;
            }
            else if (currentBeatInMeasure == 4)
            {
                countdownText.text = "1";
                allowGo = true;

                metronomeBeat.enabled = false;
                metronomeBeat.enabled = true;
            }
            else
            {
                if (allowGo)
                {
                    countdownText.text = "Go!";

                    metronomeBeat.enabled = false;
                    metronomeBeat.enabled = true;
                }
            }
            lastBeatInMeasure = currentBeatInMeasure;
        }
    }
}
