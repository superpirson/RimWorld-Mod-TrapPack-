// ------------------------------------------------------------------------------
// Written by Alex Patton
// Released under the Zlib licence
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using Verse.Sound;
using Verse;
using RimWorld;
using System.Collections.Generic;



public class AnimatedThingDef : ThingDef{
	public List<ThingAddons.Frame> frames;
	public bool loop_around = true;
}


namespace ThingAddons
{ 
	public class Frame{
		public Frame(Material material){
			this.material = material;
		}
		
		public Material material = Verse.BaseContent.BadMat;
		public string tex_name = "UNAMED FRAME!";
		public bool play_through = true;
		public int frame_number = 0;
		public int frame_delay = 0;
		
		private int next_frame_index = -1;
		public Frame get_next_frame(List<Frame> frames){
			if (this.next_frame_index >= 0){
			foreach (Frame frame in frames){
					if (frame.frame_number == this.frame_number +1){
						this.next_frame_index = frames.IndexOf(frame) ;
					}
			}
				//if we still failed, set next frame to zero and loop around
				if (this.next_frame_index < 0){
					this.next_frame_index = 0;
				}
				
			}
				return frames[this.next_frame_index];
		
		}
	
	}

	public  class AnimatedThing : ThingWithComponents
	{
		//animations are drawn on top of the thing's textures.
		private int tick_count = 0;
		protected AnimatedThingDef animated_thing_def;
		
		public List<Frame> frames;
		public Frame current_frame;
		
		//this is how long the animatior should wait before starting to cycle
		public int wait_ticks = 0;
		
		public override void SpawnSetup(){
			
			this.animated_thing_def = (AnimatedThingDef)this.def;
			
			if (frames == null){
			if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
			{
				Log.Error("Animated thing tried to find texture folder, but it was empty/null!");
				return;
			}
			else if (this.def.folderDrawMats.Count == 1)
			{
				Log.Warning("Animated thing tried to find texture folder, but found only one texture.");
					this.frames.Add(new Frame(this.def.folderDrawMats[0]));
			}
			else{
					foreach (Material mat in this.def.folderDrawMats){
						this.frames.Add(new Frame(mat));
					}
			}
			}
			else{
			this.frames = animated_thing_def.frames;
			}
			this.current_frame = frames[0];
			base.SpawnSetup();
		}	
		public override void Tick(){
			if (!this.current_frame.play_through){base.Tick();return;}
			if (wait_ticks > 0){
				wait_ticks--;
				base.Tick();
				return;
			}
			if (tick_count++ < current_frame.frame_delay){
			base.Tick (); return;
			}
			tick_count = 0;
			
			current_frame = current_frame.get_next_frame(frames);
			this.def.drawMat = current_frame.material;
			Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
			base.Tick ();
		}
		public override void Draw ()
		{
		//	Log.Message("thingaddon's Draw was called!");	
			this.Comps_Draw ();
			base.Draw ();
			//Log.Message("draw was called!");
		}	

		public override void PrintOnto(SectionLayer layer)
		{
			base.PrintOnto(layer);
		}

		public override Material DrawMat (IntRot rot)
		{
			//by default, this function does not get called!
			//Log.Message("thingaddon's drawmat was called!");
			if (this.frames == null || this.frames.Count <= 0)
			{
				return this.def.DrawMat (rot);
			}
		
			// ---- the game gets the textures in the texture folder in alphabetical order, I recomend nameing your textures thing1, thing2, thing3 if there are many frames
			return this.current_frame.material;
		}
	}

	public class AnimatedBuilding : Building{
		//this is an exact copy of animatedThing, but a it extends buildings
//*/

}
}
