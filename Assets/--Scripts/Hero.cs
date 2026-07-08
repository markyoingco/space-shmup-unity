using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S{get; private set;} //Singleton property

    [Header("Insscribed")]

    //These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Dynamic")] [Range(0,4)]
    private float _shieldLevel = 1;
    [Tooltip("This field holds a refernce to the last triggering GameObject")]
    private GameObject lastTriggerGo = null;
    public delegate void WeaponFireDelegate();
    public event WeaponFireDelegate fireEvent;

    void Awake(){
        if(S == null){
            S = this; // Set the Singeleton only if it's null
        } else {
            Debug.LogError("Hero.Awake() - Attempting to assign second Hero.S!");
        }

        ClearWeapons();
        weapons[0].SetType(eWeaponType.blaster);
    }

    // Update is called once per frame
    void Update()
    {
        //Pull in information from the Input class
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        //Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis * speed * Time.deltaTime;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(vAxis*pitchMult, hAxis*rollMult, 0);

        //Rotate the ship to make it feel more dynamic
        if(Input.GetKey(KeyCode.Space) && fireEvent != null){
            fireEvent();
        }
    }

    void OnTriggerEnter(Collider other){
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;

        if(go == lastTriggerGo) return;
        lastTriggerGo = go;

        Enemy enemy = go.GetComponent<Enemy>();
        PowerUp pUp = go.GetComponent<PowerUp>();
        if(enemy != null){
            shieldLevel--;
            Destroy(go);
        } else if (pUp != null){
            AbsorbPowerUp(pUp);
        } else {
            Debug.LogWarning("Shield trigger hit by non-enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(PowerUp pUp){
        Debug.Log("Absorbed PowerUp: " + pUp.type);
        switch (pUp.type){
            case eWeaponType.shield:
                shieldLevel++;
                break;

            default:
                if (pUp.type == weapons[0].type){
                    Weapon weap = GetEmptyWeaponSlot();
                    if(weap != null){
                        weap.SetType(pUp.type);
                    }
                } else{
                    ClearWeapons();
                    weapons[0].SetType(pUp.type);
                }
                break;

        }
        pUp.AbsorbedBy(this.gameObject);
    }


    public float shieldLevel{
        get{return(_shieldLevel);}
        private set{
            _shieldLevel = Mathf.Min(value, 4);
            if(value < 0){
                Destroy(this.gameObject);
                Main.HERO_DIED();
            }
        }
    }

    Weapon GetEmptyWeaponSlot(){
        for( int i=0; i <weapons.Length; i++){
            if (weapons[i].type ==eWeaponType.none){
                return (weapons[i]);
            }
        }
        return null;
    }

    void ClearWeapons(){
        foreach(Weapon w in weapons){
            w.SetType(eWeaponType.none);
        }
    }

    }
