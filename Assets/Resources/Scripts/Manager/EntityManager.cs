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

    //[SerializeField] GameObject damagePrefab;//대미지 프리펩(포톤 인스턴싱이라 필요없음)
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

        var entityObj =
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
        atkReset();//유지비 처리 필요
    }


    //저축칸 반환
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
            if (isColor)
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
        ShowTargeting(ExistTargetingEntity);
    }

    //타겟팅 보이기
    private void ShowTargeting(bool isShow)
    {
        targeting.SetActive(isShow);//타겟팅 중일때는 보인다
        if (ExistTargetingEntity)//타겟이 존재하면, 타겟의 위치로
            targeting.transform.position = targetEntity.transform.position;
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

    //대미지 이미지
    void spawnDamage(int damage, Transform tr)
    {
        var damageTemp = PhotonNetwork.Instantiate("Prefabs/Damage", tr.position, tr.rotation).GetComponent<Damage>();
        damageTemp.SetupTransform(tr);
        damageTemp.Damaged(damage);
    }
    //공격
    void Attack(Entity atk, Entity def)
    {
        atk.setAttackable(false);//공격했으니 공격불가 상태
        atk.GetComponent<Order>().SetMostFrontOrder(true);//공격중 가장 위로
        
        Sequence sequence = DOTween.Sequence()
            .Append(atk.transform.DOMove(def.getOriginPos(), 0.4f)).SetEase(Ease.InSine)
            //공격자가 0.4초만에 방어자 위치로 이동
            .AppendCallback(
                () =>
                {//양측 대미지 계산
                    if (def.getHQorEmpty())
                    { }// DamageBoss(defender.isMine, attacker.attack);
                    else
                        def.Damaged(atk.getAttack());
                    spawnDamage(atk.getAttack(), def.transform);
                    if (atk.getHQorEmpty())
                    { }// DamageBoss(attacker.isMine, defender.attack);
                    else
                        atk.Damaged(def.getAttack());
                    spawnDamage(def.getAttack(), atk.transform);
                }
            )
            .Append(atk.transform.DOMove(atk.getOriginPos(), 0.4f)).SetEase(Ease.OutSine)
            //공격자가 0.4초만에 공격자 위치로 이동
            .OnComplete(//전투 종료
                () => {
                    //AttackCallback(attacker, defender);//죽은 엔티티 처리
                    //if (myEntities.Contains(attacker)//공격자가 살아남았고(아직 본인 엔트리에 존재)
                    //    && !attacker.isBorder)//경계가 아니라면
                    //    attacker.locked = true;//락을 건다
                }
            );
    }
    //공격가능여부 초기화
    void atkReset()
    {
        foreach (var item in myEntities)
        {
            if (item.getFear())
                item.setFear(false);
            else if(item.getLock())
            {
                if (item.getMaintain())
                {
                    //유지비 고민좀
                }
                else
                {
                    item.setLock(false);
                    item.setAttackable(true);
                }
            }
        }
    }
    #region 마우스 조작
    public void entityMouseDown(Entity entity)
    {
        switch (TurnManager.Inst.getPhase())
        {
            case PHASE.MAIN:
                selectedEntity = entity;
                break;
            case PHASE.TARGETING:
                break;
            case PHASE.WAITING:
                return;
            default:
                break;
        }
    }
    public void entityMouseUp(Entity entity)
    {
        switch (TurnManager.Inst.getPhase())
        {
            case PHASE.MAIN:
                if (selectedEntity
                    && targetEntity
                    && selectedEntity.getAttackable())
                    Attack(selectedEntity, targetEntity);
                break;
            case PHASE.TARGETING:
                break;
            case PHASE.WAITING:
                return;
            default:
                break;
        }
        selectedEntity = null;
        targetEntity = null;
    }
    #endregion
}
