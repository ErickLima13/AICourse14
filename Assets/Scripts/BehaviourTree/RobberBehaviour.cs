using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private GameObject diamondChild;

    private ActionState state = ActionState.IDLE;

    private Node.Status treeStatus = Node.Status.RUNNING;

    private int randomIndex;

    public GameObject diamond;
    public GameObject van;
    public GameObject backDoor;
    public GameObject frontDoor;

    public List<GameObject> stealsThings;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        tree = new();

        diamondChild = diamond.transform.GetChild(0).gameObject;

        stealsThings = GameObject.FindGameObjectsWithTag("Player").ToList();
        randomIndex = Random.Range(0, stealsThings.Count);
        

        Sequence steal = new("Steal Something");
        Selector opendoor = new("Open Door");

        Leaf goToDiamond = new("Go To Diamond", GoToDiamond);
        Leaf goToVan = new("Go To Van", GoToVan);
        Leaf goToBackDoor = new("Go To Backdoor", GoToBackDoor);
        Leaf goToFrontDoor = new("Go To Frontdoor", GoToFrontDoor);

        tree.AddChild(steal);

        steal.AddChild(opendoor);
        steal.AddChild(goToDiamond);
        //steal.AddChild(goToFrontDoor);
        steal.AddChild(goToVan);

        opendoor.AddChild(goToFrontDoor);
        opendoor.AddChild(goToBackDoor);


        tree.PrintTree();






    }

    public Node.Status GoToDiamond()
    {
        GameObject go = stealsThings[randomIndex];

        Node.Status s = GoToLocation(go.transform.position);

        if (s == Node.Status.SUCCES)
        {
            go.transform.parent = transform;
            go.transform.position = transform.position;

            if(go.GetComponent<Rotate>() != null)
            {
                go.GetComponent<Rotate>().enabled = false;
            }
            
        }

        return s;
    }

    public Node.Status GoToVan()
    {
        Node.Status s = GoToLocation(van.transform.position);

        if (s == Node.Status.SUCCES)
        {
            //diamondChild.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            stealsThings.Remove(transform.GetChild(1).gameObject);
        }

        return s;
    }

    public Node.Status GoToBackDoor()
    {
        return GoToDoor(backDoor);
    }

    public Node.Status GoToFrontDoor()
    {
        return GoToDoor(frontDoor); ;
    }

    public Node.Status GoToDoor(GameObject door)
    {
        Node.Status s = GoToLocation(door.transform.position);

        if (s == Node.Status.SUCCES)
        {
            if (!door.GetComponent<Lock>().isLocked)
            {
                door.SetActive(false);
                return Node.Status.SUCCES;
            }

            return Node.Status.FAILURE;
        }
        else
        {
            return s;
        }
    }

    private Node.Status GoToLocation(Vector3 destination)
    {
        float distanceToTarget = Vector3.Distance(destination, transform.position);

        if (state == ActionState.IDLE)
        {
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }
        else if (distanceToTarget < 2)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCES;
        }

        return Node.Status.RUNNING;
    }

    // Update is called once per frame
    void Update()
    {
        if (treeStatus == Node.Status.RUNNING)
        {
            treeStatus = tree.Process();
        }


    }
}
