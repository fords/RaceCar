using System;
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


		public enum SteeringDirection { Left = 0, Middle, Right }
		public enum ThrottleState { Slow = 0, Coast = 1, Fast = 2 }
		public enum FutureState { Left = 0, Middle = 1, Right = 2 }
		//public enum CurrentDirection { Negative = 0, Middle =1, Positive =2 }
		// TODO create some Fuzzy Set enumeration types, and member variables for:
		// Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
		// Rule Sets for each output.
		// Also, create some methods to instantiate each of the member variables
		FuzzySet<SteeringDirection> steeringDirectionSet;
		FuzzyRuleSet<SteeringDirection> steeringDirectionRuleSet;
		FuzzySet<FutureState> futureSet;
		FuzzySet<ThrottleState> speedSet;
		FuzzyRuleSet<ThrottleState> speedRulesSet;

		private FuzzySet<ThrottleState> GetSpeedSet()
		{
			IMembershipFunction slowFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(20f, 0f), 70f);
			IMembershipFunction coastFx = new TriangularMembershipFunction(new Coords(15f, 0f), new Coords(35f, 1f), new Coords(40f, 0f));
			IMembershipFunction fastFx = new ShoulderMembershipFunction(-1f, new Coords(30f, 0f), new Coords(50f, 1f), 70f);

			FuzzySet<ThrottleState> set = new FuzzySet<ThrottleState>();
			set.Set(new FuzzyVariable<ThrottleState>(ThrottleState.Slow, slowFx));
			set.Set(new FuzzyVariable<ThrottleState>(ThrottleState.Coast, coastFx));
			set.Set(new FuzzyVariable<ThrottleState>(ThrottleState.Fast, fastFx));
			return set;
		}

		private FuzzySet<SteeringDirection> GetSteeringSet()
		{
			//new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			IMembershipFunction leftFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			//IMembershipFunction leftSlightFx = new TrapezoidMembershipFunction(new Coords(-0.1f, 0f), new Coords(-0.05f, 1f), new Coords(-.02f, 1f), new Coords(0f, 0f));
			IMembershipFunction middleFx = new TrapezoidMembershipFunction(new Coords(-0.425f, 0f), new Coords(-0.32f, 1f), new Coords(0.32f, 1f), new Coords(0.425f, 0f));
			//IMembershipFunction rightSlightFx = new TrapezoidMembershipFunction(new Coords(0f, 0f), new Coords(.02f, 1f), new Coords(.05f, 1f), new Coords(0.10f, 0f));
			IMembershipFunction rightFx = new ShoulderMembershipFunction(-1f, new Coords(0f, 0f), new Coords(1f, 1f), 1f);

			FuzzySet<SteeringDirection> set = new FuzzySet<SteeringDirection>(); // Trying opposite left and right
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Left, leftFx));
			//set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Middle, leftSlightFx));
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Middle, middleFx));
			//set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Middle, rightSlightFx));
			set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Right, rightFx));
			return set;
		}


		private FuzzySet<FutureState> GetFutureSet()
		{

			IMembershipFunction leftFx = new TrapezoidMembershipFunction(new Coords(-12.5f, 0f), new Coords(-7f, 1f), new Coords(0.5f, 1f), new Coords(0f, 0f));
			IMembershipFunction middleFx1 = new ShoulderMembershipFunction(-30f, new Coords(-30f, 1f), new Coords(-16.5f, 0f), 30f);
			IMembershipFunction middleFx2 = new ShoulderMembershipFunction(-30f, new Coords(16.5f, 0f), new Coords(30f, 1f), 30f);
			IMembershipFunction rightFx = new TrapezoidMembershipFunction(new Coords(0f, 0f), new Coords(0.5f, 1f), new Coords(7f, 1f), new Coords(12.5f, 0f));

			FuzzySet<FutureState> set = new FuzzySet<FutureState>();
			set.Set(new FuzzyVariable<FutureState>(FutureState.Left, leftFx));
			set.Set(new FuzzyVariable<FutureState>(FutureState.Middle, middleFx1));
			set.Set(new FuzzyVariable<FutureState>(FutureState.Middle, middleFx2));
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
			FuzzyRule<SteeringDirection>[] rules = new FuzzyRule<SteeringDirection>[6];
			// rule 0: if negative, turn left
			rules[0] = SteeringDirection.Left.Expr().Then(SteeringDirection.Left);
			rules[1] = SteeringDirection.Middle.Expr().Then(SteeringDirection.Middle); //mid
			rules[2] = SteeringDirection.Right.Expr().Then(SteeringDirection.Right);

			rules[3] = FutureState.Right.Expr().Then(SteeringDirection.Left); // if curve right , turn left
			rules[4] = FutureState.Middle.Expr().Then(SteeringDirection.Middle);

			rules[5] = FutureState.Left.Expr().Then(SteeringDirection.Right);
			//rules[5] = FutureState.Left.Expr().Then(SteeringDirection.Right);

			//rules[6] = ThrottleState.Slow.Expr().Then(ThrottleState.Medium);
			//rules[7] = ThrottleState.Medium.Expr().Then(ThrottleState.Medium);
			//rules[8] = ThrottleState.Fast.Expr().Then(ThrottleState.Medium);

			return rules;
		}


		private FuzzyRule<ThrottleState>[] GetSpeedRules()
		{
			FuzzyRule<ThrottleState>[] rules = new FuzzyRule<ThrottleState>[3];
			rules[0] = ThrottleState.Slow.Expr().Then(ThrottleState.Fast);
			rules[1] = ThrottleState.Coast.Expr().Then(ThrottleState.Coast);
			rules[2] = ThrottleState.Fast.Expr().Then(ThrottleState.Coast);
			return rules;
		}

		private FuzzyRuleSet<SteeringDirection> GetDirectionRuleSet(FuzzySet<SteeringDirection> steer)
		{
			var rules = this.GetDirectionRules();
			return new FuzzyRuleSet<SteeringDirection>(steer, rules);
		}

		private FuzzyRuleSet<ThrottleState> GetSpeedRulesSet(FuzzySet<ThrottleState> throttle)
		{
			var rules = this.GetSpeedRules();
			return new FuzzyRuleSet<ThrottleState>(throttle, rules);
		}


		protected override void Awake()
		{
			base.Awake();

			StudentName = "Z W";

			// Only the AI can control. No humans allowerd!
			IsPlayer = false;

			//initialize a bunch of Fuzzy stuff here
			steeringDirectionSet = this.GetSteeringSet();
			futureSet = this.GetFutureSet();
			steeringDirectionRuleSet = this.GetDirectionRuleSet(steeringDirectionSet);
			speedSet = this.GetSpeedSet();
			speedRulesSet = this.GetSpeedRulesSet(speedSet);
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

			Throttle = .15f; //.22f
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
			float dist = pathTracker.distanceTravelled + Speed * 2.5f;
			//Debug.Log("distance: " + pathTracker.pathCreator.path.GetPointAtDistance(dist));
			float curveAhead = Vector3.SignedAngle(transform.position, pathTracker.pathCreator.path.GetPointAtDistance(dist), Vector3.up);
			float curveDistance = Mathf.Sign(curveAhead) * pathTracker.pathCreator.path.GetPointAtDistance(dist).magnitude;

			Debug.Log("Before calculation speed: " + this.Speed);
			//Throttle = speedRulesSet.Evaluate(this.Speed, inputs);
			Debug.Log("speed: " + Throttle);
			//Debug.Log("vehicle position: " + carPosition);
			Debug.Log(pathTracker.pathCreator.path.GetPointAtDistance(dist).magnitude);
			Debug.Log("Difference distance btwn car and road " + (transform.position - pathTracker.pathCreator.path.GetPointAtDistance(dist)).magnitude);
			//if (curveAhead > 0)
			//{
			//	//distanceFromRoad = Mathf.Sign(angle) * (carPosition - pathTracker.closestPointOnPath);
			//	Debug.Log("curve on right side");
			//}
			//else
			//{
			//	//distanceFromRoad = -1 * (carPosition - pathTracker.closestPointOnPath);
			//	Debug.Log("on left  side");

			//}



			Steering = steeringDirectionRuleSet.Evaluate(distance, inputs);
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
