using CustomRoom;
using GameMessages;
using LightUI;
using LightUtility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

internal class CreateRoomUI
{
	public UIDataBinder m_Host;

	public InputField m_RoomNameInputField;

	public ToggleGroup m_PlayModeLayout;

	public ToggleGroup m_MapLayout;

	public ToggleGroup m_PwdLayout;

	public InputField m_PwdInputField;

	public Slider m_GradeLimitSlider;

	public ToggleGroup m_ModeLayout;

	public Slider m_ThiefSlider;

	public Slider m_PoliceSlider;

	public Button m_CreateButton;

	public GameObject m_CampDetails;

	public GameObject m_ThiefPowerParams;

	public GameObject m_PolicePowerParams;

	public Text m_GradeName;

	public Text m_ThiefCountText;

	public Text m_PoliceCountText;

	public UIScrollRect m_ScrollView;

	public GameObject m_NewMapToggle;

	public ToggleGroup m_Judge;

	public ToggleGroup m_Chat;

	public GameObject m_PoliceLayout;

	public ToggleGroup m_WitnessPermit;

	public Slider m_RageStartTimeSlider;

	public Text m_RageStartTimeValue;

	private int MaxPeopleCount = 15;

	private static List<CustomParamID> OrderedParamIDs;

	private static RoomConfig m_RoomConfig;

	private static Delegates.ObjectCallback2<RoleType, CustomParamID> CustomParamChanged;

	public void Bind(CommonDataCollection args)
	{
		m_RoomConfig = (RoomConfig)args["roomConfig"].val;
		OrderedParamIDs = CustomRoomUtility.OrderedParamIDs;
		CustomParamChanged = CustomParamChangedBySlider;
		m_Host.EventProxy(m_CreateButton, "OnCreateRoom");
		int id = LocalResources.GradeTable.First.Id;
		int id2 = LocalResources.GradeTable.Last.Id;
		GradeMappingInfo gradeMapping = LocalResources.GetGradeMapping(m_RoomConfig.grade);
		RoleType roleType = (m_RoomConfig.Map() == MapType.TypeBoss) ? RoleType.Boss : RoleType.Police;
		SetSliderValue(m_GradeLimitSlider, id, id2, gradeMapping.GradeId, OnGradeLimitChanged);
		MapType mapType = m_RoomConfig.Map();
		if (mapType == MapType.TypeBattleRoyale)
		{
			MaxPeopleCount = 23;
			SetSliderValue(m_ThiefSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.thiefCnt, OnThiefCountChanged);
			SetSliderValue(m_PoliceSlider, 0f, 1f, 0f, OnPoliceCountChanged);
		}
		else
		{
			MaxPeopleCount = 15;
			SetSliderValue(m_ThiefSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.thiefCnt, OnThiefCountChanged);
			SetSliderValue(m_PoliceSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.policeCnt, OnPoliceCountChanged);
		}
		SetValue(m_MapLayout, m_RoomConfig.map);
		SetValue(m_PlayModeLayout, (m_RoomConfig.Map() == MapType.TypeBoss) ? 1 : 0);
		SetValue(m_ModeLayout, (m_RoomConfig.Mode() != 0) ? 1 : 0);
		SetValue(m_PwdLayout, (!string.IsNullOrEmpty(m_RoomConfig.pwd)) ? 1 : 0);
		int toggleLevel = GetToggleLevel(m_RoomConfig, RoleType.Thief);
		SetValue(m_ThiefPowerParams.GetComponentInChildren<ToggleGroup>(includeInactive: true), toggleLevel);
		toggleLevel = GetToggleLevel(m_RoomConfig, roleType);
		SetValue(m_PolicePowerParams.GetComponentInChildren<ToggleGroup>(includeInactive: true), toggleLevel);
		SetValue(m_Judge, (!m_RoomConfig.judge) ? 1 : 0);
		SetValue(m_Chat, (!m_RoomConfig.chatClose) ? 1 : 0);
		SetValue(m_WitnessPermit, (!m_RoomConfig.canWitness) ? 1 : 0);
		m_Chat.gameObject.SetActive(m_RoomConfig.judge);
		m_RageStartTimeSlider.transform.parent.gameObject.SetActive(m_RoomConfig.Mode() == RoomMode.Custom && m_RoomConfig.Map() != MapType.TypeBattleRoyale && m_RoomConfig.Map() != MapType.TypeBattleRoyaleTeam && m_RoomConfig.Map() != MapType.TypeBoss);
		int num = m_RoomConfig.rageStartTime / 60;
		m_RageStartTimeSlider.value = num;
		m_RageStartTimeValue.text = num + Localization.Minute;
		DoPlayerModeChanged(m_PlayModeLayout.GetToggleGroupValue());
		OnPwdLayoutChanged(isOn: true);
		OnModeChanged(isOn: true);
		CustomParamsToggleChanged(m_ThiefPowerParams, RoleType.Thief, justSetSlider: true);
		CustomParamsToggleChanged(m_PolicePowerParams, roleType, justSetSlider: true);
		m_RoomNameInputField.text = m_RoomConfig.name;
		m_RoomNameInputField.GetComponent<SensitiveWordFilter>().SetMaxInputCount(8);
		m_PwdInputField.text = m_RoomConfig.pwd;
		m_PwdInputField.gameObject.SetActive(m_PwdLayout.GetToggleGroupValue() == 1);
		SetListener(m_MapLayout, OnMapChanged);
		SetListener(m_PlayModeLayout, OnPlayerModeChanged);
		SetListener(m_PwdLayout, OnPwdLayoutChanged);
		SetListener(m_ModeLayout, OnModeChanged);
		SetListener(m_ThiefPowerParams.GetComponentInChildren<ToggleGroup>(includeInactive: true), OnThiefPowerParamsChanged);
		SetListener(m_PolicePowerParams.GetComponentInChildren<ToggleGroup>(includeInactive: true), OnPolicePowerParamsChanged);
		SetListener(m_Judge, OnJudgeChanged);
		m_ScrollView.ScrollToStart(immediately: true);
	}

