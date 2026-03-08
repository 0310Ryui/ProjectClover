using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

public class EnsureMainCamera {
    [MenuItem("ProjectClover/Ensure Main Camera")]
    public static void DoSetup() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        
        var cameraGo = GameObject.Find("Main Camera");
        if (cameraGo == null) {
            cameraGo = new GameObject("Main Camera");
            var cam = cameraGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            
            // Set 2D / UI friendly settings
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.95f, 0.9f, 0.85f, 1f); // Cozy color
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cameraGo.transform.position = new Vector3(0, 0, -10);

            // Universal Additional Camera Data for URP
            var urpCamData = cameraGo.GetComponent<UniversalAdditionalCameraData>();
            if (urpCamData == null) {
                cameraGo.AddComponent<UniversalAdditionalCameraData>();
            }

            // Also ensure AudioListener exists
            if (cameraGo.GetComponent<AudioListener>() == null) {
                cameraGo.AddComponent<AudioListener>();
            }

            Debug.Log("Main Camera restored properly!");
            EditorSceneManager.SaveScene(scene);
        } else {
            Debug.Log("Main Camera already exists.");
        }
    }
}
