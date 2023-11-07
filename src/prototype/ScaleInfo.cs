using Godot;

namespace GithubGameJam2023.prototype
{
    public class ScaleInfo
    {
        public Texture Tex { get; set; }
        public Vector2 Scale { get; set; }
        public Vector2 Offset { get; set; }
        public bool Carryable { get; set; }
        public Vector2 CarryOffset { get; set; }
    }
}