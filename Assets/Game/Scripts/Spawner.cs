using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    private List<SpawnPoint> _spawnPoints;
    private bool _hasSpawned;
    private List<Character> _spawnedCharacters;

    public Collider colider;
    public UnityEvent OnAllSpawnedCharacterEliminated;

    private void Awake()
    {
        var spawnPointArray = transform.parent.GetComponentsInChildren<SpawnPoint>();
        _spawnPoints = new List<SpawnPoint>(spawnPointArray);
        _spawnedCharacters = new List<Character>();
    }
    public void SpawnCharacters()
    {
        if (_hasSpawned)
            return;
        _hasSpawned = true;
        foreach (SpawnPoint point in _spawnPoints) 
        {
            if(point.enemyToSpawn != null)
            {
                GameObject spawnedGameObject = Instantiate(point.enemyToSpawn, point.transform.position, point.transform.rotation);
                _spawnedCharacters.Add(spawnedGameObject.GetComponent<Character>());
            }
        }
    }
    private void Update()
    {
        if(!_hasSpawned || _spawnedCharacters.Count == 0) 
            return;
        bool allSpawnedAreDead = true;
        foreach (Character character in _spawnedCharacters) 
        {
            if(character.currentState !=  Character.characterState.Dead)
            {
                allSpawnedAreDead = false;
                break;
            }
        }
        if(allSpawnedAreDead)
        {
            if (OnAllSpawnedCharacterEliminated != null)
                OnAllSpawnedCharacterEliminated.Invoke();
            _spawnedCharacters.Clear();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            SpawnCharacters();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, colider.bounds.size);
    }
}
