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
	public class Building_Pit_Trap :Building
    {
		// globals
		bool set = true;
      
		static DamageTypeDef pit_trap_damage_type = DefDatabase<DamageTypeDef>.GetNamed("pit_trap_damage_type");
        public override void Tick()
        {
			if (!set){
				base.Tick();   
				return;
			}
			List<Thing> things = new List<Thing>();
			things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
				if (target is Pawn){
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
					target.TakeDamage(new DamageInfo( pit_trap_damage_type, Rand.Range(0,20), this, new BodyPartDamageInfo(BodyPartHeight.Bottom,BodyPartDepth.Outside)));
					target.TakeDamage(new DamageInfo(DamageTypeDefOf.Stun, 100, this));
					set = false;
					this.set_frame("_Unset");
				}
			}
			base.Tick();       
		}
		
    }

//*/
}
