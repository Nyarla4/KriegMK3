using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;//������ ��ġ�Ƿ� ����� ������ ����� ������ ����
using Photon.Pun;
using DG.Tweening;
using UnityEngine.UI;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Inst { get; private set; }
    void Awake() => Inst = this;
    //�̱��� ����(�Ŵ����� �ϳ��� ����)

    //[SerializeField] GameObject damagePrefab;//����� ������(���� �ν��Ͻ��̶� �ʿ����)
    [SerializeField] List<Entity> myEntities;//���� ��ƼƼ
    [SerializeField] GameObject targeting;//Ÿ���� ������Ʈ
    [SerializeField] Entity myEmptyEntity;//�� ��ƼƼ
    [SerializeField] Entity myBossEntity;//HQ ��ƼƼ

    [SerializeField] List<Entity> mySaves;//����ĭ
    [SerializeField] List<Entity> myDeads;//���ĭ

    //�� ��ƼƼ ���翩��
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);

    //�� ��ƼƼ �ε���
    int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);

    //Ÿ�� ��ƼƼ ���翩��
    bool ExistTargetingEntity => targetEntity != null;

    Entity selectedEntity;//�������� ��ƼƼ
    Entity targetEntity;//�������� ��ƼƼ�� ����� ��ƼƼ

    //HQ ����
    public void setBossEntity()
    {
        myBossEntity = CardManager.Inst.getPlayer().GetComponent<Entity>();
        myEmptyEntity.GetComponent<Entity>().setHQorEmpty(true);
    }
    //��ƼƼ ����
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
                    targetX = (targets.Count - 1) * -1.7f + i * 3.4f;//����������ġ
                    break;
                case 1:
                    targetX = (targets.Count - 1) * 1.7f + i * -3.4f;//�İ�������ġ(�Ƹ�)
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
    //�� ��ƼƼ ����
    public void InsertEmptyEntity(float xPos)
    {
        if (!ExistMyEmptyEntity)//�� ��ƼƼ�� ������
            myEntities.Add(myEmptyEntity);//�߰��Ѵ�

        //x��ġ ����
        Vector3 emptyPos = myEmptyEntity.transform.position;
        emptyPos.x = xPos;
        myEmptyEntity.transform.position = emptyPos;

        int emptyIndex = MyEmptyEntityIndex;//�� ��ƼƼ �ε��� ����
        myEntities.Sort((entity1, entity2) //x�� ���ؼ� ������->ū�� ������ ����
            => entity1.transform.position.x.CompareTo(entity2.transform.position.x));
        if (MyEmptyEntityIndex != emptyIndex)//������ ������ �ٲ���� ���
            EntityAlignment();//������(������)
    }
    //�� ��ƼƼ ����
    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity)
            return;//�� ��ƼƼ�� ������ �����ʴ´�

        myEntities.RemoveAt(MyEmptyEntityIndex);//�� ��ƼƼ�� �����Ѵ�
        EntityAlignment();//�����ϰ� �������Ѵ�
    }
    //��ƼƼ ����
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

    //ī�忡�� ��ƼƼ��(��� Ȥ�� ����)
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
    //�� �� �ʱ�ȭ
    public void turnInit()
    {
        atkReset();//������ ó�� �ʿ�
    }


    //����ĭ ��ȯ
    public List<Entity> getSaves()
    {
        return mySaves;
    }
    //����ĭ ���� �� ��ƼƼ ���
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

    //Ÿ���� ���̱�
    private void ShowTargeting(bool isShow)
    {
        targeting.SetActive(isShow);//Ÿ���� ���϶��� ���δ�
        if (ExistTargetingEntity)//Ÿ���� �����ϸ�, Ÿ���� ��ġ��
            targeting.transform.position = targetEntity.transform.position;
    }

    //��ƼƼ ���
    void deadEntities(int from, Entity entity)
    {
        Quaternion rot = CardManager.Inst.getPlayer().transform.rotation.eulerAngles.z == 0 ? Utils.QI : Quaternion.Euler(0, 0, 180);
        switch (from)
        {
            case 0://�ʵ忡��
                myEntities.Remove(entity);
                myDeads.Add(entity);
                entity.moveTransform(CardManager.Inst.deadPos(), rot, false);
                EntityAlignment();
                entity.setSaveorDead(false, true);
                break;
            case 1://���࿡��
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

    //����� �̹���
    void spawnDamage(int damage, Transform tr)
    {
        var damageTemp = PhotonNetwork.Instantiate("Prefabs/Damage", tr.position, tr.rotation).GetComponent<Damage>();
        damageTemp.SetupTransform(tr);
        damageTemp.Damaged(damage);
    }
    //����
    void Attack(Entity atk, Entity def)
    {
        atk.setAttackable(false);//���������� ���ݺҰ� ����
        atk.GetComponent<Order>().SetMostFrontOrder(true);//������ ���� ����
        
        Sequence sequence = DOTween.Sequence()
            .Append(atk.transform.DOMove(def.getOriginPos(), 0.4f)).SetEase(Ease.InSine)
            //�����ڰ� 0.4�ʸ��� ����� ��ġ�� �̵�
            .AppendCallback(
                () =>
                {//���� ����� ���
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
            //�����ڰ� 0.4�ʸ��� ������ ��ġ�� �̵�
            .OnComplete(//���� ����
                () => {
                    //AttackCallback(attacker, defender);//���� ��ƼƼ ó��
                    //if (myEntities.Contains(attacker)//�����ڰ� ��Ƴ��Ұ�(���� ���� ��Ʈ���� ����)
                    //    && !attacker.isBorder)//��谡 �ƴ϶��
                    //    attacker.locked = true;//���� �Ǵ�
                }
            );
    }
    //���ݰ��ɿ��� �ʱ�ȭ
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
                    //������ �����
                }
                else
                {
                    item.setLock(false);
                    item.setAttackable(true);
                }
            }
        }
    }
    #region ���콺 ����
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
