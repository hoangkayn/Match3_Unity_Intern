using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    public int BoardSizeX = 4;

    public int BoardSizeY = 6;
    
    public int BottomRowSize = 5;


    public int MatchesMin = 3;


    public float LevelTime = 30f;

    public float TimeForHint = 5f;
}
