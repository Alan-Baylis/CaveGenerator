﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** Generates the cave by recursive calls each time a hole is done **/
public class RecursiveGenerator : AbstractGenerator {

	void Awake() {
		base.Awake ();
	}
		
	public override IEnumerator generate(Polyline originPoly, float holeProb) {
		//Hole is done, update the counter
		--maxHoles;

		//Case base is implicit, as the operation generation takes into account the maxHoles variables in order to stop generating holes

		//TODO: change maxExtrudeTimes as holes are done (eg, random number between a rank)

		//Generate the actual hallway/tunnel
		Geometry.Mesh m = initializeTunnel(ref originPoly);

		ExtrusionOperations actualOperation = DecisionGenerator.Instance.generateNewOperation (originPoly);
		//Add initial polyline to the BB
		IntersectionsController.Instance.addPolyline(originPoly);
		for (int i = 0; i < maxExtrudeTimes; ++i) {
			//In case the hole is finally not done, same operation will need to be applied
			ExtrusionOperations actualOpBackTrack = new ExtrusionOperations(actualOperation);
			//Generate the new polyline applying the corresponding operation
			Polyline newPoly = extrude (actualOperation, originPoly);
			if (newPoly == null) { //Intersection produced
				//TODO: improve this
				//DecisionGenerator.Instance.generateNextOperation(originPoly, ref actualOperation,i,holeProb);
				//actualOperation = DecisionGenerator.Instance.generateNewOperation (originPoly);
				continue;
			}
			//Make hole?
			if (actualOperation.holeOperation ()) {
				Polyline polyHole = makeHole (originPoly, newPoly);
				if (polyHole != null) { //Check the hole was done without problems
					IntersectionsController.Instance.addPolyline(newPoly);
					IntersectionsController.Instance.addActualBox ();
					actualOperation.setCanIntersect (IntersectionsController.Instance.getLastBB ()); //Avoid intersection check with own extrusion BB
					if (showGeneration)
						yield return StartCoroutine(generate (polyHole, holeProb - 0.001f));
					else 
						StartCoroutine(generate (polyHole, holeProb - 0.001f));
					//IntersectionsController.Instance.addPolyline (originPoly);
					actualMesh = m;
				} else { //No hole could be done, reextrude will smaller distance
					//Force to have little extrusion distance
					actualOpBackTrack.distanceOperation().forceOperation(1, DecisionGenerator.Instance.generateDistance (false));
					//It can't be null if with bigger extrusion distance it wasn't already: if
					//with bigger distance it didn't intersect, it can't intersect with a smaller one
					newPoly = extrude (actualOpBackTrack, originPoly);
					actualOperation = actualOpBackTrack;
					actualMesh.addPolyline (newPoly);
				}
				actualOperation.forceHoleOperation (false);
			} else {
				//Adds the new polyline to the mesh, after all the changes previously done
				actualMesh.addPolyline (newPoly);
			}
			//Triangulate from origin to new polyline as a tube/cave shape
			actualMesh.triangulatePolylines (originPoly, newPoly);
			//Make stalagmite?
			if (actualOperation.stalagmiteOperation ().needApply()) {
				makeStalagmite (actualOperation.stalagmiteOperation().apply(), originPoly, newPoly);
			}
			//Make light?
			if (actualOperation.pointLightOperation().needApply()) {
				actualOperation.pointLightOperation().apply();
				makePointLight(originPoly,newPoly);
			}
			//Set next operation and continue from the new polyline
			originPoly = newPoly;
			//Add actual polyline to the next intersection BB and get next operation
			IntersectionsController.Instance.addPolyline(originPoly);
			DecisionGenerator.Instance.generateNextOperation(originPoly, actualOperation,i,holeProb, maxHoles);
			if (showGeneration) {
				gameObject.GetComponent<CaveGenerator> ().updateMeshes (this);
				yield return null;
			} //else do nothing
		}
		//Finally, close the actual hallway/tunnel
		IntersectionsController.Instance.addActualBox ();
		actualMesh.closePolyline(originPoly);
		if (m == proceduralMesh [1]) {
			finished = true;
			gameObject.GetComponent<CaveGenerator> ().updateMeshes (this);
		}
	}

}