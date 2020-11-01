using System.Collections;
using System.Linq;
using Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils {
public static class SceneUtils {
    private const string General = "General";
    private static void log(string message) => Debug.Log($"SceneUtils {message}");

    public static IEnumerator UnloadExceptGeneralAsync() {
        var scenes =
            from i in Enumerable.Range(0, SceneManager.sceneCount)
            select SceneManager.GetSceneAt(i);
        // call ToList to make sure we get all the scenes before doing any unloading
        foreach (var scene in scenes.ToList()) {
            if (scene.isLoaded && scene.name != General) {
                yield return UnloadSceneAsync(scene.name);
            }
        }
    }

    public static IEnumerator LoadSceneWithNeighborsAsync(IScene scene) {
        yield return LoadSceneAsync(scene.name);
        yield return LoadNeighborsAsync(scene);
    }

    public static IEnumerator LoadNeighborsAsync(IScene scene) {
        foreach (var neighbor in scene.GetNeighbors()) {
            if (!SceneManager.GetSceneByName(neighbor.name).isLoaded) {
                yield return LoadSceneAsync(neighbor.name);
            }
        }
    }

    public static IEnumerator UnloadSceneNeighborsAsync(IScene currentScene, IScene newScene) {
        // Get all scenes adjacent to our destination
        foreach (var currentNeighbor in currentScene.GetNeighbors()) {
            var shouldUnload =
                // Don't unload the scene we are about to move into!
                currentNeighbor.name != newScene.name
                // Find all scenes that are not adjacent to the new scene
                && newScene.GetNeighbors().All(newNeighbor => newNeighbor.name != currentNeighbor.name)
                // Make sure the scene is actually loaded before we try to unload
                && SceneManager.GetSceneByName(currentNeighbor.name).isLoaded;
            if (shouldUnload) {
                yield return UnloadSceneAsync(currentNeighbor.name);
            }
        }
    }

    private static AsyncOperation LoadSceneAsync(string name) {
        const LoadSceneMode mode = LoadSceneMode.Additive;
        log($"Load: {name}, {mode}");
        return SceneManager.LoadSceneAsync(name, mode);
    }

    private static AsyncOperation UnloadSceneAsync(string name) {
        log($"Unload: {name}");
        return SceneManager.UnloadSceneAsync(name);
    }

}
}
