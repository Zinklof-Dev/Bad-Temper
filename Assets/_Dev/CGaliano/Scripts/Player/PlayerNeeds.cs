/*using System;
using Unity;

public class PlayerNeeds : NetworkBehavior
{
  [SerializeField] float hp; // 0-100 abritrary system, player gets damaged by AI attacks, etc. etc. meant to represent physical damage.
  [SerializeField] float blood; // 2500 will be represented as 0%, rest is linear going to 5000
  [SerializeField] float water; // water / fluid intake, measured in ML, 2000 is about the need for a human to function, <1500 healing is hampered <1000 healing wont take place, <500 damage is taken. >2000 healing is boosted slightly, >2500 sickness starts to begin. (GUI will represnet 2500 as full, 2000 as 75%, 1500 as 50%, 1000 as 25%, and 500 below as 0%)
  [SerializeField] float food; // food content, measured in Calories, this will be simpler though, below 1000 calories healing and blood production is hampered, below 0 health is directly lowered, above 2500 sickness can take place from over eating. (GUI will represent 2000 as 100%, 1500 as 75%, 1000 as 50%, 500 as 25%, 0 as 0%, reasoning is a player will feel full before they get stick, and will know for sure when they're starving)

  private float maxHP = 100;
  private float maxBlood = 5000; // in ML, 14% blood loss is when symptoms are minor, blurry vision, light headed. 20% is hemorrhagic shock, various symptoms rapidly rear their heads and get worse as you reach 40% bloodloss, at which point you cannot keep blood pressure high enough to supply all organs, heartrate accelerates, you will passout and enter a coma with cardiac arrest, 50% blood loss you no longer have enough to pump oxygen throughout your body, your heart will stop and unless IMMEDIATE action is taken, you will die.

  private float SpO2 = 100; // blood oxygen level this is seperate from blood loss, if you don't breathe, SpO2 goes down, if you do, it rises.
  private float bloodO2Efficiency = 1.0f; // this is a made up detail, this will be affected by blood loss and low bp, it will change how efficently your body is getting oxygen to keep running
  private float bloodPressure = 100; // NOT FOLLOWING REALISTIC VALUES!! tied to bloodloss and various other health effects, the lower the BP the faster and harder your heart must beat to compensate, the higher your BP, well... thats a whole different area and problem.

  private float foodWater5 = 2500; // above this value, sickness
  private float foodWater4 = 2000; // "full" or "satisfied"
  private float foodWater3 = 1500; // above this value normal healing
  private float foodWater2 = 1000; // above this value hampered healing
  private float foodWater1 = 500; // above this value no healing
  private float foodwater0 = 0; // above this value slow damage, below rapid damage.

  // according to some random site, donating 450 ml of blood burns 650 calories, so every ml of blood we replace will cost, 0.7 calories more than normal homeostatis costs. blood is slightly less than 80% water, so we will also use 0.8 ml of water for every ml of blood we produce to replenish blood loss.
  // so in total, every ml of blood costs 0.7 calories, and 0.8 ml of blood, this is an added cost ontop of the players homeostatis cost for either value.
  
  // plan:
  // blood volume will directly affect BP, as your blood volume lowers the pressure obviously lowers, thus your BPM must rise to increase pressure, causing a faster heart rate.
  // the high BPM causes some of the symptoms of blood loss, the lower BP causes others, and the lack of oxygen delivery causes the rest. 
  // so this system, as blood volume drops, we will lower the BP and BO2E, BO2E will further lower because of the low BP, low BP will make the heart rate rise to increase BP which will then somwhat bring the BO2E back up. this can only work for so long before the heart rate cannot go any higher, thus the player will suffer from suffocation as they cannot get the O2 content they need, while experiencing the affects of anxiety from the high heart rate, and total organ shutdown (represented as control delays, desaturated vision, etc. etc.)
  
}*/
