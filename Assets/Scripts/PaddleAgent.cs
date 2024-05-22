using System;
using UnityEngine;

public class PaddleAgent : MonoBehaviour // Agent // Need to import MLAgent plugin like in NeuralBird
{
    [NonSerialized] public int Points;

    public void AddPoint()
    {
        Points++;
    }
}
