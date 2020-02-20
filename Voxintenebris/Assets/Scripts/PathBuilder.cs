﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathBuilder : MonoBehaviour
{

    public GameObject Tile;
    public float spawnDelay;
    public int TileThreshold = 4;
    public bool stopSpawning;
    public int keep_direction_for_n_turns = 3;

    private float spawnTimer = 0;
    private int number_tiles_spawned = 0;

    private GameObject floor;

    private System.Random rand = new System.Random ();  

    private List<Vector3> possibleDirections = new List<Vector3> {
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
        new Vector3(1,0,0),
        new Vector3(-1,0,0)
    };

    private enum Directions {
        FORWARD = 0,
        BACKWARD = 1,
        RIGHT = 2,
        LEFT = 3,

    }
    private Directions previousDirection = Directions.FORWARD;

    void Update() {
        if (number_tiles_spawned < TileThreshold && spawnTimer > spawnDelay && !stopSpawning)
        {
           SpawnTile(new List<Directions>(){ Directions.RIGHT, Directions.LEFT, Directions.FORWARD, Directions.BACKWARD });
        //    SpawnTile(new List<Directions>(){ Directions.FORWARD });
           spawnTimer = 0;
        }
        else {
            spawnTimer += Time.deltaTime;
        }
    }

    private void SpawnTile(List<Directions> this_turn_possible_direction) {

        if(this_turn_possible_direction.Count == 0){
            Debug.Log("Dead-end reached!");
            stopSpawning = true;
            return;
        } 

        if (keep_direction_for_n_turns > 0 && !is_colliding_with_obstacle(transform.position, previousDirection) && is_space_available(transform.position, possibleDirections[(int)previousDirection])) {
            instantiateTile();
        }
        else{

            Directions random_move_direction = this_turn_possible_direction[rand.Next(this_turn_possible_direction.Count)];
            Vector3 random_move = possibleDirections[(int)random_move_direction];

            if (is_colliding_with_obstacle(transform.position, random_move_direction) || !is_space_available(transform.position, random_move)) {
                this_turn_possible_direction.Remove(random_move_direction);
                SpawnTile(new List<Directions>(this_turn_possible_direction));
            }
            else {
                keep_direction_for_n_turns = 3;
                previousDirection = random_move_direction;
                instantiateTile();
            }
        }
    }

    private bool is_space_available(Vector3 current_position, Vector3 move) {
        Vector3 newPosition = current_position + (move * transform.localScale.x);

        Ray out_of_bound_ray = new Ray(newPosition, Vector3.down);
        RaycastHit hit_out_of_bound;

        bool is_collision_detected = Physics.Raycast(out_of_bound_ray, out hit_out_of_bound, 1);

        if (!is_collision_detected) {
            Debug.Log("Out of bound!");
            return false;
        }

        return true;

    }

    private bool is_colliding_with_obstacle(Vector3 position, Directions direction) {
        RaycastHit collision_hit;
        Ray collision_ray;

        switch (direction)
        {
            case Directions.RIGHT:
                collision_ray = new Ray(position, Vector3.right);
                break;
            case Directions.LEFT:
                collision_ray = new Ray(position, Vector3.left);
                break;
            case Directions.FORWARD:
                collision_ray = new Ray(position, Vector3.forward);
                break;
            case Directions.BACKWARD:
                collision_ray = new Ray(position, Vector3.back);
                break;
            default:
                Debug.Log("Something went wrong. Weird direction detected!");
                return true;
        }

        Debug.DrawRay(collision_ray.origin, collision_ray.direction * 3f, Color.blue);
        Debug.DrawRay(collision_ray.origin, collision_ray.direction * 9f, Color.red);

        bool is_collision_detected = Physics.Raycast(collision_ray, out collision_hit, 3f);
        bool is_deadend_detected = Physics.Raycast(collision_ray, out collision_hit, 9f);

        if (is_collision_detected){
            if (collision_hit.collider.tag == "Tile"){
                Debug.Log("A previous tile was already present in this space!");
            } else {
                Debug.Log("Collision detected with an obstacle");
            }
            return true;
        }
        
        if (is_deadend_detected && collision_hit.collider.tag == "Tile"){
             Debug.Log("Possible deadend, stir away!");
             return true;
        }

        return false; 

    }

    private void instantiateTile() {
        transform.position = transform.position + (possibleDirections[(int)previousDirection] * transform.localScale.x);
        GameObject spawned_tile = Instantiate(Tile, transform.position, transform.rotation);
        number_tiles_spawned += 1;
        keep_direction_for_n_turns -= 1; 
        Debug.Log(String.Format("# Tile spawned {0}/{1}", number_tiles_spawned, TileThreshold));
    }

}
