using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Resemble_Window : EditorWindow
{
    public static Resemble_Window window;
    public static AudioPreview preview;
    public PostPod pod = new PostPod();
    private static PodText text = new PodText();
    private static string placeHolderText = "";
    private static PlaceHolderAPIBridge.ClipRequest request;

    [MenuItem("Window/Audio/Resemble")]
    static void Init()
    {
        window = (Resemble_Window) EditorWindow.GetWindow(typeof(Resemble_Window));
        window.titleContent = new GUIContent("Resemble");
        window.Show();
    }

    private void OnEnable()
    {
    }

    void OnGUI()
    {
        Styles.Load();

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        /*
        Rect rect = GUILayoutUtility.GetRect(Screen.width, 150);
        rect.Set(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
        GUIStyle style = new GUIStyle(EditorStyles.largeLabel);
        text.OnGUI(rect, style);*/

        pod.title = "Some text";
        pod.body = "Some text that will be transformed into audio.";
        pod.voice = "a22c5ba6";
        pod.emotion = "style1";

        if (GUILayout.Button("Generate preview"))
        {
            APIBridge.CreateClipSync(pod, GetClipCallback);
        }

        if (GUILayout.Button("Get all pods"))
        {
            APIBridge.GetAllPods(GetAllPodsCallback);
        }

        if (GUILayout.Button("Create project"))
        {
            Project project = new Project();
            project.name = "Watermelon";
            project.description = "Project generated from unity plugin";
            APIBridge.CreateProject(project, CreateProjectCallback);
        }

        if (preview != null && preview.clip != null && GUILayout.Button("Play clip"))
        {
            AudioPreview.PlayClip(preview.clip);
        }

        GUILayout.Space(50);
        placeHolderText = EditorGUILayout.TextField("PlaceHolderText", placeHolderText);
        if (GUILayout.Button(request == null ? "Request placeHolder api" : request.status.ToString()))
        {
            request = PlaceHolderAPIBridge.GetAudioClip(placeHolderText, GetClipCallback);
        }

    }

    private void GetClipCallback(AudioClip clip, Error error)
    {
        if (error)
            Debug.LogError("Error : " + error.code + " - " + error.message);
        else
        {
            AudioPreview.PlayClip(clip);
            request = null;
        }
    }

    private void GetClipCallback(AudioPreview preview, Error error)
    {
        if (error)
            Debug.LogError("Error : " + error.code + " - " + error.message);
        else
            Resemble_Window.preview = preview;
    }

    private void GetAllPodsCallback(ResemblePod[] pods, Error error)
    {
        if (error)
            Debug.LogError("Error : " + error.code + " - " + error.message);

        for (int i = 0; i < pods.Length; i++)
        {
            Debug.Log(pods[i]);
        }
    }

    private void CreateProjectCallback(ProjectStatus status, Error error)
    {
        if (error)
            Debug.LogError("Error : " + error.code + " - " + error.message);
        else
            Debug.Log(status);
    }
}