	private void OnThiefCountChanged(float count)
	{
		if (m_ThiefSlider.value + m_PoliceSlider.value > (float)MaxPeopleCount)
		{
			m_PoliceSlider.value = (int)((float)MaxPeopleCount - m_ThiefSlider.value);
		}
		m_ThiefCountText.text = ((int)count).ToString();
	}

	private void OnPoliceCountChanged(float count)
	{
		if (m_ThiefSlider.value + m_PoliceSlider.value > (float)MaxPeopleCount)
		{
			m_ThiefSlider.value = (int)((float)MaxPeopleCount - m_PoliceSlider.value);
		}
		m_PoliceCountText.text = ((int)count).ToString();
	}

	private void OnGradeLimitChanged(float grade)
	{
		int id = (int)grade;
		m_GradeName.text = LocalResources.GradeTable.Get(id).Name;
	}

	private void OnMapChanged(bool isOn)
	{
		if (isOn)
		{
			MapType mapTypeByToggleGroupValue = GetMapTypeByToggleGroupValue();
			DoMapChanged(mapTypeByToggleGroupValue);
		}
	}

	private void DoMapChanged(MapType map)
	{
		switch (map)
		{
		case MapType.Type4v1:
			m_ThiefSlider.value = 4f;
			m_PoliceSlider.value = 1f;
			break;
		case MapType.Type8v2:
		case MapType.TypeAmusementPark:
		case MapType.TypeNew8v2:
			m_ThiefSlider.value = 8f;
			m_PoliceSlider.value = 2f;
			break;
		case MapType.TypeBoss:
			m_ThiefSlider.value = 5f;
			m_PoliceSlider.value = 1f;
			break;
		case MapType.TypeBattleRoyale:
			m_ThiefSlider.value = 15f;
			m_PoliceSlider.value = 0f;
			break;
		}
		SetDefaultTimeSetting();
	}

