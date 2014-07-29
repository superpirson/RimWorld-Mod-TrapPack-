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
		private int max_frames = 1;
		private int tick_count = 0;
		public bool draw_Comps_First = true;
		public bool cycleing = true;
		public bool loop = true;
		public int current_frame = 0;
		public int inter_frame_delay = 1;
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
			this.max_frames = this.def.folderDrawMats.Count;
			base.SpawnSetup();
		}

		public override void Draw ()
		{
			if (draw_Comps_First){
				this.Comps_Draw ();
			}
				if (cycleing && tick_count++ >= inter_frame_delay){
				tick_count = 0;
				if (loop ){
				current_frame = (current_frame +1) % max_frames;
				}
				else{
					current_frame = Math.Max(max_frames, current_frame+1);
					cycleing = false;
				}
				}
			if (!draw_Comps_First){
				this.Comps_Draw ();
			}

		}

		public override Material DrawMat (IntRot rot)
		{
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
}
