using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Valve.VR;

namespace vpet
{
    [CustomEditor(typeof(VIVEInput))]
    [CanEditMultipleObjects]
    public class VIVEInputEditor : Editor
    {
        private VIVEInput input;
        private SerializedProperty recording;
        private Texture recordStart, recordStop;
        private static List<SteamVR_TrackedObject.EIndex> assignedDevices;
        private ServerAdapterHost adapter;

        private float timer = 1;
        private int searchCounter = 0;
        private const float TIMER_DELAY = 0.3f;     //Can be tweaked if wanted (Problems at < 0.2f)
        private PlayModeStateChange currentState;

        void Awake()
        {
            assignedDevices = new List<SteamVR_TrackedObject.EIndex>();

            //Fix for already selected devices not in assignedDevices (Unity Startup)
            foreach (VIVEInput i in FindObjectsOfType<VIVEInput>())
                if (i.device != SteamVR_TrackedObject.EIndex.None && !assignedDevices.Contains(i.device))
                    assignedDevices.Add(i.device);

            adapter = FindObjectOfType<ServerAdapterHost>();
        }

        void OnEnable()
        {
            //Used for setting recording on multiple objects
            recording = serializedObject.FindProperty("recording");
            recordStart = Resources.Load<Texture>("VPET/Icons/RecordStart");
            recordStop = Resources.Load<Texture>("VPET/Icons/RecordStop");

            EditorApplication.update += SearchDevice;
        }

        void OnDisable()
        {
            EditorApplication.update -= SearchDevice;
        }

        public override void OnInspectorGUI()
        {
            //Custom editor script fix when not saving on play, DrawDefaultInspector not needed in that case (Would draw all public variables)
            EditorUtility.SetDirty(target);     //DrawDefaultInspector();
            serializedObject.Update();

            if (!input) input = (VIVEInput)target;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            input.capture = GUILayout.Toggle(input.capture && !input.isOrigin, "Capture", "Button");
            input.isOrigin = GUILayout.Toggle(input.isOrigin && !input.capture, "Origin", "Button");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            input.newDevice = (SteamVR_TrackedObject.EIndex)EditorGUILayout.EnumPopup("Device", input.device);
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            input.searching = GUILayout.Toggle(input.searching, "Search", "Button", GUILayout.Height(14f), GUILayout.Width(56f));
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            CheckSelectedDevice();

            if (!input.capture)
            {
                if (!input.isOrigin)
                    EditorGUILayout.HelpBox("Object can't be used as an origin and doesn't get captured as well!", MessageType.Warning);
                else
                    EditorGUILayout.HelpBox("Object can now be used as an origin for other VIVEInputs!", MessageType.Info);
            }
            else
            {
                input.origin = (Transform)EditorGUILayout.ObjectField(new GUIContent("Origin", "Position origin of this object"), input.origin, typeof(Transform), true);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                input.offsetSelection = (VIVEInput.OffsetSelection)EditorGUILayout.EnumPopup(new GUIContent("Apply Offset To", "Offset is used to change the initial position/rotation"), input.offsetSelection);
                input.positionOffset = EditorGUILayout.Vector3Field("Position Offset", input.positionOffset);
                input.rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", input.rotationOffset);
                if (GUILayout.Button("Get Current Offset"))
                    input.GetCurrentOffset();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (adapter.fileOutput)
                {
                    EditorGUI.BeginDisabledGroup(Application.isPlaying);
                    input.fileName = EditorGUILayout.TextField(new GUIContent("File Name", "Object name is used when file name is empty"), input.fileName);
                    EditorGUI.EndDisabledGroup();

                    //Serialized property this.recording updates input.recording as well!
                    EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                    if (!recording.boolValue)
                    {
                        if (GUILayout.Button(new GUIContent("Start Recording", recordStart), GUILayout.Height(30)))
                        {
                            recording.boolValue = true;

                            foreach (VIVEInput i in targets)
                                InitWriteFile(i);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("Stop Recording", recordStop), GUILayout.Height(30)))
                            recording.boolValue = false;
                    }
                    EditorGUI.EndDisabledGroup();
                }

                if (!input.CheckTrackerSignal() && !input.searching)
                    EditorGUILayout.HelpBox("Tracker signal lost, check result for errors!", MessageType.Error);
            }

