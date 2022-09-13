using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SquareTarget : MonoBehaviour
{
    StringReader reader = null;

    ConductorScript conductorScript;
    private float startBeat;
    private float waitBeat;
    private float thisTargetHitTime;

    private string currentLine;
    private string targetColor;

    private bool searchNextSound = true;
    private bool hasHit = false;

    private float tickAfterHit = 0;

    [SerializeField] GameObject soundOne;
    [SerializeField] GameObject soundTwo;
    [SerializeField] GameObject soundThree;
    [SerializeField] GameObject soundFour;
    [SerializeField] GameObject soundFive;

    [SerializeField] Gradient greenFadeOutGradient;
    [SerializeField] Gradient yellowFadeOutGradient;
    [SerializeField] Gradient redFadeOutGradient;


    void SearchUntilLine(string searchString) //Code snippet method for reading lines until a line with a certain string is found.
    {
        int warningLineCount = 0;
        while (true)
        {
            currentLine = reader.ReadLine();
            if (currentLine == searchString)
            {
                break;
            }
            warningLineCount++;
            if (warningLineCount == 100)
            {
                print(searchString + " not found");
                break;
            }
        }
    }

    void CheckIfRemove(float actualHitTime, float currentBeat) //Code snippet method for checking if a target should be removed from the targetList.
    {
        bool removeThisTarget = true;
        if (conductorScript.targetsList.Count > 1)
        {
            float earliestHitTime = Mathf.Infinity;
            foreach (float targetTime in conductorScript.targetsList) //Find the first targetTime in the targetList (time is lowest).
            {
                if (targetTime < earliestHitTime)
                {
                    earliestHitTime = targetTime;
                }
            }
            if (actualHitTime > earliestHitTime) //If the current target is later than the earliest, do not remove this target.
            {
                removeThisTarget = false;
            }

        }

        //Distance from currentBeat to actualHitTime converted to a percentage.
        float accuracyHit = 100 - ((Mathf.Abs(currentBeat - actualHitTime)) * 100);

        if (removeThisTarget) //If is allowed to remove, display accuracy, record it, make the target invisible and remove the target from targetList.
        {
            //print("%Accuracy: " + accuracyHit.ToString());
            conductorScript.hitAccuracies.Add(accuracyHit);
            conductorScript.targetsList.RemoveAt(conductorScript.targetsList.IndexOf(actualHitTime));
            conductorScript.buttonPressedOnce = true;
            hasHit = true;

            if (accuracyHit >= 75)
            {
                //print("Hit!");
                conductorScript.numberTargetsHit++; //Only count the target as hit when the accuracy is grater than 75%.
                GetComponent<SpriteRenderer>().color = Color.green;
                targetColor = "green";
            }
            else if (50 <= accuracyHit && accuracyHit < 75)
            {
                //print("Almost!");
                GetComponent<SpriteRenderer>().color = Color.yellow;
                targetColor = "yellow";
            }
            else if (accuracyHit < 50)
            {
                //print("Miss!");
                GetComponent<SpriteRenderer>().color = Color.red;
                targetColor = "red";
                print("bruh");
            }
        }

    }

    void Start()
    {
        TextAsset songData = (TextAsset)Resources.Load("Sounds\\Targets\\Square\\SquareData", typeof(TextAsset));
        reader = new StringReader(songData.text);

        conductorScript = GameObject.Find("Conductor").GetComponent<ConductorScript>();
        //The calculation for startBeat is simply the same calculation for the currentBeat, but not rounded.
        startBeat = (conductorScript.songPosition * (conductorScript.bpm / 60)) + 1;

        SearchUntilLine("HIT"); //Read when the target should be hit and add the actual time to the targetList.
        thisTargetHitTime = float.Parse(reader.ReadLine());
        conductorScript.targetsList.Add(startBeat + thisTargetHitTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (searchNextSound) //If allowed to search, search until the next sound to play and read when to play it. This can only happen one at a time.
        {
            SearchUntilLine("START");
            waitBeat = float.Parse(reader.ReadLine());
            searchNextSound = false;
        }

        float actualHitTime = startBeat + thisTargetHitTime;

        /*If the current time in beats has passed the sound play time and is no longer searching a sound, try to play all sounds designated to 
         * the current time. */
        if ((conductorScript.songPosition * (conductorScript.bpm / 60)) + 1 >= (startBeat + waitBeat) && !searchNextSound)
        {
            string playFile = reader.ReadLine();

            if (playFile == "END") //END designates the end of sound playing for the target.
            {
              
                conductorScript.totalTargets++;
                Destroy(gameObject);
            }

            if (playFile == "STOP") //END designates the end of sound playing at the current time.
            {
                searchNextSound = true;
            }

            if (playFile == "Play 1") //The numbers after "Play " map to the respecitve audio players that are children to the target object. 
            {
                soundOne.GetComponent<AudioSource>().enabled = false;
                soundOne.GetComponent<AudioSource>().enabled = true;

                transform.localScale = new Vector3(transform.localScale.x + 0.75f, transform.localScale.y + 0.75f, transform.localScale.z);
                GetComponent<SpriteRenderer>().color = Color.white;
            }
            else if (playFile == "Play 2")
            {
                soundTwo.GetComponent<AudioSource>().enabled = false;
                soundTwo.GetComponent<AudioSource>().enabled = true;


                transform.localScale = new Vector3(transform.localScale.x + 0.25f, transform.localScale.y + 0.25f, transform.localScale.z);
            }
            else if (playFile == "Play 3")
            {
                soundThree.GetComponent<AudioSource>().enabled = false;
                soundThree.GetComponent<AudioSource>().enabled = true;
                transform.localScale = new Vector3(transform.localScale.x + 0.75f, transform.localScale.y + 0.75f, transform.localScale.z);
            }
            else if (playFile == "Play 4")
            {
                soundFour.GetComponent<AudioSource>().enabled = false;
                soundFour.GetComponent<AudioSource>().enabled = true;
            }
            else if (playFile == "Play 5")
            {
                soundFive.GetComponent<AudioSource>().enabled = false;
                soundFive.GetComponent<AudioSource>().enabled = true;
            }
        }

        float currentBeat = conductorScript.songPosition * (conductorScript.bpm / 60) + 1;

        if (!hasHit) //Does not trigger after hit.
        {
            if (conductorScript.buttonPressedOnce) //This public variable prevents two targets from being hit at the same time.
            {
                conductorScript.buttonPressedOnce = false;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if ((Mathf.Abs(currentBeat - actualHitTime)) <= 1)
                    {
                        CheckIfRemove(actualHitTime, currentBeat);
                    }
                }
            }

            if ((currentBeat - actualHitTime) > 1) //If the time passes one beat after when the target should be hit, count it as a miss.
            {
                conductorScript.targetsList.RemoveAt(conductorScript.targetsList.IndexOf(actualHitTime));
                GetComponent<SpriteRenderer>().color = Color.red;
                targetColor = "red";
                hasHit = true;
                //print("Miss!");
            }
        }
        else 
        {
            if (!(tickAfterHit == 100f))
            {
                tickAfterHit += 0.005f;
            }

            if (targetColor == "green")
            {
                GetComponent<SpriteRenderer>().color = greenFadeOutGradient.Evaluate(tickAfterHit);
            }
            else if (targetColor == "yellow")
            {
                GetComponent<SpriteRenderer>().color = yellowFadeOutGradient.Evaluate(tickAfterHit);
            }
            else if (targetColor == "red")
            {
                GetComponent<SpriteRenderer>().color = redFadeOutGradient.Evaluate(tickAfterHit);
            }
        }

        /*
        Testing code to see if targets are correctly placed. 

        int iteration = 1;
        foreach (float targetTime in conductorScript.targetsList)
        {
            print(currentBeat.ToString() + " " + iteration.ToString() + ": " + targetTime.ToString());
            iteration++;
        }
        */
    }
}
