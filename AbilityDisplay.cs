using System;
using UnityEngine;
using UnityEngine.UI;
using WowStatConstants;
using WowStaticData;

public class AbilityDisplay : MonoBehaviour
{
	public Image m_abilityIcon;

	public Text m_abilityNameText;

	public Button m_mainButton;

	public Text m_iconErrorText;

	public GameObject m_counteredMechanicGroup;

	public Text m_counteredMechanicName;

	public Image m_counteredMechanicIcon;

	public Image m_canCounterMechanicIcon;

	public Image m_canCounterMechanicButBusyIcon;

	public Shader m_grayscaleShader;

	private bool m_isCountered;

	public GameObject m_redFailX;

	public FollowerDetailView m_followerDetailView;

	private int m_garrAbilityID;

	private int m_counteredGarrMechanicTypeID;

	private FollowerCanCounterMechanic m_canCounterStatus;

	public void SetCanCounterStatus(FollowerCanCounterMechanic canCounterStatus)
	{
		this.m_canCounterStatus = canCounterStatus;
		if (this.m_canCounterMechanicIcon == null || this.m_canCounterMechanicButBusyIcon == null)
		{
			return;
		}
		this.m_canCounterMechanicIcon.get_gameObject().SetActive(canCounterStatus == FollowerCanCounterMechanic.yesAndAvailable);
		this.m_canCounterMechanicButBusyIcon.get_gameObject().SetActive(canCounterStatus == FollowerCanCounterMechanic.yesButBusy);
	}

	public FollowerCanCounterMechanic GetCanCounterStatus()
	{
		return this.m_canCounterStatus;
	}

	public void SetCountered(bool isCountered, bool playCounteredEffect = true)
	{
		if (isCountered && this.m_isCountered)
		{
			return;
		}
		this.m_isCountered = isCountered;
		if (this.m_abilityIcon.get_material() != null)
		{
			this.m_abilityIcon.get_material().SetFloat("_GrayscaleAmount", (!this.m_isCountered) ? 0f : 1f);
		}
		if (this.m_isCountered && playCounteredEffect)
		{
			UiAnimMgr.instance.PlayAnim("RedFailX", base.get_transform(), Vector3.get_zero(), 0.8f, 0f);
		}
		if (this.m_redFailX != null)
		{
			this.m_redFailX.SetActive(this.m_isCountered);
		}
	}

	public void SetAbility(int garrAbilityID, bool hideCounterInfo = false, bool hideName = false, FollowerDetailView followerDetailView = null)
	{
		this.m_followerDetailView = followerDetailView;
		if (this.m_iconErrorText != null)
		{
			this.m_iconErrorText.get_gameObject().SetActive(false);
		}
		this.m_garrAbilityID = garrAbilityID;
		GarrAbilityRec record = StaticDB.garrAbilityDB.GetRecord(this.m_garrAbilityID);
		if (record == null)
		{
			Debug.LogWarning("Invalid garrAbilityID " + this.m_garrAbilityID);
			return;
		}
		this.m_abilityNameText.set_text(record.Name);
		if (record.IconFileDataID > 0)
		{
			Sprite sprite = GeneralHelpers.LoadIconAsset(AssetBundleType.Icons, record.IconFileDataID);
			if (sprite != null)
			{
				this.m_abilityIcon.set_sprite(sprite);
				if (this.m_grayscaleShader != null)
				{
					Material material = new Material(this.m_grayscaleShader);
					this.m_abilityIcon.set_material(material);
				}
			}
			else if (this.m_iconErrorText != null)
			{
				this.m_iconErrorText.get_gameObject().SetActive(true);
				this.m_iconErrorText.set_text(string.Empty + record.IconFileDataID);
			}
			this.m_abilityIcon.set_enabled(true);
		}
		else
		{
			this.m_abilityIcon.set_enabled(false);
		}
		this.m_garrAbilityID = record.ID;
		GarrAbilityCategoryRec record2 = StaticDB.garrAbilityCategoryDB.GetRecord((int)record.GarrAbilityCategoryID);
		if (record2 != null)
		{
			this.m_counteredMechanicName.set_text(record2.Name);
		}
		if (this.m_counteredMechanicGroup != null)
		{
			if (hideCounterInfo)
			{
				this.m_counteredMechanicGroup.SetActive(false);
			}
			else
			{
				this.m_counteredGarrMechanicTypeID = 0;
				StaticDB.garrAbilityEffectDB.EnumRecordsByParentID(record.ID, delegate(GarrAbilityEffectRec garrAbilityEffectRec)
				{
					if (garrAbilityEffectRec.GarrMechanicTypeID == 0u)
					{
						return true;
					}
					if (garrAbilityEffectRec.AbilityAction != 0u)
					{
						return true;
					}
					GarrMechanicTypeRec record3 = StaticDB.garrMechanicTypeDB.GetRecord((int)garrAbilityEffectRec.GarrMechanicTypeID);
					if (record3 == null)
					{
						return true;
					}
					Sprite sprite2 = GeneralHelpers.LoadIconAsset(AssetBundleType.Icons, record3.IconFileDataID);
					if (sprite2 != null)
					{
						this.m_counteredMechanicIcon.set_sprite(sprite2);
					}
					else
					{
						this.m_counteredMechanicName.set_text("ERR " + record3.IconFileDataID);
					}
					this.m_counteredGarrMechanicTypeID = record3.ID;
					return false;
				});
			}
		}
		this.SetCountered(false, true);
		if (this.m_counteredMechanicGroup != null)
		{
			this.m_counteredMechanicGroup.SetActive(this.m_counteredGarrMechanicTypeID > 0);
		}
		this.m_abilityNameText.get_gameObject().SetActive(!hideName);
	}

	public void ShowTooltip()
	{
		Main.instance.allPopups.ShowAbilityInfoPopup(this.m_garrAbilityID);
	}

	public void ShowEquipmentPopup()
	{
		this.ShowTooltip();
	}

	public int GetAbilityID()
	{
		return this.m_garrAbilityID;
	}

	public void ShowEquipmentDialog()
	{
		if (AllPopups.instance.GetCurrentFollowerDetailView() != this.m_followerDetailView)
		{
			return;
		}
		AllPopups.instance.ShowEquipmentDialog(this.m_garrAbilityID, this.m_followerDetailView, true);
	}
}
