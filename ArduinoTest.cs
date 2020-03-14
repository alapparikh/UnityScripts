using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class ArduinoTest : MonoBehaviour {

    public string portName;
    public float minArduinoVal = 0;
    public float maxArduinoVal = 1024;

    SerialPort stream;
    public static float arduinoInput;

	// Use this for initialization
	void Start () {

        stream = new SerialPort(portName, 9600);
        stream.ReadTimeout = 50;
        stream.Open();

        StartCoroutine
        (
            AsynchronousReadFromArduino
            ((string s) => UpdateArduinoValue(s),     // Callback
                () => Debug.LogError("Error!"), // Error callback
                10000f                          // Timeout (milliseconds)
            )
        );
    }
	
    // Update Arduino input value
    public void UpdateArduinoValue(string inputData)
    {
        Debug.Log(inputData);
        arduinoInput = Mathf.Clamp((float.Parse(inputData) - minArduinoVal) / (maxArduinoVal - minArduinoVal), 0, 1);
    }

    // Getting and handling Arduino input data. No need to touch this. 
    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            try
            {
                dataString = stream.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                initialTime = DateTime.Now;
                yield return null;
                //yield break; // Terminates the Coroutine
            }
            else
                yield return null; // Wait for next frame

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }

}
