using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IAEnemy : MonoBehaviour
{

    // Composants NavMeshAgent et Animator pour le mouvement et l'animation
    NavMeshAgent agent;      // Référence à l'agent de navigation
    Animator anim;           // Référence à l'animateur pour les animations du NPC
    State currentState;      // État actuel du NPC
    AudioManager audioManager;  

    public Transform player; // Référence au joueur pour interagir avec le NPC

    // Initialisation au démarrage de la scène
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();    // Initialisation de l'agent de navigation
        anim = GetComponent<Animator>();         // Initialisation de l'animateur
        player = GameObject.FindGameObjectWithTag("Player").transform;
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        currentState = new Idle(gameObject, agent, anim, player, audioManager); // Le NPC commence en état "Idle"
    }

    // Mise à jour à chaque frame pour traiter l'état actuel
    void Update()
    {
        currentState = currentState.Process(); // Mise à jour de l'état actuel du NPC
    }
}