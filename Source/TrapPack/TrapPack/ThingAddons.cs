// ------------------------------------------------------------------------------
// Written by Alex Patton
// Released under the Zlib licence
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using Verse.Sound;
using Verse;
using RimWorld;
using System.Collections;
using System.Collections.Generic;


public class AnimatedThingDef : ThingDef{
	public Hashtable frame_hashmap;
	public List<ThingAddons.Frame> frames;
	//last_frame is used only to keep track of the animation order if the frames are not explictildy defed
	public bool play = true;
	public override void PostLoad(){
		frame_hashmap = new Hashtable();
		if (frames == null){
			frames = new List<ThingAddons.Frame>();
		}
		foreach (ThingAddons.Frame frame in frames){
			if (frame.tex_name == null){
				Log.Error("failed to get a frame texture!");
				continue;
			}
			
	Material material = new Material(VerseBase.MatBases.Cutout);
			material.mainTexture = ContentFinder<Texture2D>.Get (this.textureFolderPath + frame.tex_name, true);
			frame.material = material;
			frame_hashmap.Add (frame.tex_name, frame);
	}
		base.PostLoad();
	}	
}


namespace ThingAddons
{ 
	public class Frame{
		
		public Frame(Material material){
			this.material = material;
		}
		public Frame(){
			material = Verse.BaseContent.BadMat;
			tex_name = "Default_frame_name";
		}
		public Material material;
		public string tex_name ;
		public string next_frame;
		public int frame_delay = 0;
	}
	
	public  class AnimatedThing : ThingWithComponents
	{
		//this is an exact copy of animatedThing, but a it extends buildings
		//animations are drawn on top of the thing's textures
		private int tick_count = 0;
		protected AnimatedThingDef animated_thing_def;
		public Frame current_frame;
		
		//this is how long the animatior should wait before starting to cycle
		public int wait_ticks = 0;
		
		
		public void set_frame(string new_frame){
		try{
			current_frame = (Frame)this.animated_thing_def.frame_hashmap[new_frame];
			}catch (NullReferenceException e){
				Log.Message("exception, tried to set frame to " + new_frame + " but found null! exception: " + e.Message);
				current_frame = new Frame();
			}
			this.def.drawMat = current_frame.material;
			Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
		}
		
		
		public override void SpawnSetup(){
			if (this.def is AnimatedThingDef){
				this.animated_thing_def = (AnimatedThingDef)this.def;
			}else{
				// make a new animated object for just this context
				this.animated_thing_def = new AnimatedThingDef();
				this.animated_thing_def.texturePath = this.def.texturePath;
			}
			
			
			if (this.animated_thing_def.frames.Count <= 0){
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
					this.animated_thing_def.play = false;
				}
				else{
				int i = 0;
					foreach (Material mat in this.def.folderDrawMats){
						Frame quick_fix_frame = new Frame(mat);
						quick_fix_frame.tex_name = ("Unamed frame " + i.ToString());
						
						this.animated_thing_def.frames.Insert(i, quick_fix_frame);
						quick_fix_frame.next_frame = this.animated_thing_def.frames[0].tex_name;
						//set the prev frame's next_frame to this
						if (i != 0){
						this.animated_thing_def.frames[i-1].next_frame = quick_fix_frame.tex_name;
						}
						this.animated_thing_def.frame_hashmap.Add("Unamed frame " + i.ToString(), quick_fix_frame);
						i++;
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
			
			
			if (current_frame.next_frame != null && this.animated_thing_def.play){
				this.set_frame(this.current_frame.next_frame);
			}
			base.Tick ();
			
		}
		public override void Draw ()
		{	
			this.Comps_Draw ();
			base.Draw ();
		}
		public override Material DrawMat (IntRot rot)
		{
			return this.current_frame.material;
		}
	}

	public class AnimatedBuilding : Building{
		//this is an exact copy of animatedThing, but a it extends buildings
		//animations are drawn on top of the thing's textures
		//animations are drawn on top of the thing's textures
		//this is an exact copy of animatedThing, but a it extends buildings
		//animations are drawn on top of the thing's textures
		private int tick_count = 0;
		protected AnimatedThingDef animated_thing_def;
		public Frame current_frame;
		
		//this is how long the animatior should wait before starting to cycle
		public int wait_ticks = 0;
		
		
		public void set_frame(string new_frame){
				current_frame = (Frame)this.animated_thing_def.frame_hashmap[new_frame];
			if (current_frame == null){
				Log.Message("exception, tried to set frame to " + new_frame + " but found null!");
				current_frame = new Frame();
			}
			this.def.drawMat = current_frame.material;
			Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
		}
		
		
		public override void SpawnSetup(){
			if (this.def is AnimatedThingDef){
				this.animated_thing_def = (AnimatedThingDef)this.def;
			}else{
				// make a new animated object for just this context
				this.animated_thing_def = new AnimatedThingDef();
				this.animated_thing_def.texturePath = this.def.texturePath;
			}
			
			
			if (this.animated_thing_def.frames.Count <= 0){
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
					this.animated_thing_def.play = false;
				}
				else{
					int i = 0;
					foreach (Material mat in this.def.folderDrawMats){
						Frame quick_fix_frame = new Frame(mat);
						quick_fix_frame.tex_name = ("Unamed frame " + i.ToString());
						
						this.animated_thing_def.frames.Insert(i, quick_fix_frame);
						quick_fix_frame.next_frame = this.animated_thing_def.frames[0].tex_name;
						//set the prev frame's next_frame to this
						if (i != 0){
							this.animated_thing_def.frames[i-1].next_frame = quick_fix_frame.tex_name;
						}
						this.animated_thing_def.frame_hashmap.Add("Unamed frame " + i.ToString(), quick_fix_frame);
						i++;
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
			
			
			if (current_frame.next_frame != null && this.animated_thing_def.play){
				this.set_frame(this.current_frame.next_frame);
			}
			base.Tick ();
			
		}
		public override void Draw ()
		{	
			this.Comps_Draw ();
			base.Draw ();
		}
		public override Material DrawMat (IntRot rot)
		{
			return this.current_frame.material;
		}
	}
	}