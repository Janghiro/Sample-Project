using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
                        PlayerData

            - 플레이어 데이터 정보
            
            - 플레이어 데이터와 관련된 기능 함수 제공
                - UsePortion() : 포션사용시 포션종류에 따른 능력치 변화
                - EquipItem()  : 장비장착시 장비종류에 따른 능력치 변화
                - UnequipItem(): 장비해제시 장비종류에 따른 능력치 변화
                - GetDamaged() : 피격당한 데미지에따른 능력치 변화
                - UseGold()    : 골드사용에 따른 보유 골드 변화
*/

public class PlayerData
{
    #region ** Player Status **
    [SerializeField] private float maxHp;
    [SerializeField] private float curHp;
    [SerializeField] private float speed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float defense;
    [SerializeField] private int gold;
    #endregion

    #region ** Player Position **
    [SerializeField] private float posX;
    [SerializeField] private float posY;
    [SerializeField] private float posZ;
    #endregion

    #region ** Properties **
    public float MaxHp => maxHp;
    public float CurHp => curHp;
    public float Speed => speed;
    public float RotateSpeed => rotateSpeed;
    public float Damage => damage;
    public float Defense => defense;
    public int Gold => gold;
    public float PosX => posX;
    public float PosY => posY;
    public float PosZ => posZ;
    #endregion

    // 플레이어 데이터 초기화
    public PlayerData(PlayerDataDTO dto)
    {
        this.maxHp = dto.Status.maxHp;
        this.curHp = dto.Status.curHp;
        this.speed = dto.Status.speed;
        this.rotateSpeed = dto.Status.rotateSpeed;
        this.damage = dto.Status.damage;
        this.defense = dto.Status.defense;
        this.gold = dto.Status.gold;

        this.posX = dto.Position.posX;
        this.posY = dto.Position.posY;
        this.posZ = dto.Position.posZ;
    }

    // PlayerData -> DTO
    public PlayerDataDTO ToDTO()
    {
        return new PlayerDataDTO
        {
            Status = new PlayerDataDTO.StatusDTO
            {
                maxHp = this.maxHp,
                curHp = this.curHp,
                speed = this.speed,
                rotateSpeed = this.rotateSpeed,
                damage = this.damage,
                defense = this.defense,
                gold = this.gold
            },
            Position = new PlayerDataDTO.PositionDTO
            {
                // 플레이어 현재 위치
                posX = this.posX,
                posY = this.posY,
                posZ = this.posZ
            }
        };
    }

    // 포션 회복
    public void UsePortion(float value, string type)
    {
        switch(type)
        {
            case "Health":
                curHp = Mathf.Min(curHp + value, maxHp);
                break;
        }
    }

    // 장비 장착
    public void EquipItem(float value, string type)
    {
        switch(type)
        {
            case "Weapon":
                damage += value;
                break;
            case "Armor":
                defense += value;
                break;
        }
    }

    // 장비 해제
    public void UnequipItem(float value, string type)
    {
        switch(type)
        {
            case "Weapon":
                damage -= value;
                break;
            case "Armor":
                defense -= value;
                break;
        }
    }

    // 데미지 입음
    public void GetDamaged(float damage)
    {
        Debug.Log(damage + "만큼 데미지 입음");
        curHp -= damage;
        if (curHp <= 0)
            curHp = 0;
    }
}