	private void OnPlayerModeChanged(bool isOn)
	{
		if (isOn)
		{
			int toggleGroupValue = m_PlayModeLayout.GetToggleGroupValue();
			DoPlayerModeChanged(toggleGroupValue);
			DoMapChanged(GetMapTypeByPlayModeValue(toggleGroupValue));
		}
	}

	private void DoPlayerModeChanged(int playMode)
	{
		Text componentInChildren = m_PoliceSlider.transform.parent.GetComponentInChildren<Text>(includeInactive: true);
		UIStateItem componentInChildren2 = m_PolicePowerParams.GetComponentInChildren<UIStateItem>(includeInactive: true);
		switch (playMode)
		{
		case 0:
			m_MapLayout.gameObject.SetActive(value: true);
			componentInChildren.text = RoleType.Police.GetRoleTypeInfo().Name;
			if (componentInChildren2 != null)
			{
				componentInChildren2.State = 0;
			}
			m_PolicePowerParams.SetActive(value: true);
			SetValue(m_PolicePowerParams.GetComponentInChildren<ToggleGroup>(includeInactive: true), GetToggleLevel(m_RoomConfig, RoleType.Police));
			CustomParamsToggleChanged(m_ThiefPowerParams, RoleType.Thief);
			CustomParamsToggleChanged(m_PolicePowerParams, RoleType.Police, justSetSlider: true);
			m_PoliceLayout.SetActive(value: true);
			MaxPeopleCount = 15;
			SetSliderValue(m_ThiefSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.thiefCnt, OnThiefCountChanged);
			SetSliderValue(m_PoliceSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.policeCnt, OnPoliceCountChanged);
			break;
		case 1:
			m_MapLayout.gameObject.SetActive(value: false);
			componentInChildren.text = RoleType.Boss.GetRoleTypeInfo().Name;
			if (componentInChildren2 != null)
			{
				componentInChildren2.State = 1;
			}
			m_PolicePowerParams.SetActive(value: true);
			SetValue(m_PolicePowerParams.GetComponentInChildren<ToggleGroup>(includeInactive: true), GetToggleLevel(m_RoomConfig, RoleType.Boss));
			CustomParamsToggleChanged(m_ThiefPowerParams, RoleType.Thief);
			CustomParamsToggleChanged(m_PolicePowerParams, RoleType.Boss, justSetSlider: true);
			m_PoliceLayout.SetActive(value: true);
			MaxPeopleCount = 15;
			SetSliderValue(m_ThiefSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.thiefCnt, OnThiefCountChanged);
			SetSliderValue(m_PoliceSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.policeCnt, OnPoliceCountChanged);
			break;
		case 2:
			m_MapLayout.gameObject.SetActive(value: false);
			m_PolicePowerParams.SetActive(value: false);
			m_PoliceLayout.SetActive(value: false);
			CustomParamsToggleChanged(m_ThiefPowerParams, RoleType.Thief);
			MaxPeopleCount = 23;
			SetSliderValue(m_ThiefSlider, 1f, MaxPeopleCount - 3, m_RoomConfig.thiefCnt, OnThiefCountChanged);
			SetSliderValue(m_PoliceSlider, 0f, 1f, 0f, OnPoliceCountChanged);
			break;
		}
	}

	private void OnPwdLayoutChanged(bool isOn)
	{
		if (isOn)
		{
			m_PwdInputField.gameObject.SetActive(m_PwdLayout.GetToggleGroupValue() == 1);
		}
	}

	private void OnModeChanged(bool isOn)
	{
		if (isOn)
		{
			bool active = m_ModeLayout.GetToggleGroupValue() == 0;
			m_CampDetails.SetActive(active);
			m_ThiefPowerParams.SetActive(active);
			m_PolicePowerParams.SetActive(active);
			SetDefaultTimeSetting();
		}
	}

