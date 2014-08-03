// ------------------------------------------------------------------------------
// Written by Alex Patton
// Released under the Zlib licence
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using Verse.Sound;
using Verse;
using RimWorld;

namespace ThingAddons
{ 
	public  class AnimatedThing : ThingWithComponents
	{
		//animations are drawn on top of the thing's textures.

		private int max_frames = 0;
		private int tick_count = 0;
		public bool cycleing = true;
		public bool loop = true;
		public int current_frame = 0;
		public int inter_frame_delay = 3;
		public override void SpawnSetup(){
			if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
			{
				Log.Error("Animated thing tried to find texture folder, but it was empty/null!");
				return;
			}
			if (this.def.folderDrawMats.Count == 1)
			{
				Log.Warning("Animated thing tried to find texture folder, but found only one texture.");
			}
			this.max_frames = this.def.folderDrawMats.Count -1;
			base.SpawnSetup();
		}
		public override void Tick(){
			if (!cycleing){base.Tick();return;}
			if (tick_count++ >= inter_frame_delay){
				tick_count = 0;
				if (loop ){
					current_frame++;
					if( current_frame > max_frames){
						current_frame = 0;
					}
				}
				else{
					current_frame = Math.Min(max_frames, current_frame+1);
					cycleing = false;
				}
			}
			this.def.drawMat = this.def.folderDrawMats[current_frame];
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
			if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
			{
				return this.def.DrawMat (rot);
			}
		
			// ---- the game gets the textures in the texture folder in alphabetical order, I recomend nameing your textures thing1, thing2, thing3 if there are many frames
			return this.def.folderDrawMats[current_frame];
		}
		public void play_Once(int start_Frame = 0){
			current_frame = start_Frame;
			cycleing = true;
			loop = false;
			
		}
	}

//*/
}
