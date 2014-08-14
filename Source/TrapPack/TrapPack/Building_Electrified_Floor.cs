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
		private static Texture2D texUI_Disarm = ContentFinder<Texture2D>.Get("UI/Commands/UI_Disarm", true);

		const int POWERDRAW = 800000;
		// globals
		private CompPowerTrader power_Trader;
		private bool has_power = true;
		private bool armed = false; 
		uint tick_delay = 0;
		static DamageTypeDef elec_damage_type = DefDatabase<DamageTypeDef>.GetNamed("elec_damage_type");
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
			optX.defaultDesc = "Arms floor. The floor draws a lot of power when armed, so watch out!";
			optX.activateSound = SoundDef.Named("Click");
			optX.action = Arm_Disarm;
			optX.groupKey = 313123004;
			yield return optX;
			base.GetCommands();
		}
		private void Arm_Disarm()
		{
			if (armed){
				armed = false;
			}
			else{
				armed = true;
			}
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
				Log.Message("power was null on a tick of a electrofloor, returning.");
				return;
			}
			if (power_Trader.PowerNet == null){
				Log.Message("powernet was null!");
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
			if (armed) {
				this.power_Trader.powerOutput = -POWERDRAW/1000;
				List<Thing> things = new List<Thing>();
				things.AddRange(Find.Map.thingGrid.ThingsAt(this.Position));
				
				foreach (Thing target in things){
					if (target is Pawn && !target.Destroyed){
						//Log.Message("someone stepd on the trap! doing damage to " + target.ToString());
						this.power_Trader.powerOutput = -POWERDRAW;
						Pawn pawn = (Pawn)target;
						List<BodyDefPart> bodyparts = pawn.healthTracker.bodyModel.GetNotMissingParts().ToList();
						foreach (BodyDefPart part in bodyparts.InRandomOrder()){
							if (part.def.activities != null &&  part.def.activities.Contains("BloodPumping")){
								pawn.TakeDamage(new DamageInfo(elec_damage_type, 1, this, new BodyPartDamageInfo(part, false)));
								break;
							}
						}
						((Pawn)target).stances.stunner.Notify_DamageApplied(new DamageInfo( DamageTypeDefOf.Stun,3, this), false);

						explosion_sound.PlayOneShot(this.Position);
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
