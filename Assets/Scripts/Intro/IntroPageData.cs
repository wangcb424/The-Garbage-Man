using UnityEngine;

[CreateAssetMenu(fileName = "NewIntroPage", menuName = "Game/Intro Page Data")]
public class IntroPageData : ScriptableObject
{
    [Header("Page Text")]
    public string pageTitle;

    [TextArea(4, 10)]
    public string pageBody;

    [Header("Images")]
    public Sprite backgroundImage;
    public Sprite storyImage;
}