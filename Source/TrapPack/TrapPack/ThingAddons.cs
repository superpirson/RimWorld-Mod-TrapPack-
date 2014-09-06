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
	public bool play = true;
	//last_frame is used only to keep track of the animation order if the frames are not explictildy defed

	public override void PostLoad(){
		base.PostLoad();
		//do the blueprint text:
		this.blueprintMat = new Material(VerseBase.MatBases.Blueprint);
		if (!this.blueprintTexturePath.NullOrEmpty()){
			Log.Message("makeing a blueprint drawmat useing tex at " + this.blueprintTexturePath);
			this.blueprintMat.mainTexture = ContentFinder<Texture2D>.Get (this.blueprintTexturePath, true);
		}
		else{
			Log.Message("trying to autogen a blueprint mat");
			this.blueprintMat.mainTexture = ContentFinder<Texture2D>.GetAllInFolder(this.textureFolderPath).RandomElement();
		}
	
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
		if (this.frames.Count <= 0){
			if (this.folderDrawMats == null || this.folderDrawMats.Count <= 0)
			{
				Log.Error("Animated thing tried to find texture folder, but it was empty/null!");
				return;
			}
			else if (this.folderDrawMats.Count == 1)
			{
				Log.Warning("Animated thing tried to find texture folder, but found only one texture.");
				this.frames.Add(new ThingAddons.Frame(this.folderDrawMats[0]));
			}
			else{
			try{
				int i = 0;
				foreach (Material mat in this.folderDrawMats){
					ThingAddons.Frame quick_fix_frame = new ThingAddons.Frame(mat);
					quick_fix_frame.tex_name = ("Unamed frame " + i.ToString());
					
					this.frames.Insert(i, quick_fix_frame);
					quick_fix_frame.next_frame = this.frames[0].tex_name;
					//set the prev frame's next_frame to this
					if (i != 0){
						this.frames[i-1].next_frame = quick_fix_frame.tex_name;
					}
					this.frame_hashmap.Add("Unamed frame " + i.ToString(), quick_fix_frame);
					i++;
				}
				}catch (Exception e){
					Log.Error("hi, this was my fault. damn. (error loading quick fix frames for object " + this.defName +" ) threw a    " + e.Message);
					this.frames.Clear();
					this.frames.Add(new ThingAddons.Frame(BaseContent.BadMat));	
					this.frame_hashmap.Add("Unamed frame error" , frames[0]);
				return;
				
				}
			}
		}
	
		
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
		public bool play = true;
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
			this.current_frame = this.animated_thing_def.frames[0];
			this.play = this.animated_thing_def.play;
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
			
			
			if (current_frame.next_frame != null && this.play){
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
		public bool play = true;
			private int tick_count = 0;
		protected AnimatedThingDef animated_thing_def;
		public Frame current_frame;
		
		//this is how long the animatior should wait before starting to cycle
		public int wait_ticks = 0;
		
		
		public void set_frame(string new_frame){
				current_frame = (Frame)this.animated_thing_def.frame_hashmap[new_frame];
			if (current_frame == null){
				Log.Error("error, tried to set frame to " + new_frame + " but found null!");
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
			this.current_frame = this.animated_thing_def.frames[0];
			this.play = this.animated_thing_def.play;
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
			
			
			if (current_frame.next_frame != null && this.play){
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
	
	public class AnimatedBuilding_WorkTable : Building_WorkTable{
		//this is an exact copy of animatedThing, but a it extends buildings
		//animations are drawn on top of the thing's textures
		//animations are drawn on top of the thing's textures
		//this is an exact copy of animatedThing, but a it extends buildings
		//animations are drawn on top of the thing's textures
		public bool play = true;
		private int tick_count = 0;
		protected AnimatedThingDef animated_thing_def;
		public Frame current_frame;
		
		//this is how long the animatior should wait before starting to cycle
		public int wait_ticks = 0;
		
		
		public void set_frame(string new_frame){
			current_frame = (Frame)this.animated_thing_def.frame_hashmap[new_frame];
			if (current_frame == null){
				Log.Error("error, tried to set frame to " + new_frame + " but found null!");
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
			this.current_frame = this.animated_thing_def.frames[0];
			this.play = this.animated_thing_def.play;
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
			
			
			if (current_frame.next_frame != null && this.play){
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