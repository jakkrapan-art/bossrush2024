using System.Collections.Generic;
using UnityEngine;

public class BossSkillController
{
  private float _lastUsed;
  private readonly Boss _boss;

  private readonly int RAGE_STATE_MIN = 81;
  private readonly int IDLE_STATE_MIN = 31;
  private readonly int ALMOST_STATE_MIN = 0;

  private List<BossSkill> _idleStateSkills = new();
  private List<BossSkill> _rageStateSkills = new();
  private List<BossSkill> _almostFullStateSkills = new();

  private List<BossSkill> _cooldownSkills = new();
  public BossSkillController(Boss boss)
  {
    _boss = boss;

    var shout = new BossSkill(true);
    var think = new BossSkill(true);
    var blockWay = new BossSkill(true);
    var blockWayPlus = new BossSkill(true);
    var plantCurse = new BossSkill(true);
    var glaze = new BossSkill(true);

    _idleStateSkills.AddRange(new BossSkill[] { shout, think, glaze, blockWay });
    _rageStateSkills.AddRange(new BossSkill[] { shout, think, glaze, blockWay });
    _almostFullStateSkills.AddRange(new BossSkill[] { shout, think, blockWayPlus, plantCurse });
  }

  public bool CanUse()
  {
    return Time.time >= _lastUsed + Const.SKILL_COOLTIME;
  }

  public void TriggerSkill(float currentRage)
  {
    if (!CanUse()) return;
    UpdateSkillCooldown();

    _lastUsed = Time.time;

    List<BossSkill> skillList = null;
    if(currentRage >= RAGE_STATE_MIN)
    {
      skillList = _rageStateSkills;
    }
    else if(currentRage >= IDLE_STATE_MIN)
    {
      skillList = _idleStateSkills;
    }
    else if(currentRage > ALMOST_STATE_MIN)
    {
      skillList = _almostFullStateSkills;
    }

    if (skillList == null) return;
    var skill = GetOneReadySkill(skillList);
    if(skill == null) return;
    skill.Use();
    _cooldownSkills.Add(skill);
  }

  private BossSkill GetOneReadySkill(List<BossSkill> skillList)
  {
    List<BossSkill> readySkillList = new();
    foreach(BossSkill skill in skillList) 
    {
      if(skill.IsReady()) readySkillList.Add(skill);
    }

    if (readySkillList.Count == 0) return null;

    return readySkillList[Random.Range(0, readySkillList.Count)];
  }
  
  private void UpdateSkillCooldown()
  {
    if (_cooldownSkills.Count == 0) return;
    _cooldownSkills.RemoveAll(skill => skill.IsReady());
  }
}
