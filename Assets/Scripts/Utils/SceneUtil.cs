using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public static class SceneUtils {
    public const string General = "General";

    public static IEnumerator UnloadExceptGeneralAsync() {
        for (var i = 0; i < SceneManager.sceneCount; i++) {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != General) {
                yield return SceneManager.UnloadSceneAsync(scene.buildIndex);
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
        // Get all scenes adjacent to our destination
        var newNeighbors = newScene.GetNeighbors();
        var neighborsToUnload =
            currentScene.GetNeighbors()
                .Where(currentNeighbor =>
                    // Don't unload the scene we are about to move into!
                    currentNeighbor.name != newScene.name
                    // Find all scenes that are not adjacent to the new scene
                    && !newNeighbors.Any(newNeighbor => newNeighbor.name == currentNeighbor.name));
        foreach (var neighbor in neighborsToUnload) {
            if (SceneManager.GetSceneByName(neighbor.name).isLoaded) {
                yield return SceneManager.UnloadSceneAsync(neighbor.name);
            }
        }
    }
}
