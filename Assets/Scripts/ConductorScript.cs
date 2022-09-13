using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ConductorScript : MonoBehaviour
{
    [SerializeField] GameObject squareObject;

    [SerializeField] bool metronomeEnabled;

    [SerializeField] string loadSongName;

    [SerializeField] Vector2 laneOnePos;
    [SerializeField] Vector2 laneTwoPos;
    [SerializeField] Vector2 laneThreePos;
    [SerializeField] Vector2 laneFourPos;

    FileInfo theSourceFile = null;
    StringReader reader = null;
    AudioSource music = null;

    Dictionary<float, string> eventsDictionary = new Dictionary<float, string>();

    TextMeshProUGUI displayText;
    TextMeshProUGUI displaySubtitle;

    private string currentLine;
    private string repeatCondition;

    public float bpm;
    public float offset;
    public float songPosition;
    public float startDspTime;

    public int currentBeat;
    public int currentBeatInMeasure;
    public int currentMeasure;

    public int numberTargetsHit = 0;
    public int totalTargets = 0;

    private int lastBeat;
    private int lastMeasure;

    public bool buttonPressedOnce = false;
    private bool repeating = false;

    public ArrayList targetsList = new ArrayList();
    public ArrayList hitAccuracies = new ArrayList();

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

    void Start()
    {
        // Loads song data from a given path in the Resources folder.
        TextAsset songData = (TextAsset)Resources.Load($"Songs\\{loadSongName}\\Data\\{loadSongName}", typeof(TextAsset)); 
        reader = new StringReader(songData.text);

        displayText = GameObject.Find("Main Text").GetComponent<TextMeshProUGUI>();
        displaySubtitle = GameObject.Find("Subtitle Text").GetComponent<TextMeshProUGUI>();

        startDspTime = (float)(AudioSettings.dspTime);

        // Keep reading until the line BPM: is found, after which will set the number on the next line to the bpm and offset variables.

        SearchUntilLine("BPM:");
        bpm = int.Parse(reader.ReadLine());

        SearchUntilLine("Offset:");
        offset = int.Parse(reader.ReadLine());
    }
    void Update()
    {
        /* offset is the song offset. music.pitch is used to dilate the position if changing the pitch of the song is necessary.
         * startDspTime is set when the song is played.
         */
        
        music = GetComponent<AudioSource>();

        songPosition = (float)(AudioSettings.dspTime - startDspTime) * music.pitch - offset;

        /* Since songPosition is in seconds, currentBeat is calculated by multiplying the current time in seconds by how many beats there should 
         * be in a second. 
         * 
         * currentMeasure is calculated by dividing the currentBeat by 4, subtracting a little bit from it (so that the fourth 
         * beat will not count as the second measure), adding one to it (so that the first measure is not the zeroth) and finally taking the 
         * floor integer of the result.
         */



        currentBeat = Mathf.FloorToInt(songPosition*(bpm/60)) + 1;
        currentBeatInMeasure = (Mathf.FloorToInt(songPosition * (bpm / 60)) % 4) + 1;

        if (lastBeat != currentBeatInMeasure) //When the beat changes, play the correct metronome sound.
        {
            lastBeat = currentBeatInMeasure;

            if (metronomeEnabled)
            {
                GameObject.Find("Metronome Beat").GetComponent<AudioSource>().enabled = false;
                GameObject.Find("Metronome Measure").GetComponent<AudioSource>().enabled = false;

                if (currentBeatInMeasure == 1)
                {
                    GameObject.Find("Metronome Measure").GetComponent<AudioSource>().enabled = true;

                }
                else
                {
                    GameObject.Find("Metronome Beat").GetComponent<AudioSource>().enabled = true;
                }
            }

        }



        currentMeasure = Mathf.FloorToInt(((currentBeat - 0.01f) / 4)) + 1;

        if (lastMeasure != currentMeasure) //When the measure changes, add the events of the next measure to the current events dictionary.
        {
            if (!repeating)
            {
                lastMeasure = currentMeasure;
                SearchUntilLine("["); //See Concept.txt data structure.
                string firstMeasureLine = reader.ReadLine();
                if (firstMeasureLine == "Yes") //Allow empty measures to skip the loop
                {
                    while (true) //While loop to search for all events in the measure.
                    {
                        SearchUntilLine("{");

                        SearchUntilLine("Time:");
                        string searchedTime = reader.ReadLine();

                        /* If the event is a "Beat #" type, set the time equal to the ((currentMeasure - 1) * 4) (since there are 4 beats in a measure)
                         * + the number of beat specified in the number after "Beat". */
                        if (searchedTime[..4] == "Beat")
                        {
                            float beatTime = ((currentMeasure - 1) * 4) + float.Parse(searchedTime[5..].ToString());
                            searchedTime = beatTime.ToString();
                        }

                        SearchUntilLine("Event:");
                        string searchedEvent = reader.ReadLine();

                        if (!eventsDictionary.ContainsKey(float.Parse(searchedTime)))
                        {
                            eventsDictionary.Add(float.Parse(searchedTime), searchedEvent);
                        }

                        SearchUntilLine("}");

                        string endOfMeasure = reader.ReadLine();

                        if (endOfMeasure.Length >= 8)
                        {
                            if (endOfMeasure[..6] == "REPEAT") //The string "REPEAT" designates that the events of the measure need to be repeated.
                            {
                                repeating = true;
                                repeatCondition = endOfMeasure[7..];
                                break;
                            }
                        }

                        if (endOfMeasure == "STOP") //The string "STOP" designates the end of a measure.
                        {
                            break;
                        }
                    }
                }

                else if (firstMeasureLine == "END") //Displays statistics at song end.
                {
                    print("Song End");
                    print("Number of Targets Hit:" + numberTargetsHit.ToString() + " / " + totalTargets.ToString());

                    float sumAccuracies = 0;
                    foreach (float accuracy in hitAccuracies)
                    {
                        sumAccuracies += accuracy;
                    }

                    float avgAccuracy = (sumAccuracies / hitAccuracies.Count);
                    print("Average Accuracy Percentage:" + avgAccuracy.ToString());
                    Destroy(gameObject);
                }

                SearchUntilLine("]");
            }
            else
            {
                if (repeatCondition == "Space")
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        repeating = false;
                    }
                }
            }
        }

        Dictionary<float, string> designatedDict = eventsDictionary; //If repeating, the measureEventsDict is used since the eventsDict is cleared.

        foreach ((float dicTime, string dicEvent) in designatedDict) //Cycles through eventsDictionary to activate events.
        {
            print(dicEvent);
            if ((songPosition * (bpm / 60)) + 1 >= dicTime) //Events are activated after the current time (in terms of beats) has passed the event time.
            {
                if (dicEvent == "Print")
                {
                    print("PrintEvent");
                }
                else if (dicEvent[..6] == "Square")
                {
                    GameObject instantiatedObject = Instantiate(squareObject);
                    instantiatedObject.SetActive(true);

                    if (dicEvent[7].ToString() == "1") // The number after "Square" designates the lane position of the target.
                    {
                        instantiatedObject.transform.position = laneOnePos;
                    }
                    else if (dicEvent[7].ToString() == "2")
                    {
                        instantiatedObject.transform.position = laneTwoPos;
                    }
                    else if (dicEvent[7].ToString() == "3")
                    {
                        instantiatedObject.transform.position = laneThreePos;
                    }
                    else
                    {
                        instantiatedObject.transform.position = laneFourPos;
                    }
                }
                else if (dicEvent[..7] == "Display")
                {
                    displayText.text = dicEvent[8..];
                }
                else if (dicEvent[..8] == "Subtitle")
                {
                    displaySubtitle.text = dicEvent[9..];
                }

                if (repeating && !eventsDictionary.ContainsKey(dicTime + 4))
                {
                    eventsDictionary.Add(dicTime + 4, dicEvent);
                }
                eventsDictionary.Remove(dicTime); //Delete the event after activation.
                break;
            }
        }
    }
}
