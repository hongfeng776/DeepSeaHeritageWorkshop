using UnityEngine;
using UnityEngine.AI;

public static class EnemyFactory
{
    public static GameObject CreateBasicEnemy(Vector3 position, Transform parent = null)
    {
        GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemyObj.name = "Enemy_Basic";
        enemyObj.transform.position = position;
        enemyObj.transform.parent = parent;

        enemyObj.tag = "Enemy";

        CapsuleCollider collider = enemyObj.GetComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.center = new Vector3(0, 1f, 0);

        Rigidbody rb = enemyObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        NavMeshAgent agent = enemyObj.AddComponent<NavMeshAgent>();
        agent.height = 2f;
        agent.radius = 0.5f;
        agent.speed = 2.5f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 1f;
        agent.angularSpeed = 120f;

        BaseEnemy baseEnemy = enemyObj.AddComponent<BaseEnemy>();

        Renderer renderer = enemyObj.GetComponent<Renderer>();
        renderer.material.color = Color.red;

        return enemyObj;
    }

    public static GameObject CreateFastEnemy(Vector3 position, Transform parent = null)
    {
        GameObject enemyObj = CreateBasicEnemy(position, parent);
        enemyObj.name = "Enemy_Fast";

        Renderer renderer = enemyObj.GetComponent<Renderer>();
        renderer.material.color = Color.yellow;

        BaseEnemy baseEnemy = enemyObj.GetComponent<BaseEnemy>();

        return enemyObj;
    }

    public static GameObject CreateTankEnemy(Vector3 position, Transform parent = null)
    {
        GameObject enemyObj = CreateBasicEnemy(position, parent);
        enemyObj.name = "Enemy_Tank";
        enemyObj.transform.localScale = Vector3.one * 1.3f;

        Renderer renderer = enemyObj.GetComponent<Renderer>();
        renderer.material.color = new Color(0.5f, 0f, 0.5f);

        BaseEnemy baseEnemy = enemyObj.GetComponent<BaseEnemy>();

        return enemyObj;
    }

    public static GameObject CreateRandomEnemy(Vector3 position, Transform parent = null)
    {
        int randomType = Random.Range(0, 3);
        switch (randomType)
        {
            case 0:
                return CreateBasicEnemy(position, parent);
            case 1:
                return CreateFastEnemy(position, parent);
            case 2:
                return CreateTankEnemy(position, parent);
            default:
                return CreateBasicEnemy(position, parent);
        }
    }
}
