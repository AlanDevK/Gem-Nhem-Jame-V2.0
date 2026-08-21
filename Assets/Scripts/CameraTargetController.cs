using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] Transform player;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float searchRadius = 15f;
    [SerializeField] float maxMousePanDistance = 4f;
    
    [Header("Camera Feel")]
    [SerializeField] float smoothSpeed = 10f;

    Camera mainCam;

    void Start(){
        mainCam = Camera.main;
    }

    void Update(){
        if (player==null || mainCam == null) return;
        bool isCombatMode = Physics2D.OverlapCircle(player.position, searchRadius, enemyLayer) != null;

        Vector3 targetPosition;
        if (isCombatMode){
            Vector2 mouseScreen2D = Mouse.current.position.ReadValue();
            Vector3 mouseScreen3D = new Vector3(mouseScreen2D.x, mouseScreen2D.y, Mathf.Abs(mainCam.transform.position.z));
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(mouseScreen3D);
            Vector3 offset = mouseWorld - player.position;
            Vector3 middleGround = offset/2f;
            mouseWorld.z = player.position.z;
            Vector3 clampedOffset = Vector3.ClampMagnitude(middleGround, maxMousePanDistance);
            targetPosition = player.position + clampedOffset;
        }
        else{
            targetPosition = player.position;
        }
        targetPosition.z = player.position.z;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime*smoothSpeed);
    }

    void OnDrawGizmosSelected(){
        if (player!=null){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, searchRadius);
        }
    }
}
