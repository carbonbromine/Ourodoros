using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;

public enum Direction
{
    Left,Right,Up,Down,NULL,
}
public class CharacterMove : MonoBehaviour
{
    [Header("移动相关")]
    [SerializeField]private bool moveable = false;
    [SerializeField]private bool isMoving = false;
    [SerializeField]private Direction LastDirection = Direction.NULL;
    [SerializeField]private Direction CurDirection = Direction.NULL;
    [SerializeField]private float Fluency = 0.02f;
    [SerializeField] private int speed = 1;
    [Header("颜色(0为黑色，1为白色)")]
    public int BodyColor = 0;
    [Header("死亡效果")]
    [SerializeField] private bool isDead = false;
    private float DeadTimer = 0f;
    [SerializeField] private float DeadTime = 1.5f;
    [Header("资源")]
    [SerializeField] GameObject BlackBody;
    [SerializeField] GameObject WhiteBody;
    public List<GameObject> Body = new List<GameObject>();

    public bool Moveable { get { return moveable; } set { moveable = value;if (!moveable) { isMoving = false; } } }
    public bool IsMoving
    {
        get { return isMoving; }
    }

    void Move()
    {
        Vector3 MoveDir = (CurDirection == Direction.Left)? Vector3.left : (CurDirection == Direction.Right)? Vector3.right:(CurDirection == Direction.Up)?Vector3.forward:Vector3.back;
        transform.position += MoveDir * Fluency*speed;
        GameObject temp = (BodyColor == 0)?BlackBody : WhiteBody;
        Vector3 Pos = transform.position;
        Pos -= transform.localScale.x/2 * MoveDir;
        for (int i = 0; i < speed; i++)
        {
            GameObject t = Instantiate(temp, Pos-MoveDir*Fluency*(2*i+1)/2,Quaternion.identity ) as GameObject;
            t.transform.localScale = new Vector3((MoveDir.x==0)?transform.localScale.x:Mathf.Abs(MoveDir.x)*Fluency,transform.localScale.y, (MoveDir.z == 0) ? transform.localScale.z : Mathf.Abs(MoveDir.z) * Fluency);
            Body.Add(t);
            if(BodyColor == 0)
            {
                GameObject p = GameObject.Find("WhiteBodyCollection");
                if (p==null)
                {
                    p = new GameObject("WhiteBodyCollection");
                }
                t.transform.SetParent(p.transform);
            }
            else
            {
                GameObject p = GameObject.Find("BlackBodyCollection");
                if (p == null)
                {
                    p = new GameObject("BlackBodyCollection");
                }
                t.transform.SetParent(p.transform);
            }

        }
    }
    public void Died()
    {
        isMoving = false ;
        moveable = false ;
        GetComponent<MeshRenderer>().material = new Material(GameManager.Instance.DiedMaterial);
        gameObject.AddComponent<BodyDestory>();
        if (BodyColor == 0) { GameManager.Instance.BlackSnake = null; }
        else { GameManager.Instance.WhiteSnake = null; }
        isDead = true;
    }
    Direction GetDirection()
    {
        Direction direction = Direction.NULL;
        if (Input.GetKeyDown(GameManager.Up)) { direction = Direction.Up; }
        if (Input.GetKeyDown(GameManager.Down)) { direction = Direction.Down; }
        if (Input.GetKeyDown(GameManager.Left)) { direction = Direction.Left; }
        if (Input.GetKeyDown(GameManager.Right)) { direction = Direction.Right; }
        return direction;
    }
    Direction GetBackDir(Direction d)
    {
        Direction dir = Direction.NULL;
        if(d == Direction.Left) { dir = Direction.Right;}
        if(d == Direction.Right) { dir = Direction.Left; }
        if(d == Direction.Up) {dir = Direction.Down;}
        if(d == Direction.Down) {dir = Direction.Up;}
        return dir;
    }
    public void Arrive()
    {
        moveable = false;
        isMoving = false ;
        if (BodyColor == 0)
        {
            GameManager.Instance.BlackArrive = true;
        }
        else
        {
            GameManager.Instance.WhiteArrive = true;
        }
        
    }
    private void Start()
    { 
        BlackBody = Resources.Load<GameObject>("Pre/BlackBody");
        WhiteBody = Resources.Load<GameObject>("Pre/WhiteBody");
        if (BodyColor == 0)
        {
            GameManager.Instance.BlackSnake = this;
        }
        else
        {
            GameManager.Instance.WhiteSnake = this;
        }
    }
    private void Update()
    {
        if (moveable&&!isMoving)
        {
            CurDirection = GetDirection();
            if(CurDirection!=Direction.NULL&&CurDirection != GetBackDir(LastDirection)&&CurDirection != LastDirection&&!isMoving)
            {
                isMoving = true;
                LastDirection = CurDirection;
            }
        }
        
    }
    private void FixedUpdate()
    {
        if (moveable && isMoving)
        {
            Move();
        }
        if (isDead)
        {
            Shader.SetGlobalFloat("unityTime", DeadTimer);
            /*
            for (int i = 0; i < SandFlySpeed; i++)
            {
                if (Sands >= Body.Count)
                {
                    break;
                }
                Body[Sands].transform.localScale = Vector3.one*transform.localScale.x;
                Body[Sands].GetComponent<MeshRenderer>().material = new Material(GameManager.Instance.DiedMaterial);
                Body[Sands].GetComponent<BodyDestory>().enabled = true;
                Body[Sands].GetComponent<MeshRenderer>().material.SetFloat("_ShaderSatrtTime", Time.time);
                Sands++;
            }
            */
            for(int i = 0;i<Body.Count;i+=(int)(transform.localScale.x/Fluency)/3)
            {
                Body[i].transform.localScale = Vector3.one * transform.localScale.x*UnityEngine.Random.Range(0.3f,0.9f);
                Body[i].GetComponent<MeshRenderer>().material = new Material(GameManager.Instance.DiedMaterial);
                Body[i].AddComponent<BodyDestory>();
                Body[i].GetComponent<MeshRenderer>().material.SetFloat("_ShaderSatrtTime", DeadTimer);
                Body.Remove(Body[i]);
            }
            for(int i = 0; i<Body.Count;i++)
            {
                Destroy(Body[i]);
            }
            Body.Clear();
            DeadTimer += Time.deltaTime;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Untouchable")
        {
            GameManager.Instance.GameOver();
        }
        if(other.tag == "Barrier")
        {
            isMoving = false;
            GameManager.Instance.ChangeSide(BodyColor);
        }
    }
}
