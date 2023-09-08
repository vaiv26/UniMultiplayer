using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable =  ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
	[SerializeField] private TMP_Text healthText;
	[SerializeField] private TMP_Text AmmoText;
	[SerializeField] private GameObject ui;
	[SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;
    [SerializeField] Item[] items;
    [SerializeField] private Item[] essentials;


    int itemIndex;
    int previousItemIndex = -1;
    private bool[] itemunlocked = { false, false, false };

    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;
    
	PhotonView PV;

	const float maxHealth = 100f;
	float currentHealth = maxHealth;

	private int[] maxmagazine = { 2,2,2 };
	private int[] maxammo = { 30, 20, 15 };
	private int[] ammo = { 30, 20, 15 };
	
	private Hashtable hash = new Hashtable();

	PlayerManager playerManager;

	private enum EssentialsAddition
	{
		increaseessentials,
		decreaseessentials,
		reload,

	}

	private EssentialsAddition state;



	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		PV = GetComponent<PhotonView>();

		playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			int randomValue = Random.Range(0, items.Length - 1);
			itemunlocked[randomValue] = true;
			EquipItem(randomValue);
			healthText.text = currentHealth.ToString();
		}
		else
		{
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(rb);
			Destroy(ui);
		}
	}

	void Update()
	{
		if(!PV.IsMine)
			return;

		Look();
		Move();
		Jump();
		
		if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
		{
			if(itemIndex >= items.Length - 1)
			{
				if(itemunlocked[0]) EquipItem(0);
			}
			else if(itemunlocked[itemIndex + 1])
			{
				EquipItem(itemIndex + 1);
			}
		}
		else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
		{
			if(itemIndex <= 0)
			{
				if(itemunlocked[(items.Length - 1)]) EquipItem(items.Length - 1);
			}
			else if(itemunlocked[itemIndex - 1])
			{
				EquipItem(itemIndex - 1);
			}
		}

		if(Input.GetMouseButtonDown(0) && ammo[itemIndex] > 0)
		{
			items[itemIndex].Use();
			ammo[itemIndex]--;
			SyncAmoText(itemIndex);
		}
		else if (ammo[itemIndex] == 0)
		{
			ReloadWeapon(itemIndex);
		}

		if(transform.position.y < -10f) // Die if you fall out of the world
		{
			Die();
		}
	}

	void Look()
	{
		transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

		verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

		cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
	}

	void Move()
	{
		Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

		moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
	}

	void Jump()
	{
		if(Input.GetKeyDown(KeyCode.Space) && grounded)
		{
			rb.AddForce(transform.up * jumpForce);
		}
	}

	void EquipItem(int _index)
	{
		if(_index == previousItemIndex)
			return;

		itemIndex = _index;

		items[itemIndex].itemGameObject.SetActive(true);

		if(previousItemIndex != -1)
		{
			items[previousItemIndex].itemGameObject.SetActive(false);
		}

		previousItemIndex = itemIndex;
		SyncAmoText(itemIndex);

		if(PV.IsMine)
		{
			if (hash.ContainsKey("itemIndex"))
			{
				hash["itemIndex"] = itemIndex;
			}
			else
			{
				hash.Add("itemIndex", itemIndex);
			}
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if(changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
		{
			EquipItem((int)changedProps["itemIndex"]);
		}
	}

	public void SetGroundedState(bool _grounded)
	{
		grounded = _grounded;
	}

	void FixedUpdate()
	{
		if(!PV.IsMine)
			return;

		rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
	}

	public void TakeHealth(float increaseHealth)
	{
		PV.RPC(nameof(RPC_TakeHealthAction), PV.Owner, EssentialsAddition.increaseessentials,increaseHealth);
	}
	public void TakeDamage(float damage)
	{
		PV.RPC(nameof(RPC_TakeHealthAction), PV.Owner, EssentialsAddition.decreaseessentials, damage);
	}

	[PunRPC]
	void RPC_TakeHealthAction(EssentialsAddition state ,float value,PhotonMessageInfo info)
	{
		switch (state)
		{
			case EssentialsAddition.decreaseessentials:
				currentHealth -= value;
				break;
			case EssentialsAddition.increaseessentials:
				currentHealth = Mathf.Clamp((currentHealth + value), 0, maxHealth);
				break;
		}

		healthText.text = currentHealth.ToString();

		if(currentHealth <= 0)
		{
			Die();
			Debug.Log("info.Sender:" + info.Sender);
			PlayerManager.Find(info.Sender).GetKill();
		}
	}

	void Die()
	{
		playerManager.Die();
	}

	void ReloadWeapon(int weaponIndex)
	{
		if (PV.IsMine && maxmagazine[weaponIndex] > 0)
		{
			PV.RPC(nameof(RPC_TogglingAmmo), PV.Owner, EssentialsAddition.reload,weaponIndex,maxammo[weaponIndex]);
			maxmagazine[weaponIndex]--;
		}
	}
	
	public void ToggleAmmo(int index, int valueToAdd)
	{
		PV.RPC(nameof(RPC_TogglingAmmo), PV.Owner, EssentialsAddition.increaseessentials,index,valueToAdd);
	}
	
	[PunRPC]
	void RPC_TogglingAmmo(EssentialsAddition state, int index, int maxValue)
	{
		switch (state)
		{
			case EssentialsAddition.reload:
				ammo[index] = maxValue;
				SyncAmoText(index);
				break;
			case EssentialsAddition.increaseessentials:
				ammo[index - 1] = Mathf.Clamp(ammo[index - 1] + maxValue, 0, maxammo[index - 1]);
				SyncAmoText(index - 1);
				break;
		}
		
	}

	void SyncAmoText(int weaponIndex)
	{
		if (PV.IsMine)
		{
			AmmoText.text = ammo[weaponIndex].ToString();
		}
	}

	public void AttemptToTakeItem(GameObject pickekObject,int itemSystem,int value)// item System: 0 is Weapons 1 is essentials
	{
		if(!PV.IsMine)
			return;
		if (itemSystem == 0)// itemSystem: all weapons
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] != null && items[i].name == pickekObject.name && !itemunlocked[i])
				{
					Debug.Log("Item Added");
					itemunlocked[i] = true;
					break;
				}
			}
		}
		else if (itemSystem == 1)// Health Pacck and ammunition
		{
			for (int i = 0; i < essentials.Length; i++)
			{
				if (essentials[i] != null && essentials[i].name == pickekObject.name && i == 0)
				{
					TakeHealth(value);// Health
				}
				else if(essentials[i] != null && essentials[i].name == pickekObject.name)
				{
					ToggleAmmo(i,value);// Ammunition
				}
			}
		}
	}
	
}
