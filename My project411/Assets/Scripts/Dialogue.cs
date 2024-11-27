using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Choice
{
    public string text;
    public List<Action> actions; // Действия, выполняемые при выборе

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
    public int id;  // Сделаем это публичным полем для корректной сериализации
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

    public List<Condition> conditions; // Условия для показа диалога

    public Vector2 startPosition; // Позиция появления
    public Vector2 endPosition;   // Позиция перемещения
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
    public string background;  // Это фон по умолчанию для сцены
    public List<Dialogue> dialogues;
    public string backgroundAnimation;
    public int frameDelay;
}


[System.Serializable]
public class VisualNovelData
{
    public List<Episode> episodes;  // Список эпизодов
}

[System.Serializable]
public class Episode
{
    public int episodeId;
    public string episodeName;
    public string backgroundImage;
    public List<SceneData> scenes;
}

