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
public class Mine_Def : ThingDef {

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
	public string armed_effect_texture_path;
	
	public bool has_sensor = false;
	public bool checks_for_frendly = false;
	public bool can_trigger = true;
}



namespace TrapPack
{


	
	//--mines
	public class Mine : Building
	{
		protected  Texture2D texUI_Arm;
		protected  Texture2D texUI_Disarm;
		protected  Texture2D texUI_Trigger;
		protected  Texture2D tex_Armed_Effect;
		protected static Material Armed_Mat;
	
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
			Armed_Mat = VerseBase.MatBases.MetaOverlay;
			Armed_Mat.mainTexture = tex_Armed_Effect;
			tex_Armed_Effect = ContentFinder<Texture2D>.Get(this.mine_def.armed_effect_texture_path, true);
			texUI_Trigger = ContentFinder<Texture2D>.Get(this.mine_def.trigger_ui_texture_path, true);
			texUI_Disarm = ContentFinder<Texture2D>.Get(this.mine_def.disarm_ui_texture_path, true);
			texUI_Arm = ContentFinder<Texture2D>.Get(this.mine_def.arm_ui_texture_path, true);
		}
		catch (NullReferenceException e){
			Log.Error("Mine object tried to load it's textures from it's minedef and failed, threw : " + e.Message);
			
			texUI_Arm = BaseContent.BadTex;
			texUI_Disarm =BaseContent.BadTex;
			texUI_Trigger =BaseContent.BadTex;
			tex_Armed_Effect = BaseContent.BadTex;
			Armed_Mat = BaseContent.BadMat;
		}
			base.SpawnSetup();
		}
		
		public override void Tick()
		{
			if (armed) {
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn){
						if (!this.mine_def.checks_for_frendly || target.Faction != this.Faction){
							Detonate();
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
			if (this.mine_def.has_sensor){
				
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
			}
			else{
				armed = true;
			}
		}
		public override void Draw()
		{
			if (armed){
				//from thing's draw code
				Quaternion quaternion;
				if (this.def.graphic == null)
				{
					quaternion = this.rotation.AsQuat;
				}
				else
				{
					quaternion = Quaternion.identity;
				}
				
				Graphics.DrawMesh (this.DrawMesh, this.DrawPos, quaternion, Armed_Mat, 0);
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
			}
	}

/*
	public class Building_Mine : Mine
	{
		static DamageTypeDef mine_damage_type = DefDatabase<DamageTypeDef>.GetNamed("mine_damage_type");
		public override void Tick()
		{
			if (armed) {
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn){
						Detonate();
					}
				}
			}
			base.Tick();
		}

		public override void Detonate(){
			explodeSound.PlayOneShot(this.Position);
			Destroy();
			ExplosionInfo e = default(ExplosionInfo);
			e.center = this.Position;
			e.dinfo = new DamageInfo( mine_damage_type, (int) UnityEngine.Random.Range(20,140), this);
			e.radius = 1.5f;
			e.Explode();

		}
	}
	public class Building_Mine_Incend : Mine
	{

		public override void Tick()
		{
			if (armed) {
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn){
						Detonate();
					}
				}
			}
			base.Tick();
		}
		public override void Detonate(){
			fireSound.PlayOneShot(this.Position);
			Destroy();
			ExplosionInfo e = default(ExplosionInfo);
			e.center = this.Position;
			e.dinfo = new DamageInfo( DamageTypeDefOf.Flame, (int) UnityEngine.Random.Range(20,30), this);
			e.radius = UnityEngine.Random.Range(4,5);
			e.Explode();
		}
	}
	public class Building_Smart_Mine : Mine
	{
		static DamageTypeDef s_mine_damage_type = DefDatabase<DamageTypeDef>.GetNamed("s_mine_damage_type");
		public override void Tick()
		{
			if (armed) {
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn && target.Faction != this.Faction){
					Detonate();
					}
				}
			}
			base.Tick();
		}
	public override void Detonate(){
		explodeSound.PlayOneShot(this.Position);
		Destroy();
		ExplosionInfo e = default(ExplosionInfo);
		e.center = this.Position;
		e.dinfo = new DamageInfo( s_mine_damage_type, (int) UnityEngine.Random.Range(20,140), this);
		e.radius = 1.5f;
		e.Explode();		
	}
	}

	public class Building_FireBomb : Mine
	{	
		public override IEnumerable<Command> GetCommands()
		{
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

		public override void Detonate(){
						fireSound.PlayOneShot(this.Position);
						Destroy();
						ExplosionInfo e = default(ExplosionInfo);
						e.center = this.Position;
						e.dinfo = new DamageInfo( DamageTypeDefOf.Flame, (int) UnityEngine.Random.Range(2,20), this);
						e.radius = UnityEngine.Random.Range(0,2);
						e.Explode();
		}
	}
	
//*/
	public class Building_Gas_Mine : Mine
	{
		//protected int ticks_until_next_puff = 0;
	//	protected int puff_count = 0;
	//	const int max_puffs = 5;

		public override void Tick()
		{
			if (armed) {
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn){
						Detonate();
					}
				}
			}
			base.Tick();
		}
		public override void Detonate(){
			Poison_Gas.try_place_Poison_Gas(this.Position, 1000);
			this.Destroy();
		}
	}




}
