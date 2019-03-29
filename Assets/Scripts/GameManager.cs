using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

//GameManger is a singleton Monobehavior
public class GameManager : MonoBehaviour {

    #region Instance

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                print("Instance of GameObject does not exist!");

            return instance;
        }
    }

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    #endregion

    public GameObject cursorActive, cursorDefault;
    public List<GameObject> cleanup = new List<GameObject>();   //Use this to queue cleanup later;
     
    public void SetCursor(cursor cursor)
    {
        if (cursor == cursor.normal)
        {
            cursorActive.SetActive(false);
            cursorDefault.SetActive(true);
        }

        else if (cursor == cursor.interact)
        {
            cursorActive.SetActive(true);
            cursorDefault.SetActive(false);
        }

        //while its grabbing
        else if (cursor == cursor.grab || cursor == cursor.none)
        {
            cursorActive.SetActive(false);
            cursorDefault.SetActive(false);
        }

    }

}



