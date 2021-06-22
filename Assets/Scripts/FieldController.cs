using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{
    [SerializeField]
    private List<Field> fields;

    [SerializeField]
    private List<Transform> raycastTransforms;

    [SerializeField]
    private LayerMask figureMask;

    private void Start()
    {
        Player.Instance.FieldController = this;
    }

    public void PutFigureInField(string fieldName, int figureType)
    {
        var field = fields.Find(field => field.name == fieldName);
        field.PutFigure((TicTacToeFigureType)figureType);
        CheckWin();
    }

    public void CheckWin()
    {
        Debug.Log("checking win");
        foreach(var transform in raycastTransforms)
        {
            Debug.Log("Next");
            Debug.DrawRay(transform.position, transform.right * 10, Color.yellow, 3);
            var hits = Physics2D.RaycastAll(transform.position, transform.right* 10, 10, figureMask);
            foreach (var hit in hits)
                Debug.Log(hit.collider);
            if (hits.Length > 2)
            {
                var type = hits[0].collider.GetComponent<TicTacToeFigure>().Type;
                bool ok = true;
                for(int i = 0; i < hits.Length && ok; i++)
                {
                    var figureType = hits[i].collider.GetComponent<TicTacToeFigure>().Type;
                    if (figureType != type)
                    {
                        ok = false;
                    }
                    else
                    {
                        type = figureType;
                    }
                }
                if(ok)
                {
                    Debug.Log(type);
                    UIManager.Instance.OnWin?.Invoke(type);
                    Player.Instance.CanMakeTurn = false;
                    foreach(var obj in hits)
                    {
                        obj.collider.GetComponent<SpriteRenderer>().color = Color.yellow;
                    }
                    Debug.Log("won");
                    break;
                }

            }
        }
    }
}
