using System.Collections.Generic;

class Scene_1_1 : IScene {
    public const string Name = "1-1";
    public string name => Name;

    public List<IScene> GetNeighbors() => new List<IScene> { new Scene_1_2() };
}