using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    private AudioSource audioSource;
    public Transform shotSpawn;
    public GameObject shot;
    public float fireRate;
    public float delay;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();   
        InvokeRepeating("Fire", delay, fireRate);
    }

    void Fire()
    {
        Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
    }
}
