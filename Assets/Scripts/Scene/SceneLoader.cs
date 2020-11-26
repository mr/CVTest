using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Scene {
public class SceneLoader : MonoBehaviour {
    private IScene currentScene = new Scene_1_1();

    // Finished initial load and ready to start game.
    [HideInInspector]
    public bool ready;
    [HideInInspector]
    private bool loading;

    private Queue<SceneLoadRequest> pendingRequests = new Queue<SceneLoadRequest>();

    private void Start() {
        StartCoroutine(Startup());
    }

    private IEnumerator Startup() {
        yield return SceneUtils.UnloadExceptGeneralAsync();
        yield return SceneUtils.LoadSceneWithNeighborsAsync(currentScene);
        ready = true;
    }

    private IEnumerator LoadSceneAsync() {
        while (pendingRequests.Count > 0) {
            var request = pendingRequests.Dequeue();
            Debug.Log($"Request: {request}");
            // Must start at the current scene
            if (currentScene.name != request.from) {
                continue;
            }

            // Must be loading a neighbor
            var newScene = currentScene.GetNeighbors().Find(n => n.name == request.to);
            if (newScene == null) {
                continue;
            }

            // Unload scenes that are outside the buffer
            yield return SceneUtils.UnloadSceneNeighborsAsync(currentScene, newScene);

            // Load all new neighbors that aren't loaded
            yield return SceneUtils.LoadNeighborsAsync(newScene);

            currentScene = newScene;
        }

        // These can be reset at the lowest point of the recursive call (or anywhere above) since there is no more
        // loading.
        loading = false;
        Debug.Log("Done loading.");
        yield return null;
    }

    public void QueueLoad(SceneLoadRequest request) {
        pendingRequests.Enqueue(request);
        if (!loading) {
            Debug.Log("Starting scene load.");
            StartCoroutine(LoadSceneAsync());
        }
    }
}
}