	private void OnThiefPowerParamsChanged(bool isOn)
	{
		if (isOn)
		{
			CustomParamsToggleChanged(m_ThiefPowerParams, RoleType.Thief);
		}
	}

	private void OnPolicePowerParamsChanged(bool isOn)
	{
		if (isOn)
		{
			RoleType roleType = (m_PlayModeLayout.GetToggleGroupValue() != 0) ? RoleType.Boss : RoleType.Police;
			CustomParamsToggleChanged(m_PolicePowerParams, roleType);
		}
	}

	private void OnJudgeChanged(bool isOn)
	{
		if (isOn)
		{
			m_Chat.gameObject.SetActive(m_Judge.GetToggleGroupValue() == 0);
		}
	}

	private void CustomParamChangedBySlider(RoleType roleType, CustomParamID paramID)
	{
		ToggleGroup componentInChildren = ((roleType == RoleType.Thief) ? m_ThiefPowerParams : m_PolicePowerParams).GetComponentInChildren<ToggleGroup>(includeInactive: true);
		SetValue(componentInChildren, 3);
	}

	private void OnRageStartTimeChange(float minute)
	{
		int num = (int)minute;
		m_RageStartTimeValue.text = num + Localization.Minute;
		m_RoomConfig.rageStartTime = ((m_RoomConfig.map == 3 || m_RoomConfig.map == 5 || m_RoomConfig.map == 6) ? (num * 60) : 0);
	}

	public void OnCreateRoom()
	{
		string text = m_RoomNameInputField.text;
		if (!CustomRoomUtility.ValidateName(text))
		{
			return;
		}
		if (m_PwdInputField.gameObject.activeInHierarchy && string.IsNullOrEmpty(m_PwdInputField.text))
		{
			UILobby.Current.ShowTips(Localization.TipsInvalidPwd);
			return;
		}
		switch (m_PlayModeLayout.GetToggleGroupValue())
		{
		case 0:
			m_RoomConfig.map = (int)GetMapTypeByToggleGroupValue();
			m_RoomConfig.policeCnt = (int)m_PoliceSlider.value;
			break;
		case 1:
			m_RoomConfig.map = 3;
			m_RoomConfig.policeCnt = (int)m_PoliceSlider.value;
			break;
		case 2:
			m_RoomConfig.map = 5;
			m_RoomConfig.policeCnt = 0;
			break;
		}
		int gradeID = (int)m_GradeLimitSlider.value;
		int id = LocalResources.GradeMappingTable.Find((GradeMappingInfo x) => x.GradeId == gradeID).Id;
		m_RoomConfig.name = text;
		m_RoomConfig.grade = id;
		m_RoomConfig.pwd = (m_PwdInputField.gameObject.activeInHierarchy ? m_PwdInputField.text : string.Empty);
		m_RoomConfig.mode = m_ModeLayout.GetToggleGroupValue();
		m_RoomConfig.thiefCnt = (int)m_ThiefSlider.value;
		m_RoomConfig.judge = (m_Judge.GetToggleGroupValue() == 0);
		m_RoomConfig.canWitness = (m_WitnessPermit.GetToggleGroupValue() == 0);
		m_RoomConfig.judgeCnt = (m_RoomConfig.judge ? 4 : 4);
		m_RoomConfig.chatClose = (m_RoomConfig.judge && m_Chat.GetToggleGroupValue() == 0);
		if (m_RoomConfig.Mode() == RoomMode.Classic)
		{
			SetClassicMode();
		}
		UILobby.Current.ShowWait(immediately: true);
		CustomRoomUtility.CreatCustomRoom(m_RoomConfig, delegate(HttpResponseCreateRoom response)
		{
			UILobby.Current.GoBack();
			CustomRoomUtility.CachedRoomConfig = m_RoomConfig;
			GameRuntime.SetRoomMapAndConfig(m_RoomConfig.Map(), GameMode.CustomRoom, m_RoomConfig.Copy());
			if (m_RoomConfig.policeCnt == 0 && GameRuntime.PlayingRole == RoleType.Police)
			{
				CharacterUtility.SetActiveCharacter(LocalPlayerDatabase.PlayerInfo.activeCharacterID[1]);
			}
			EnterRoom(response);
		});
	}

