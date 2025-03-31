using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    // Game State enum - used to track the current state of the game
    public enum GameState
    {
        Running = 0,
        Paused = 1,
        GameOver = 2,
        Win = 3,
        MainMenu = 4,
        Instructions = 5
    }
    
    // PowerUpType enum - defines the types of power-ups available in the game
    public enum PowerUpType
    {
        Invincibility,
        SpeedBoost,
        ExtraLife
    }
} 