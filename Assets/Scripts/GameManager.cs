using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform boidPrefab;
    public int boidsCount;
    public List<Transform> boids = new List<Transform>();
    public List<GameObject> obstacles = new List<GameObject>();

    void Start()
    {
        for (var i = 0; i < boidsCount; i++)
        {
            var relativeSpawn = new Vector3(i % 2 * 6.0f, 4.5f, i / 2 * 6.0f);
            boids.Add(Instantiate(boidPrefab,new Vector3(this.transform.position.x + relativeSpawn.x, 4.5f, this.transform.position.z + relativeSpawn.z), transform.rotation));
        }
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle").ToList();
    }
}
