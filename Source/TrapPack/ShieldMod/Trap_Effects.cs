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

	public class Poison_Gas : ThingAddons.AnimatedThing{
		//damage defs
		static DamageTypeDef Poisoned = DefDatabase<DamageTypeDef>.GetNamed("Poisoned");


		// globals
		public int thickness = 0;
		public uint ticks_untill_next_update = 20;
		public override void SpawnSetup(){
			this.wait_ticks = Rand.Range(0,100);
			base.SpawnSetup();
		}
		public override void Tick(){
			if (ticks_untill_next_update-- > 0){
				//return, do nothing untill time is right
				base.Tick ();
				return;
			}else{
				//reload the tick timer with a random number of ticks
				ticks_untill_next_update += (uint)Rand.Range(40,200);
			}
					if (this.thickness-- == 0){
				base.Tick();	
				this.Destroy();
				return;
				}
					foreach (IntVec3 pos  in this.Position.AdjacentSquares8Way().InRandomOrder()){
					if (Find.PathGrid.Walkable(pos)){
					if (this.thickness > 5){
					this.thickness = try_place_Poison_Gas(pos, this.thickness);
					}
					}
			}
			List<Thing> things = new List<Thing>();
			things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
			foreach (Thing target in things){
				if (target is Pawn){

					//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
					Pawn pawn = (Pawn)target;
					List<BodyDefPart> bodyparts = pawn.healthTracker.bodyModel.GetNotMissingParts().ToList();
					foreach (BodyDefPart part in bodyparts.InRandomOrder()){
						if (part.def.activities != null &&  part.def.activities.Contains("Breathing_main")){
							float damage_mod = pawn.apparel.GetDamageAbsorption(part,Poisoned.injury);
							if (damage_mod < 0.99f){
								pawn.healthTracker.ApplyDamage(new DamageInfo(Poisoned, (int)((float)this.thickness * (1.0f-damage_mod)), this, new BodyPartDamageInfo(part, false)));
							break;
							}
						}
					}
				}
					}
			base.Tick();
	}
		public static int try_place_Poison_Gas(IntVec3 pos, int thickness = 100){
			Thing found_thing = Find.Map.thingGrid.ThingAt(pos,ThingDef.Named("Poison_Gas"));
			if (found_thing == null){
				// there is no Poison_Gas, make a new one with 1/8 ours
				Poison_Gas new_Poison_Gas = (Poison_Gas)GenSpawn.Spawn(ThingDef.Named("Poison_Gas"), pos);
				new_Poison_Gas.thickness = thickness/ 8;
				thickness-= thickness/8;
			}else{
				// we found a Poison_Gas, add to it's thickness with 1/4 of ours
				Poison_Gas adj_Poison_Gas = (Poison_Gas)found_thing;
				adj_Poison_Gas.thickness += (thickness/ 4);
				thickness-= thickness/4;
			}
			return thickness; 
		}
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
				stringBuilder.Append("Thickness : " + this.thickness);
			return stringBuilder.ToString();
		}
	}
	public class Zap_Effect : ThingAddons.AnimatedThing{
		//gloables: 
		int lifetime = 15;
		public override void Tick (){
			if (lifetime-- < 0){
				this.Destroy();
		}
			base.Tick();
		}
	}
	public class Proj_Caltrops : Projectile{
		protected override void Impact (Thing hitThing){
			if (hitThing == null || !(hitThing is Caltrops)){
				FilthMaker.MakeFilth(this.Position, ThingDef.Named("Caltrops"),3);
			}else{
				for (int i = 0; i<3; i++){
				((Caltrops)hitThing).ThickenFilth();
				}
			}
		
			this.Destroy();
		}
		}
		public class Caltrops : Filth
		{
			// globals
		 uint ticks_timer = 0;
			public override void SpawnSetup(){
				base.SpawnSetup();
		}
			
		public override void Tick(){
			if (ticks_timer-- % 3000 == 1){
				if (this.thickness != 0 && 1 < (Find.WeatherManager.CurWindIntensity * Find.WeatherManager.RainRate)) {
					this.ThinFilth();
				}
			}
				if (ticks_timer % 20 > 0){
				//Log.Message("ticktimer =" + ticks_timer.ToString());
				return;
			}
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn){
					//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
					if ((int)UnityEngine.Random.Range(0,32) < this.thickness){
					target.TakeDamage(new DamageInfo( DamageTypeDefOf.Bleeding, 5 , this));
					}
					}
				}	

		}
		public override string Label{
			get
			{

				StringBuilder stringBuilder = new StringBuilder ();
				stringBuilder.Append (base.Label);
				stringBuilder.Append("thickness: " + this.thickness);
				return stringBuilder.ToString ();
		}
	
	}
	}


}