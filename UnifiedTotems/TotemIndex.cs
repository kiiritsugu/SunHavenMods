using System.Collections.Generic;
using Wish;

namespace UnifiedTotems;

public static class TotemIndex
{
  public const int HarvestTotemId = 65301;
  public const int ExperienceTotemId = 65302;
  public const int SeasonalTotemId = 65303;
  public const int AtlasTotemId = 65304;

  public static ScareCrowEffect[] HarvestEffects = new ScareCrowEffect[]
  {
      ScareCrowEffect.Royal,
      ScareCrowEffect.Fire,
      ScareCrowEffect.Water,
      ScareCrowEffect.Farming
  };

  public static ScareCrowEffect[] ExperienceEffects = new ScareCrowEffect[]
  {
      ScareCrowEffect.Combat,
      ScareCrowEffect.Mining,
      ScareCrowEffect.Fishing,
      ScareCrowEffect.Exploration
  };
  public static ScareCrowEffect[] SeasonalEffects = new ScareCrowEffect[]
  {
      ScareCrowEffect.Spring,
      ScareCrowEffect.Summer,
      ScareCrowEffect.Fall,
      ScareCrowEffect.Winter
  };

  public static ScareCrowEffect[] AtlasEffects = new ScareCrowEffect[]
  {
      ScareCrowEffect.SunHaven,
      ScareCrowEffect.Nelvari,
      ScareCrowEffect.Withergate,
  };

  public static Dictionary<int, ScareCrowEffect[]> TotemDictionary = new Dictionary<int, ScareCrowEffect[]>
  {
      { HarvestTotemId, HarvestEffects },
      { ExperienceTotemId, ExperienceEffects },
      { SeasonalTotemId, SeasonalEffects },
      { AtlasTotemId, AtlasEffects }
  };
}


