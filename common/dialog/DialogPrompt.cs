using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ChiciStudios.GithubGameJam2023.Common.Dialog
{
    public class DialogPrompt : Resource
    {
        [Export] public string Title { get; set; }
        [Export] public Array<string> Texts { get; set; }
        public TextSpeed? TextSpeed { get; set; }
        [Export] public Texture Icon { get; set; }
        [Export] public Color FillColor { get; set; }
        [Export] public Color BorderColor { get; set; }
        
        // public DialogPrompt() { }
        //
        // public DialogPrompt(IEnumerable<string> texts, string title = "", Texture icon = null, Color fillColor = null, Color borderColor = null, TextSpeed? textSpeed = null)
        // {
        //     Title = title;
        //     Texts = new Array<string>(texts);
        //     Icon = icon;
        //     FillColor = fillColor;
        //     BorderColor = borderColor;
        //     TextSpeed = textSpeed;
        // }

        public override string ToString()
        {
            var str = $"DIALOG PROMPT:\nTitle: {Title}\nTexts: ";
            foreach (var text in Texts)
            {
                str += $"{text}\n";
            }

            return str;
        }
    }
}