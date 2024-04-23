using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.EndVertical();
    }

    private float GetInputMoveX()
    {
        float inputMoveX = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _entity._DashMovement();
            }
            inputMoveX = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _entity._DashMovement();
            }
            inputMoveX = 1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            _entity._DashMovement();
        }

        return inputMoveX;
    }

    private void Update()
    {
        _entity.SetMoveDirX(GetInputMoveX());
    }
}