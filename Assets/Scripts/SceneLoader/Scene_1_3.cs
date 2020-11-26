using System.Collections.Generic;

namespace Scene {
class Scene_1_3 : IScene {
    public const string Name = "1-3";
    public string name => Name;

    public List<IScene> GetNeighbors() => new List<IScene> { new Scene_1_2() };
}
}