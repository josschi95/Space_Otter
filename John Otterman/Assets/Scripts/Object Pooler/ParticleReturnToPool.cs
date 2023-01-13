using UnityEngine;

public class ParticleReturnToPool : MonoBehaviour
{
    public string poolTag;
    public void OnParticleSystemStopped()
    {
        ObjectPooler.Return(poolTag, gameObject);
    }
}