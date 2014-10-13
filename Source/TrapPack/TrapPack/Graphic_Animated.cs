using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;
using RimWorld;
using System.Collections;
namespace TrapPack
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
	
	public class Graphic_Animated : Graphic
	{
		public Hashtable frame_hashmap;
		public bool play = true;
		public Frame current_frame;
		public List<TrapPack.Frame> frames;
				public string GraphicPath
		{
			get
			{
				return this.initPath;
			}
		}
		//
		// Constructors
		//
		public Graphic_Animated (List<TrapPack.Frame> frames,string graphicPath, Shader shader, Color color, Color colorTwo)
		{
			this.frame_hashmap = new Hashtable();
			this.shader = shader;
			this.color = color;
			this.initPath = graphicPath;
			this.colorTwo = colorTwo;
			if (frames.NullOrEmpty()){
			int idx = 0;
			foreach (Texture2D tex in ContentFinder<Texture2D>.GetAllInFolder(this.initPath)){
				Material material = new Material(MatBases.EdgeShadow);
				material.mainTexture = tex;
				Frame newFrame = new Frame(material);
				frames.Add(newFrame);
				if (idx > 0){
				frames[idx-1].next_frame = newFrame.tex_name;
				}
				idx++;
			}
			//set the last element to point to the first for looping
				frames[idx-1].next_frame = frames[0].tex_name;
			}
			foreach(Frame frame in frames){
				frame_hashmap.Add(frame.tex_name,frame);
			}
			//set current frame To 0
			current_frame = frames[0];
			
			/*
			Texture2D[] array2 = new Texture2D[3];
			if (shader.SupportsMaskTex ())
			{
				array2 [0] = ContentFinder<Texture2D>.Get (graphicPath + "_backm", false);
				if (array2 [0] != null)
				{
					array2 [1] = ContentFinder<Texture2D>.Get (graphicPath + "_sidem", false);
					if (array2 [1] == null)
					{
						array2 [1] = array2 [0];
					}
					array2 [2] = ContentFinder<Texture2D>.Get (graphicPath + "_frontm", false);
					if (array2 [2] == null)
					{
						array2 [2] = array2 [0];
					}
				}
			}
			*/
		}
		
		//
		// Methods
		//
		public override Graphic GetColoredVersion (Shader newShader, Color newColor, Color newColorTwo)
		{
			return new Graphic_Animated(this.frames,this.initPath, newShader, newColor, newColorTwo);
		}
		
		public override int GetHashCode ()
		{
			int num = this.initPath.GetHashCode () * 7553;
			num ^= this.color.GetHashCode () * 921;
			return num ^ this.colorTwo.GetHashCode () * 511;
		}
		
		public override string ToString ()
		{
			return string.Concat (new object[]
			                      {
				"Graphic_Animated(initPath=",
				this.initPath,
				", color=",
				this.color,
				", colorTwo=",
				this.colorTwo,
				")"
			});
		}
		public void set_frame(string new_frame){
			current_frame = (Frame)this.frame_hashmap[new_frame];
			if (current_frame == null){
				Log.Message("exception, tried to set frame to " + new_frame + " but found null!");
				current_frame = new Frame();
			}
			//Find.MapDrawer.MapChanged(this.Position, MapChangeType.Things);
		}
		public  void DrawWorker (Vector3 loc, IntRot rot, ThingDef thingDef, Thing thing)
		{
			bool overdraw;
			IntVec2 size;
			if (thingDef != null)
			{
				overdraw = thingDef.overdraw;
				size = thingDef.size;
			}
			else
			{
				overdraw = false;
				size = new IntVec2 (1, 1);
			}
			Mesh mesh = this.MeshAt (rot, size, overdraw);
			Quaternion rotation = this.QuatFromRot (rot);
			Material material = this.current_frame.material;
			Graphics.DrawMesh (mesh, loc, rotation, material, 0);
			set_frame(current_frame.next_frame);
		}
	}
}