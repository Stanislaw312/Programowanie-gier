using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridButton : MonoBehaviour
{
    private BoardManager boardManager;

    public void ButtonClick()
    {
        GameObject clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        boardManager.OnGridButtonClick();
    }

    public void SetBoardManagerReference( BoardManager boardManager )
    {
        this.boardManager = boardManager;
    }
    
}
