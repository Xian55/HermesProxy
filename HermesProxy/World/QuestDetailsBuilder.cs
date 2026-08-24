using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World;

public static class QuestDetailsBuilder
{
    public static QuestGiverQuestDetails FromTemplate(WowGuid128 giver, QuestTemplate template)
    {
        QuestGiverQuestDetails quest = new()
        {
            QuestGiverGUID = giver,
            InformUnit = giver,
            QuestGiverCreatureID = giver.GetEntry(),
            QuestID = template.QuestID,
            QuestTitle = template.LogTitle,
            DescriptionText = template.QuestDescription,
            LogDescription = template.LogDescription,
            SuggestedPartyMembers = template.SuggestedGroupNum,
            AutoLaunched = false,
            StartCheat = false,
            DisplayPopup = true,
            QuestPackageID = (int)template.QuestPackageID,
            PortraitGiver = template.PortraitGiver,
            PortraitTurnIn = template.PortraitTurnIn,
            PortraitGiverText = template.PortraitGiverText,
            PortraitGiverName = template.PortraitGiverName,
            PortraitTurnInText = template.PortraitTurnInText,
            PortraitTurnInName = template.PortraitTurnInName,
        };
        quest.QuestFlags[0] = QuestFlagUtil.StripAutoAccept(template.Flags);
        quest.QuestFlags[1] = template.FlagsEx;

        quest.Rewards.Money = template.RewardMoney > 0 ? (uint)template.RewardMoney : 0;
        quest.Rewards.XP = template.RewardXPDifficulty;
        quest.Rewards.Honor = template.RewardHonor;
        quest.Rewards.Title = template.RewardTitle;
        quest.Rewards.SpellCompletionID = template.RewardSpell;
        quest.Rewards.NumSkillUps = template.RewardNumSkillUps;
        quest.Rewards.SkillLineID = template.RewardSkillLineID;

        for (int i = 0; i < template.RewardItems.Length && i < quest.Rewards.ItemID.Length; i++)
        {
            quest.Rewards.ItemID[i] = template.RewardItems[i];
            quest.Rewards.ItemQty[i] = template.RewardAmount[i];
        }

        for (int i = 0; i < template.UnfilteredChoiceItems.Length && i < quest.Rewards.ChoiceItems.Length; i++)
        {
            quest.Rewards.ChoiceItems[i].Item.ItemID = template.UnfilteredChoiceItems[i].ItemID;
            quest.Rewards.ChoiceItems[i].Quantity = template.UnfilteredChoiceItems[i].Quantity;
        }

        foreach (QuestObjective objective in template.Objectives)
        {
            quest.Objectives.Add(new QuestObjectiveSimple
            {
                Id = objective.Id,
                ObjectID = objective.ObjectID,
                Amount = objective.Amount,
                Type = (byte)objective.Type,
            });
        }

        return quest;
    }
}
