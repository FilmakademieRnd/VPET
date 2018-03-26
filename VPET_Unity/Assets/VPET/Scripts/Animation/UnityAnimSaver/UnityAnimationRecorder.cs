/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

public class UnityAnimationRecorder : MonoBehaviour {

	// save file path
	public string savePath;
	public string fileName;

	// use it when save multiple files
	int fileIndex = 0;

	public KeyCode startRecordKey = KeyCode.Q;
	public KeyCode stopRecordKey = KeyCode.W;

	// options
	public bool showLogGUI = false;
	string logMessage = "";

	public bool recordLimitedFrames = false;
	public int recordFrames = 1000;
	int frameIndex = 0;

	public bool changeTimeScale = false;
	public float timeScaleOnStart = 0.0f;
	public float timeScaleOnRecord = 1.0f;

	Transform[] recordObjs;
	UnityObjectAnimation[] objRecorders;

	bool isStart = false;
	float nowTime = 0.0f;

	// Use this for initialization
	void Start () {
		SetupRecorders ();

	}

	void SetupRecorders () {
		recordObjs = gameObject.GetComponentsInChildren<Transform> ();
		objRecorders = new UnityObjectAnimation[recordObjs.Length];

		frameIndex = 0;
		nowTime = 0.0f;

		for (int i = 0; i < recordObjs.Length; i++) {
			string path = AnimationRecorderHelper.GetTransformPathName (transform, recordObjs [i]);
			objRecorders [i] = new UnityObjectAnimation ( path, recordObjs [i]);
		}

		if (changeTimeScale)
			Time.timeScale = timeScaleOnStart;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyDown (startRecordKey)) {
			StartRecording ();
		}

		if (Input.GetKeyDown (stopRecordKey)) {
			StopRecording ();
		}

		if (isStart) {
			nowTime += Time.deltaTime;

			for (int i = 0; i < objRecorders.Length; i++) {
				objRecorders [i].AddFrame (nowTime);
			}
		}

	}

	public void StartRecording () {
		CustomDebug ("Start Recorder");
		isStart = true;
		Time.timeScale = timeScaleOnRecord;
	}


	public void StopRecording () {
		CustomDebug ("End Record, generating .anim file");
		isStart = false;

		ExportAnimationClip ();
		ResetRecorder ();
	}

	void ResetRecorder () {
		SetupRecorders ();
	}


	void FixedUpdate () {

		if (isStart) {

			if (frameIndex < recordFrames) {
				for (int i = 0; i < objRecorders.Length; i++) {
					objRecorders [i].AddFrame (nowTime);
				}

				++frameIndex;
			} else {
				isStart = false;
				ExportAnimationClip ();
				CustomDebug ("Recording Finish, generating .anim file");
			}
		}
	}

	void OnGUI () {
		if (showLogGUI)
			GUILayout.Label (logMessage);
	}

	void ExportAnimationClip () {

		string exportFilePath = savePath + fileName;

		// if record multiple files when run
		if (fileIndex != 0)
			exportFilePath += "-" + fileIndex + ".anim";
		else
			exportFilePath += ".anim";


		AnimationClip clip = new AnimationClip ();
		clip.name = fileName;

		for (int i = 0; i < objRecorders.Length; i++) {
			UnityCurveContainer[] curves = objRecorders [i].curves;

			for (int x = 0; x < curves.Length; x++) {
				clip.SetCurve (objRecorders [i].pathName, typeof(Transform), curves [x].propertyName, curves [x].animCurve);
			}
		}

		clip.EnsureQuaternionContinuity ();
		AssetDatabase.CreateAsset ( clip, exportFilePath );

		CustomDebug (".anim file generated to " + exportFilePath);
		fileIndex++;
	}

	void CustomDebug ( string message ) {
		if (showLogGUI)
			logMessage = message;
		else
			Debug.Log (message);
	}
}
#endif