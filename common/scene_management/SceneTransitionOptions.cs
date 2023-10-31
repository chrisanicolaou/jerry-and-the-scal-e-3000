using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ChiciStudios.GithubGameJam2023.Common.SceneManagement
{
    public class SceneTransitionOptions
    {
        public SceneTransitionType Transition { get; set; } = SceneTransitionType.Fade;
        public SceneTransitionDirection Direction { get; set; } = SceneTransitionDirection.In;
        public Texture TransitionTexture { get; set; }
        public float Duration { get; set; } = 1f;
        public Tween.TransitionType Trans { get; set; } = Tween.TransitionType.Linear;
        public Tween.EaseType Ease { get; set; } = Tween.EaseType.InOut;
        public Godot.Collections.Dictionary<string, object> ShaderParameters { get; set; }
    }
}