using System.Collections.Generic;

public interface IScene {
    string name { get; }
    List<IScene> GetNeighbors();
}