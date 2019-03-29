using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public GameObject[] obstacles;
    public enum AI { Follow, Wait, Flee }
    public AI behavior = AI.Follow;
    public float timerLeft;
    public Renderer[] renderers;
    private Ray ray;
    private RaycastHit hit;
    public NavMeshAgent nav;
    private float origSpeed;
    public float sprintModifier = 2.5f;

    public bool invisable = false;

    private GameObject player;

    private void Start()
    {
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        //renderer = GetComponent<Renderer>();
        ray.origin = transform.position;
        nav = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        origSpeed = nav.speed;
    }

    // Update is called once per frame
    void Update()
    {
        switch (behavior)
        {
            case AI.Follow:
                Follow();
                break;
            case AI.Wait:
                break;
            case AI.Flee:
                Flee();
                break;
            default:
                break;
        }
    }

    public void Follow()
    {
        if (CanPlayerSeeMe(0))
        {
            //Hide behind wall!
            GameObject rdmOb = obstacles[Random.Range(0, obstacles.Length)];
            Vector3 sideOfWallAwayFromPlayer = (rdmOb.transform.position - player.transform.position).normalized;
            nav.destination = rdmOb.transform.position + sideOfWallAwayFromPlayer * 2f;

            nav.speed *= sprintModifier;
            timerLeft = Random.Range(2, 3f);
            behavior = AI.Flee;
            Debug.Log("VISABLE!");
        }

        else
        {
            nav.Resume();
            nav.destination = player.transform.position;
            Debug.Log("INVISABLE!");
        }
    }

    public void Flee()
    {
        timerLeft -= Time.deltaTime;

        if (!CanPlayerSeeMe(0) && !CanPlayerSeeMe(1) && !CanPlayerSeeMe(2) && !CanPlayerSeeMe(3) && !CanPlayerSeeMe(4) && renderers[0].enabled && invisable)
        {
            ToggleRenderers(false);
        }

        //if made it to destination
        if (Vector3.Distance(nav.destination, transform.position) < 3f && CanPlayerSeeMe(0) && renderers[0].enabled)
        {
            GameObject rdmOb = obstacles[Random.Range(0, obstacles.Length)];
            Vector3 sideOfWallAwayFromPlayer = (rdmOb.transform.position - player.transform.position).normalized;
            nav.destination = rdmOb.transform.position + sideOfWallAwayFromPlayer * 2f;
        }


        if (timerLeft <= 0)
        {
            nav.speed = origSpeed;

            //Re-enable renderers if player can't see monster if re-enabled
            if (!CanPlayerSeeMe(0) && !CanPlayerSeeMe(4))
            {
                ToggleRenderers(true);
            }

            if (renderers[0])
                behavior = AI.Follow;
        }
    }


    private bool CanPlayerSeeMe(int rendererID)
    {
        //Debug.Log("DADADA");

        //ray.origin = transform.position;

        ray.origin = renderers[rendererID].transform.position;
        ray.direction = Camera.main.transform.position - ray.origin;


        if (Physics.Raycast(ray, out hit, Vector3.Distance(Camera.main.transform.position, ray.origin)))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

            //Debug.Log(hit.distance + " VS " + Vector3.Distance(Camera.main.transform.position, transform.position));

            if (renderers[rendererID].isVisible == false || hit.distance + 5 < Vector3.Distance(Camera.main.transform.position, ray.origin))
            {
                return false;
                Debug.Log("INVISABLE!");
            }

            else
            {
                return true;


                //nav.Stop();
            }

        }

        return false;
    }

    private void ToggleRenderers(bool value)
    {
        foreach (Renderer rend in renderers)
            rend.enabled = value;
    }
}
