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
/*
	//--pit traps
    public class Building_Pit_Trap : Building
    {
		// globals

      
		static DamageTypeDef trap_damage_type;
		static Building_Pit_Trap(){
			trap_damage_type = new DamageTypeDef();
			trap_damage_type.deathMessage = "{0} fell to their death in a spiked trap";
			trap_damage_type.incapChanceMultiplier = 1000;
		}

		public override void SpawnSetup(){
			base.SpawnSetup();
		}
        public override void Tick()
        {
			List<Thing> things = new List<Thing>();
			things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
				if (target is Pawn){
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						target.TakeDamage(new DamageInfo( trap_damage_type, 10, this));
						
					((Pawn)target).healthTracker.ForceIncap();
						
					Destroy();
					GenSpawn.Spawn(ThingDef.Named("Building_Tripped_Pit_Trap"), this.Position);
					}
			

	
			}

			base.Tick();
                
		}
        public override void DrawExtraSelectionOverlays()
        {
           // GenDraw.DrawRadiusRing(base.Position, 1.0);
        }
	    public override string GetInspectString()
	    {
	    	StringBuilder stringBuilder = new StringBuilder();
	    	//stringBuilder.Append(base.GetInspectString());
           stringBuilder.Append("looks nasty!");

	    	return stringBuilder.ToString();
	    }
        //Saving game
        public override void ExposeData()
        {
			base.ExposeData();
        }
    }
	public class Building_Tripped_Pit_Trap : Building
{	
	
	public override void SpawnSetup(){
			def.tickerType = TickerType.Normal;
			base.SpawnSetup();
	}
	
	public override void Tick()
	{
		base.Tick();
	}
	public override void DrawExtraSelectionOverlays()
	{
		// GenDraw.DrawRadiusRing(base.Position, 1.0);
	}
	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		//stringBuilder.Append(base.GetInspectString());
		stringBuilder.Append("dang!");
		
		return stringBuilder.ToString();
	}
	//Saving game
	public override void ExposeData()
	{
		base.ExposeData();
	}

}
*/
	//--mines
	public abstract class Mine : Building
	{
		protected static readonly SoundDef fireSound = SoundDef.Named("mine_explosion"); 
		protected static readonly SoundDef explodeSound = SoundDef.Named("mine_explosion");
		private static Texture2D texUI_Arm = ContentFinder<Texture2D>.Get("UI/Commands/UI_Arm", true);
		private static Texture2D texUI_Disarm = ContentFinder<Texture2D>.Get("UI/Commands/UI_Disarm", true);
		private static Texture2D tex_Armed_Effect = ContentFinder<Texture2D>.Get("Things/Armed_Effect", true);
		private static Material Armed_Mat;
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

		public override void SpawnSetup(){
			base.SpawnSetup();
		}
		public override void Tick(){
			base.Tick();	
		}
		public override void Draw()
		{
					base.Draw();	
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
	//		if (changed){
	//			Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
	//			changed = false;
	//		}

	
		}
//		public override Material DrawMat(Verse.IntRot rot)
//		{
//				//Log.Message(this.def.folderDrawMats.Count.ToString());
//				if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
//				{
//					Log.Error("ERROR, drawmatfolder is null for a mine!");
//					return null;
//				}
//				if (this.def.folderDrawMats.Count == 1)
//				{
//					return this.def.folderDrawMats[0];
//				}
//				//folderDrawMats is a list filled with textures picked from the folder in alphabeticil order, sinced the armed tex is always followed by _armed, we can assume it is always [1]
//				if (armed)
//				{
//				return this.def.folderDrawMats [1];
//				}
//				return this.def.folderDrawMats [0];
//		}
	}

	public class Building_Mine : Mine
	{
		static DamageTypeDef mine_damage_type;
		static Building_Mine(){
			mine_damage_type = new DamageTypeDef();
			mine_damage_type.deathMessage = "{0} was violently dispersed over 10 KM by a mine.";
		}
		public override void Tick()
		{
			if (armed) {
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn){
						//--------I used the soundInteract to define the explosion sound.
						explodeSound.PlayOneShot(this.Position);
						
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						Destroy();
						Explosion e = default(Explosion);
						e.center = this.Position;
						e.dinfo = new DamageInfo( mine_damage_type, (int) UnityEngine.Random.Range(20,140), this);
						e.radius = 1.5f;
						e.Explode();
					}
				}
			}
			base.Tick();
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
						//--------I used the soundInteract to define the explosion sound.
						fireSound.PlayOneShot(this.Position);
						
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						Destroy();
						Explosion e = default(Explosion);
						e.center = this.Position;
						e.dinfo = new DamageInfo( DamageTypeDefOf.Flame, (int) UnityEngine.Random.Range(5,20), this);
						e.radius = UnityEngine.Random.Range(1,2);

						e.Explode();
					}
				}
			}
			base.Tick();
		}
	}
	public class Building_Smart_Mine : Mine
	{
		static DamageTypeDef s_mine_damage_type;
		static Building_Smart_Mine(){
			s_mine_damage_type = new DamageTypeDef();
			s_mine_damage_type.deathMessage = "{0} was inteligently dispersed over 10 KM by a smart mine.";	
		}
		public override void Tick()
		{
			if (armed) {
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				foreach (Thing target in things){
					if (target is Pawn && target.Faction != this.Faction){
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						explodeSound.PlayOneShot(this.Position);
						//--------I used the soundInteract to define the explosion sound.
						Destroy();
						Explosion e = default(Explosion);
						e.center = this.Position;
						e.dinfo = new DamageInfo( s_mine_damage_type, (int) UnityEngine.Random.Range(20,140), this);
						e.radius = 1.5f;
						e.Explode();			
					}
				}
			}
			base.Tick();
		}
	}
}