	private void EnterRoom(HttpResponseCreateRoom createRoom)
	{
		GameRuntime.IsJudge = GameRuntime.CurrentRoom.config.judge;
		TeamRoomManager.Inst.CustomConnectToTeamServer(createRoom, delegate
		{
			TeamRoomManager.Inst.ActiveOpenRoomUI();
		});
	}

	private void SetClassicMode()
	{
		switch (m_RoomConfig.Map())
		{
		case MapType.Type4v1:
			m_RoomConfig.thiefCnt = 4;
			m_RoomConfig.policeCnt = 1;
			break;
		case MapType.Type8v2:
		case MapType.TypeAmusementPark:
		case MapType.TypeNew8v2:
			m_RoomConfig.thiefCnt = 8;
			m_RoomConfig.policeCnt = 2;
			break;
		case MapType.TypeBoss:
			m_RoomConfig.thiefCnt = 6;
			m_RoomConfig.policeCnt = 0;
			break;
		case MapType.TypeBattleRoyale:
			m_RoomConfig.thiefCnt = 21;
			m_RoomConfig.policeCnt = 0;
			break;
		}
	}

	private void SetDefaultTimeSetting()
	{
		bool num = m_ModeLayout.GetToggleGroupValue() == 0;
		MapType map = GetMapTypeByPlayModeValue(m_PlayModeLayout.GetToggleGroupValue());
		if (num && map != MapType.TypeBattleRoyale && map != MapType.TypeBattleRoyaleTeam && map != MapType.TypeBoss)
		{
			m_RageStartTimeSlider.transform.parent.gameObject.SetActive(value: true);
			int num2 = LocalResources.MapTypeInfo.Find((MapTypeInfo x) => x.MapType == map).RageStartTime / 60;
			SetSliderValue(m_RageStartTimeSlider, num2, num2 + 10, num2, OnRageStartTimeChange);
		}
		else
		{
			m_RageStartTimeSlider.transform.parent.gameObject.SetActive(value: false);
		}
	}

	private void CustomParamsToggleChanged(GameObject layout, RoleType roleType, bool justSetSlider = false)
	{
		UITemplateInitiator componentInChildren = layout.GetComponentInChildren<UITemplateInitiator>(includeInactive: true);
		int toggleGroupValue = layout.GetComponentInChildren<ToggleGroup>(includeInactive: true).GetToggleGroupValue();
		if (!justSetSlider)
		{
			foreach (CustomParamID orderedParamID in OrderedParamIDs)
			{
				RoomCustomConfigInfo roomCustomConfig = LocalResources.GetRoomCustomConfig(orderedParamID, roleType);
				if (roomCustomConfig != null && roomCustomConfig.Enabled)
				{
					int num = -1;
					switch (toggleGroupValue)
					{
					case 0:
						num = 0;
						break;
					case 1:
						num = roomCustomConfig.DefaultLevelIndex;
						break;
					case 2:
						num = roomCustomConfig.Levels.Length - 1;
						break;
					}
					if (num >= 0)
					{
						if (roomCustomConfig.ParamID == CustomParamID.TalentEffect && m_PlayModeLayout.GetToggleGroupValue() == 2)
						{
							SetParams(roleType, roomCustomConfig.ParamID, 0, invokeChanged: false);
						}
						else
						{
							SetParams(roleType, roomCustomConfig.ParamID, num, invokeChanged: false);
						}
					}
				}
			}
		}
		CommonDataCollection commonDataCollection = new CommonDataCollection();
		foreach (CustomParamID pID in OrderedParamIDs)
		{
			RoomCustomConfigInfo roomCustomConfig2 = LocalResources.GetRoomCustomConfig(pID, roleType);
			if (roomCustomConfig2 != null && roomCustomConfig2.Enabled && (m_PlayModeLayout.GetToggleGroupValue() != 2 || (roomCustomConfig2.ParamID != CustomParamID.TalentEffect && roomCustomConfig2.ParamID != CustomParamID.CoinGain)))
			{
				int val = m_RoomConfig.GetParams(roleType).Find((CustomParam x) => x.id == (int)pID).val;
				CommonDataCollection commonDataCollection2 = new CommonDataCollection();
				commonDataCollection2["paramID"] = (int)roomCustomConfig2.ParamID;
				commonDataCollection2["roleType"] = (int)roleType;
				commonDataCollection2["level"] = val;
				commonDataCollection[commonDataCollection.ArraySize] = commonDataCollection2;
			}
		}
		componentInChildren.Args = commonDataCollection;
	}

