using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Search;
using System.Diagnostics;

[CustomEditor(typeof(TreeGeneration))]
public class TreeGenerationEditor : Editor
{
    public VisualTreeAsset visualTreeAsset;
    public TreeGeneration t;

    //// Local Bools ////
    bool _Misc = false;
    bool _Trees = false;
    bool _Rocks = false;
    bool _Exclusion = false;

    //// Buttons ////
    Button _MiscButton;
    Button _TreesButton;
    Button _RocksButton;
    Button _ExclusionButton;
    Button _TreePerlinButton;
    Button _RockPerlinButton;

    //// Panes ////
    VisualElement _MiscPane;
    VisualElement _TreesPane;
    VisualElement _RocksPane;
    VisualElement _ExclusionPane;

    //// PropertyFields ////
    PropertyField _OverrideSeed;
    PropertyField _OverrideSeedBoolean;

    // Cameron | old code, new editor in the works
    /*public override void OnInspectorGUI()
    {
        TreeGeneration tg = (TreeGeneration)target;

        if(GUILayout.Button("Generate Perlin"))
        {
            tg.DrawPerlinEditor();
        }
        if(GUILayout.Button("Clear Editor Memory"))
        {
            tg.ClearEditorOnlyVariables();
        }
    }*/

    private void OnEnable()
    {
        t = (TreeGeneration)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement visualElement = new VisualElement();

        visualTreeAsset.CloneTree(visualElement);

        _MiscButton = visualElement.Q<Button>("MiscButton");
        _TreesButton = visualElement.Q<Button>("TreesButton");
        _RocksButton = visualElement.Q<Button>("RocksButton");
        _ExclusionButton = visualElement.Q<Button>("ExclusionButton");
        _TreePerlinButton = visualElement.Q<Button>("TreePerlinButton");
        _RockPerlinButton = visualElement.Q<Button>("RockPerlinButton");

        _MiscPane = visualElement.Q<VisualElement>("MiscPane");
        _TreesPane = visualElement.Q<VisualElement>("TreesPane");
        _RocksPane = visualElement.Q<VisualElement>("RocksPane");
        _ExclusionPane = visualElement.Q<VisualElement>("ExclusionPane");

        _OverrideSeed = visualElement.Q<PropertyField>("OverrideSeed");
        _OverrideSeedBoolean = visualElement.Q<PropertyField>("OverrideSeedBoolean");

        _OverrideSeedBoolean.RegisterCallback<ClickEvent>(HandleOverrideSeedBoolean);
        _MiscButton.RegisterCallback<ClickEvent>(HandleMiscButton);
        _TreesButton.RegisterCallback<ClickEvent>(HandleTreesButton);
        _RocksButton.RegisterCallback<ClickEvent>(HandleRocksButton);
        _ExclusionButton.RegisterCallback<ClickEvent>(HandleExclusionButton);
        _TreePerlinButton.RegisterCallback<ClickEvent>(GenerateTreePerlin);
        _RockPerlinButton.RegisterCallback<ClickEvent>(GenerateRockPerlin);

        CheckForDisplaySettings();
        return visualElement;
    }

    public void HandleMiscButton(ClickEvent evt)
    {
        _Misc = !_Misc;
        CheckForDisplaySettings();
    }
    void HandleTreesButton(ClickEvent evt) 
    {
        _Trees = !_Trees;
        CheckForDisplaySettings();
    }
    void HandleRocksButton(ClickEvent evt)
    {  
        _Rocks = !_Rocks;
        CheckForDisplaySettings();
    }
    void HandleExclusionButton(ClickEvent evt) 
    {
        _Exclusion = !_Exclusion;
        CheckForDisplaySettings();
    }
    void HandleOverrideSeedBoolean(ClickEvent evt)
    {
        CheckForDisplaySettings();
    }

    void GenerateTreePerlin(ClickEvent evt)
    {
        t.DrawPerlinEditor(0);
    }
    void GenerateRockPerlin(ClickEvent evt)
    {
        t.DrawPerlinEditor(1);
    }

    public void CheckForDisplaySettings()
    {
        if (_Misc)
        {
            _MiscButton.style.opacity = 1.0f;
            _MiscPane.style.display = DisplayStyle.Flex;
            
        }
        else
        {
            _MiscButton.style.opacity = 0.5f;
            _MiscPane.style.display= DisplayStyle.None;
        }

        if (_Trees)
        {
            _TreesButton.style.opacity = 1.0f;
            _TreesPane.style.display = DisplayStyle.Flex;

        }
        else
        {
            _TreesButton.style.opacity = 0.5f;
            _TreesPane.style.display = DisplayStyle.None;
        }

        if (_Rocks)
        {
            _RocksButton.style.opacity = 1.0f;
            _RocksPane.style.display = DisplayStyle.Flex;

        }
        else
        {
            _RocksButton.style.opacity = 0.5f;
            _RocksPane.style.display = DisplayStyle.None;
        }

        if (_Exclusion)
        {
            _ExclusionButton.style.opacity = 1.0f;
            _ExclusionPane.style.display = DisplayStyle.Flex;

        }
        else
        {
            _ExclusionButton.style.opacity = 0.5f;
            _ExclusionPane.style.display = DisplayStyle.None;
        }

        if (!t._OverrideRandomSeed)
        {
            _OverrideSeed.style.display = DisplayStyle.None;
        }
        else
        {
            _OverrideSeed.style.display= DisplayStyle.Flex;
        }
    }
}
