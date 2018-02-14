using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPerspective : MonoBehaviour
{
    [SerializeField]
    Transform cameraTransform;

    [SerializeField]
    Transform player1Transform;
    [SerializeField]
    Transform player2Transform;

    private float speed = 5f;
    private bool isSwitching = false;

    enum CamPositon
    {
        Player1,
        Player2
    }
    CamPositon currentPosition = CamPositon.Player1;

    public void SwitchPerspective()
    {
        if (!isSwitching)
            StartCoroutine(SwitchCamera());
    }

    IEnumerator SwitchCamera()
    {
        isSwitching = true;
        currentPosition = (currentPosition == CamPositon.Player1) ? CamPositon.Player2 : CamPositon.Player1;
        Vector3 targetPos = (currentPosition == CamPositon.Player1) ? player1Transform.position : player2Transform.position;
        Quaternion targetRot = (currentPosition == CamPositon.Player1) ? player1Transform.rotation : player2Transform.rotation;

        float dist = Vector3.Distance(cameraTransform.position, targetPos);

        while (dist > 0.025f)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, Time.deltaTime * speed);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRot, Time.deltaTime * speed);

            dist = Vector3.Distance(cameraTransform.position, targetPos);
            yield return null;
        }

        // The camera has finished moving/switching
        isSwitching = false;
    }


}
