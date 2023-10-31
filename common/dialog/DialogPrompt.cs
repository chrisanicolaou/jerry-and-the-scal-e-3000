using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChiciStudios.GithubGameJam2023.Common.Dialog
{
    public class DialogPrompt
    {
        public string Title { get; set; }
        public string[] Texts { get; set; }
        public TextSpeed? TextSpeed { get; set; }
        public Texture Icon { get; set; }
        public Color? FillColor { get; set; }
        public Color? BorderColor { get; set; }

        public DialogPrompt(IEnumerable<string> texts, string title = "", Texture icon = null, Color? fillColor = null, Color? borderColor = null, TextSpeed? textSpeed = null)
        {
            Title = title;
            Texts = texts.ToArray();
            Icon = icon;
            FillColor = fillColor;
            BorderColor = borderColor;
            TextSpeed = textSpeed;
        }

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