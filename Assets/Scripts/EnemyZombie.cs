using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class EnemyZombie : MonoBehaviour
{

    public enum AI { Idle, Wander, LookAt, GoTo }
    public AI behavior;
    private Animator anim;

    [Header("Idle")]
    public float idleMaxWait = 4f;

    [Header("Wander")]
    public float wanderRange = 15;
    public float wanderMinReachDistance = 1.4f;
    private NavMeshHit navMeshHit;
    private Vector3 wanderPosition = Vector3.zero;

    [Header("Attack")]
    public float aggroRange = 6.5f;    //To start going into Attack Behavior
    public float interestRange = 7f; //If outside this range long enough wolf loses interest
    public float aggresiveRange = 3f;  //This is a small range where the wolf is very encourged to attack the player
    private float interestTimer;

    [Header("Misc")]
    public float timerLeft; //Used for interest and idle
    public float maxSpeed = 15;

    public bool playerInSightBox = false;
    private Ray ray;
    private RaycastHit hit;


    private GameObject player;
    private NavMeshAgent nav;
    private float origSpeed;
    public float sprintSpeed = 8;

    public UnityEvent OnSighted;
    public UnityEvent OnLostSight;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        origSpeed = nav.speed;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(transform.position, player.transform.position), Color.red);

        switch (behavior)
        {
            case AI.Idle:
                Idle();
                break;
            case AI.Wander:
                Wander();
                break;
            case AI.LookAt:
                Attack();
                break;
            case AI.GoTo:
                break;
            default:
                break;
        }
    }

    public void Idle()
    {
        //Interest gauge when to reaggro
        if (interestTimer > 0f) { interestTimer -= Time.deltaTime; return; }

        //anim.SetFloat("Speed", 0f);

        wanderPosition = Vector3.zero;
        nav.destination = transform.position;
        nav.Stop();


        ////Immediately Attack
        //if (Vector3.Distance(transform.position, player.transform.position) <= aggroRange - 2f)
        //{
        //    GoAttack();
        //    return;
        //}

        if (CanSeePlayer())
        {
            GoAttack();

            return;
        }

        //Wait then go back to wander
        timerLeft -= Time.deltaTime;

        if (timerLeft <= 0f)
            behavior = AI.Wander;
    }

    public void SetPlayerInSightBox(bool value)
    {
        playerInSightBox = value;
    }

    private bool CanSeePlayer()
    {
        ray.origin = transform.position;
        ray.direction = player.transform.position - ray.origin;

        if (playerInSightBox == false) return false;





        if (Physics.Raycast(ray, out hit, Vector3.Distance(player.transform.position, ray.origin)))
        {

            if (hit.distance + 5 < Vector3.Distance(Camera.main.transform.position, ray.origin))
                return false;

            else
                return true;
        }

        return false;
    }

    public void Wander()
    {


        ////*HACKISH
        //if (player == null)
        //    player = GameManager.Instance.player;

        //Immediately Attack
        //if (Vector3.Distance(transform.position, player.transform.position) <= aggroRange)
        //{
        //    GoAttack();
        //    return;
        //}

        if (CanSeePlayer())
        {
            GoAttack();
            return;
        }

        //If Needs to get new Position
        if (wanderPosition == Vector3.zero)
        {
            //print("s123dfsdfsdfsd");

            Vector3 point;
            if (RandomPoint(transform.position, wanderRange, out point))
            {
                nav.Resume();
                //anim.SetFloat("Speed", 1f);
                wanderPosition = point;
                nav.destination = wanderPosition;
            }

            //Vector3 randomPoint = transform.position + Random.insideUnitSphere * 15f;// Random.Range(1f, wanderMaxRange);
            //if (NavMesh.SamplePosition(randomPoint, out navMeshHit, NavMesh.AllAreas, 1))
            //{
            //    print("dslfjsdlfjsdljf");
            //    nav.Resume();
            //    anim.SetFloat("Speed", 1f);
            //    wanderPosition = navMeshHit.position;
            //    nav.destination = wanderPosition;
            //    timerLeft = 0f;
            //}
        }

        //If made it to wander position go back idle
        if (Vector3.Distance(transform.position, nav.destination) <= wanderMinReachDistance)
        {
            //Go into Idle, assign random wait time
            timerLeft = Random.Range(0f, idleMaxWait);
            behavior = AI.Idle;
        }

    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    private void GoAttack()
    {
        nav.Resume();
        //interestTimer = Random.Range(3f, 10f);   //Min and Max Interest Timer Range
        //anim.SetFloat("Speed", 1f);
        //timerLeft = 0f;
        nav.speed = sprintSpeed;
        behavior = AI.LookAt;


        if (playerInSightBox == true)
            OnSighted.Invoke();
    }

    public void Attack()
    {
        //print(interestTimer);

        //Wait a bit after attacking
        //if (timerLeft > 0f) { timerLeft -= Time.deltaTime; nav.Resume(); return; }

        //anim.SetFloat("Speed", 1f);

        ////If outside attack range and is in interest range
        //if (Vector3.Distance(transform.position, player.transform.position) > aggresiveRange &&
        //    Vector3.Distance(transform.position, player.transform.position) <= interestRange)
        //{
        //    //Start losing interest
        //    //interestTimer -= Time.deltaTime;
        //    //print("RRRRRR");
        //}

       

        //Leave player alone if far away or loses interest
        if (Vector3.Distance(transform.position, nav.destination) <=  2f && CanSeePlayer() == false)
        {
            //print("DDDDDDDDDDD");
            OnLostSight.Invoke();
            timerLeft = Random.Range(1.3f, idleMaxWait);
            nav.destination = transform.position;
            nav.Stop();
            //anim.SetFloat("Speed", 0f);
            wanderPosition = Vector3.zero;
            behavior = AI.Idle;
            nav.speed = origSpeed;
            //interestTimer = Random.Range(1f, 3f);   //Min and Max Interest Timer Range
            return;
        }

        //Chase Player when in sight
        if (CanSeePlayer() )
            nav.destination = player.transform.position;

        transform.LookAt(player.transform);



    }


    public void Flee()
    {

    }
}
