using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class SpectatePlayer : NetworkComponent
{
    PlayerController CurrentSpectate;
    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {
        if (IsLocalPlayer)
        {
            Camera.main.transform.position = this.transform.position;

            StartCoroutine(ChooseNextPlayer());
        }

    }

    public IEnumerator ChooseNextPlayer()
    {
        while (IsConnected)
        {
            PlayerController[] PlayerList = FindObjectsOfType<PlayerController>();
            CurrentSpectate = PlayerList[Random.Range(0, PlayerList.Length)];
            yield return new WaitForSeconds(20f);
        }
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            float cameraSpeed = 5f;
            Vector3 offsetVector = transform.forward * -10;
            Vector3 targetCameraPosition = CurrentSpectate.gameObject.transform.position;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, cameraSpeed * Time.deltaTime);
            Camera.main.transform.forward = new Vector3(0, 0, 1);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.05f);
    }
}
