using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxerAI : MonoBehaviour {

    public int health = 1000;
    public float reactionTime = 0.1f;
    public float reactionDist = 2;
    public float idleTime = 5;
    public float punchTime = 0.5f;
    public float punchChance = 0.5f;
    public float handChance = 0.5f;

    private float localTimer = 0;
    private float punchTimer = 0;
    private bool animPlaying = false;
    private Coroutine randPunchCoroutine = null;

    private Vector3 oldLeftPos;
    private Vector3 oldRightPos;

	// Use this for initialization
	void Start () {
        HandsManager player = GameObject.FindGameObjectWithTag("Player").GetComponent<HandsManager>();
        oldLeftPos = player.leftHand.transform.position;
        oldRightPos = player.rightHand.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        localTimer += Time.deltaTime;

        HandsManager player = GameObject.FindGameObjectWithTag("Player").GetComponent<HandsManager>();

        Vector3 leftPos = Vector3.zero;
        Vector3 rightPos = Vector3.zero;

        if(player.leftHand.activeSelf)
            leftPos = player.leftHand.transform.position;
        if(player.rightHand.activeSelf)
            rightPos = player.rightHand.transform.position;

        if (player.leftHand.activeSelf && Vector3.Distance(transform.position, leftPos) < reactionDist)
        {
            Vector3 leftVel = (leftPos - oldLeftPos)/Time.deltaTime;
            //Debug.Log(leftVel);
            if (Vector3.Dot(leftVel, transform.forward) < 0 && !animPlaying)
            {
                StartCoroutine(PlayAnimation("leftDodge"));
                ResetTimer();
            }
        }

        if (player.rightHand.activeSelf && Vector3.Distance(transform.position, rightPos) < reactionDist && !animPlaying)
        {
            Vector3 rightVel = (rightPos - oldRightPos)/Time.deltaTime;
            //Debug.Log(rightVel);
            if(Vector3.Dot(rightVel, transform.forward) < 0)
            {
                StartCoroutine(PlayAnimation("rightDodge"));
                ResetTimer();
            }
        }

        Vector3 playerPos = Camera.main.transform.position - new Vector3(0, 0.2f, 0);

        if(Vector3.Distance(transform.position, playerPos) < reactionDist && localTimer > idleTime && randPunchCoroutine == null)
        {
            randPunchCoroutine = StartCoroutine(RandomPunch());
        }

        oldLeftPos = leftPos;
        oldRightPos = rightPos;
    }

    IEnumerator RandomPunch()
    {
        while (true)
        {
            if(Random.value < punchChance && !animPlaying)
            {
                if(Random.value > handChance)
                {
                    StartCoroutine(PlayAnimation("rightPunch"));
                }
                else
                {
                    StartCoroutine(PlayAnimation("leftPunch"));
                }
            }
            yield return new WaitForSeconds(punchTime);
        }
    }

    IEnumerator PlayAnimation(string name)
    {
        localTimer = 0;
        animPlaying = true;

        yield return new WaitForSeconds(reactionTime);

        Animator anim = GetComponent<Animator>();
        anim.SetTrigger(name);

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        animPlaying = false;
    }

    private void ResetTimer()
    {
        localTimer = 0;
        if (randPunchCoroutine != null)
        {
            StopCoroutine(randPunchCoroutine);
            randPunchCoroutine = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.tag == "Player")
        {
            health -= 100 + (int)(Random.value * 100);

            if(health < 0)
            {
                //Play death animation, game over screen, etc, etc
            }
        }
    }
}
