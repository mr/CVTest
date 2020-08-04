using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public static class SceneUtils {
    public const string General = "General";

    public static IEnumerator UnloadExceptGeneralAsync() {
        var tasks = new List<AsyncOperation>();
        for (var i = 0; i < SceneManager.sceneCount; i++) {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != General) {
                yield return SceneManager.UnloadSceneAsync(scene.buildIndex);
                i = 0;
            }
        }
    }

    public static IEnumerator LoadSceneAsync(IScene scene) {
        yield return SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
        yield return LoadNeighborsAsync(scene);
    }

    public static IEnumerator LoadNeighborsAsync(IScene scene) {
        foreach (var neighbor in scene.GetNeighbors()) {
            if (!SceneManager.GetSceneByName(neighbor.name).isLoaded) {
                yield return SceneManager.LoadSceneAsync(neighbor.name, LoadSceneMode.Additive);
            }
        }
    }

    public static IEnumerator UnloadSceneNeighborsAsync(IScene currentScene, IScene newScene) {
        var newNeighbors = newScene.GetNeighbors();
        var neighborsToUnload =
            currentScene.GetNeighbors()
                .Where(currentNeighbor =>
                    !newNeighbors.Any(newNeighbor => newNeighbor.name == currentNeighbor.name));
        foreach (var neighbor in neighborsToUnload) {
            yield return SceneManager.UnloadSceneAsync(neighbor.name);
        }
    }
}
