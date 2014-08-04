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

	//--mines
	public abstract class Mine : Building
	{
		protected static readonly SoundDef fireSound = SoundDef.Named("mine_explosion"); 
		protected static readonly SoundDef explodeSound = SoundDef.Named("mine_explosion");
		protected static Texture2D texUI_Arm = ContentFinder<Texture2D>.Get("UI/Commands/UI_Arm", true);
		protected static Texture2D texUI_Disarm = ContentFinder<Texture2D>.Get("UI/Commands/UI_Disarm", true);
		protected static Texture2D texUI_Trigger = ContentFinder<Texture2D>.Get("UI/Commands/UI_Trigger", true);
		protected static Texture2D tex_Armed_Effect = ContentFinder<Texture2D>.Get("Things/Armed_Effect", true);
		protected static Material Armed_Mat;

		// globals
		public bool armed = false; 
		public bool changed = false;
		 static Mine(){
			Armed_Mat = VerseBase.MatBases.MetaOverlay;
			Armed_Mat.mainTexture = tex_Armed_Effect;
		}
		/// <summary>
		/// taken from powerswitch mod by Haplo
		/// This creates new selection buttons with a new graphic
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Command> GetCommands()
		{
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
			Log.Error(this.ToString() + "  just called an unimplimented detonate method!");
			}
	}


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
	public class Building_Gas_Mine : Mine
	{
		public bool spraying = false;
		//protected int ticks_until_next_puff = 0;
	//	protected int puff_count = 0;
	//	const int max_puffs = 5;

		public override void Tick()
		{


			/* old code idea, diden't work :(
			 if (spraying){
				if (ticks_until_next_puff-- < 0){
					Log.Message("spraed a puff!");
					Smoke.try_place_smoke(this.Position.RandomAdjSquare8Way());
					ticks_until_next_puff = Rand.Range(50,100);
				
				if (puff_count++ >= max_puffs){
					this.Destroy();
				}
				}
				return;
			}
			//*/
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
			spraying = true;
			Poison_Gas.try_place_Poison_Gas(this.Position, 1000);
			this.Destroy();
		}
	}


//*/


}
