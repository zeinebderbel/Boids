using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class Boid : MonoBehaviour
{
    public float Speed = 1f;
    public Vector3 Velocity;
    public float MaxVelocity = 20;
    public BoidStatesEnum State;
    public GameObject Target;
    private Vector3 cohesionVelocity;
    private Vector3 repulsionVelocity;
    private Vector3 alignementVelocity;
    public GameManager GM;
    private float nestRadius = 100;

    private float avoidanceDistance = 5;
    private float repulsionDistance = 100f;


    private float RepulsionForce = 70f;
    private float AttractionForce = 2f;
    private float AlignementForce = 3f;
    private float TargetForce = 10f;
    private float AvoidanceForce = 200f;

    public Vector3 Borders = new Vector3(48, 0, 48);
    public Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        GM = GameObject.FindObjectOfType<GameManager>();
        Target = GameObject.FindGameObjectWithTag("Target");
        State = BoidStatesEnum.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeState();
        switch (State)
        {
            case BoidStatesEnum.Idle:
                break;
            case BoidStatesEnum.FollowTarget:
                Velocity += CombineVector() + GoToTarget()*TargetForce;
                break;
            case BoidStatesEnum.RunAwayFromTarget:
                Velocity += CombineVector() - GoToTarget() * 5f;
                break;
        }
        if (Velocity.magnitude > MaxVelocity)
        {
            Velocity = Velocity.normalized * MaxVelocity;
        }

        Vector3 pos = transform.position + Velocity * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -Borders.x, Borders.x);
        pos.z = Mathf.Clamp(pos.z, -Borders.z, Borders.z);
        pos.y = 4.5f;
        this.transform.position = pos;
    }
    Vector3 CombineVector()
    {
        return  MoveAway() * RepulsionForce
            + MoveCloser() / AttractionForce 
            + AlignWith() / AlignementForce 
            + AvoidObstacle() * AvoidanceForce;
    }
  
    private void ChangeState()
    {
        if (Target == null)
            State = BoidStatesEnum.Idle;
        else
        {
            if (Target.gameObject.tag == "Obstacle")
                State = BoidStatesEnum.RunAwayFromTarget;
            else
                State = BoidStatesEnum.FollowTarget;
        }
    }

    public Vector3 MoveCloser()
    {
        cohesionVelocity = Vector3.zero;
        var average = Vector3.zero;
        int matesNb = 0;
        foreach (var boid in GM.boids.Where(b => b != this.transform))
        {
            var diff = boid.transform.position - this.transform.position;
            if (diff.magnitude <= nestRadius)
            {
                average += diff;
                matesNb++;
            }
            if (matesNb > 0)
            {
                average = average / matesNb;
                cohesionVelocity += average;
            }
        }
        //Debug.DrawRay(transform.position, cohesionVelocity, Color.blue);

        return cohesionVelocity;

    }
    public Vector3 MoveAway()
    {
        repulsionVelocity = Vector3.zero;
          var average = Vector3.zero;
        int matesNb = 0;
        foreach (var boid in GM.boids.Where(b => b != this.transform))
        {
            var diff = boid.transform.position - this.transform.position;
            if (diff.magnitude < repulsionDistance)
            {
                average += diff.normalized / diff.magnitude;
                matesNb++;
            }
        }
        if (matesNb > 0)
        {
            average = average / matesNb;
            repulsionVelocity -= average;
        }
        //Debug.DrawRay(transform.position, repulsionVelocity, Color.red);
        return repulsionVelocity;
    }
    public Vector3 AlignWith()
    {
        alignementVelocity = Vector3.zero;
        var average = Vector3.zero;
        int matesNb = 0;
        foreach (var boid in GM.boids.Where(b => b != this.transform))
        {
            var diff = boid.transform.position - this.transform.position;
            if (diff.magnitude <= nestRadius)
            {
                average += boid.gameObject.GetComponent<Boid>().Velocity.normalized;
                matesNb++;
            }
            if (matesNb > 0)
            {
                average = average / matesNb;
                alignementVelocity += average;
            }
        }
        //Debug.DrawRay(transform.position, alignementVelocity, Color.green);
        return alignementVelocity.normalized;
    }

    Vector3 GoToTarget()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = Target.transform.position - transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = Speed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);

        return targetDirection.normalized;
    }

    Vector3 AvoidObstacle()
    {
        var avoidVector = new Vector3();
        if (GM.obstacles.Count > 0)
        {
            foreach (var O in GM.obstacles)
            {
                var diff = transform.position - O.transform.position;
                if (diff.magnitude <= avoidanceDistance)
                {
                    var neededVelocity = diff.normalized * MaxVelocity;
                    avoidVector += neededVelocity;
                }
            }
        }
        return avoidVector;
    }
}
