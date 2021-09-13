using System.Collections.Generic;

namespace SceneLoader {
class Scene_1_4 : IScene {
    public const string Name = "1-4";
    public string name => Name;

    public List<IScene> GetNeighbors() => new List<IScene> { new Scene_1_3() };
}
}