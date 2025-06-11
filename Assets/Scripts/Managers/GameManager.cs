using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    //[SerializeField] public PlayerController player;
    [SerializeField] public GameObject profileUI;

    protected override void Awake()
    {
        base.Awake();
    }

  
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            profileUI.SetActive(!profileUI.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            DataManager.Instance.SavePlayerData();
            Debug.Log("저장완료");
        }
        else if (Input.GetKeyDown(KeyCode.Space))
            DataManager.Instance.GetPlayerData().GetDamaged(100f);
    }
}
