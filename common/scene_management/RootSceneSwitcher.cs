using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChiciStudios.GithubGameJam2023.Common.SceneManagement
{
    public class RootSceneSwitcher : CanvasLayer
    {
        [Export] private NodePath _transitionSpritePath;
        [Export] private Texture _defaultTransitionTex;
        [Export] private Godot.Collections.Dictionary<string, ShaderMaterial> _transitionShaderMatInfo;

        private const string ShaderParamPrefix = "material:shader_param/";
        
        private Sprite _transitionSprite;
        private Dictionary<SceneTransitionType, ShaderMaterial> _shaderMap = new Dictionary<SceneTransitionType, ShaderMaterial>();
        private Dictionary<SceneTransitionType, Func<SceneTransitionOptions, Task>> _transitionTypeFunctionMap = new Dictionary<SceneTransitionType, Func<SceneTransitionOptions, Task>>();
        private SceneTransitionOptions _defaultInTransition;
        private SceneTransitionOptions _defaultOutTransition;
        private SceneTransitionType _previousSceneTransition = SceneTransitionType.Fade;

        public override void _Ready()
        {
            _transitionSprite = GetNode<Sprite>(_transitionSpritePath);
            CreateShaderMap();
            CreateTransitionTypeFunctionMap();
            _defaultInTransition = new SceneTransitionOptions { TransitionTexture = _defaultTransitionTex };
            _defaultOutTransition = new SceneTransitionOptions { TransitionTexture = _defaultTransitionTex, Direction = SceneTransitionDirection.Out };

            // Transition(new SceneTransitionOptions { Direction = SceneTransitionDirection.Out, Duration = 5 });
            // var tween = _tween.CreateTween();
            // tween.TweenProperty(_transitionSprite, "material:shader_param/fade_amount", 1, 5);
        }

        public async Task Transition(SceneTransitionDirection dir) => await Transition(dir == SceneTransitionDirection.In ? _defaultInTransition : _defaultOutTransition);

        public async Task Transition(SceneTransitionOptions opts)
        {
            if (opts.TransitionTexture == null) opts.TransitionTexture = _defaultTransitionTex;
            var transitionExists = _transitionTypeFunctionMap.TryGetValue(opts.Transition, out var transitionFunc);
            if (!transitionExists)
            {
                GD.PrintErr($"No transition function created for transition of type: {opts.Transition}.");
                return;
            }
            SetupTransitionTextureAndShader(opts);
            // I had to wait a frame after setting shader, now not sure if this is needed since we're setting material?
            // await ToSignal(GetTree(), "idle_frame");
            await transitionFunc(opts);
            _previousSceneTransition = opts.Transition;
        }

        private void SetupTransitionTextureAndShader(SceneTransitionOptions opts)
        {
            _transitionSprite.Texture = opts.TransitionTexture;
            _transitionSprite.Material = _shaderMap[opts.Transition];
            var shaderMat = _transitionSprite.Material as ShaderMaterial;
            if (!(opts.ShaderParameters?.Count > 0)) return;
            foreach (var param in opts.ShaderParameters)
            {
                shaderMat.SetShaderParam(param.Key, param.Value);
            }
        }

        private void CreateShaderMap()
        {
            foreach (var kvp in _transitionShaderMatInfo)
            {
                var typeFound = Enum.TryParse<SceneTransitionType>(kvp.Key, out var type);
                if (!typeFound)
                {
                    GD.PrintErr($"Cannot find {nameof(SceneTransitionType)} of type {kvp.Key}. Make sure a {nameof(SceneTransitionType)} has been created for this type.");
                    continue;
                }
                _shaderMap.Add(type, kvp.Value);
            }
        }

        private void CreateTransitionTypeFunctionMap()
        {
            _transitionTypeFunctionMap.Add(SceneTransitionType.Fade, HandleFade);
            _transitionTypeFunctionMap.Add(SceneTransitionType.Circle, HandleCircle);
            _transitionTypeFunctionMap.Add(SceneTransitionType.Diamond, HandleDiamond);
            _transitionTypeFunctionMap.Add(SceneTransitionType.Line, HandleLine);
            _transitionTypeFunctionMap.Add(SceneTransitionType.Pixel, HandlePixel);
            _transitionTypeFunctionMap.Add(SceneTransitionType.Saw, HandleSaw);
        }

        private async Task HandleFade(SceneTransitionOptions opts)
        {
            var (startVal, finalVal) = opts.Direction == SceneTransitionDirection.In ? (1f, 0f) : (0f, 1f);
            var tween = CreateShaderParamTween("fade_amount", finalVal, opts.Duration, startVal);
            await ToSignal(tween, "finished");
        }

        private async Task HandleCircle(SceneTransitionOptions opts)
        {
            var (startVal, finalVal) = opts.Direction == SceneTransitionDirection.In ? (0f, 1.05f) : (1.05f, 0f);
            var tween = CreateShaderParamTween("circle_size", finalVal, opts.Duration, startVal);
            await ToSignal(tween, "finished");
        }

        private async Task HandleDiamond(SceneTransitionOptions opts)
        {
            var (startVal, finalVal) = opts.Direction == SceneTransitionDirection.In ? (1f, 0f) : (0f, 1f);
            var tween = CreateShaderParamTween("progress", finalVal, opts.Duration, startVal);
            await ToSignal(tween, "finished");
        }

        private async Task HandleLine(SceneTransitionOptions opts)
        {
            var (startVal, finalVal) = opts.Direction == SceneTransitionDirection.In ? (1f, 0f) : (0f, 1f);
            var tween = CreateShaderParamTween("y_threshold", finalVal, opts.Duration, startVal);
            await ToSignal(tween, "finished");
        }

        private async Task HandlePixel(SceneTransitionOptions opts)
        {
            var (startVal, finalVal) = opts.Direction == SceneTransitionDirection.In ? (1.55f, 0f) : (0f, 1.55f);
            var tween = CreateShaderParamTween("time", finalVal, opts.Duration, startVal);
            await ToSignal(tween, "finished");
        }

        private async Task HandleSaw(SceneTransitionOptions opts)
        {
            SceneTreeTween tween;
            var shaderMat = _transitionSprite.Material as ShaderMaterial;
            if (opts.Direction == SceneTransitionDirection.In)
            {
                shaderMat.SetShaderParam("right", 3f);
                tween = CreateShaderParamTween("left", 3f, opts.Duration, -1f);
            }
            else
            {
                shaderMat.SetShaderParam("left", -1f);
                tween = CreateShaderParamTween("right", 3f, opts.Duration, -0.5f);
            }
            await ToSignal(tween, "finished");
        }

        private SceneTreeTween CreateShaderParamTween(string property, object finalVal, float duration, object startVal = default, SceneTreeTween tween = default, float delay = default, Tween.TransitionType trans = Tween.TransitionType.Linear, Tween.EaseType ease = Tween.EaseType.InOut)
        {
            tween = tween ?? GetTree().CreateTween();
            var propertyTween = tween.TweenProperty(_transitionSprite, $"material:shader_param/{property}", finalVal, duration).SetTrans(trans).SetEase(ease);
            if (startVal != null) propertyTween.From(startVal);
            if (delay > 0.001) propertyTween.SetDelay(delay);
            return tween;
        }
    }  
};
