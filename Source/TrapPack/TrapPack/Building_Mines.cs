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
	public float explosion_max_damage = 0f;
	public DamageTypeDef damage_def;
	public SoundDef explode_sound;
	public ThingDef thing_to_spawn;
	public ThingDef projectile_to_launch = null;
	public int projectile_count;
	
	public List<IntVec3> trigger_spots;

	
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
					IntVec3 corrected_pos = pos.RotatedBy(this.rotation);
					foreach (Pawn pawn in Find.Map.thingGrid.ThingsAt(this.Position +corrected_pos).OfType<Pawn>()){
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
				this.animated_thing_def.play = false;
			}
			else{
				armed = true;
				this.set_frame("_Armed");
				this.animated_thing_def.play = true;
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
			if (this.mine_def.projectile_to_launch != null){
				foreach (IntVec3 targ in this.mine_def.trigger_spots){
				for ( int i = 0; i< this.mine_def.projectile_count; i++){
					Projectile proj = (Projectile)GenSpawn.Spawn(this.mine_def.projectile_to_launch, this.Position);
						proj.Launch(this, new TargetPack(targ.RotatedBy(this.rotation)+this.Position));
				}
				}
			}
			
			//spawn gas if we need to
			if (this.mine_def.gas_to_spawn != null){
				Gas.try_place_Gas(this.Position, (GasDef)this.mine_def.gas_to_spawn, this.mine_def.gas_thickness, this.Faction);
			}
			}
	}
	
	public class Glitterworld_Trap : Mine{
		public override void Detonate ()
		{
			this.Destroy();
			int rand_int = Rand.Range(0,8);
			switch (rand_int){
				case 0:
				Find.History.AddGameEvent("The glitterworld trap seems to have vanished without doing anything.", GameEventType.Good, true);
				break;
				case 1:
				Find.History.AddGameEvent("The glitterworld trap seems to have vanished without doing anything.", GameEventType.Good, true);
				foreach (Pawn pawn in Find.ListerPawns.PawnsHostileToColony){
					if (pawn.gender == Gender.Male){
						pawn.gender = Gender.Female;
					}
					else if (pawn.gender == Gender.Female){
						pawn.gender = Gender.Male;
					}
				}
				break;
				case 2:
				Find.History.AddGameEvent("The glitterworld trap seems to have identified and frozen all hostile entities in stasis.", GameEventType.Good, true);
				foreach (Pawn pawn in Find.ListerPawns.PawnsHostileToColony){
					pawn.stances.stunner.Notify_DamageApplied(new DamageInfo( DamageTypeDefOf.Stun,300, this), false);
				}
				break;
				
				case 3:
				Find.History.AddGameEvent("The glitterworld trap seems to have triggerd some sort of psychic weapon on some enemies. Some of them may have gone insane.", GameEventType.Good, true);
				foreach (Pawn pawn in Find.ListerPawns.PawnsHostileToColony){
					if (Rand.Range(0,2) != 0){
						pawn.psychology.mood.CurLevel = 0;
					}
				}
				break;
			case 4:
				Find.History.AddGameEvent("The glitterworld trap seems to have frozen everyone in stasis!", GameEventType.BadUrgent, true);
				foreach (Pawn pawn in Find.ListerPawns.AllPawns){
					pawn.stances.stunner.Notify_DamageApplied(new DamageInfo( DamageTypeDefOf.Stun,500, this), false);
				}
				break;
				
			case 5:
				Find.History.AddGameEvent("The glitterworld trap seems to have created a gigantic explosion!", GameEventType.BadUrgent, true);
				ExplosionInfo explosion = new ExplosionInfo();
				explosion.radius = Rand.Range(5, 20);
				explosion.dinfo = new DamageInfo(DamageTypeDefOf.Bomb, (int)Rand.Range(200, 1000), this);
				explosion.center = this.Position;
				explosion.explosionSound = this.mine_def.explode_sound;
				explosion.Explode();
				break;
				
			case 6:
				Find.History.AddGameEvent("The glitterworld trap seems to have emmited some sort of substance!", GameEventType.BadUrgent, true);
					Gas.try_place_Gas(this.Position, (GasDef)GasDef.Named("Poison_Gas"), Rand.Range(20000, 90000));
				break;
			case 7:
				Find.History.AddGameEvent("The glitterworld trap seems to have identified and killed every enemy in sight! What horrific tecnology!", GameEventType.Good, true);
				List<Pawn> pawn_list = Find.ListerPawns.PawnsHostileToColony.ToList();
				foreach (Pawn pawn in pawn_list){
				pawn.TakeDamage(new DamageInfo(DamageTypeDefOf.Bomb, 5000, this));
				}
					
				break;
			case 8:
				Find.History.AddGameEvent("The glitterworld trap seems to have caused random enemies to vanish!", GameEventType.Good, true);
				foreach (Pawn pawn in Find.ListerPawns.PawnsHostileToColony){
					if (Rand.Range(0,10) < 3){
					pawn.Destroy();
					}
				}
				break;
			}
		}
	
	}

}
