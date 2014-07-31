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
	
	public class Smoke : ThingAddons.AnimatedThing{
		// globals
		public int thickness = 0;
		public override void SpawnSetup(){
			base.SpawnSetup();
		}
		public override void TickRare(){
					if (this.thickness-- == 0){
					this.Destroy();
					return;
				}
					foreach (IntVec3 pos  in this.Position.AdjacentSquaresCardinalInBounds()){
					if (UnityEngine.Random.Range(0,10) < 8){
					this.thickness = try_place_smoke(pos, this.thickness);
					}
			}
			List<Thing> things = new List<Thing>();
			things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
			foreach (Thing target in things){
				if (target is Pawn){
					//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						target.TakeDamage(new DamageInfo( DamageTypeDefOf.Bleeding, 10 * (int)this.thickness, this));
				}
			}
	}
		public static int try_place_smoke(IntVec3 pos, int thickness = 100){
			Thing found_thing = Find.Map.thingGrid.ThingAt(pos,ThingDef.Named("Smoke"));
			if (found_thing == null){
				// there is no smoke, create a new one with half of ours
				Smoke new_smoke = (Smoke)GenSpawn.Spawn(ThingDef.Named("Smoke"), pos);
				new_smoke.thickness = thickness/=2;
			}else{
				// we found a smoke, add to it's thickness with 1/3 of ours
				Smoke adj_smoke = (Smoke)found_thing;
				adj_smoke.thickness += (thickness/= 3);
			}
			return thickness;

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
	public class Zap_Effect : ThingAddons.AnimatedThing{
		//gloables: 
		int lifetime = 15;
		public override void Tick (){
			if (lifetime-- < 0){
				this.Destroy();
			}
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