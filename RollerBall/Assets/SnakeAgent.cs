using MLAgents;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class SnakeAgent : Agent
{
    public int gridWidth = 4;
    public int gridHeight = 4;
    public GameObject bodyPrefab;
    public GameObject head;
    public Rigidbody headRigidBody;
    public Vector3 headDirection;
    public List<GameObject> bodyList;

    public Vector3 tail;
    public int[,] gameGrid;


    const int FORWARD = 0;  // do nothing!
    const int LEFT = 1;
    const int RIGHT = 2;
    const int NOTHING = 3;

    void Start()
    {
        head = this.gameObject.transform.GetChild(0).gameObject;
        headRigidBody = GetComponentInChildren<Rigidbody>();

        bodyList = new List<GameObject>();
        gameGrid = new int[gridWidth, gridHeight];
        headDirection = randomDirection();
        tail = vectorCopy(head.transform.localPosition);

        //InvokeRepeating("RequestDecision", 2.0f, 0.2f);
    }

    public Transform Target;
    public override void AgentReset()
    {
        
        this.headRigidBody.angularVelocity = Vector3.zero;
        this.headRigidBody.velocity = Vector3.zero;

        head.transform.localPosition = new Vector3(rande(gridWidth), 0, rande(gridHeight));
        gameGrid[(int)head.transform.localPosition.x, (int)head.transform.localPosition.z] = 1;
        headDirection = randomDirection();
        foreach(GameObject body in bodyList)
        {
            Destroy(body);
        }
        bodyList = new List<GameObject>();
        gameGrid = new int[gridWidth, gridHeight];

        // Move the target to a new spot
        SpawnFood();
    }
    public override void CollectObservations()
    {
        Debug.Log("CollectObservations");
        // Target and Agent positions
        AddVectorObs(Target.localPosition.x);
        AddVectorObs(Target.localPosition.z);
        AddVectorObs(this.head.transform.localPosition.x);
        AddVectorObs(this.head.transform.localPosition.z);
        AddVectorObs(this.headDirection.x);
        AddVectorObs(this.headDirection.z);
        for(int i = 0; i < gridWidth; i++)
        {
            for(int j = 0; j < gridHeight; j++)
            {
                AddVectorObs(gameGrid[i,j]);
            }
        }
    }

    // public float speed = 10;
    public override void AgentAction(float[] vectorAction)
    {
        AddReward(-0.00005f);

        var action = Mathf.FloorToInt(vectorAction[0]);
        //Debug.Log(string.Format("action: {0}",action));
        Vector3 oldHeadPos = head.transform.localPosition;
        
        //Debug.Log(string.Format("(int)oldHeadPos.x: {0}  (int)oldHeadPos.z: {1}",fti(oldHeadPos.x), fti(oldHeadPos.z)));
        gameGrid[fti(oldHeadPos.x), fti(oldHeadPos.z)] = 0;

        switch (action)
        {
            case FORWARD:
                head.transform.localPosition = vectorRound(head.transform.localPosition + headDirection);
                break;
            case LEFT:
                headDirection = Quaternion.Euler(0, -90, 0) * headDirection;
                head.transform.localPosition = vectorRound(head.transform.localPosition + headDirection);
                break;
            case RIGHT:
                headDirection = Quaternion.Euler(0, 90, 0) * headDirection;
                head.transform.localPosition = vectorRound(head.transform.localPosition + headDirection);
                break;
            case NOTHING:
                //Debug.Log("nothing");
                return;
            default:
                throw new ArgumentException("Invalid action value");
        }
        Vector3 newheadPos = head.transform.localPosition;
        if (newheadPos.x < 0 || newheadPos.x >= gridWidth || newheadPos.z < 0 || newheadPos.z >= gridHeight)
        {
            AddReward(-0.1f);
            Debug.Log("OUT OF BOUNDS");
            Done();
            
        }

        for(int i = bodyList.Count - 1; i > 0; i--)
        {
            if (i == bodyList.Count - 1)
            {
                Vector3 oldPos = bodyList[i].transform.localPosition;
                gameGrid[(int)oldPos.x, (int)oldPos.z] = 0;
            }
            bodyList[i].transform.localPosition = vectorCopy(bodyList[i-1].transform.localPosition);
        }
        if (bodyList.Count > 0)
        {
            if (bodyList.Count - 1 == 0)
            {
                Vector3 oldPos = vectorCopy(bodyList[0].transform.localPosition);
                Debug.Log(string.Format("oldPos: {0}, {1} {2}", (int)oldPos.x, (int)oldPos.y, (int)oldPos.z));
                gameGrid[(int)oldPos.x, (int)oldPos.z] = 0;
            }
            bodyList[0].transform.localPosition = vectorCopy(oldHeadPos);
            gameGrid[(int)bodyList[0].transform.localPosition.x, (int)bodyList[0].transform.localPosition.z] = 1;
        }
        
        //Debug.Log(string.Format("(int)head.transform.localPosition.x: {0}  (int)head.transform.localPosition.z: {1}",
             //fti(head.transform.localPosition.x), fti(head.transform.localPosition.z)));

        if (gameGrid[fti(head.transform.localPosition.x), fti(head.transform.localPosition.z)] == 1)
        {
            Debug.Log("RAN INTO SELF");
            //AddReward(-0.01f);
            Done();
        }
        gameGrid[fti(head.transform.localPosition.x), fti(head.transform.localPosition.z)] = 1;

        // Rewards 
        float distanceToTarget = Vector3.Distance(head.transform.localPosition,
                                                Target.localPosition);
        //Debug.Log(string.Format("distanceToTarget: {0}", distanceToTarget));
        // Ate Food
        if (distanceToTarget < 0.6f)
        {
            //AddReward(2/(gridWidth*gridHeight));
            AddReward(1.0f);
            if (bodyList.Count >= gridWidth*gridHeight - 1)
            {
                Done();
            }
            GrowSnake();
            SpawnFood();
        }

        if (bodyList.Count > 0)
        {
            tail = vectorCopy(bodyList[bodyList.Count-1].transform.localPosition);
            
        } 
        else
        {
            tail = vectorCopy(head.transform.localPosition); //position rather than localPosition seems correct?
            
        }
        
        //Debug.Log(gameGrid);

        // Fell off platform
        // if (this.transform.localPosition.y < 0)
        // {
        //     Done();
        // }

    }

    private void SpawnFood()
    {
        Debug.Log("SPAWNFOOD");
        while(true)
        {
            int newx = (int)Mathf.Round(UnityEngine.Random.value * gridWidth);
            int newz = (int)Mathf.Round(UnityEngine.Random.value * gridHeight);
            //Debug.Log(string.Format("newx:{0}",newx));
            //Debug.Log(string.Format("newz:{0}",newz));
            if (newx < gridWidth && newz < gridHeight && gameGrid[newx,newz] == 0)
            {
                Target.localPosition = new Vector3(newx, 0.5f, newz);
                break;
            }
        }
    }

    private void GrowSnake()
    {
        Debug.Log("GROWSNAKE");
        Debug.Log(string.Format("tail: {0}, {1}", (int)tail.x, (int)tail.z));
        GameObject newBody = Instantiate(bodyPrefab, this.transform, false);
        newBody.transform.localPosition = tail;
        bodyList.Add(newBody);
        gameGrid[(int)tail.x, (int)tail.z] = 1;
        Debug.Log(string.Format("position: {0}",newBody.transform.position));
        Debug.Log(string.Format("localPosition: {0}",newBody.transform.localPosition));
    }

    private Vector3 vectorRound(Vector3 floatVec)
    {
        return new Vector3(Mathf.Round(floatVec.x), Mathf.Round(floatVec.y), Mathf.Round(floatVec.z));
    }
    private Vector3 vectorCopy(Vector3 vec)
    {
        return vectorRound(new Vector3(vec.x, vec.y, vec.z));
    }

    private int rande(int bound){
        return UnityEngine.Random.Range(0,bound);
    }

    private int fti(float f)
    {
        return (int)Mathf.Round(f);
    }

    private Vector3 randomDirection(){
        int r = rande(4);
        switch (r)
        {
            case 0:
                return new Vector3(1,0,0);
            case 1:
                return new Vector3(0,0,1);
            case 2:
                return new Vector3(-1,0,0);
            case 3:
                return new Vector3(0,0,-1);    
            default:
                throw new Exception("poopoo");
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[1];

        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { RIGHT };
        }
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { FORWARD };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { LEFT };
        }
        return new float[] { NOTHING };
    }

}
