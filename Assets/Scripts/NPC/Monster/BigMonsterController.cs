using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigMonsterController : MonsterControllerAbstract
{
    public int m_BigDamage = 2;

    protected override void Awake() {
        base.Awake();
        m_Strength = m_BigDamage;
    }

    protected override void SpawnEnemies() {
        Transform m_Spawnpoints = gameObject.transform.Find("SpawnPoints");
        Transform m_Spawn1 = m_Spawnpoints.transform.Find("Spawn1");
        Transform m_Spawn2 = m_Spawnpoints.transform.Find("Spawn2");
        GameObject littleMonster1 = Instantiate(m_LittleMonster, SpawnNoise(m_Spawn1), Quaternion.identity);
        GameObject littleMonster2 = Instantiate(m_LittleMonster, SpawnNoise(m_Spawn2), Quaternion.identity);

        //making one monster walk the other way 
        SmallMonsterController littleMonster1Controller = littleMonster1.GetComponent<SmallMonsterController>();
        littleMonster1Controller.DetectorsSwitcher();
        littleMonster1Controller.m_LookingDirection = -littleMonster1Controller.m_LookingDirection;
    }

    private Vector2 SpawnNoise(Transform spawn) {
        float xNoise = 0f;
        xNoise = Random.Range(spawn.position.x - 1f, spawn.position.x + 1f);
        return new Vector2(xNoise, spawn.position.y);
    }
    
}
