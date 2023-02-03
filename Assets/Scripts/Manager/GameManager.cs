using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Transform bookAimingTransform;
    public Transform playerTransform;

    private void Awake()
    {
        Instance = this;
    }
}
