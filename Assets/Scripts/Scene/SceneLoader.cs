using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneLoader : MonoBehaviour {
    private IScene currentScene = new Scene_1_1();

    // Finished initial load and ready to start game.
    [HideInInspector]
    public bool ready = false;
    [HideInInspector]
    public bool loading = false;

    private SceneLoadRequest? currentRequest = null;
    private SceneLoadRequest? pendingRequest = null;

    void Start() {
        StartCoroutine(Startup());
    }

    void Update() {
        
    }

    private IEnumerator Startup() {
        yield return SceneUtils.UnloadExceptGeneralAsync();
        yield return SceneUtils.LoadSceneAsync(currentScene);
        ready = true;
    }

    private IEnumerator LoadSceneAsync(SceneLoadRequest request) {
        // Must start at the current scene
        if (currentScene.name != request.from) {
            yield return null;
        }

        // Must be loading a neighbor
        var newScene = currentScene.GetNeighbors().Find(n => n.name == request.to);
        if (newScene == null) {
            yield return null;
        }

        // Unload scenes that are outside the buffer
        yield return SceneUtils.UnloadSceneNeighborsAsync(currentScene, newScene);

        // Load all new neighbors that aren't loaded
        yield return SceneUtils.LoadNeighborsAsync(newScene);

        // If another request was queued (already checked to not be a duplicate) start it now
        if (pendingRequest != null) {
            // Remove old pending request in case we get another (unlikely)
            var newRequest = pendingRequest.Value;
            pendingRequest = null;
            // Start loading that new scene
            yield return LoadSceneAsync(newRequest);
        } else {
            // Base case, we don't have to do any more loading. The new scene is now current.
            currentScene = newScene;
        }

        // These can be reset at the lowest point of the recursive call (or anywhere above) since there is no more
        // loading.
        currentRequest = null;
        loading = false;
    }

    public void QueueLoad(SceneLoadRequest request) {
        if (loading) {
            if (currentRequest != request) {
                pendingRequest = request;
            }
            return;
        }
        currentRequest = request;
        StartCoroutine(LoadSceneAsync(request));
    }
}
