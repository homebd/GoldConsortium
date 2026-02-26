using UnityEngine;

public class GameTrigger : MonoBehaviour
{
    private void Awake()
    {
        Instantiate(Resources.Load("UIs/DebugUI"));

        var ui = UIManager.Instance;

        GameManager.Instance.InitPlayers();
        GameManager.Instance.SetPhase(Game.Enum.Phase.TravelSelection);
    }
}