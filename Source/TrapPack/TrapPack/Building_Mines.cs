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



namespace TrapPack
{

	public class Mine_Def : AnimatedThingDef {
		
		public float explosion_min_radius;
		public float explosion_max_radius;
		public float explosion_min_damage;
		public float explosion_max_damage = 0f;
		public DamageTypeDef damage_def;
		public SoundDef explode_sound;
		public ThingDef thing_to_spawn;
		public ThingDef projectile_to_launch = null;
		public int projectile_count;
		
		public List<IntVec3> trigger_spots;
		public int random_sense_radius = 0;
		public List<IntVec3> hit_spots;
		
		public Type trigger_type = null;
		
		public string arm_ui_texture_path;
		public string disarm_ui_texture_path;
		public string trigger_ui_texture_path;
		//	public string gas_def_name;
		public ThingDef gas_to_spawn;
		public int gas_thickness = 1000;
		
		public bool checks_for_frendly = false;
		public bool can_trigger = true;
		/*
	public override void ResolveReferences ()
	{
		if (gas_def_name != null){
			this.gas_to_spawn = DefDatabase<Thi>.GetNamed(gas_def_name);
		}
		base.ResolveReferences();
	}
	//*/
	}
	
	
	
	public class PlacementRestricter_Next_To_Wall : PlacementRestricter{
		public override AcceptanceReport CanPlaceWithRestriction (EntityDef checkingDef, IntVec3 loc, IntRot rot)
		{
			foreach (Thing thing in Find.ThingGrid.ThingsAt(loc + IntVec3.north.RotatedBy(rot))){
				if (thing.def.eType == EntityType.Wall){
					return true;
				}
			}
			
			return "Must Be Next To Wall";
		}
	}
	
	
	//--mines
	public class Mine : Building{
	
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
		/*	if (!(this.mine_def.trigger_spots == null || this.mine_def.trigger_spots.Count ==0)){
			//default to disarmed:
			armed = false;
			this.set_frame("_Disarmed");
			this.animated_thing_def.play = false;
		}
		*/
			
				base.SpawnSetup();
		}
		
		public override void Tick()
		{
			if (armed) {
				foreach (IntVec3 pos in this.mine_def.trigger_spots){
					IntVec3 corrected_pos = pos.RotatedBy(this.Rotation);
					foreach (Thing thing in Find.Map.thingGrid.ThingsAt(this.Position +corrected_pos)){
						if (this.mine_def.trigger_type != null &&  thing.GetType() ==  this.mine_def.trigger_type){
							if (!this.mine_def.checks_for_frendly || thing.Faction != this.Faction){
								//Log.Message("found a " + thing.ToString() + "  whitch was a  "+ thing.def.defName);
							Detonate();
							return;
						}
						}
					}
				}
				
				//this is a random sensing mine, do a random check in some direction
				if (this.mine_def.random_sense_radius > 0){
				//pick a random squaire
					int radius = this.mine_def.random_sense_radius;
					IntVec3 tile = new IntVec3(Rand.Range(-radius,radius),0,Rand.Range(-radius, radius));
					//Log.Message("chose tile at " + tile.ToString());
						foreach (Thing thing in Find.Map.thingGrid.ThingsAt(this.Position + tile)){
	
						if (this.mine_def.trigger_type != null &&  thing.GetType() ==  this.mine_def.trigger_type){
							if (!this.mine_def.checks_for_frendly || thing.Faction != this.Faction){
								Detonate();
								return;
							}
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
			if ((this.mine_def.random_sense_radius > 0 || this.mine_def.trigger_spots.Any<IntVec3>()) && this.mine_def.trigger_type != null){
				
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
		public override void DrawExtraSelectionOverlays ()
		{
			if (!(this.mine_def.trigger_spots == null || this.mine_def.trigger_spots.Count ==0)){
			List<IntVec3> draw_trig_pos = new List<IntVec3>();
			foreach (IntVec3 pos in this.mine_def.trigger_spots){
					if (!pos.Equals(IntVec3.zero)){
				draw_trig_pos.Add(pos.RotatedBy(this.Rotation) + this.Position);
				}
			}
				GenDraw.DrawFieldEdges(draw_trig_pos, Color.red);
				
			}
			if (!(this.mine_def.hit_spots == null || this.mine_def.hit_spots.Count ==0)){
			List<IntVec3> draw_hit_pos = new List<IntVec3>();
			foreach (IntVec3 pos in this.mine_def.hit_spots){
					if (!pos.Equals(IntVec3.zero)){
				draw_hit_pos.Add(pos.RotatedBy(this.Rotation) + this.Position);
				}
				}
			GenDraw.DrawFieldEdges(draw_hit_pos, Color.white);
			}
			//Log.Message("tried to draw selection overlays");
			base.DrawExtraSelectionOverlays();
		}
		
		
		private void Arm_Disarm()
		{
			changed = true;
			if (armed){
				armed = false;
			this.mine_def.set_frame("_Disarmed");
				this.mine_def.play = false;
			}
			else{
				armed = true;
				this.mine_def.set_frame("_Armed");
				this.mine_def.play = true;
			}
		}
		public virtual void Detonate(){
			this.health = 0;
			//if our damagedef is not null, we are supposed to explode
			if (this.mine_def.explosion_max_damage > 0f){
			ExplosionInfo explosion = new ExplosionInfo();
			explosion.radius = Rand.Range(mine_def.explosion_min_radius, mine_def.explosion_max_radius);
			explosion.dinfo = new DamageInfo(this.mine_def.damage_def, (int)Rand.Range(mine_def.explosion_min_damage, mine_def.explosion_max_damage), this);
			explosion.center = this.Position;
			explosion.explosionSound = this.mine_def.explode_sound;
				explosion.postExplosionSpawnThingDef = this.mine_def.thing_to_spawn;
				explosion.explosionSpawnChance = .5f;
			explosion.Explode();
			}
			else{
				this.Destroy(DestroyMode.Deconstruct);
			}
			if (this.mine_def.projectile_to_launch != null){
				foreach (IntVec3 targ in this.mine_def.hit_spots){
					//Log.Message("fireing proj at: " + targ.ToString());
				for ( int i = 0; i< this.mine_def.projectile_count; i++){
					
					Projectile proj = (Projectile)GenSpawn.Spawn(this.mine_def.projectile_to_launch, this.Position);
						proj.Launch(this, new TargetPack(targ.RotatedBy(this.Rotation)+this.Position));
				}
				}
			}
			
			//spawn gas if we need to
			if (this.mine_def.gas_to_spawn != null){
				Gas.try_place_Gas(this.Position, (GasDef)this.mine_def.gas_to_spawn, this.mine_def.gas_thickness, this.Faction);
			}
			}
	}

}
