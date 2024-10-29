using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BadTemper.AICreator
{
    public static class CurrentTheme
    {
        public static string name;

        public static Color color0;
        public static Color color1;
        public static Color color2;
        public static Color color3;
        public static Color color4;
        public static Color color5;
        public static Color color6;
        public static Color color7;
        public static Color color8;
        public static Color color9;

        public static bool themeHasBeenSet;

        public static void ThemeToCurrentTheme(Theme theme)
        {
            name = theme.name;

            color0 = theme.color0;
            color1 = theme.color1;
            color2 = theme.color2;
            color3 = theme.color3;
            color4 = theme.color4;
            color5 = theme.color5;
            color6 = theme.color6;
            color7 = theme.color7;
            color8 = theme.color8;
            color9 = theme.color9;
        }

        public static Color GetColor(byte colorID)
        {
            switch (colorID)
            {
                case 0:
                    return color0;
                case 1: 
                    return color1;
                case 2: 
                    return color2;
                case 3: 
                    return color3;
                case 4:
                    return color4;
                case 5: 
                    return color5;
                case 6:
                    return color6;
                case 7:
                    return color7;
                case 8: 
                    return color8;
                case 9: 
                    return color9;
                default:
                    return new Color(1,0.412f,0.706f);
            }
        }
    }

    [Serializable]
    public struct Theme
    {
        public string name;
        [Space(7)]
        public byte ID;
        [Space(7)]
        public Color color0;
        public Color color1;
        public Color color2;
        public Color color3;
        public Color color4;
        public Color color5;
        public Color color6;
        public Color color7;
        public Color color8;
        public Color color9;
    }

    public class Themizer : MonoBehaviour
    {
        public byte imageColor;
        public byte textColor;

        private UnityEngine.UI.Image image;
        private RawImage rawImage;
        private Camera mainCamera;
        private TextMeshPro textMeshPro;
        private TextMeshProUGUI textMeshProUGUI;
        private Text text;
        //private TMP_Dropdown dropdown;

        private Color _color;
        private Color _color2;

        private void OnValidate()
        {
            UpdateColor();
        }

        private void Start()
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (imageColor > 9)
            {
                imageColor = 9;
                Debug.LogWarning("Themes only have 10 colors! (starts counting from zero)");
            }
            if (textColor > 9)
            {
                imageColor = 9;
                Debug.LogWarning("Themes only have 10 colors! (starts counting from zero)");
            }

            image = GetComponent<UnityEngine.UI.Image>();
            rawImage = GetComponent<RawImage>();
            mainCamera = GetComponent<Camera>();
            textMeshPro = GetComponent<TextMeshPro>();
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            text = GetComponent<Text>();
            //dropdown = GetComponent<TMP_Dropdown>();

            _color = CurrentTheme.GetColor(imageColor);
            _color2 = CurrentTheme.GetColor(textColor);

            if (rawImage != null)
            {
                rawImage.color = _color;
            }
            if (image != null)
            {
                image.color = _color;
            }
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = _color;
            }
            if (textMeshPro != null)
            {
                textMeshPro.color = _color2;
            }
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.color = _color2;
            }
            if (text != null)
            {
                text.color = _color2;
            }
            /*if (dropdown != null)
            {
                dropdown.colors.normalColor = _color2;
            }*/
        }
    }
}

