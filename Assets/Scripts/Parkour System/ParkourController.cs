using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;

    bool inAction;
    public bool IsManualController = true;

    EnvironmentScanner environmentScanner;
    Animator animator;
    PlayerController playerController;
    NavMeshAgent agent;
    //bool slideNxt = false;
    //CharacterController characterController;
    //float jumpForce = 10;
    //float gravity = -9.81f;
    //float velocity = 0;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        agent = GetComponent<NavMeshAgent>();
        //characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if ((Input.GetButton("Jump") && !inAction) || (IsManualController && !inAction))
        {
            //Debug.Log("in action");
            var hitData = environmentScanner.ObstacleCheck();
            if (hitData.forwardHitFound)
            {
                if (IsManualController)
                    agent.autoTraverseOffMeshLink = false;
                foreach (var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        //if (action.AnimName == "RollingJump")
                        //{ 
                        //    slideNxt = true;
                        //    Debug.Log("slideNxt fixed");
                        //}
                        //Debug.Log(action.AnimName);
                        StartCoroutine(DoParkourAction(action));
                        if (IsManualController)
                            agent.autoTraverseOffMeshLink = true;
                        break;
                    }
                }
            }
            //else if (slideNxt)
            //{
            //    Debug.Log("Animation played");
            //    animator.CrossFade("RunningSlide", 0.2f);
            //    slideNxt = false;
            //}
            else
            {
                //groundCheck();
                //velocity += gravity * Time.deltaTime;
                //velocity = jumpForce;
                //characterController.Move(new Vector3(0, velocity, 0) * Time.deltaTime);
                //StartCoroutine(DoParkourAction(ParkourAction Jump));
                //Debug.Log("TabJump");
                //Debug.Log("Movement: " + playerController.MoveAmount);
                if (playerController.MoveAmount > 0)
                {
                    animator.CrossFade("RunJump", 0.2f);
                    //animator.SetBool("runJump", true);
                }
                else
                {
                    animator.CrossFade("IdleJump", 0.2f);
                    //animator.SetBool("runJump", true);
                }

            }
        }
        else
        {
            //Debug.Log("No action taken");
        }
    }


    IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;
        if (!IsManualController)
            playerController.SetControl(false);

        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
            Debug.LogError("The parkour animation is wrong!");

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;

            if (action.RotateToObstacle && !IsManualController)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.RotationSpeed * Time.deltaTime);

            if (action.EnableTargetMatching)
                MatchTarget(action);

            if (animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }

        yield return new WaitForSeconds(action.PostActionDelay);

        if (!IsManualController)
            playerController.SetControl(true);
        inAction = false;
    }

    void MatchTarget(ParkourAction action)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0), 
            action.MatchStartTime, action.MatchTargetTime);
    }
}