	private void SetValue(ToggleGroup toggleGroup, int value)
	{
		Toggle[] componentsInChildren = toggleGroup.transform.GetComponentsInChildren<Toggle>(includeInactive: false);
		value = Mathf.Clamp(value, 0, componentsInChildren.Length - 1);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].isOn = (i == value);
		}
	}

	private void SetListener(ToggleGroup toggleGroup, UnityAction<bool> onValueChanged)
	{
		ToggleGroupUtility.SetListener(toggleGroup, onValueChanged);
	}

	private void SetSliderValue(Slider slider, float minValue, float maxValue, float value, UnityAction<float> listener)
	{
		slider.onValueChanged.RemoveAllListeners();
		slider.onValueChanged.AddListener(listener);
		slider.minValue = minValue;
		slider.maxValue = maxValue;
		slider.value = value;
	}

	private int GetToggleLevel(RoomConfig roomConfig, RoleType roleType)
	{
		CustomParam[] @params = roomConfig.GetParams(roleType);
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		foreach (CustomParam customParam in @params)
		{
			RoomCustomConfigInfo roomCustomConfig = LocalResources.GetRoomCustomConfig((CustomParamID)customParam.id, roleType);
			if (roomCustomConfig != null && roomCustomConfig.Enabled)
			{
				int defaultLevelIndex = roomCustomConfig.DefaultLevelIndex;
				int num = roomCustomConfig.Levels.Length - 1;
				if (customParam.val != 0)
				{
					flag = false;
				}
				if (customParam.val != num)
				{
					flag3 = false;
				}
				if (customParam.val != defaultLevelIndex)
				{
					flag2 = false;
				}
			}
		}
		if (flag)
		{
			return 0;
		}
		if (flag2)
		{
			return 1;
		}
		if (flag3)
		{
			return 2;
		}
		return 3;
	}

	private MapType GetMapTypeByToggleGroupValue()
	{
		int toggleGroupValue = m_MapLayout.GetToggleGroupValue();
		if (toggleGroupValue <= 2)
		{
			return (MapType)toggleGroupValue;
		}
		return (MapType)(toggleGroupValue + 1);
	}

	private MapType GetMapTypeByPlayModeValue(int playMode)
	{
		switch (playMode)
		{
		case 1:
			return MapType.TypeBoss;
		case 2:
			return MapType.TypeBattleRoyale;
		default:
			return GetMapTypeByToggleGroupValue();
		}
	}

	public static void SetParams(RoleType roleType, CustomParamID paramID, int value, bool invokeChanged = true)
	{
		CustomParam[] @params = m_RoomConfig.GetParams(roleType);
		int num = 0;
		CustomParam customParam;
		while (true)
		{
			if (num < @params.Length)
			{
				customParam = @params[num];
				if (customParam.id == (int)paramID)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		if (customParam.val != value)
		{
			customParam.val = value;
			if (CustomParamChanged != null && invokeChanged)
			{
				CustomParamChanged(roleType, paramID);
			}
		}
	}
}
