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

  public static int [] VanillaTotemIds = new int[] { 10696, 10697, 10698, 10699 };
  
  public static Dictionary<int, string> VanillaTotems = new Dictionary<int, string>
  {
    { 10678, "Spring Scarecrow" },
    { 10729, "Summer Scarecrow" },
    { 10719, "Fall Scarecrow" },
    { 10730, "Winter Scarecrow" },
    { 10694, "Fire Fertilizer Totem" },
    { 10695, "Watering Can Totem" },
    { 10696, "Royal Totem" },
    { 10733, "Farming Totem" },
    { 10699, "Combat Totem" },
    { 10692, "Exploration Totem" },
    { 10732, "Mining Totem" },
    { 10731, "Fishing Totem" },
    { 10735, "Spring Totem" },
    { 10736, "Summer Totem" },
    { 10734, "Fall Totem" },
    { 10737, "Winter Totem" },
    { 10739, "Sun Haven Totem" },
    { 10697, "Nel'Vari Totem" },
    { 10693, "Withergate Totem" },
    { 10740, "Crop Totem" } //Placeholder unused totem
  };
}
