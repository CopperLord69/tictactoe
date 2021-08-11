using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldPicker : MonoBehaviour
{
    private Camera _camera;

    [SerializeField]
    private LayerMask fieldMask;

    void Start()
    {
        _camera = Camera.main;
        UIManager.Instance.SetPlayerType(Player.Instance.FigureType);
    }

    void Update()
    {
# if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            var startPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            var endPosition = startPosition;
            endPosition.z = 100;
            Debug.DrawLine(startPosition, endPosition, Color.cyan, 3);
            var hit = Physics2D.Linecast(startPosition, endPosition, fieldMask);
            if (hit)
            {
                if (hit.collider.TryGetComponent(out Field field) && !field.IsBusy)
                {
                    Player.Instance.MakeTurn(field);
                }
            }
        }
#endif

#if UNITY_ANDROID

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            var position = touch.position;
            var startPosition = _camera.ScreenToWorldPoint(position);
            var endPosition = startPosition;
            endPosition.z = 100;
            Debug.DrawLine(startPosition, endPosition, Color.cyan, 3);
            var hit = Physics2D.Linecast(startPosition, endPosition, fieldMask);
            
            if (hit)
            {
                if (hit.collider.TryGetComponent(out Field field) && !field.IsBusy)
                {
                    Player.Instance.MakeTurn(field);
                }
            }
        }
#endif


    }
}
