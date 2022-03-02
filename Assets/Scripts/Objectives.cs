using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objectives : MonoBehaviour
{
    public float objectivesComplete;
    public static Objectives Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        objectivesComplete = 0;
    }

    void Update()
    {
        if(objectivesComplete >= 4)
        {
            onFinalGenCompleted();
            print(objectivesComplete);
        }
    }

    void onFinalGenCompleted()
    {
        print("End Game started");
    }
}
