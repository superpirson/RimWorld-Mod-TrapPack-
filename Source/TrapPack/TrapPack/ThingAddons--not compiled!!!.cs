// ------------------------------------------------------------------------------
// Written by Alex Patton
// Released under the Zlib licence
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using VerseBase;
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
		/*
		//do the blueprint text:
		this.blueprintMat = new Material(MatBases.LightOverlay);
		if (!this.blueprintTexturePath.NullOrEmpty()){
			Log.Message("makeing a blueprint drawmat useing tex at " + this.blueprintTexturePath);
			this.blueprintMat.mainTexture = ContentFinder<Texture2D>.Get (this.blueprintTexturePath, true);
		}
		else if(!this.graphicPathSingle.NullOrEmpty()){
			Log.Message("trying to autogen a blueprint mat");
			this.blueprintMat.mainTexture = ContentFinder<Texture2D>.GetAllInFolder(this.textureFolderPath).RandomElement();
		}
		*/
		if (this.graphicPathSingle == null){
			Log.Message("had to return due to a null graphicpathsingle in " + this.defName);
		return;
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
			
	Material material = new Material(MatBases.EdgeShadow);
			material.mainTexture = ContentFinder<Texture2D>.Get (this.graphicPathSingle + frame.tex_name, true);
			frame.material = material;
			frame_hashmap.Add (frame.tex_name, frame);
	}
		if (this.frames.Count <= 0){
		Log.Error("ohgod, no frames on " + this.defName);
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
		public Material drawMat = BaseContent.BadMat;
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
			//Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
		}
		
		
		public override void SpawnSetup(){
			if (this.def is AnimatedThingDef){
				this.animated_thing_def = (AnimatedThingDef)this.def;
			}else{
				// make a new animated object for just this context
				this.animated_thing_def = new AnimatedThingDef();
				this.animated_thing_def.graphicPathSingle = this.def.graphicPathSingle;
			}
			this.current_frame = this.animated_thing_def.frames[0];
			this.play = this.animated_thing_def.play;
			base.SpawnSetup();
		}	
		public override void Tick(){
			
			base.Tick ();
			
		}
		public override void Draw ()
		{	
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
			Mesh mesh = null;
			if (this.Rotation == IntRot.west)
			{
				mesh= MeshPool.gridPlanesFlip [this.RotatedSize.x, this.RotatedSize.z];
			} else{
				mesh= MeshPool.gridPlanes [this.RotatedSize.x, this.RotatedSize.z];
			}
			Graphics.DrawMesh( mesh,this.Position.ToVector3(), this.Rotation.AsQuat, this.current_frame.material, 0);
		}
	
		
	}

	public class AnimatedBuilding : Building{
		public Material drawMat = BaseContent.BadMat;
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
			//Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
		}
		
		
		public override void SpawnSetup(){
			if (this.def is AnimatedThingDef){
				this.animated_thing_def = (AnimatedThingDef)this.def;
			}else{
				// make a new animated object for just this context
				this.animated_thing_def = new AnimatedThingDef();
				this.animated_thing_def.graphicPathSingle = this.def.graphicPathSingle;
			}
			this.current_frame = this.animated_thing_def.frames[0];
			this.play = this.animated_thing_def.play;
			base.SpawnSetup();
		}	
		public override void Tick(){
			
			base.Tick ();
			
		}
		public override void Draw ()
		{	
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
			Mesh mesh = null;
			if (this.Rotation == IntRot.west)
			{
				mesh= MeshPool.gridPlanesFlip [this.RotatedSize.x, this.RotatedSize.z];
			} else{
				mesh= MeshPool.gridPlanes [this.RotatedSize.x, this.RotatedSize.z];
			}
			Graphics.DrawMesh( mesh,this.Position.ToVector3(), this.Rotation.AsQuat, this.current_frame.material, 0);
		}
		
	}
	
	public class AnimatedBuilding_WorkTable : Building_WorkTable{
		public Material drawMat = BaseContent.BadMat;
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
			//Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
		}
		
		
		public override void SpawnSetup(){
			if (this.def is AnimatedThingDef){
				this.animated_thing_def = (AnimatedThingDef)this.def;
			}else{
				// make a new animated object for just this context
				this.animated_thing_def = new AnimatedThingDef();
				this.animated_thing_def.graphicPathSingle = this.def.graphicPathSingle;
			}
			this.current_frame = this.animated_thing_def.frames[0];
			this.play = this.animated_thing_def.play;
			base.SpawnSetup();
		}	
		public override void Tick(){
			
			base.Tick ();
			
		}
		public override void Draw ()
		{	
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
			Mesh mesh = null;
			if (this.Rotation == IntRot.west)
			{
				mesh= MeshPool.gridPlanesFlip [this.RotatedSize.x, this.RotatedSize.z];
			} else{
				mesh= MeshPool.gridPlanes [this.RotatedSize.x, this.RotatedSize.z];
			}
			Graphics.DrawMesh( mesh,this.Position.ToVector3(), this.Rotation.AsQuat, this.current_frame.material, 0);
		}
		
	}
	}