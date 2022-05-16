using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class ScreenRecorder : MonoBehaviour
{
    public bool Record;

    [Tooltip("Supersizing factor between 1 and 4.")]
    public int Supersize = 1;

    private int _frameCounter = 0;
    private string _framesDirectory;
    private string _captureDirectory;

    private void Start()
    {
        string dataPath = Application.dataPath;
        _framesDirectory = Directory.GetParent(dataPath) + $"/SavedFrames/{SceneManager.GetActiveScene().name}/Frames/";
        _captureDirectory = Directory.GetParent(dataPath) + $"/SavedFrames/{SceneManager.GetActiveScene().name}/ScreenShots/";

        //Ensure directories exist
        Directory.CreateDirectory(_framesDirectory);
        Directory.CreateDirectory(_captureDirectory);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) Record = !Record;

        if (Record)
        {
            StartCoroutine(RecordFrames());
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            string file = _captureDirectory + $"Frame_{DateTime.Now.ToString("y-MM-dd-THH-mm-ss")}.png";
            ScreenCapture.CaptureScreenshot(file, Supersize);
        }
    }

    IEnumerator RecordFrames()
    {
        string file = _framesDirectory + $"Frame_{_frameCounter}.png";
        ScreenCapture.CaptureScreenshot(file, Supersize);
        _frameCounter++;
        yield return new WaitForEndOfFrame();
    }
}
