﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {

	/**Class that helps to create the initial polyline **/
	public class InitialPolyline : Polyline { 

		private int mActualPos = 0; //Actual vertex position

		//******** Constructors ********//
		public InitialPolyline() : base() {}
		public InitialPolyline(int numV) : base(numV){}

		//******** Setters ********//
		public void initializeIndices() {
			for (int i = 0; i < mVertices.Length; ++i) {
				mVertices [i].setIndex (i);
			}
		}

		public void addPosition(Vector3 newPos) {
			if (mActualPos >= mVertices.Length) { //TODO:exception
				Debug.Log ("Number of index bigger than size");
				return;
			}
			mVertices[mActualPos].setPosition(newPos);
			++mActualPos;
		}

		public void addVertex(Vertex newV) {
			if (mActualPos >= mVertices.Length) { //TODO:exception
				Debug.Log ("Number of index bigger than size");
				return;
			}
			mVertices [mActualPos] = new Vertex(newV);
			++mActualPos;
		}

		public void generateUVs () {
			for (int i = 0; i < mVertices.Length; ++i) {
				//TODO:take into account distance between vertices
				mVertices[i].setUV(new Vector2((float)i/(float)(mVertices.Length-1),0.0f));
			}
		}
	}
}