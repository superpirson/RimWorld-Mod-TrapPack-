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
namespace TrapPack
{
	public class Graphic_Animation : Graphic_Linked
	{
		public Graphic_Animation (Graphic subGraphic)
		{
			this.subGraphic = subGraphic;
		}
		//
		// Fields
		//
		protected Graphic subGraphic;
		
		//
		// Properties
		//
		public override LinkDrawerType LinkerType
		{
			get
			{
				return LinkDrawerType.Basic;
			}
		}
		
		
		//
		// Methods
		//
		public override Graphic GetColoredVersion (Shader newShader, Color newColor, Color newColorTwo)
		{
			return new Graphic_Linked (this.subGraphic.GetColoredVersion (this.shader, newColor, newColorTwo));
		}
		
		protected Material LinkedDrawMatFrom (Thing parent, IntVec3 cell)
		{
			int num = 0;
			int num2 = 1;
			foreach (IntVec3 current in cell.AdjacentSquaresCardinal ())
			{
				if (this.ShouldLinkWith (current, parent))
				{
					num += num2;
				}
				num2 *= 2;
			}
			LinkDirections linkSet = (LinkDirections)num;
			Material mat = this.subGraphic.MatSingleFor (parent);
			return MaterialAtlasPool.SubMaterialFromAtlas (mat, linkSet);
		}
		
		public override void Print (SectionLayer layer, Thing thing)
		{
			Material mat = this.LinkedDrawMatFrom (thing, thing.Position);
			Printer_Plane.PrintPlane (layer, thing.TrueCenter (), new Vector2 (1f, 1f), mat, 0f, false, null, null);
		}
		
		public override bool ShouldLinkWith (IntVec3 sq, Thing parent)
		{
			if (!sq.InBounds ())
			{
				return (parent.def.linkFlags & LinkFlags.MapEdge) != LinkFlags.None;
			}
			return (LinkGrid.LinkFlagsAt (sq) & parent.def.linkFlags) != LinkFlags.None;
		}
	}
}

