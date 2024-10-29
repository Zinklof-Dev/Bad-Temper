using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    [SerializeField] private float sensitivity;
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private GameObject cameraPivot2;

    private Vector3 cameraRotationEuler;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            float x = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            float y = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

            cameraRotationEuler += new Vector3(-x, 0, 0);
            if (cameraRotationEuler.x > 90)
            {
                cameraRotationEuler.x = 90;
            }
            if (cameraRotationEuler.x < -90)
            {
                cameraRotationEuler.x = -90;
            }
            cameraPivot2.transform.localRotation = Quaternion.Euler(cameraRotationEuler);
            cameraPivot.transform.Rotate(0, y, 0);
            
        }
    }
}
