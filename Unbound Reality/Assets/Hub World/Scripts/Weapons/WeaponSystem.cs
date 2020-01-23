﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;

public class WeaponSystem : MonoBehaviourPun {

    // Private data
    private Weapon weapon;
    private int weaponLayer;

    void Awake()
    {
        weaponLayer = LayerMask.NameToLayer("Weapon");
    }

	// Update is called once per frame
	void Update () {
        
        if(this.photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        // If we have a weapon and the player right clicks, drop the weapon
        if (weapon!=null && Input.GetMouseButtonDown(1))
        {
            if(weapon.GetType() ==  typeof(Party))
            {
                PhotonNetwork.Destroy(weapon.gameObject.GetPhotonView());
            } else 
            {
                //Setting transform here sets it globally
                weapon.gameObject.transform.SetParent(null);
                weapon.gameObject.transform.position = weapon.spawn;

                photonView.RPC("DropWeapon", RpcTarget.AllBuffered, this.photonView.ViewID, weapon.gameObject.GetPhotonView().ViewID);
            }
        }

	}

    [PunRPC]
    public void DropWeapon(int playerViewID, int weaponViewID)
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        GameObject weaponObject = PhotonView.Find(weaponViewID).gameObject;

        if(player == null || weaponObject == null)
        {
            return;
        }

        Weapon wep = weaponObject.GetComponent<Weapon>();
        wep.BeingUsed = false;

        if(player == gameObject)
        {
            weapon = null;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(this.photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if(weapon == null && col.gameObject.layer == weaponLayer && col.gameObject.transform.parent == null)
        {
            weapon = col.gameObject.GetComponent<Weapon>();
            if(weapon.GetType() ==  typeof(Party))
            {
                GameObject weaponObj = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Party Object"), Vector3.zero, Quaternion.identity);
                weaponObj.transform.SetParent(transform);
                photonView.RPC("PickUpWeapon", RpcTarget.AllBuffered, this.photonView.ViewID, weaponObj.GetPhotonView().ViewID);
            } else
            {
                //Setting transform here sets it globally
                col.transform.SetParent(transform);

                photonView.RPC("PickUpWeapon", RpcTarget.AllBuffered, this.photonView.ViewID, col.gameObject.GetPhotonView().ViewID);
            }
            
        }

    }

    [PunRPC]
    public void PickUpWeapon(int playerViewID, int weaponViewID )
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        GameObject weaponObject = PhotonView.Find(weaponViewID).gameObject;

        if(player == null || weaponObject == null)
        {
            return;
        }

        Weapon wep = weaponObject.GetComponent<Weapon>();
        wep.BeingUsed = true;

        if(player == gameObject)
        {
            weapon = wep;
        }
    }

    //Anthony's Unity is f'd up
    [PunRPC]
    public void DestroyWeapon(int var1, int var2)
    {
        Debug.Log("Destroy Weapon Called! var1: " + var1  + " var2: " + var2);
    }

    // Get the Weapon variable
    public Weapon GetWeapon()
    {
        return weapon;
    }
}
