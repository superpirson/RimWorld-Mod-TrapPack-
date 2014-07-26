using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;
using RimWorld;
// used some code from Haplo's PowerSwitch mod, and used sheildmod by Darker as a template
//
namespace TrapPack
{
	//--pit traps
    public class Building_Pit_Trap : Building
    {
		// globals

      
		static DamageTypeDef trap_damage_type;
		static Building_Pit_Trap(){
			trap_damage_type = new DamageTypeDef();
			trap_damage_type.deathMessage = "{0} fell to their death in a spiked trap";
		}

		public override void SpawnSetup(){
			base.SpawnSetup();
		}
        public override void Tick()
        {
			List<Thing> things = new List<Thing>();
			things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
				if (target is Pawn){
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						target.TakeDamage(new DamageInfo( trap_damage_type, 10, this));
						
					((Pawn)target).healthTracker.ForceIncap();
						
					Destroy();
					GenSpawn.Spawn(ThingDef.Named("Building_Tripped_Pit_Trap"), this.Position);
					}
			

	
			}

			base.Tick();
                
		}
        public override void DrawExtraSelectionOverlays()
        {
           // GenDraw.DrawRadiusRing(base.Position, 1.0);
        }
	    public override string GetInspectString()
	    {
	    	StringBuilder stringBuilder = new StringBuilder();
	    	//stringBuilder.Append(base.GetInspectString());
           stringBuilder.Append("looks nasty!");

	    	return stringBuilder.ToString();
	    }
        //Saving game
        public override void ExposeData()
        {
			base.ExposeData();
        }
    }
	public class Building_Tripped_Pit_Trap : Building
{	
	
	public override void SpawnSetup(){
			def.tickerType = TickerType.Normal;
			base.SpawnSetup();
	}
	
	public override void Tick()
	{
		base.Tick();
	}
	public override void DrawExtraSelectionOverlays()
	{
		// GenDraw.DrawRadiusRing(base.Position, 1.0);
	}
	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		//stringBuilder.Append(base.GetInspectString());
		stringBuilder.Append("dang!");
		
		return stringBuilder.ToString();
	}
	//Saving game
	public override void ExposeData()
	{
		base.ExposeData();
	}

}

}
