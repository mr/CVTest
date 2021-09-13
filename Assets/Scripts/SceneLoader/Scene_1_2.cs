using System.Collections.Generic;

namespace SceneLoader {
class Scene_1_2 : IScene {
    public const string Name = "1-2";
    public string name => Name;

    public List<IScene> GetNeighbors() => new List<IScene> { new Scene_1_1(), new Scene_1_3() };
}
}