            //Updates all serialized properties on selected objects (Only recording in this case)
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Searches for the first valid tracker device, which is not already assigned to another VIVEInput.
        /// </summary>
        private void SearchDevice()
        {
            if (input && input.searching && assignedDevices != null)
            {
                timer += Time.deltaTime;

                if (timer > TIMER_DELAY)
                {
                    timer -= timer;

                    if (input.tracker && input.device > SteamVR_TrackedObject.EIndex.Hmd)
                    {
                        //Only allows to use found device if tracker (NO Base Station, HMD, etc.) - Crashes Unity somehow (Prop_RenderModelName_String)
                        StringBuilder sb = new StringBuilder();
                        ETrackedPropertyError error = new ETrackedPropertyError();
                        OpenVR.System.GetStringTrackedDeviceProperty((uint)input.device, ETrackedDeviceProperty.Prop_SerialNumber_String, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
                        //Utilities.CustomLog(sb.ToString());

                        if (input.tracker.isValid && sb.ToString().Contains("LHR"))     //LHR = Tracker/Controller/HMD, LHB = Base Station
                        {
                            //Quickfix for always selecting the same
                            if (searchCounter >= 1 && !assignedDevices.Contains(input.device))
                            {
                                input.searching = false;
                                return;
                            }
                        }
                    }

                    input.device++;
                    searchCounter++;

                    //Restarts checking devices from "None" after the last one
                    if (input.device > SteamVR_TrackedObject.EIndex.Device15)
                    {
                        input.searching = false;
                        input.device = SteamVR_TrackedObject.EIndex.None;
                    }
                }
            }
            else
                searchCounter = 0;
        }

        /// <summary>
        /// Initializes all directories and files for recording VIVEInputs
        /// </summary>
        private void InitWriteFile(VIVEInput input, bool reInit = false)
        {
            string fileName = "data\\" + ((input.fileName == "") ? input.name : input.fileName);
            string directoryName = fileName + "History";

            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            if (File.Exists(fileName))
            {
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);

                File.Move(fileName, directoryName + "\\" + ((input.fileName == "") ? input.name : input.fileName) + "_" + Directory.GetFiles(directoryName).Length);
            }
            
            File.WriteAllText(fileName, "UpdateRate | " + adapter.updateRate + " FPS" + Environment.NewLine);
        }

        /// <summary>
        /// Checks if the selected device for the VIVEInput is not already assigned to another one.
        /// </summary>
        private void CheckSelectedDevice()
        {
            if (assignedDevices != null)
            {
                //Assignes the desired device which couldn't be used before (When selected device gets changed on the other object)
                if (input.assignedDevice != SteamVR_TrackedObject.EIndex.None)
                {
                    if (!assignedDevices.Contains(input.assignedDevice))
                    {
                        input.newDevice = input.assignedDevice;
                        input.assignedDevice = SteamVR_TrackedObject.EIndex.None;
                    }

                    EditorGUILayout.HelpBox(input.assignedDevice + " already assigned to another object!", MessageType.Warning);
                }

                //Handles the actual checking if the selected device can be used
                if (input.newDevice != SteamVR_TrackedObject.EIndex.None)
                {
                    if (input.newDevice != input.device)
                    {
                        if (!assignedDevices.Contains(input.newDevice))
                        {
                            assignedDevices.Remove(input.device);
                            assignedDevices.Add(input.newDevice);
                            input.assignedDevice = SteamVR_TrackedObject.EIndex.None;
                            input.device = input.newDevice;
                        }
                        else
                            input.assignedDevice = input.newDevice;
                    }
                }
                else
                {
                    assignedDevices.Remove(input.device);
                    if (input.device != SteamVR_TrackedObject.EIndex.None)
                        input.assignedDevice = SteamVR_TrackedObject.EIndex.None;
                    input.device = input.newDevice;
                }

                //Replaces Update from input
                if (input.tracker != null)
                    input.tracker.index = input.device;
            }
        }
    }
}