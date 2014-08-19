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

public class GasDef : AnimatedThingDef {
	
	
	public int damage_per_tick = 0;
	public bool extinguish_fire = false;
	public DamageTypeDef damage_type;
	public float new_gas_dispersion_rate = .2f;
	public float found_gas_dispersion_rate = .4f;
}

namespace TrapPack
{

	public class Gas : ThingAddons.AnimatedThing{
		//damage defs
		//static DamageTypeDef Poisoned = DefDatabase<DamageTypeDef>.GetNamed("Poisoned");

		public GasDef gas_def;
		// globals
		public int thickness = 0;
		public uint ticks_untill_next_update = 20;
		public override void SpawnSetup(){
			this.gas_def = (GasDef)this.def;
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
			// begin determing if we need to spread
			if (this.thickness-- == 0 && !this.Destroyed){
				base.Tick();	
				this.Destroy();
				return;
				}
					foreach (IntVec3 pos  in this.Position.AdjacentSquares8Way().InRandomOrder()){
					if (Find.PathGrid.Walkable(pos)){
					if (this.thickness > 5){
					try_place_Gas(pos,this.gas_def, this);
					
					}
					}
			}
			
			// begin doing what this gas is supposed to do, be it damage or fire extingusihng
			List<Thing> things = new List<Thing>();
			things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
			foreach (Thing target in things){
			
			// if we found a pawn, prepare to do damage to it
				if ( this.gas_def.damage_per_tick > 0 && target is Pawn ){
					//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
					Pawn pawn = (Pawn)target;
					List<BodyDefPart> bodyparts = pawn.healthTracker.bodyModel.GetNotMissingParts().ToList();
					foreach (BodyDefPart part in bodyparts.InRandomOrder()){
						if (part.def.activities != null &&  part.def.activities.Contains("Breathing_main")){
							float damage_mod = 0;
							if (pawn.apparel != null){
								BodyDefPart protected_part = bodyparts.Find(protected_part_can => protected_part_can.def.activities.Contains("Breathing_main"));
								if (protected_part != null){
								damage_mod = pawn.apparel.GetDamageAbsorption(protected_part,this.gas_def.damage_type.injury);
								}
							}
							if (damage_mod < 0.99f){
								pawn.healthTracker.ApplyDamage(new DamageInfo(this.gas_def.damage_type, (int)((float)this.thickness * (1.0f-damage_mod)), this, new BodyPartDamageInfo(part, false)));
							break;
							}
						}
					}
				}
				if (this.gas_def.extinguish_fire && target is Fire){
					Fire fire = (Fire)target;
				if (fire.fireSize > .05f){
					fire.fireSize -= .05f * this.thickness;
				}
					else{
						fire.Destroy();
					}
				}
					}
			base.Tick();
	}
		/// <summary>
		/// trys to place gas, checking to see if there is gas of the same gasdef present. uses your current gas and it's disapation rate to determine if gas needs to be dispersed
		/// to assure that the thickness is used in considering dissapation. also transfers faction ownership.
		/// </summary>
		/// <param name="pos">where to place the gas</param>
		/// <param name="gasdef">gasdef to place</param>
		/// <param name="placer">thing to use for ownership. also uses placer's thickness if placer is a gas.</param>
		public static void try_place_Gas(IntVec3 pos, GasDef gas_def, Gas placer = null){
			Thing found_thing = Find.Map.thingGrid.ThingAt<Gas>(pos);
			int thickness = 1000;
			if (placer != null ){
				thickness = ((Gas)placer).thickness;
			}
			if (found_thing == null){
				// there is no Poison_Gas, make a new one with 1/8 ours
				Gas new_gas = (Gas)GenSpawn.Spawn(gas_def, pos);
				if (placer != null){
				new_gas.SetFactionDirect(placer.Faction);
				}
				new_gas.thickness = (int)(thickness * placer.gas_def.new_gas_dispersion_rate);
				thickness-= (int)(thickness* placer.gas_def.new_gas_dispersion_rate);
			}else if (!found_thing.Destroyed){
				// we found a gas, check if it is our type and then exchange, add to it's thickness with 1/4 of ours
				Gas adj_gas = (Gas)found_thing;
				if (adj_gas.gas_def == gas_def){
					adj_gas.thickness += (int)(thickness* placer.gas_def.found_gas_dispersion_rate);
					thickness-= (int)(thickness* placer.gas_def.found_gas_dispersion_rate);
				}
			}
			if (placer != null && placer is Gas){
				((Gas)placer).thickness = thickness;
			}
		}
		public static void try_place_Gas(IntVec3 pos, GasDef gas_def, int thickness = 1000, Faction faction = null){
			Thing found_thing = Find.Map.thingGrid.ThingAt<Gas>(pos);
			if (found_thing == null){
				// there is no Poison_Gas, make a new one with 1/8 ours
				Gas new_gas = (Gas)GenSpawn.Spawn(gas_def, pos);
				if (faction != null){
					new_gas.SetFactionDirect(faction);
				}
				new_gas.thickness = thickness;
				thickness-= thickness;
			}else if (!found_thing.Destroyed){
				// we found a gas, check if it is our type and then exchange, add to it's thickness with 1/4 of ours
				Gas adj_gas = (Gas)found_thing;
				if (adj_gas.gas_def == gas_def){
					adj_gas.thickness += (int)(thickness* gas_def.found_gas_dispersion_rate);
					thickness-= (int)(thickness* gas_def.found_gas_dispersion_rate);
				}
			}
		}
		 public override string Label{
		get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.Label);
				stringBuilder.Append(" Thickness : " + this.thickness);
				return stringBuilder.ToString();
			}
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