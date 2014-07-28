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
	public class AnimatedThing : ThingWithComponents
	{
		public uint current_frame = 0;
		public uint inter_frame_delay = 1;
		public override void Draw ()
		{

			this.Comps_Draw ();
		}


public void play(){

}
public void play(int startFrame, int endFrame){

}




	}
}