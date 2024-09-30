using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCaster : MonoBehaviour
{
    private Collider _damageCasterColider;
    public int damage = 30;
    public string targetTag;
    private List<Collider> _damagedTargetList;
    private void Awake()
    {
        _damageCasterColider = GetComponent<Collider>();
        _damageCasterColider.enabled = false;
        _damagedTargetList = new List<Collider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == targetTag && !_damagedTargetList.Contains(other))
        {
            Character targetCharacter = other.GetComponent<Character>();
            if (targetCharacter != null) 
            {
                targetCharacter.ApplyDamage(damage);
                EnemyVFXManager enemyVFXManager = transform.parent.GetComponent<EnemyVFXManager>();
                if (enemyVFXManager != null)
                {
                    RaycastHit hit;
                    Vector3 originalPos = transform.position + (-_damageCasterColider.bounds.extents.z) * transform.forward;
                    bool isHit = Physics.BoxCast(originalPos, _damageCasterColider.bounds.extents / 2, transform.forward, out hit, transform.rotation, _damageCasterColider.bounds.extents.z, 1 << 6);
                    if (isHit) 
                    {
                        enemyVFXManager.PlaySlash(hit.point + new Vector3(0, 0.5f, 0));
                    }
                }
            }
            _damagedTargetList.Add(other);
        }
    }
    public void EnableDamageCaster()
    {
        _damagedTargetList.Clear();
        _damageCasterColider.enabled = true;
    }
    public void DisableDamageCaster()
    {
        _damagedTargetList.Clear();
        _damageCasterColider.enabled = false;
    }
    private void OnDrawGizmos()
    {
        if (_damageCasterColider == null)
            _damageCasterColider = GetComponent<Collider>();

        RaycastHit hit;
        Vector3 originalPos = transform.position + (-_damageCasterColider.bounds.extents.z) * transform.forward;
        bool isHit = Physics.BoxCast(originalPos, _damageCasterColider.bounds.extents / 2, transform.forward, out hit, transform.rotation, _damageCasterColider.bounds.extents.z, 1 << 6);
        if (isHit) 
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.point, 1.3f);
        }
    }
}
