using System.Collections.Generic;

namespace SceneLoader {
public readonly struct SceneLoadRequest {
    public readonly string from;
    public readonly string to;

    public SceneLoadRequest(string from, string to) {
        this.from = from;
        this.to = to;
    }

    public override bool Equals(object obj) {
        return obj is SceneLoadRequest request &&
               from == request.from &&
               to == request.to;
    }

    public override int GetHashCode() {
        var hashCode = -1951484959;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(from);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(to);
        return hashCode;
    }

    public static bool operator ==(SceneLoadRequest first, SceneLoadRequest second) {
        return first.from == second.from && first.to == second.to;
    }

    public static bool operator !=(SceneLoadRequest first, SceneLoadRequest second) {
        return first.from != second.from || first.to != second.to;
    }

    public override string ToString() {
        return $"<SceneLoadRequest from:{from} to:{to}>";
    }
}
}