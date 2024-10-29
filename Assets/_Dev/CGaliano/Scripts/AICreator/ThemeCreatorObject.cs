using BadTemper.AICreator;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BadTemper.AICreator
{
    public class ThemeCreatorObject : MonoBehaviour
    {
        [SerializeField] List<Theme> themeList = new List<Theme>();
        [SerializeField] byte currentTheme;

        private void OnValidate()
        {
            if (themeList.Count > 255)
            {
                Debug.LogError("Theme system only supports 255 themes, please remove one to avoid errors!");
            }

            //if (!CurrentTheme.themeHasBeenSet)
            //{
            //    ChangeCurrentTheme(0);
            //}

            ChangeCurrentTheme(currentTheme);
        }

        private void Start()
        {
            //if (!CurrentTheme.themeHasBeenSet)
            //{
            //    ChangeCurrentTheme(0);
            //}
            ChangeCurrentTheme(currentTheme);
        }

        public void ChangeCurrentTheme(byte themeID)
        {
            foreach (Theme theme in themeList)
            {
                if (theme.ID == themeID)
                {
                    CurrentTheme.ThemeToCurrentTheme(theme);
                }
            }
        }
    }
}