using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool lockCursor;

    public Transform playerTf;
    public ParticleSystem spawnParticle;

    private void Awake()
    {
        Instance = this;

        if(lockCursor)
            Cursor.lockState = CursorLockMode.Locked;
    }
}
