using System.Collections.Generic;

namespace SceneLoader {
public interface IScene {
    string name { get; }
    List<IScene> GetNeighbors();
}
}