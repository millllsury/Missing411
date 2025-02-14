using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Choice
{
    public string text;
    public List<Action> actions;
    public int cost; 
    public int reward;
    public string buttonID;  // Если кнопка есть в сцене
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
    public int sceneId;
    public int id;  
    public string speaker;
    public string character;
    public int place;
    public bool isNarration;
    
    public List<string> texts;
    public List<Choice> choices;

    public string emotion;
    public bool isAnimationPlayed = false;
    public string animation;
   
    public string background;
    public bool smoothBgReplacement = false;
    public string backgroundAnimation;

    public float frameDelay;
    public int repeatCount;
    public bool? keepLastFrame; // Nullable bool
    public bool stopBackgroundAnimation = false;

    public List<Condition> conditions; // Условия для показа диалога

    public string soundTrigger;

    public float? targetPosition;

    public bool screenRipple;

    public string feedback;

    public bool unlockNewItem;
    public int reward;

    public bool smoothDisappear;
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
    public List<Episode> episodes;

    public SceneData GetSceneById(int sceneId)
    {
        foreach (var episode in episodes)
        {
            foreach (var scene in episode.scenes)
            {
                if (scene.sceneId == sceneId)
                {
                    return scene;
                }
            }
        }
        Debug.LogError($"Сцена с ID {sceneId} не найдена!");
        return null;
    }
}

[System.Serializable]
public class Episode
{
    public int episodeId;
    public string episodeName;
    public string backgroundImage;
    public List<SceneData> scenes;
}


