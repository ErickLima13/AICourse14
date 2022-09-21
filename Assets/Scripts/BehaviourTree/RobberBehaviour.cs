using System.Collections.Generic;
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

    private int index = 0;
    private int childCount = 0;

    private Leaf hasGotMoney;
    private Leaf hasThings;
    private Leaf goToDiamond;
    private Leaf goToVan;
    private Leaf goToBackDoor;
    private Leaf goToFrontDoor;

    private Sequence steal;
    private Selector opendoor;
    private Selector things;

    public GameObject diamond;
    public GameObject van;
    public GameObject backDoor;
    public GameObject frontDoor;

    public List<GameObject> stealsThings;

    [Range(0, 1000)] public int money = 800;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        tree = new();

        diamondChild = diamond.transform.GetChild(0).gameObject;

        stealsThings = GameObject.FindGameObjectsWithTag("Player").ToList();

        steal = new("Steal Something");
        opendoor = new("Open Door");

        hasGotMoney = new("Has Got Money", HasMoney);
        hasThings = new("Has  Things", Hasthings);
        goToDiamond = new("Go To Diamond", GoToDiamond);
        goToVan = new("Go To Van", GoToVan);
        goToBackDoor = new("Go To Backdoor", GoToBackDoor);
        goToFrontDoor = new("Go To Frontdoor", GoToFrontDoor);

        //tree.AddChild(steal);

        //steal.AddChild(hasGotMoney);

        //steal.AddChild(opendoor);
        // steal.AddChild(goToDiamond);
        //steal.AddChild(goToFrontDoor);
        // steal.AddChild(goToVan);
        //steal.AddChild(hasThings);

        GoingToSteal();

        opendoor.AddChild(goToFrontDoor);
        opendoor.AddChild(goToBackDoor);

        tree.PrintTree();

        Time.timeScale = 5;

    }

    private void GoingToSteal()
    {
        tree.AddChild(steal);
        steal.AddChild(opendoor);
        steal.AddChild(goToDiamond);
        steal.AddChild(goToVan);
        steal.AddChild(hasThings);
    }

    public Node.Status HasMoney()
    {
        if (money >= 500)
        {
            return Node.Status.FAILURE;
        }

        return Node.Status.SUCCES;
    }

    public Node.Status Hasthings()
    {
        if (index >= stealsThings.Count)
        {
            index = 0;
        }

        if (stealsThings.Count <= 0)
        {
            GetComponent<EndScene>().animator.Play("FadeIn");
            return Node.Status.FAILURE;
        }

        GoingToSteal();

        return Node.Status.SUCCES;
    }

    public Node.Status GoToDiamond()
    {
        GameObject go = stealsThings[index];

        Node.Status s = GoToLocation(go.transform.position);

        if (s == Node.Status.SUCCES)
        {
            go.transform.parent = transform;
            go.transform.position = transform.position;

            if (go.GetComponent<Rotate>() != null)
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
            transform.GetChild(childCount + 1).gameObject.SetActive(false);
            stealsThings.RemoveAt(index);
            money += 300;
            index++;
            childCount++;
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
        if (treeStatus != Node.Status.SUCCES)
        {
            treeStatus = tree.Process();
        }
    }
}
