namespace Server.Models
{
	public sealed class DbGameSettingModel : IObjectModel
	{
		public float MonsterBiteDurationBeforeEscape { get; set; }

		public float WaitingGachaPointAccumulationRate { get; set; }

		public int WaitingGachaPointAccumulationMax { get; set; }

		public float BoundaryRingMonsterSpeedMin { get; set; }

		public float BoundaryRingMonsterSpeedMax { get; set; }

		public float BoundaryRingMonsterSpeedClampMin { get; set; }

		public float BoundaryRingMonsterSpeedClampMax { get; set; }

		public float EnergyRecoveryRate { get; set; }

		public float EnergyWarningPercent { get; set; }

		public float MonsterDirectionChangeBufferMin { get; set; }

		public float MonsterDirectionChangeBufferMax { get; set; }

		public string MonsterPresentationPrefabPath { get; set; }

		public string MonsterPresentationImagesPath { get; set; }

		public string MonsterIconsPath { get; set; }

		public string RodImagePath { get; set; }

		public float TextFeedbackDurationEaseIn { get; set; }

		public float TextFeedbackDurationWait { get; set; }

		public float TextFeedbackDurationEaseOut { get; set; }

		public float BattlePointsRegenDurationPerPoint { get; set; }

		public int BattlePointsCountCap { get; set; }

		public int RodEvolutionLevelCapRarityTier1 { get; set; }

		public int RodEvolutionLevelCapRarityTier2 { get; set; }

		public int RodEvolutionLevelCapRarityTier3 { get; set; }

		public int RodEvolutionLevelCapRarityTier4 { get; set; }

		public int RodEvolutionLevelCapRarityTier5 { get; set; }

		public int RodEvolutionLevelCapRarityTier6 { get; set; }

		public int RodEvolutionLevelCapRarityTier7 { get; set; }

		public int RodEvolutionLevelCapRarityTier8 { get; set; }

		public int RodEvolutionLevelCapRarityTier9 { get; set; }

		public int RodEvolutionLevelCapRarityTier10 { get; set; }

		public int RodEvolutionLevelCapRarityTier11 { get; set; }

		public int RodEvolutionLevelCapRarityTier12 { get; set; }

		public int RodEvolutionLevelCapRarityTier13 { get; set; }

		public int RodEvolutionLevelCapRarityTier14 { get; set; }

		public int RodEvolutionLevelCapRarityTier15 { get; set; }

		public int RodEvolutionLevelCapRarityTier16 { get; set; }

		public float ComboCoolDownDuration { get; set; }

		public float MonsterAttackMissedTap_Multiplier { get; set; }

		public int FusionMainRodCostMultiplierPerSubRod { get; set; }

		public float FusionXPGain_Multiplier { get; set; }

		public float RodSellPriceXPPerUnit { get; set; }

		public int RodSellPricePerUnitPrice { get; set; }

		public int RodEnergyCostTooHighWarning { get; set; }

		public float WaitingGachaPointDurationBeforeCountDown { get; set; }

		public float WaitingGachaPointEarlyClickPenalty { get; set; }

		public float Combo_Duration { get; set; }

		public float Combo_DamageIncrement { get; set; }

		public float Combo_DamageIncrementClamp { get; set; }

		public float Combo_FeedbackScaleIncrement { get; set; }

		public float Combo_FeedbackScaleIncrementClamp { get; set; }

		public float MissStunDuration { get; set; }

		public int TutorialAttack_CountRequired { get; set; }

		public int TutorialBlock_CountRequired { get; set; }

		public float HintsDuration { get; set; }

		public int EventKing_QualifyingLevel { get; set; }

		public float XPGainElementalMultiplierMainRodIsStronger { get; set; }

		public float XPGainElementalMultiplierMainRodIsEqual { get; set; }

		public float XPGainElementalMultiplierMainRodIsWeaker { get; set; }

		public float RodAttackMultiplierIfElementStronger { get; set; }

		public float RodAttackMultiplierIfElementSame { get; set; }

		public float RodAttackMultiplierIfElementWeaker { get; set; }

		public float RodDefenceMultiplierIfElementStronger { get; set; }

		public float RodDefenceMultiplierIfElementSame { get; set; }

		public float RodDefenceMultiplierIfElementWeaker { get; set; }

		public float PulseHPPotion_PlayerHPPercent { get; set; }

		public float LevellingBar_SpeedMultipler_Min { get; set; }

		public float LevellingBar_SpeedMultipler_Max { get; set; }

		public float LevellingBar_SpeedMultipler_Ratio { get; set; }

		public float IfOneAttackEscape_NoAttackDurationMin { get; set; }

		public float IfOneAttackEscape_NoAttackDurationMax { get; set; }

		public int MonsterDazeComboCount { get; set; }

		public float MonsterDazeDurationMin { get; set; }

		public float MonsterDazeDurationMax { get; set; }

		public int Tutorial_MonsterHp { get; set; }

		public int EvolutionCostPerMaterialRod_Elemental { get; set; }

		public int EvolutionCostPerMaterialRod_Silver { get; set; }

		public int EvolutionCostPerMaterialRod_Gold { get; set; }

		public int EvolutionCostAtRarity_Normal { get; set; }

		public int EvolutionCostAtRarity_NormalPlus { get; set; }

		public int EvolutionCostAtRarity_Rare { get; set; }

		public int EvolutionCostAtRarity_RarePlus { get; set; }

		public int EvolutionCostAtRarity_SuperRare { get; set; }

		public int HomePage_EventText_TextScrollSpeed { get; set; }

		public int Playername_CharacterLimit_min { get; set; }

		public int Playername_CharacterLimit_max { get; set; }

		public string Playername_AllowedCharacters { get; set; }

		public int Chat_CharacterInputs_min { get; set; }

		public int Chat_CharacterInputs_max { get; set; }

		public int FISHINGSPOTS_DefaultMaxNumberOfAreas { get; set; }

		public uint FISHINGSPOTS_DefaultKingArea { get; set; }

		public float EventKingFriendRod_AttributesMultiplier { get; set; }

		public int AskForReview_Level_Start { get; set; }

		public int AskForReview_Level_Interval { get; set; }

		public int EventKing_CreateTeamCost { get; set; }

		public int EventKing_PlayerTeamNameChangeCost { get; set; }

		public int EventKingTeamSize_PlayerCreated { get; set; }

		public int EventKing_MinLevelRequiredToCreateTeam { get; set; }

		public int Facebook_Popup_50Friends_isActive { get; set; }

		public int FBFriend_RewardGet_ClassId { get; set; }

		public int FBFriendInvite_Reward_Count { get; set; }

		public int FBFriendInvite_Reward_ClassId { get; set; }

		public int SN_Login_Type { get; set; }

		public uint FBFriend_CoolDownDuration { get; set; }

		public int FBFriend_INVITE_ListLimit { get; set; }
	}
}
