using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;//랜덤이 겹치므로 어느쪽 랜덤을 사용할 것인지 정의
using Photon.Pun;
using DG.Tweening;
using UnityEngine.UI;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Inst { get; private set; }
    void Awake() => Inst = this;
    //싱글톤 형식(매니저는 하나만 존재)

    [SerializeField] GameObject damagePrefab;//대미지 프리펩
    [SerializeField] List<Entity> myEntities;//본인 엔티티
    [SerializeField] GameObject targeting;//타겟팅 오브젝트
    [SerializeField] Entity myEmptyEntity;//빈 엔티티
    [SerializeField] Entity myBossEntity;//HQ 엔티티

    [SerializeField] List<Entity> mySaves;//저축칸
    [SerializeField] List<Entity> myDeads;//폐기칸

    //빈 엔티티 존재여부
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);

    //빈 엔티티 인덱스
    int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);

    //타겟 엔티티 존재여부
    bool ExistTargetingEntity => targetEntity != null;

    Entity selectedEntity;//선택중인 엔티티
    Entity targetEntity;//선택중인 엔티티의 대상인 엔티티

    //HQ 설정
    public void setBossEntity()
    {
        myBossEntity = CardManager.Inst.getPlayer().GetComponent<Entity>();
        myEmptyEntity.GetComponent<Entity>().setHQorEmpty(true);
    }
    //엔티티 정렬
    void EntityAlignment()
    {
        int upDown = CardManager.Inst.getPlayer().transform.rotation.eulerAngles.z == 0 ? -1 : 1;
        float targetY = 2f * upDown;// CardManager.Inst.getPlayer().transform.rotation.eulerAngles.z == 0 ? -2f : 2f;
        var targets = myEntities;
        for (int i = 0; i < targets.Count; i++)
        {
            float targetX = 0;
            switch (upDown)
            {
                case -1:
                    targetX = (targets.Count - 1) * -1.7f + i * 3.4f;//선공기준위치
                    break;
                case 1:
                    targetX = (targets.Count - 1) * 1.7f + i * -3.4f;//후공기준위치(아마)
                    break;
            }
            
            var target = targets[i];
            target.setOriginPos(new Vector3(targetX, targetY, 0));
            Quaternion rot = upDown == -1 ? Utils.QI : Quaternion.Euler(0, 0, 180);
            target.moveTransform(target.getOriginPos(), rot, true, 0.5f);
            if (!target.getHQorEmpty())
                target.GetComponent<Order>().SetOriginOrder(i);
        }
    }
    //빈 엔티티 생성
    public void InsertEmptyEntity(float xPos)
    {
        if (!ExistMyEmptyEntity)//빈 엔티티가 없으면
            myEntities.Add(myEmptyEntity);//추가한다

        //x위치 조정
        Vector3 emptyPos = myEmptyEntity.transform.position;
        emptyPos.x = xPos;
        myEmptyEntity.transform.position = emptyPos;

        int emptyIndex = MyEmptyEntityIndex;//빈 엔티티 인덱스 저장
        myEntities.Sort((entity1, entity2) //x만 비교해서 작은것->큰것 순서로 정렬
            => entity1.transform.position.x.CompareTo(entity2.transform.position.x));
        if (MyEmptyEntityIndex != emptyIndex)//정렬후 순서가 바뀌었을 경우
            EntityAlignment();//재정렬(물리적)
    }
    //빈 엔티티 제거
    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity)
            return;//빈 엔티티가 없으면 하지않는다

        myEntities.RemoveAt(MyEmptyEntityIndex);//빈 엔티티를 제거한다
        EntityAlignment();//제거하고 재정렬한다
    }
    //엔티티 스폰
    public bool SpawnEntity(CardData card, Vector3 spawnPos)
    {
        Quaternion rot = CardManager.Inst.getPlayer().transform.rotation;

        var entityObj= 
            PhotonNetwork.Instantiate("Prefabs/Entity", spawnPos, rot);
        entityObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        var entity = entityObj.GetComponent<Entity>();

        myEntities[MyEmptyEntityIndex] = entity;
        entity.setup(card);
        EntityAlignment();

        return true;
    }

    //카드에서 엔티티로(폐기 혹은 저축)
    public void cardToEntity(CardData card, Vector3 spawnPos, bool save)
    {
        Quaternion rot = CardManager.Inst.getPlayer().transform.rotation;

        var entityObj =
            PhotonNetwork.Instantiate("Prefabs/Entity", spawnPos, rot);
        entityObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        var entity = entityObj.GetComponent<Entity>();
        entity.setup(card);
        entity.setSaveorDead(true, true);
        if (save)
            mySaves.Add(entity);
        else
            myDeads.Add(entity);
    }
    //각 턴 초기화
    public void turnInit()
    {
        foreach (var item in myEntities)
        {
            item.setLock(false);//공포나 유지비 처리 필요
            item.setTurn();
        }
    }


    //저축칸
    public List<Entity> getSaves()
    {
        return mySaves;
    }
    //저축칸 제일 앞 엔티티 사용
    public void useSave(bool isColor)
    {
        Entity card = null;
        foreach (var item in mySaves)
        {
            if(isColor)
            {
                if (item.getColor() == TurnManager.Inst.getSelected())
                {
                    card = item;
                    break;
                }
            }
            else
            {
                card = item;
                break;
            }
        }
        if (isColor
            && card != null)
        {
            TurnManager.Inst.useCost(true);
        }
        else if (card != null)
        {
            TurnManager.Inst.useCost(false);
        }
        else
            return;
        deadEntities(1, card);
    }


    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    //엔티티 폐기
    void deadEntities(int from, Entity entity)
    {
        Quaternion rot = CardManager.Inst.getPlayer().transform.rotation.eulerAngles.z == 0 ? Utils.QI : Quaternion.Euler(0, 0, 180);
        switch (from)
        {
            case 0://필드에서
                myEntities.Remove(entity);
                myDeads.Add(entity);
                entity.moveTransform(CardManager.Inst.deadPos(), rot, false);
                EntityAlignment();
                entity.setSaveorDead(false, true);
                break;
            case 1://저축에서
                mySaves.Remove(entity);
                myDeads.Add(entity);
                entity.moveTransform(CardManager.Inst.deadPos(), rot, false);
                entity.setSaveorDead(true, false);
                entity.setSaveorDead(false, true);
                break;
            default:
                break;
        }
        
    }
}
