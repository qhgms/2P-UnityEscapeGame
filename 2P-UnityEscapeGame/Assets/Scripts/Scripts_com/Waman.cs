﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waman : MonoBehaviour
{
    float speed = 20;

    float hAxis;
    float vAxis;

    Vector3 moveVec;
    Vector3 preVec;

    Animator anim;
    Rigidbody rigid;

    bool isSwap; // 스왑할땐 아무런 뭣도 안하도록 함.
    bool isBump;
    bool isJump;
    bool jDown;

    // 무기 부분
    public GameObject weapon; // 이게 손에 들려있는 가려진 무기
    Weapon equipWeapon; // 무기의 스크립트를 가져오겠다는 거임.

    bool AttackDown; // 공격키
    GameObject nearObject;

    bool isFireReady = true;
    float fireDelay;

    void Start()
    {

    }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
    }
    private void FixedUpdate()
    {
        FreezeRotation(); // 플레이어가 탄피나 그런거에 닿으면 회전을 하기 시작.. 그거 없애려고 해주는것임
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }
    void GetInput()
    {
        hAxis = Input.GetAxis("HorizontalW");
        vAxis = Input.GetAxis("VerticalW");
        jDown = Input.GetButtonDown("JumpW");

        AttackDown = Input.GetButtonDown("AttackW");
    }



    void Move()
    {
        if (isBump || isSwap)
        {
            return;
        }

        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isJump)
        {
            moveVec *= 0.5f;
        }

        if (moveVec != Vector3.zero)
        {
            preVec = moveVec;
        }

        transform.position += moveVec * speed * Time.deltaTime;

        anim.SetBool("isWalk", (moveVec != Vector3.zero));  // 속도가 0이 아니면 걸어라.
    }

    void Turn()
    {
        // 가는 방향 보기.
        transform.LookAt(transform.position + moveVec);
    }
    void Jump()
    {
        // 점프 키 눌렀을 때 아이템 있으면 아이템 먹음.
        if (jDown)
        {
            if (nearObject != null)
            {
                // 아이템 먹기
                Destroy(nearObject);

                equipWeapon = weapon.GetComponent<Weapon>();
                equipWeapon.gameObject.SetActive(true);
                equipWeapon.init();

                anim.SetTrigger("Swap");
                isSwap = true;

                Invoke("SwapOut", 0.5f);
            }
            else if (!isJump)
            {
                // 점프는 그냥 위로 속도주기.
                rigid.AddForce(Vector3.up * 50, ForceMode.Impulse);

                //anim.SetBool("isJump", true);
                anim.SetTrigger("Jump");
                isJump = true;
            }
        }
    }

    void SwapOut()
    {
        //speed *= 0.5f;
        isSwap = false;
    }

    void Attack()
    {
        if (equipWeapon == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay; // 공격속도보다 파이어딜레이가 크면 된다고..?

        if (AttackDown && isFireReady)
        {
            equipWeapon.Use();
            anim.SetTrigger("Swing");
            fireDelay = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Weapon:
                    nearObject = other.gameObject;
                    break;

                case Item.Type.Coin:
                    Destroy(other.gameObject);
                    this.transform.localScale *= 2;
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Weapon:
                    nearObject = null;
                    break;

            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // 플레이어끼리 부딪히면 튕기기 애니메이션
            Bump();
        }

        // 바닥 닿으면 다시 점프 가능상태로 바꿔주기.
        if (collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Box")
        {
            isJump = false;
        }
    }

    void Bump()
    {
        anim.SetTrigger("Bump");
        isBump = true;
        transform.position += preVec * -7;

        Invoke("BumpOut", 1.5f);
    }

    void BumpOut()
    {
        isBump = false;
    }


    //void OnGUI()
    //{
    //    //무슨 키 입력했는지 알려주는 코드.
    //    Event e = Event.current;
    //    if (e.isKey)
    //    {
    //        Debug.Log("Detected a keyboard event!" + e.keyCode);
    //    }

    //}
}
