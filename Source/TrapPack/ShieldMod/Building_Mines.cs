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

//has to be outside namespace for odd resons requred by the game 
public class Mine_Def : AnimatedThingDef {

	public float explosion_min_radius;
	public float explosion_max_radius;
	public float explosion_min_damage;
	public float explosion_max_damage;
	public DamageTypeDef damage_def;
	public SoundDef explode_sound;
	
	public List<IntVec3> trigger_spots;

	
	public string arm_ui_texture_path;
	public string disarm_ui_texture_path;
	public string trigger_ui_texture_path;
	public GasDef gas_to_spawn;
	public int gas_thickness = 1000;
	
	public bool checks_for_frendly = false;
	public bool can_trigger = true;
}



namespace TrapPack
{


	
	//--mines
	public class Mine : ThingAddons.AnimatedBuilding{
	
		protected  Texture2D texUI_Arm;
		protected  Texture2D texUI_Disarm;
		protected  Texture2D texUI_Trigger;
		public Mine_Def mine_def;
		// globals
		public bool armed = false; 
		public bool changed = false;
		
		public override void SpawnSetup(){
			mine_def = (Mine_Def)this.def;
			if (this.mine_def == null){
				Log.Error("mine def of a mine type was null!");
		}
		try{
			texUI_Trigger = ContentFinder<Texture2D>.Get(this.mine_def.trigger_ui_texture_path, true);
			texUI_Disarm = ContentFinder<Texture2D>.Get(this.mine_def.disarm_ui_texture_path, true);
			texUI_Arm = ContentFinder<Texture2D>.Get(this.mine_def.arm_ui_texture_path, true);
			
		}
		catch (NullReferenceException e){
			Log.Error("Mine object tried to load it's textures from it's minedef and failed, threw : " + e.Message);
			texUI_Arm = BaseContent.BadTex;
			texUI_Disarm =BaseContent.BadTex;
			texUI_Trigger =BaseContent.BadTex;
		}
			base.SpawnSetup();
		}
		
		public override void Tick()
		{
			if (armed) {
				foreach (IntVec3 pos in this.mine_def.trigger_spots){
					foreach (Pawn pawn in Find.Map.thingGrid.ThingsAt(this.Position +pos).OfType<Pawn>()){
						if (!this.mine_def.checks_for_frendly || pawn.Faction != this.Faction){
							Detonate();
							return;
						}
					}
				}
			}
			base.Tick();
		}
		
		/// <summary>
		/// taken from powerswitch mod by Haplo
		/// This creates new selection buttons with a new graphic
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Command> GetCommands()
		{
			if (this.mine_def.trigger_spots.Any<IntVec3>()){
				
			Command_Action optX;
			optX = new Command_Action();
			if (!armed)
				optX.icon = texUI_Arm;
			else
				optX.icon = texUI_Disarm;
			optX.disabled = false;
			optX.defaultDesc = "Arms mine. (You'll want to be far away)";
			optX.activateSound = SoundDef.Named("Click");
			optX.action = Arm_Disarm;
			optX.groupKey = 313123004;
			yield return optX;
			}
			if (this.mine_def.can_trigger){	
			Command_Action optT;
			optT = new Command_Action();
			optT.icon = texUI_Trigger;
			optT.disabled = false;
			optT.defaultDesc = "Triggers the trap immediately.";
			optT.activateSound = SoundDef.Named("Click");
			optT.action = Detonate;
			optT.groupKey = 313123005;
			yield return optT;
			}
		}
		private void Arm_Disarm()
		{
			changed = true;
			if (armed){
				armed = false;
			this.set_frame("_Disarmed");
			}
			else{
				armed = true;
				this.set_frame("_Armed");
			}
		}
		public virtual void Detonate(){
			this.Destroy();
			ExplosionInfo explosion = new ExplosionInfo();
			explosion.radius = Rand.Range(mine_def.explosion_min_radius, mine_def.explosion_max_radius);
			explosion.dinfo = new DamageInfo(this.mine_def.damage_def, (int)Rand.Range(mine_def.explosion_min_damage, mine_def.explosion_max_damage), this);
			explosion.center = this.Position;
			explosion.explosionSound = this.mine_def.explode_sound;
			explosion.Explode();
			
			
			//spawn gas if we need to
			if (this.mine_def.gas_to_spawn != null){
				Gas.try_place_Gas(this.Position, this.mine_def.gas_to_spawn, this.mine_def.gas_thickness);
			}
			}
	}

}
