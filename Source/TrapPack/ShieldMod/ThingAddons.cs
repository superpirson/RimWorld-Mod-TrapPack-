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
	public bool play = true;
}


namespace ThingAddons
{ 
	public class Frame{
		public Frame(Material material){
			this.material = material;
		}
		public Frame(){
		}
		public Material material = Verse.BaseContent.BadMat;
		private string tex_name_intenal = "err, bad tex string";
		public string tex_name {
			get {
				return tex_name_intenal;
			}
			set{
				tex_name_intenal = value;
				material = new Material(VerseBase.MatBases.Cutout);
				material.mainTexture = ContentFinder<Texture2D>.Get (tex_name_intenal, true);
 			
			}
		}
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
		public Frame current_frame;
		
		//this is how long the animatior should wait before starting to cycle
		public int wait_ticks = 0;
		
		public override void SpawnSetup(){
			
			this.animated_thing_def = (AnimatedThingDef)this.def;
			
			if (this.animated_thing_def.frames == null || this.animated_thing_def.frames.Count <= 0){
				this.animated_thing_def.frames = new List<Frame>();
			if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
			{
				Log.Error("Animated thing tried to find texture folder, but it was empty/null!");
				return;
			}
			else if (this.def.folderDrawMats.Count == 1)
			{
				Log.Warning("Animated thing tried to find texture folder, but found only one texture.");
					this.animated_thing_def.frames.Add(new Frame(this.def.folderDrawMats[0]));
					this.current_frame = this.animated_thing_def.frames[0];
			}
			else{
					foreach (Material mat in this.def.folderDrawMats){
						this.animated_thing_def.frames.Add(new Frame(mat));
					}
			}
			}
			this.current_frame = this.animated_thing_def.frames[0];
	
			base.SpawnSetup();
		}	
		public override void Tick(){
	
			if (wait_ticks > 0){
				wait_ticks--;
				base.Tick();
				return;
			}
			if (tick_count++ < current_frame.frame_delay){
			base.Tick (); return;
			}
			tick_count = 0;
			
			this.def.drawMat = current_frame.material;
			Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
			if (!this.animated_thing_def.play){base.Tick();return;}

			current_frame = current_frame.get_next_frame(animated_thing_def.frames);
			base.Tick ();
		}
		public override void Draw ()
		{
		//	Log.Message("thingaddon's Draw was called!");	
			this.Comps_Draw ();
			base.Draw ();
			//Log.Message("draw was called!");
		}
		public override Material DrawMat (IntRot rot)
		{
			return this.current_frame.material;
		}
	}

	public class AnimatedBuilding : Building{
		//this is an exact copy of animatedThing, but a it extends buildings
		//animations are drawn on top of the thing's textures.
		//animations are drawn on top of the thing's textures.
		private int tick_count = 0;
		protected AnimatedThingDef animated_thing_def;
		public Frame current_frame;
		
		//this is how long the animatior should wait before starting to cycle
		public int wait_ticks = 0;
		
		public override void SpawnSetup(){
			
			this.animated_thing_def = (AnimatedThingDef)this.def;
			
			if (this.animated_thing_def.frames == null || this.animated_thing_def.frames.Count <= 0){
				this.animated_thing_def.frames = new List<Frame>();
				if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
				{
					Log.Error("Animated thing tried to find texture folder, but it was empty/null!");
					return;
				}
				else if (this.def.folderDrawMats.Count == 1)
				{
					Log.Warning("Animated thing tried to find texture folder, but found only one texture.");
					this.animated_thing_def.frames.Add(new Frame(this.def.folderDrawMats[0]));
					this.current_frame = this.animated_thing_def.frames[0];
				}
				else{
					foreach (Material mat in this.def.folderDrawMats){
						this.animated_thing_def.frames.Add(new Frame(mat));
					}
				}
			}
			this.current_frame = this.animated_thing_def.frames[0];
			base.SpawnSetup();
		}	
		public override void Tick(){
			
			if (wait_ticks > 0){
				wait_ticks--;
				base.Tick();
				return;
			}
			if (tick_count++ < current_frame.frame_delay){
				base.Tick (); return;
			}
			tick_count = 0;
			
			this.def.drawMat = current_frame.material;
			Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
			if (!this.animated_thing_def.play){base.Tick();return;}
			
			current_frame = current_frame.get_next_frame(animated_thing_def.frames);
			base.Tick ();
		}
		public override void Draw ()
		{
			//	Log.Message("thingaddon's Draw was called!");	
			this.Comps_Draw ();
			base.Draw ();
			//Log.Message("draw was called!");
		}
		public override Material DrawMat (IntRot rot)
		{
			return this.current_frame.material;
		}
	}
	}