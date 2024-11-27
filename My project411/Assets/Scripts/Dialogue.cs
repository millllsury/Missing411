using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Choice
{
    public string text;
    public List<Action> actions; // ��������, ����������� ��� ������

}

[System.Serializable]
public class Action
{
    public string key;
    public bool value;
}


[System.Serializable]
public class Dialogue
{
    public int id;  // ������� ��� ��������� ����� ��� ���������� ������������
    public string speaker;
    public string character;
    public int place;
    public bool isNarration;
    
    public List<string> texts;
    public List<Choice> choices;

    public string emotion;
    public string animation;
   
    public string background;
    public bool smoothBgReplacement = false;
    public string backgroundAnimation;

    public float frameDelay;
    public int repeatCount;
    public bool? keepLastFrame; // Nullable bool
    public bool stopBackgroundAnimation = false;

    public List<Condition> conditions; // ������� ��� ������ �������

    public Vector2 startPosition; // ������� ���������
    public Vector2 endPosition;   // ������� �����������
    public bool hideAvatar = false;
}

[System.Serializable]
public class Condition
{
    public string key;
    public bool value;
}

[System.Serializable]
public class SceneData
{
    public int sceneId;
    public string background;  // ��� ��� �� ��������� ��� �����
    public List<Dialogue> dialogues;
    public string backgroundAnimation;
    public int frameDelay;
}


[System.Serializable]
public class VisualNovelData
{
    public List<Episode> episodes;  // ������ ��������
}

[System.Serializable]
public class Episode
{
    public int episodeId;
    public string episodeName;
    public string backgroundImage;
    public List<SceneData> scenes;
}

