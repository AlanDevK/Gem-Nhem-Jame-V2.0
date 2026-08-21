using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    [SerializeField] float globalShakeForce = 1f;

    void Awake(){
        if (instance == null){
            instance = this;
        }
    }

    public void CameraShakeFX(CinemachineImpulseSource impulseSource){
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}
