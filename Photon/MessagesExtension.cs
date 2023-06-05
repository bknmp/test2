using LightUtility;
using System;

namespace GameMessages
{
	public static class MessagesExtension
	{
		public static bool IsExchanged(this ItemInfo info)
		{
			if (info != null && info.exID != null)
			{
				return info.exID.Length != 0;
			}
			return false;
		}

		public static string GetExchangeTips(this ItemInfo info, string tipsFormat, DropItem dropItem = null)
		{
			if (dropItem == null)
			{
				dropItem = LocalResources.DropItemTable.Get(info.itemID);
			}
			if (info.IsExchanged())
			{
				DropItem dropItem2 = LocalResources.DropItemTable.Get(info.exID[0]);
				string text = string.Format(tipsFormat, dropItem2.Name, info.exCnt[0]);
				if (info.exID.Length > 1)
				{
					for (int i = 1; i < info.exID.Length; i++)
					{
						dropItem2 = LocalResources.DropItemTable.Get(info.exID[i]);
						text += $",{dropItem2.Name}x{info.exCnt[i]}";
					}
				}
				return text;
			}
			return "";
		}

		public static bool InTime(this ActivityConditionWithDayTime info)
		{
			int now = UtcTimeStamp.Now;
			DateTime nowDateTime = UtcTimeStamp.NowDateTime;
			int num = nowDateTime.Hour * 3600 + nowDateTime.Minute * 60 + nowDateTime.Second;
			if (info.startTime < now && now < info.endTime)
			{
				for (int i = 0; i < info.startHours.Length; i++)
				{
					uint num2 = info.startHours[i] * 3600;
					uint num3 = info.endHours[i] * 3600;
					if (num2 <= num && num <= num3)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool WeekTimeMatch(this ActivityConditionWithDayTime info)
		{
			if (info.weekTime != null && info.weekTime.Length != 0)
			{
				return info.weekTime.Contains((int)UtcTimeStamp.NowDateTime.DayOfWeek);
			}
			return true;
		}

		public static int GetNearestTimeIdx(this ActivityConditionWithDayTime info)
		{
			DateTime nowDateTime = UtcTimeStamp.NowDateTime;
			for (int i = 0; i < info.startHours.Length; i++)
			{
				uint num = info.startHours[i];
				if (nowDateTime.Hour < num)
				{
					return i;
				}
			}
			return 0;
		}

		public static bool IsCustomRoom(this InviterInfo info)
		{
			if (info != null && info.config != null)
			{
				return !string.IsNullOrEmpty(info.config.name);
			}
			return false;
		}

		public static MapType Map(this RoomConfig roomConfig)
		{
			return (MapType)roomConfig.map;
		}

		public static RoomMode Mode(this RoomConfig roomConfig)
		{
			return (RoomMode)roomConfig.mode;
		}

		public static float GetValue(this CustomParam cParam, RoleType roleType)
		{
			return (float)LocalResources.GetRoomCustomConfig((CustomParamID)cParam.id, roleType).Levels[cParam.val] / 10f;
		}

		public static CustomParam[] GetParams(this RoomConfig roomConfig, RoleType roleType)
		{
			switch (roleType)
			{
			case RoleType.Boss:
				return roomConfig.bossParams;
			case RoleType.Police:
				return roomConfig.policeParams;
			default:
				return roomConfig.thiefParams;
			}
		}

		public static bool IsNull(this RoomInfo info)
		{
			if (info != null)
			{
				return info.ID == 0;
			}
			return true;
		}

		public static RoomConfig Copy(this RoomConfig roomConfig)
		{
			RoomConfig roomConfig2 = new RoomConfig();
			roomConfig2.name = roomConfig.name;
			roomConfig2.grade = roomConfig.grade;
			roomConfig2.map = roomConfig.map;
			roomConfig2.thiefCnt = roomConfig.thiefCnt;
			roomConfig2.policeCnt = roomConfig.policeCnt;
			roomConfig2.pwd = roomConfig.pwd;
			roomConfig2.mode = roomConfig.mode;
			roomConfig2.judge = roomConfig.judge;
			roomConfig2.chatClose = roomConfig.chatClose;
			CustomParam[] array = new CustomParam[roomConfig.thiefParams.Length];
			for (int i = 0; i < array.Length; i++)
			{
				CustomParam customParam = roomConfig.thiefParams[i];
				array[i] = new CustomParam();
				array[i].id = customParam.id;
				array[i].val = customParam.val;
			}
			CustomParam[] array2 = new CustomParam[roomConfig.policeParams.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				CustomParam customParam2 = roomConfig.policeParams[j];
				array2[j] = new CustomParam();
				array2[j].id = customParam2.id;
				array2[j].val = customParam2.val;
			}
			CustomParam[] array3 = new CustomParam[roomConfig.bossParams.Length];
			for (int k = 0; k < array3.Length; k++)
			{
				CustomParam customParam3 = roomConfig.bossParams[k];
				array3[k] = new CustomParam();
				array3[k].id = customParam3.id;
				array3[k].val = customParam3.val;
			}
			roomConfig2.thiefParams = array;
			roomConfig2.policeParams = array2;
			roomConfig2.bossParams = array3;
			return roomConfig2;
		}

		public static RoleTypeInfo GetRoleTypeInfo(this RoleType roleType)
		{
			foreach (RoleTypeInfo item in LocalResources.RoleTypeInfo)
			{
				if (item.RoleType == roleType)
				{
					return item;
				}
			}
			return null;
		}

		public static MapTypeInfo GetMapTypeInfo(this MapType mapType)
		{
			foreach (MapTypeInfo item in LocalResources.MapTypeInfo)
			{
				if (item.MapType == mapType)
				{
					return item;
				}
			}
			return null;
		}

		public static bool IsMySelf(this uint roleID)
		{
			if (LocalPlayerDatabase.LoginInfo != null)
			{
				return LocalPlayerDatabase.LoginInfo.roleID == roleID;
			}
			return false;
		}

		public static bool IsDropItemAppearance(int itemID)
		{
			return IsDropItemAppearance(LocalResources.DropItemTable.Get(itemID).Type);
		}

		public static bool IsDropItemAppearance(DropItemType type)
		{
			if (type != DropItemType.SkinPart && type != DropItemType.SkinSuite && type != DropItemType.CardSkin && type != DropItemType.CardStyle && type != DropItemType.HeadBox && type != DropItemType.BubbleBox && type != DropItemType.Lightness)
			{
				return type == DropItemType.IngameEmotion;
			}
			return true;
		}

		public static bool IsDropItemMoneyLike(this DropItem item)
		{
			return item.Type.IsDropItemMoneyLike();
		}

		public static bool IsDropItemMoneyLike(this DropItemType type)
		{
			switch (type)
			{
			case DropItemType.Gold:
			case DropItemType.Diamond:
			case DropItemType.ColoringAgent:
			case DropItemType.Ticket:
			case DropItemType.FortuneCard:
			case DropItemType.CardPiece:
			case DropItemType.RandomCardPiece:
			case DropItemType.CardLotteryTicket:
			case DropItemType.LuckyCoin:
			case DropItemType.BossTicket:
			case DropItemType.CardUniversalPiece:
			case DropItemType.ExchangeCoin:
				return true;
			default:
				return false;
			}
		}

		public static string SimpleName(this string name)
		{
			if (name.Contains("\n"))
			{
				return name.Substring(0, name.IndexOf("\n", StringComparison.Ordinal));
			}
			return name;
		}

		public static string TrimNewLine(this string name)
		{
			return name.Replace("\n", string.Empty);
		}

		public static bool HasChangedName(this HttpResponseChangeAccountNameInfo info)
		{
			if (info != null)
			{
				return info.changedCount > 0;
			}
			return false;
		}
	}
}
