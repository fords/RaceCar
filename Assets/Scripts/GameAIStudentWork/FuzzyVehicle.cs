﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameAI;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;

namespace GameAICourse
{

	public class FuzzyVehicle : AIVehicle
	{
		public float DB_throttle;
		public float DB_steering;
	

		public enum SteeringDirection { Left = 0, Middle = 1, Right = 2 }
		public enum ThrottleState { Slow = 0, Medium = 1, Fast = 2 }
		public enum FutureState { Left = 0, Middle = 1, Right = 2 }
		//public enum CurrentDirection { Negative = 0, Middle =1, Positive =2 }
		// TODO create some Fuzzy Set enumeration types, and member variables for:
		// Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
		// Rule Sets for each output.
		// Also, create some methods to instantiate each of the member variables
		FuzzySet<SteeringDirection> steeringDirectionSet;
		FuzzyRuleSet<SteeringDirection> steeringDirectionRuleSet;
		FuzzySet<FutureState> futureSet;


		private FuzzySet<SteeringDirection> GetSpeedSet()
		{
			IMembershipFunction leftFx = new ShoulderMembershipFunction(0f, new Coords(20f, 1f), new Coords(35f, 0f), 70f);
			IMembershipFunction middleFx = new TriangularMembershipFunction(new Coords(25f, 0f), new Coords(35f, 1f), new Coords(45f, 0f));
			IMembershipFunction rightFx = new ShoulderMembershipFunction(0f, new Coords(35f, 0f), new Coords(45f, 1f), 70f);

			FuzzySet<SteeringDirection> set = new FuzzySet<SteeringDirection>();
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Left, leftFx));
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Middle, middleFx));
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Right, rightFx));
			return set;
		}

		private FuzzySet<SteeringDirection> GetSteeringSet()
		{
			//new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			IMembershipFunction leftFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			IMembershipFunction middleFx = new TrapezoidMembershipFunction(new Coords(-0.45f, 0f), new Coords(-0.30f, 1f), new Coords(0.30f, 1f), new Coords(0.45f, 0f));
			IMembershipFunction rightFx = new ShoulderMembershipFunction(-1f, new Coords(0f, 0f), new Coords(1f, 1f), 1f);

			FuzzySet<SteeringDirection> set = new FuzzySet<SteeringDirection>(); // Trying opposite left and right
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Left, leftFx));
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Middle, middleFx));
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Right, rightFx));
			return set;
		}


		private FuzzySet<FutureState> GetFutureSet()
		{
			IMembershipFunction leftFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(-0.3f, 0f), 1f);
			IMembershipFunction middleFx = new TrapezoidMembershipFunction(new Coords(-0.45f, 0f), new Coords(-0.30f, 1f), new Coords(0.30f, 1f), new Coords(0.45f, 0f));
			IMembershipFunction rightFx = new ShoulderMembershipFunction(-1f, new Coords(-0.3f, 0f), new Coords(1f, 1f), 1f);

			FuzzySet<FutureState> set = new FuzzySet<FutureState>();
			set.Set(new FuzzyVariable<FutureState>(FutureState.Left, leftFx));
			set.Set(new FuzzyVariable<FutureState>(FutureState.Middle, middleFx));
			set.Set(new FuzzyVariable<FutureState>(FutureState.Right, rightFx));
			return set;
		}

		//private FuzzySet<CurrentDirection> GetCurrentDirectionSet()
		//{
		//    //new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
		//    IMembershipFunction leftFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
		//    IMembershipFunction middleFx = new TriangularMembershipFunction(new Coords(-0.5f, 0f), new Coords(0f, 1f), new Coords(0.5f, 0f));
		//    IMembershipFunction rightFx = new ShoulderMembershipFunction(0f, new Coords(0f, 0f), new Coords(1f, 1f), 1f);

		//    FuzzySet<CurrentDirection> set = new FuzzySet<CurrentDirection>();
		//    set.Set(new FuzzyVariable<CurrentDirection>(CurrentDirection.Negative, leftFx));
		//    set.Set(new FuzzyVariable<CurrentDirection>(CurrentDirection.Middle, middleFx));
		//    set.Set(new FuzzyVariable<CurrentDirection>(CurrentDirection.Positive, rightFx));
		//    return set;
		//}

		private FuzzyRule<SteeringDirection>[] GetDirectionRules()
		{
			FuzzyRule<SteeringDirection>[] rules = new FuzzyRule<SteeringDirection>[3];
			// rule 0: if negative, turn left
			rules[0] = SteeringDirection.Left.Expr().Then(SteeringDirection.Left);
			rules[1] = SteeringDirection.Middle.Expr().Then(SteeringDirection.Middle); //mid
			rules[2] = SteeringDirection.Right.Expr().Then(SteeringDirection.Right); // if pos, turn right
			//rules[3] = FutureState.Right.Expr().Then(SteeringDirection.Left); // if curve right , turn left
			//rules[4] = FutureState.Middle.Expr().Then(SteeringDirection.Middle);
			//rules[5] = FutureState.Left.Expr().Then(SteeringDirection.Right);
			return rules;
		}


		private FuzzyRuleSet<SteeringDirection> GetDirectionRuleSet(FuzzySet<SteeringDirection> steer)
		{
			var rules = this.GetDirectionRules();
			return new FuzzyRuleSet<SteeringDirection>(steer, rules);
		}


		protected override void Awake()
		{
			base.Awake();

			StudentName = "Zey W";

			// Only the AI can control. No humans allowerd!
			IsPlayer = false;

			//initialize a bunch of Fuzzy stuff here
			steeringDirectionSet = this.GetSteeringSet();
			futureSet = this.GetFutureSet();
			steeringDirectionRuleSet = this.GetDirectionRuleSet(steeringDirectionSet);
			//var currentDirectionSet = this.GetCurrentDirectionSet();
			//var SteeringRuleSet = this.GetSteeringSet();
			//FuzzyValueSet inputs = new FuzzyValueSet();
		}


		override protected void Update()
		{

			// Do all your Fuzzy stuff here and pass the defuzzified values to 
			// the car like so:
			// Throttle = someValue; //[-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
			// Steering = someValue; // [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right
			//var desirability = this.GetDesirabilitySet();
			//var distance = this.GetDistanceToTargetSet();

			//distance.Evaluate(200f, inputs);
			//Steering = steeringDirectionRuleSet.Evaluate(inputs);

			//Steering = fzSteeringRuleSet.Evaluate(fzInputValueSet);
			//Throttle = fzThrottleRuleSet.Evaluate(fzInputValueSet);

			Throttle = .35f;
			Steering = 0f;

			//var currentDirectionSet = this.GetCurrentDirectionSet();
			//var steer = this.GetSteeringSet();
			//var steeringRuleSet = this.GetDirectionRuleSet(steer);

			FuzzyValueSet inputs = new FuzzyValueSet();
			//steeringDirectionSet.Evaluate(Vector2.Dot(transform.right, pathTracker.closestPointDirectionOnPath), inputs);


			Vector3 carPosition = transform.position;

			Vector3 delta_vector = transform.position - pathTracker.closestPointOnPath;
			float angle = Vector3.SignedAngle(delta_vector, pathTracker.closestPointDirectionOnPath, Vector3.up);
			//Vector3 distanceFromRoad;
			//distanceFromRoad = Mathf.Sign(angle) * (carPosition - pathTracker.closestPointOnPath);
			float distance = Mathf.Sign(angle) * ((carPosition - pathTracker.closestPointOnPath) / 5).magnitude;



			steeringDirectionSet.Evaluate(distance, inputs);
			float dist = pathTracker.distanceTravelled + Speed * 1.0f;
			//Debug.Log("distance: " + pathTracker.pathCreator.path.GetPointAtDistance(dist));
			float curveAhead = Vector3.SignedAngle(transform.position, pathTracker.pathCreator.path.GetPointAtDistance(dist), Vector3.up);
			float curveDistance = Mathf.Sign(curveAhead) * pathTracker.pathCreator.path.GetPointAtDistance(dist).magnitude;
			futureSet.Evaluate(curveAhead, inputs);

			Debug.Log("vehicle position: " + carPosition);
			Debug.Log(pathTracker.pathCreator.path.GetPointAtDistance(dist).magnitude);
			if (curveAhead > 0)
			{
				//distanceFromRoad = Mathf.Sign(angle) * (carPosition - pathTracker.closestPointOnPath);
				Debug.Log("curve on right side");
			}
			else
			{
				//distanceFromRoad = -1 * (carPosition - pathTracker.closestPointOnPath);
				Debug.Log("on left  side");

			}



			Steering = steeringDirectionRuleSet.Evaluate(inputs);
			//if (angle > 0)
			//{
			//	distanceFromRoad = Mathf.Sign(angle) * (carPosition - pathTracker.closestPointOnPath);
			//             Debug.Log("on left side");
			//         }
			//else
			//{
			//	distanceFromRoad = -1 * (carPosition - pathTracker.closestPointOnPath);
			//             Debug.Log("on right side");

			//         }

			//         bool left = false;
			//         bool right = false;
			//         if (carPosition.z - pathTracker.closestPointOnPath.z > 0)
			//{
			//             Debug.Log("on left side");
			//}
			//         if (carPosition.z - pathTracker.closestPointOnPath.z < 0)
			//{
			//             Debug.Log("on right side");
			//         }

			//Debug.Log("signed angle: " + angle);
			
			//Debug.Log("path position: " + pathTracker.closestPointOnPath);
			//Debug.Log("distance : " + distance);
			//Debug.Log("signed distance from road " + distanceFromRoad);
			//Vector2.Dot(transform.right, pathTracker.closestPointDirectionOnPath);

			/* Bad
            var car2d = new Vector2(transform.position.x, transform.position.y);
            var path2d = new Vector2(pathTracker.closestPointDirectionOnPath.x, pathTracker.closestPointDirectionOnPath.y);
            Debug.Log("angle : " + Vector2.Dot(car2d, path2d));
            */


			//Debug.Log("steering : " + Steering);
			// negative, then turn left
			// pos , turn right
			DB_throttle = Throttle;
			DB_steering = Steering;
		


			//pathTracker.closestPointOnPath
			// recommend you keep the base call at the end, after all your FuzzyVehicle code so that
			// control inputs can be processed properly (Throttle, Steering)


			base.Update();


		}

	}
}
