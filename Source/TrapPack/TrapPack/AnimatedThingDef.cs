using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;
using RimWorld;
namespace TrapPack
{
public class AnimatedThingDef : ThingDef
{
	public string graphicPathAnimated;
	public List<TrapPack.Frame> frames;
	public override void PostLoad ()
	{
		base.PostLoad ();
		if (!this.graphicPathAnimated.NullOrEmpty ())
		{
			this.graphic = new Graphic_Animated(this.frames,this.graphicPathAnimated,this.shader, this.defaultColor, this.defaultColorTwo);
		}
	}

}
}