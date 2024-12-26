using UnityEngine;

public class MapCenterer : MonoBehaviour
{
    void OnEnable()
    {
        //this object has scroll rect component i want to center the map on the player
        GameObject player = MapAvatarHandler.Instance.playerIcon;

        //get the player position
        Vector2 playerPos = player.transform.localPosition;

        gameObject.transform.localPosition = new Vector2(-playerPos.x, -playerPos.y);
    }

    void OnDisable()
    {
        //reset the map position
        transform.localPosition = Vector2.zero;
    }
}
