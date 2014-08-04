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
		bool set = true;
      
		static DamageTypeDef pit_trap_damage_type = DefDatabase<DamageTypeDef>.GetNamed("pit_trap_damage_type");
        public override void Tick()
        {
			if (!set){
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
				}
			}
			base.Tick();       
		}
		public override Material DrawMat (IntRot rot)
		{
			//Log.Message("pittrap's drawmat was invoked!");
			if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
			{
				return this.def.DrawMat (rot);
			}
			if (this.def.folderDrawMats.Count == 1)
			{
				return this.def.folderDrawMats [0];
			}
			if (set){
				return this.def.folderDrawMats [0];
			}else{
			return this.def.folderDrawMats [1];
			}
		}
    }

//*/
}
