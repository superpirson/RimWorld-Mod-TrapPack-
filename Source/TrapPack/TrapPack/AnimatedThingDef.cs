using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;
using RimWorld;
using TrapPack;

public class AnimatedThingDef : ThingDef
{
	public string graphicPathAnimated;
	public List<TrapPack.Frame> frames;
	public bool play = true;
	public override void PostLoad ()
	{
			
		base.PostLoad ();
		if (!this.graphicPathAnimated.NullOrEmpty ())
		{
			this.graphic = new Graphic_Animated(this.frames,this.graphicPathAnimated,this.shader, this.defaultColor, this.defaultColorTwo, this);
		}else{
		Log.Message(this.defName + " has no animated graphic path, but is an animated thing!");
		}
		
	}
	//depreciated helper function, will remove!!!
		public void set_frame(string new_frame){
			((Graphic_Animated)this.graphic).set_frame(new_frame);
		}

}
