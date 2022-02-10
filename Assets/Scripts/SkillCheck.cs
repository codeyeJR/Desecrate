using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using Random = UnityEngine.Random;

// Pos X = -390 < x < 390
// Pos Y = -160 < x < 160

// Loading Bar info
// Full Bar:
// PosX = 435
// ScaleX = 9
// Empty Bar:
// PosX = 0
// ScaleX = 0

public class SkillCheck : MonoBehaviour
{

    // Setting up
    [SerializeField] GameObject check;
    [SerializeField] Transform skillCheck;
    [SerializeField] Transform skillCheckArea;
    [SerializeField] TMP_Text timeElapsed;
    [SerializeField] GameObject timeElapsedObject;
    [SerializeField] float timeForGen = 800;
    private bool killedSkillCheck;
    private float timer;
    private float generatorTimer;
    private float skillCheckTime;
    private Vector2 position;
    //private Vector2 positionOfBar = 435;
    //private Vector2 sizeOfBar = 9;

    public static SkillCheck Instance;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        skillCheckTime = Random.Range(3,8);
        timer = 0;
        position = new Vector2(Random.Range(0,390), Random.Range(0,160));
    }

    private void Update()
    {
        if(Generator.generatorInstance.interacting)
        {
            // Initiating the SkillCheck
            OverlayManager.Instance.OpenOverlay("skillCheck");
            // starts the skill check
            if(timer >= skillCheckTime)
            {
                Instantiate(check, position, Quaternion.identity, skillCheckArea);
                print("skillCheckTriggered");
                timer = 0;
                position = new Vector2(Random.Range(0,780), Random.Range(0,320));
                skillCheckTime = Random.Range(3,8);
                Generator.generatorInstance.interacting = true;
                StartCoroutine(SkillChecks());
            }
            timer += Time.deltaTime;

            //Timer
            if(generatorTimer <= timeForGen)
            {
                generatorTimer += Time.deltaTime;
                timeElapsed.text = generatorTimer.ToString();
            }

        }
    }

    // Is Called by SkillCheckComponent.cs
    public void KillSkillCheckFail()
    {
        killedSkillCheck = true;
    }

    // Coroutine for the skillchecks to process whether the skillcheck is passed or failed
    private IEnumerator SkillChecks()
    {
        yield return new WaitForSeconds(3);
        if(!killedSkillCheck)
        {
            // The resuls of a failed skill check
            print("Skill Check Failed");
        }
        // Destorys each instance of skill checks three seconds after one was created
        // Ensures that in event of a failure all skillchecks eventually get destroyed
        foreach (Transform child in skillCheckArea.transform) 
        {
            if(child != timeElapsedObject)
                GameObject.Destroy(child.gameObject);
        }
        // Resetting the skill check
        killedSkillCheck = false;

    }
}