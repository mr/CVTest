using System.Collections.Generic;

namespace Scene {
public interface IScene {
    string name { get; }
    List<IScene> GetNeighbors();
}
}