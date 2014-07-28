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
		private uint max_frames = 1;
		private uint tick_count = 0;
		public bool loop = true;
		public uint current_frame = 0;
		public uint inter_frame_delay = 1;
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
			this.max_frames = (uint)this.def.folderDrawMats.Count;
			base.SpawnSetup();
		}

		public override void Draw ()
		{
			if (tick_count++ >= inter_frame_delay){
				tick_count = 0;
				if loop 
				current_frame = (current_frame +1) % max_frames;
			}
			this.Comps_Draw ();
		}

		public override Material DrawMat (IntRot rot)
		{
			if (this.def.folderDrawMats == null || this.def.folderDrawMats.Count <= 0)
			{
				return this.def.DrawMat (rot);
			}
		
			// ---- the game gets the textures in the texture folder in alphabetical order, I recomend nameing your textures thing1, thing2, thing3 if there are many frames

			return this.def.folderDrawMats[(int)current_frame];
		}

public void play(){

}
public void play(int startFrame, int endFrame){

}




	}
}
