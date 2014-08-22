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
	//--misc
	public class Building_Electrified_Floor : Building
	{
		protected static readonly SoundDef zapSound = SoundDef.Named("short_zap");
		protected static readonly SoundDef explosion_sound = SoundDef.Named ("Explosion_Bomb");
		private static Texture2D texUI_Arm = ContentFinder<Texture2D>.Get("UI/Commands/UI_Arm", true);
		private static Texture2D texUI_Overcharge = ContentFinder<Texture2D>.Get("UI/Commands/UI_Overcharge", true);
		private static Texture2D texUI_Pain = ContentFinder<Texture2D>.Get("UI/Commands/UI_Pain", true);
		private static Texture2D texUI_Disarm = ContentFinder<Texture2D>.Get("UI/Commands/UI_Disarm", true);

		const int POWERDRAW = 800000;
		public enum Floor_Mode
		{
			disarmed,
			kill,
			pain,
			overcharge,
		}
		
		Floor_Mode current_floor_mode;
		// globals
		private CompPowerTrader power_Trader;
		private bool has_power = true;
		uint tick_delay = 0;
		static DamageTypeDef elec_damage_type = DefDatabase<DamageTypeDef>.GetNamed("elec_damage_type");
		/// <summary>
		/// taken from powerswitch mod by Haplo
		/// This creates new selection buttons with a new graphic
		/// </summary>
		
		
		/// <returns></returns>
		public override IEnumerable<Command> GetCommands()
		{

			Command_Action kill;
				kill = new Command_Action();
				kill.icon = texUI_Arm;
			if (current_floor_mode == Floor_Mode.kill){
				kill.disabled = true;
			}	else{
				kill.disabled = false;
				}
				kill.defaultDesc = "Arms floor. The floor draws a lot of power when armed, so watch out!";
				kill.activateSound = SoundDef.Named("Click");
			kill.action = () => (this.current_floor_mode = Floor_Mode.kill);
				kill.groupKey = 313123004;
			yield return kill;
			
			Command_Action disarm;
				disarm = new Command_Action();
				disarm.icon = texUI_Disarm;
			if (current_floor_mode == Floor_Mode.disarmed){
				disarm.disabled = true;
			}
			else{
				disarm.disabled = false;
			}
				disarm.defaultDesc = "Disarms the floor, making it safe to walk on again.";
				disarm.activateSound = SoundDef.Named("Click");
			disarm.action= () => (this.current_floor_mode = Floor_Mode.disarmed);
				disarm.groupKey = 313123005;
			yield return disarm;
			

			Command_Action pain;
				pain = new Command_Action();
			pain.icon = texUI_Pain;
			if (current_floor_mode == Floor_Mode.pain){
				pain.disabled = true;
			}
			else{
				pain.disabled = false;
			}
			pain.defaultDesc = "Sets the floor to only ouput non-lethal shocks.";
				pain.activateSound = SoundDef.Named("Click");
			pain.action= () => (this.current_floor_mode = Floor_Mode.pain);
				pain.groupKey = 313123006;
			yield return pain;
			
			Command_Action overcharge;
				overcharge = new Command_Action();
			overcharge.icon = texUI_Overcharge;
			if (current_floor_mode == Floor_Mode.overcharge){
				overcharge.disabled = true;
			}
			else{
				overcharge.disabled = false;
			}
				overcharge.defaultDesc = "Overcharges the trap. RUN!";
				overcharge.activateSound = SoundDef.Named("Click");
			overcharge.action = () => (this.current_floor_mode = Floor_Mode.overcharge);
				overcharge.groupKey = 313123007;
			yield return overcharge;
			
			
		
			
			
			
			base.GetCommands();
			}
		public override void SpawnSetup(){
			base.SpawnSetup();
			power_Trader = this.GetComp<CompPowerTrader>();
			if (this.PowerComp == null)
			{
				Log.Error("TrapsPack: Failed to retrieve power component upon spawn for an electrofied floor!");
			}
			if (zapSound == null || explosion_sound == null)
			{
				Log.Error("TrapsPack: Failed to load sound componet, sound is NULL!");
			}
		}
		public override void Tick()
		{
			power_Trader.powerOutput = 0f;
			if (tick_delay-- > 0){
				return;
			}
			if (power_Trader == null)
			{
				Log.Error("power was null on a tick of a electrofloor, returning.");
				base.Tick ();
				return;
			}
			if (power_Trader.PowerNet == null){
				// Log.Message("powernet was null!");
			}
			if (!this.power_Trader.PowerOn || this.power_Trader.PowerNet.CurrentStoredEnergy() < POWERDRAW * CompPower.WattsToWattDaysPerTick){
				// attempt to refresh, so on the next tick we got something
				this.power_Trader = this.GetComp<CompPowerTrader>();
				has_power = false; 
				tick_delay = 10;
				return;
			}
			else{
				has_power = true;
			}
			//	Log.Message(this.powerComp.DebugString + "powernet clames to have : "+ this.powerNet.CurrentStoredEnergy().ToString());
			bool activated = false;
			if (this.current_floor_mode != Floor_Mode.disarmed) {
				this.power_Trader.powerOutput = -POWERDRAW/1000;
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				
				foreach (Thing target in things){
					if (target is Pawn && !target.Destroyed){
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						this.power_Trader.powerOutput = -POWERDRAW;
						Pawn pawn = (Pawn)target;
						List<BodyDefPart> bodyparts = pawn.healthTracker.bodyModel.GetNotMissingParts().ToList();
						
						//decide what to do to our poor victiam!
						switch (this.current_floor_mode){
							case Floor_Mode.kill:
							foreach (BodyDefPart part in bodyparts.InRandomOrder()){
								if (part.def.activities != null &&  part.def.activities.Contains("BloodPumping")){
									pawn.TakeDamage(new DamageInfo(elec_damage_type, 1, this, new BodyPartDamageInfo(part, false)));
									break;
								}
							}
							((Pawn)target).stances.stunner.Notify_DamageApplied(new DamageInfo( DamageTypeDefOf.Stun,3, this), false);
							explosion_sound.PlayOneShot(this.Position);
							break;
							case Floor_Mode.overcharge:
							BodyDefPart targit_part = bodyparts.RandomElement();
							pawn.TakeDamage(new DamageInfo(elec_damage_type, 10, this, new BodyPartDamageInfo(targit_part, false)));
							this.TakeDamage(new DamageInfo(DamageTypeDefOf.Breakdown, 5, this));
							((Pawn)target).stances.stunner.Notify_DamageApplied(new DamageInfo( DamageTypeDefOf.Stun,3, this), false);
							
							explosion_sound.PlayOneShot(this.Position);
							break;
							case Floor_Mode.pain:
							pawn.healthTracker.bodyModel.ExtraPain = 50;
							((Pawn)target).stances.stunner.Notify_DamageApplied(new DamageInfo( DamageTypeDefOf.Stun,2, this), false);
							zapSound.PlayOneShot(this.Position);
							break;
						}
						
						activated = true;
					}
				}
				if (!activated){
					//we diden't catch anyone, but do an effect anyways and wait.
					float rand = UnityEngine.Random.Range(15,30);
					if (rand < 20){
						zapSound.PlayOneShot(this.Position);
						IntVec3 shock_pos = this.Position;
						 GenSpawn.Spawn(ThingDef.Named("Zap_Effect"),shock_pos);
					}
					tick_delay += (uint)rand;
				}	
				else{
					//if we caught someone, begin chosueing how to shock them
					if (UnityEngine.Random.Range(0,20) < 1f && activated == true){
						//if we happen to get less than 1, we will let him go free, he will be givin a random number of ticks to escape
						tick_delay += (uint) UnityEngine.Random.Range(60,200);
					}
					else{
						//else, wait only a short time before shocking him again.
					}
				}
				
			}
			// min tick delay is 4.
			tick_delay += 4;
			base.Tick();

		}
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			//stringBuilder.Append(base.GetInspectString());
			if (!has_power){
				stringBuilder.Append("Insufficient power to arc once! System shutdown.");
			}
			return stringBuilder.ToString();
		}
		public override void Draw ()
		{
		}
	}
}
