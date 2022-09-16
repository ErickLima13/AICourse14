using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ActionState
{
    IDLE,
    WORKING
};

public class RobberBehaviour : MonoBehaviour
{
    private BehaviourTree tree;

    private NavMeshAgent agent;

    private ActionState state = ActionState.IDLE;

    public GameObject diamond;
    public GameObject van;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        tree = new();

        Node steal = new("Steal Something");

        Leaf goToDiamond = new("Go To Diamond", GoToDiamond);
        Leaf goToVan = new("Go To Van",GoToVan);

        tree.AddChild(steal);

        steal.AddChild(goToDiamond);
        steal.AddChild(goToVan);

        tree.PrintTree();
        tree.Process();

      
    }

    public Node.Status GoToDiamond()
    {
        return GoToLocation(diamond.transform.position); ;
    }

    public Node.Status GoToVan()
    {
        return GoToLocation(van.transform.position); ;
    }

    private Node.Status GoToLocation(Vector3 destination)
    {
        float distanceToTarget = Vector3.Distance(destination, transform.position);

        if(state == ActionState.IDLE)
        {
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        else if(Vector3.Distance(agent.pathEndPosition,destination) >= 2)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }
        else if(distanceToTarget < 2)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCES;
        }

        return Node.Status.RUNNING;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
