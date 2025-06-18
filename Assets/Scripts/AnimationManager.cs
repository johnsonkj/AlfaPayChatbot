using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public AnimationClip flyingClip;
    public AnimationClip jumpClip;
    public GameObject walkRobot;
    public GameObject flyingRobot;
    public GameObject jumpRobot;
    public GameObject talkRobot;
    public GameObject peepRobot;
    public Animator talkController;

    public float idleTimeThreshold = 15f;

    private Coroutine flyingCoroutine;
    private float lastInputTime;
    private void Awake()
    {
        DisableRobots();
    }
    void Start()
    {
        lastInputTime = Time.time;
        flyingCoroutine = StartCoroutine(PlayFlyingSequence());
    }

    void Update()
    {
        // Check for touch or mouse click
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            lastInputTime = Time.time;
        }

        // If idle for too long
        if (Time.time - lastInputTime > idleTimeThreshold)
        {
            if (peepRobot.activeSelf == false)
            {
                EnablePeep();
            }   
        }
    }

    IEnumerator PlayFlyingSequence()
    {
        yield return new WaitForSeconds(1f);
        flyingRobot.SetActive(true);
        yield return new WaitForSeconds(flyingClip.length);
        flyingRobot.SetActive(false);
        walkRobot.SetActive(true);
    }

    public void EnableJumpSequence()
    {
        StopCoroutine(flyingCoroutine);
        DisableRobots();
        jumpRobot.SetActive(true);
    }

    public void EnableTalkSequence()
    {
        DisableRobots();
        talkRobot.SetActive(true);
        talkController.SetTrigger("Talk");
    }

    public void ResetTalk()
    {
        DisableRobots();
        talkRobot.SetActive(true);
        talkController.SetTrigger("Idle");
    }

    public void DisableRobots()
    {
        flyingRobot.SetActive(false);
        walkRobot.SetActive(false);
        talkRobot.SetActive(false);
        jumpRobot.SetActive(false);
        peepRobot.SetActive(false);
    }

    public void EnablePeep()
    {
        DisableRobots();
        peepRobot.SetActive(true);
    }
}
