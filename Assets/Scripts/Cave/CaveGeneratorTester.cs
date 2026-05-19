using UnityEngine;

[RequireComponent(typeof(CaveGenerator))]
public class CaveGeneratorTester : MonoBehaviour
{
    private CaveGenerator generator;

    private void Awake()
    {
        generator = GetComponent<CaveGenerator>();
    }

    [ContextMenu("Generate Cave")]
    public void TestGenerateCave()
    {
        generator.GenerateCave();
    }

    [ContextMenu("Regenerate Cave")]
    public void TestRegenerateCave()
    {
        generator.RegenerateCave();
    }

    [ContextMenu("Clear Map")]
    public void TestClearMap()
    {
        Transform mapParent = transform.Find("CaveMap");
        if (mapParent != null)
        {
            DestroyImmediate(mapParent.gameObject);
        }
    }
}
