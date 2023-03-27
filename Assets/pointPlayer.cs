using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class pointPlayer : MonoBehaviour
{
    NavMeshAgent agent;

    RaycastHit hitInfo = new RaycastHit();
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(ray.origin, ray.direction, Color.red);
            if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
            {
                agent.destination = hitInfo.point;
            }
        }
    }
}
