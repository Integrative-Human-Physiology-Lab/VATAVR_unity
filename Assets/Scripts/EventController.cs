using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class EventController : MonoBehaviour
{
    private List<string[]> rowData = new List<string[]>();
    public Transform right;
    public Transform left;
    public int numTrials = 5;
    public Text congrats;
    public float speed = 3;

    float pos = 0.0f;
    bool stop = false;
    public bool mode = false;
    bool started = false;
    public float[] vertical;
    public float[] tangential;
    public DatabaseReference reference;

    bool touching = false;
    bool done = false;

    int vIndex = 0;
    int tIndex = 0;

    float fingerStart = 0f;
    float accuracy = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        //FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://vatavr-90af0.firebaseio.com");
        //reference = FirebaseDatabase.DefaultInstance.RootReference;
        vertical = new float[numTrials];
        tangential = new float[numTrials];
        congrats.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        pos = GvrControllerInput.TouchPos.y;
        
        if (!done) {
            if (!mode) { //do vertical first
                if (!started) {
                    ResetVertical();
                    started = true;
                }     
                if (started) {
                    if (GvrControllerInput.TouchDown) {
                        touching = true;
                        fingerStart = pos;
                    }
                    if (GvrControllerInput.TouchUp) {
                        touching = false;
                    }
                    if (touching) {
                        
                        float currTouch = (fingerStart - pos)/10.0f; //currTouch is difference in terms of controller
                        
                        float newY = Mathf.Clamp(right.localPosition.y + currTouch * speed, -15f, 15f);
                        right.localPosition = new Vector3(0f, newY, 37.2f);
                    }
                }          
            } 
            if (mode) { //tangential
                if (!started) {
                    ResetTangential();
                    started = true;
                }     
                if (started) {
                    if (GvrControllerInput.TouchDown) {
                        touching = true;
                        fingerStart = pos;
                    }
                    if (GvrControllerInput.TouchUp) {
                        touching = false;
                    }
                    if (touching) {
                        
                        float currTouch = (fingerStart - pos); //currTouch is difference in terms of controller
                        float newAngle = right.localEulerAngles.z + currTouch * speed;
                        if (newAngle > 180) {
                            newAngle = newAngle - 360;
                        }
                        float newZ = Mathf.Clamp(newAngle, -45f, 45f);
                    right.localRotation = Quaternion.Euler(new Vector3(0f, 0f, newZ));
                    }
                }         
            }
        }
        
            
        

        if (GvrControllerInput.AppButtonDown) {
            //save values to array
            if (done) {
                Close();
            }
            if (vIndex < numTrials) {
                accuracy = right.localPosition.y;
                vertical[vIndex] = accuracy;
                vIndex++;
                if (vIndex == numTrials) mode = true;
            } else if (tIndex < numTrials) {
                accuracy = right.localEulerAngles.z;
                if (accuracy > 180) accuracy -= 360;
                tangential[tIndex] = accuracy;
                tIndex++;
                if (tIndex == numTrials) {
                    done = true;//show final score
                    //congrats.text = "Great Job! View your dashboard for your results.";
                    //push the values
                }
            }
            started = false;
            Debug.Log(accuracy);
            accuracy = 0.0f;
        }
        
    }

    void Close() {
        //push the values
        //writeNewData();
        //make csv and save it
        saveData();
        Application.Quit();
    }

    void ResetVertical() { //between -15 and 15 for y
        
        right.localPosition = new Vector3(0f, UnityEngine.Random.Range(-15f,15f), 37.2f);
    }
    
    void ResetTangential() {
        right.localPosition = new Vector3(0f, 0f, 37.2f);
        Vector3 euler = new Vector3(0f, 0f, UnityEngine.Random.Range(-45f,45f));
        right.localRotation = Quaternion.Euler(euler);
    }

    void saveData()
    {

        // Creating First row of titles manually..
        string[] rowDataTemp = new string[3];
        rowDataTemp[0] = "Trial Number";
        rowDataTemp[1] = "Vertical";
        rowDataTemp[2] = "Tangential";
        rowData.Add(rowDataTemp);

        // You can add up the values in as many cells as you want.
        for (int i = 0; i < numTrials; i++)
        {
            rowDataTemp = new string[3];
            rowDataTemp[0] = "" + (i + 1); // name
            rowDataTemp[1] = "" + vertical[i]; // ID
            rowDataTemp[2] = "" + tangential[i]; // Income
            rowData.Add(rowDataTemp);
        }

        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));
        string time = System.DateTime.Now.ToString("hh:mm:ss").Replace(':', '-');
        string date = System.DateTime.Now.ToString("MM/dd/yyyy").Replace('/', '-');
        string filePath = Application.persistentDataPath + time + "_" + date + ".csv";
        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    //private void PostToDatabase()
    //{
    //    User user = new User();
    //    RestClient.Put("https://vatavr-90af0.firebaseio.com" + playerName + ".json", user);
    //}

    //private void writeNewData()
    //{
    //    string username = "meme";
    //    //string name = "Shannon Ke";
    //    //string email = "shannonke@gech.edu";
    //    string time = System.DateTime.Now.ToString("hh:mm:ss");
    //    string date = System.DateTime.Now.ToString("MM/dd/yyyy").Replace('/', '-');

    //    Result result = new Result(vertical, tangential);

    //    string json = JsonUtility.ToJson(result);

    //    //reference.Child("results").Child(username).Child(time).SetRawJsonValueAsync(json);
    //    for (int i = 0; i < vertical.Length; i++) {
    //        reference.Child("results").Child(username).Child(date).Child("v" + i).SetValueAsync(vertical[i]);
    //    }
    //    for (int i = 0; i < tangential.Length; i++)
    //    {
    //        reference.Child("results").Child(username).Child(date).Child("t" + i).SetValueAsync(tangential[i]);
    //    }

    //}

    public class Result
    {
        public float v1;
        public float v2;
        public float v3;
        public float v4;
        public float v5;

        public float t1;
        public float t2;
        public float t3;
        public float t4;
        public float t5;

        public Result()
        {
        }

        public Result(float[] v, float[] t)
        {
            this.v1 = v[0];
            this.v2 = v[1];
            this.v3 = v[2];
            this.v4 = v[3];
            this.v5 = v[4];

            this.t1 = t[0];
            this.t2 = t[1];
            this.t3 = t[2];
            this.t4 = t[3];
            this.t5 = t[4];

        }
    }
}
