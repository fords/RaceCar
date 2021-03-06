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
		public enum SpeedState { Slow = 0, Coast = 1, Fast = 2 }
		public enum NearState { Left = 0, Middle = 1, Right = 2 }
		public enum FutureState { Left = 0, Middle = 1, Right = 2 }
		public enum CurrentDirection { Negative = 0, Middle = 1, Positive = 2 }
		public enum AngleState { Low = 0, Medium , Sharp}
		// create some Fuzzy Set enumeration types, and member variables for:
		// Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
		// Rule Sets for each output.
		// Also, create some methods to instantiate each of the member variables
		FuzzySet<CurrentDirection> steeringDirectionSet;
		FuzzySet<SteeringDirection> currentCarSignedDistanceSet;
		FuzzyRuleSet<SteeringDirection> steeringDirectionRuleSet;
		FuzzySet<NearState> nearSet;
		FuzzySet<FutureState> futureSet;
		FuzzySet<SpeedState> speedSet;
		FuzzyRuleSet<SpeedState> speedRulesSet;
		FuzzySet<AngleState> angleSet;

		//FuzzySet<SpeedState> speedSet;
		//FuzzyRuleSet<SpeedState> speedRuleSet;
		FuzzyValueSet inputs;


		//if 102.47 - 150 future angle and 116.866 near angle fall down

		private FuzzySet<AngleState> GetAngleSet()
		{
			IMembershipFunction okayFx = new ShoulderMembershipFunction(0f, new Coords(150f, 1f), new Coords(167f, 0f),200f);
			IMembershipFunction sharpFx = new TriangularMembershipFunction(new Coords(165f, 0f), new Coords(170f, 1f), new Coords(185f, 0f));
			//new ShoulderMembershipFunction(0f, new Coords(150f, 0f), new Coords(160f, 1f), 200f);

			FuzzySet<AngleState> set = new FuzzySet<AngleState>();
			set.Set(new FuzzyVariable<AngleState>(AngleState.Sharp, sharpFx));
			set.Set(new FuzzyVariable<AngleState>(AngleState.Medium, okayFx));
			return set;
		}


		private FuzzySet<SpeedState> GetSpeedSet() 
		{

			IMembershipFunction slowFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0.45f, 0f), 1f);
			IMembershipFunction coastFx = new TriangularMembershipFunction(new Coords(.43f, 0f), new Coords(.46f, 1f), new Coords(.49f, 0f));
			IMembershipFunction fastFx = new ShoulderMembershipFunction(-1f, new Coords(.47f, 0f), new Coords(.52f, 1f), 1f);


			//IMembershipFunction slowFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0.45f, 0f), 1f);
			//IMembershipFunction coastFx = new TriangularMembershipFunction(new Coords(.42f, 0f), new Coords(.51f, 1f), new Coords(.57f, 0f));
			//IMembershipFunction fastFx = new ShoulderMembershipFunction(-1f, new Coords(.55f, 0f), new Coords(.60f, 1f), 1f);

			//IMembershipFunction slowFx = new ShoulderMembershipFunction(-100f, new Coords(6f, 1f), new Coords(35f, 0f), 100f);
			//IMembershipFunction coastFx = new TriangularMembershipFunction(new Coords(33f, 0f), new Coords(40f, 1f), new Coords(45f, 0f));
			//IMembershipFunction fastFx = new ShoulderMembershipFunction(-100f, new Coords(45f, 0f), new Coords(95f, 1f), 100f);


			FuzzySet<SpeedState> set = new FuzzySet<SpeedState>();
			set.Set(new FuzzyVariable<SpeedState>(SpeedState.Slow, slowFx));
			set.Set(new FuzzyVariable<SpeedState>(SpeedState.Coast, coastFx));
			set.Set(new FuzzyVariable<SpeedState>(SpeedState.Fast, fastFx));
			return set;
		}


		private FuzzySet<SteeringDirection> GetCurrentSteeringSet()
		{
			//new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			IMembershipFunction leftFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			//IMembershipFunction leftSlightFx = new TrapezoidMembershipFunction(new Coords(-0.1f, 0f), new Coords(-0.05f, 1f), new Coords(-.02f, 1f), new Coords(0f, 0f));
			IMembershipFunction middleFx = new TrapezoidMembershipFunction(new Coords(-0.33f, 0f), new Coords(-0.25f, 1f), new Coords(0.25f, 1f), new Coords(0.33f, 0f));
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


		private FuzzySet<CurrentDirection> GetSteeringSet()
		{
			//new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			IMembershipFunction leftFx = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(0f, 0f), 1f);
			//IMembershipFunction leftSlightFx = new TrapezoidMembershipFunction(new Coords(-0.1f, 0f), new Coords(-0.05f, 1f), new Coords(-.02f, 1f), new Coords(0f, 0f));
			IMembershipFunction middleFx = new TrapezoidMembershipFunction(new Coords(-0.25f, 0f), new Coords(-0.22f, 1f), new Coords(0.22f, 1f), new Coords(0.25f, 0f));
			//IMembershipFunction rightSlightFx = new TrapezoidMembershipFunction(new Coords(0f, 0f), new Coords(.02f, 1f), new Coords(.05f, 1f), new Coords(0.10f, 0f));
			IMembershipFunction rightFx = new ShoulderMembershipFunction(-1f, new Coords(0f, 0f), new Coords(1f, 1f), 1f);

			FuzzySet<CurrentDirection> set = new FuzzySet<CurrentDirection>(); // Trying opposite left and right
			set.Set(new FuzzyVariable<CurrentDirection>(CurrentDirection.Negative, leftFx));
			//set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Middle, leftSlightFx));
			set.Set(new FuzzyVariable<CurrentDirection>(CurrentDirection.Middle, middleFx));
			//set.Set(new FuzzyVariable<SteeringDirection>(SteeringDirection.Middle, rightSlightFx));
			set.Set(new FuzzyVariable<CurrentDirection>(CurrentDirection.Positive, rightFx));
			return set;
		}


		private FuzzySet<NearState> GetnearSet()
		{

			IMembershipFunction leftFx = new ShoulderMembershipFunction(-15.5f, new Coords(-15f, 0f), new Coords(-12f, 1f), 0f);
			
			//new TrapezoidMembershipFunction(new Coords(0.0f, 0f), new Coords(2.00f, 1f), new Coords(6f, 1f), new Coords(10.5f, 0f));


			//IMembershipFunction middleFx1 = new TrapezoidMembershipFunction(new Coords(-35.5f, 1f), new Coords(-12.5f, 0f), new Coords(12.5f, 0f), new Coords(35.5f, 1f));


			//	new ShoulderMembershipFunction(-50f, new Coords(-15f, 1f), new Coords(-12f, 0f), 50f);
			//IMembershipFunction middleFx2 = new ShoulderMembershipFunction(-50f, new Coords(12f, 0f), new Coords(15f, 1f), 50f);
			IMembershipFunction rightFx = new ShoulderMembershipFunction(12f, new Coords(12f, 0f), new Coords(15f, 1f), 15.5f);

			//new TrapezoidMembershipFunction(new Coords(-12.5f, 0f), new Coords(-7f, 1f), new Coords(1.00f, 1f), new Coords(0.0f, 0f));


			FuzzySet<NearState> set = new FuzzySet<NearState>();
			set.Set(new FuzzyVariable<NearState>(NearState.Left, leftFx));
			//set.Set(new FuzzyVariable<NearState>(NearState.Middle, middleFx1));
			//set.Set(new FuzzyVariable<NearState>(NearState.Middle, middleFx2));
			set.Set(new FuzzyVariable<NearState>(NearState.Right, rightFx));
			return set;
		}


		private FuzzySet<FutureState> GetfutureSet()
		{

			IMembershipFunction leftFx = new TrapezoidMembershipFunction(new Coords(-30.5f, 0f), new Coords(-12f, 1f), new Coords(-2.2f, 1f), new Coords(0.0f, 0f));
			IMembershipFunction rightFx = new TrapezoidMembershipFunction(new Coords(0f, 0f), new Coords(2.2f, 1f), new Coords(12f, 1f), new Coords(25.5f, 0f));
			
			FuzzySet<FutureState> set = new FuzzySet<FutureState>();
			set.Set(new FuzzyVariable<FutureState>(FutureState.Left, leftFx));
			
			set.Set(new FuzzyVariable<FutureState>(FutureState.Right, rightFx));
			return set;
		}



		private FuzzyRule<SteeringDirection>[] GetDirectionRules()
		{
			FuzzyRule<SteeringDirection>[] rules = new FuzzyRule<SteeringDirection>[3];
			// rule 0: if the signed distance from road center negative, turn left


			//rules[0] = NearState.Left.Expr().Then(SteeringDirection.Left); ;// if curve ahead near is left , turn left
			//rules[1] =
			//NearState.Middle.Expr().Then(SteeringDirection.Middle);

			//rules[1] =  NearState.Right.Expr().Then(SteeringDirection.Right);

			rules[0] = CurrentDirection.Negative.Expr().Then(SteeringDirection.Left);
			rules[1] = CurrentDirection.Middle.Expr().Then(SteeringDirection.Middle); //mid
			rules[2] = CurrentDirection.Positive.Expr().Then(SteeringDirection.Right);

			//rules[6] = NearState.Right.Expr().And(FutureState.Right.Expr()).Then(SteeringDirection.Right); // if curve ahead in 2 secs is right, turn  right

			//rules[7] = NearState.Left.Expr().And(FutureState.Left.Expr()).Then(SteeringDirection.Left);


			//rules[8] = NearState.Right.Expr().Then(SteeringDirection.Right); // if curve ahead in 2 secs is right, turn  right

			//rules[9] = NearState.Left.Expr().Then(SteeringDirection.Left);

			return rules;
		}


		 
		private FuzzyRule<SpeedState>[] GetSpeedRules()
		{
			FuzzyRule<SpeedState>[] rules = new FuzzyRule<SpeedState>[14];
			rules[0] = SpeedState.Slow.Expr().Then(SpeedState.Fast);
			rules[1] = SpeedState.Coast.Expr().Then(SpeedState.Coast);
			rules[2] = SpeedState.Fast.Expr().Then(SpeedState.Coast);
			rules[3] = NearState.Left.Expr().And(SpeedState.Fast.Expr()).Then(SpeedState.Coast);
			rules[4] = NearState.Left.Expr().And(SpeedState.Slow.Expr()).Then(SpeedState.Coast);
			rules[5] = NearState.Right.Expr().And(SpeedState.Fast.Expr()).Then(SpeedState.Coast);
			rules[6] = NearState.Right.Expr().And(SpeedState.Slow.Expr()).Then(SpeedState.Coast);
			rules[7] = NearState.Middle.Expr().And(SpeedState.Slow.Expr()).Then(SpeedState.Fast);
			rules[8] = CurrentDirection.Middle.Expr().Then(SpeedState.Coast);
			rules[9] = CurrentDirection.Middle.Expr().And(NearState.Middle.Expr()).Then(SpeedState.Fast); // todo put fast
			rules[10] = AngleState.Medium.Expr().And(SpeedState.Slow.Expr()).Then(SpeedState.Fast);
			rules[11] = AngleState.Sharp.Expr().And(NearState.Left.Expr()).And(SpeedState.Fast.Expr()).Then(SpeedState.Slow);
			rules[12] = AngleState.Sharp.Expr().And(NearState.Right.Expr()).And(SpeedState.Fast.Expr()).Then(SpeedState.Slow);
			rules[13] = AngleState.Sharp.Expr().Then(SpeedState.Coast);
			return rules;
		}

		private FuzzyRuleSet<SteeringDirection> GetDirectionRuleSet(FuzzySet<SteeringDirection> steer)
		{
			var rules = this.GetDirectionRules();
			return new FuzzyRuleSet<SteeringDirection>(steer, rules);
		}



		private FuzzyRuleSet<SpeedState> GetSpeedRuleSet(FuzzySet<SpeedState> throttle)
		{
			var rules = this.GetSpeedRules();
			return new FuzzyRuleSet<SpeedState>(throttle, rules);
		}


		protected override void Awake()
		{
			base.Awake();

			StudentName = "Z W";

			// Only the AI can control. No humans allowerd!
			IsPlayer = false;

			//initialize a bunch of Fuzzy stuff here
			steeringDirectionSet = this.GetSteeringSet();
			currentCarSignedDistanceSet = this.GetCurrentSteeringSet();
			nearSet = this.GetnearSet();
			futureSet = this.GetfutureSet();
			steeringDirectionRuleSet = this.GetDirectionRuleSet(currentCarSignedDistanceSet);
			angleSet = this.GetAngleSet();


			speedSet = this.GetSpeedSet();
			speedRulesSet = this.GetSpeedRuleSet(speedSet);

			inputs = new FuzzyValueSet();
		}


		override protected void Update()
		{

			// Do all your Fuzzy stuff here and pass the defuzzified values to 
			// the car like so:
			// Throttle = someValue; //[-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
			// Steering = someValue; // [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right
	
			//Throttle = .40f; //.22f
			//Steering = 0f;



			Vector3 carPosition = transform.position;

			Vector3 delta_vector = transform.position - pathTracker.closestPointOnPath;
			float angle = Vector3.SignedAngle(delta_vector, pathTracker.closestPointDirectionOnPath, Vector3.up);
			//Vector3 distanceFromRoad;
			//distanceFromRoad = Mathf.Sign(angle) * (carPosition - pathTracker.closestPointOnPath);
			float distance = Mathf.Sign(angle) * ((carPosition - pathTracker.closestPointOnPath) / 5).magnitude;



			steeringDirectionSet.Evaluate(distance, inputs);
			currentCarSignedDistanceSet.Evaluate(distance, inputs);
			

			float dist = pathTracker.distanceTravelled + this.Speed * 5.50f;
			//Debug.Log("distance: " + pathTracker.pathCreator.path.GetPointAtDistance(dist));
			float curveAhead = Vector3.SignedAngle(transform.forward, pathTracker.pathCreator.path.GetPointAtDistance(dist) - transform.position, Vector3.up);
			float curveDistance = Mathf.Sign(curveAhead) * pathTracker.pathCreator.path.GetPointAtDistance(dist).magnitude;
			
			futureSet.Evaluate(curveDistance, inputs);
			angleSet.Evaluate(Mathf.Abs(curveAhead), inputs);


			//Debug.Log("Before calculation speed: " + this.Speed);
			speedSet.Evaluate(this.Speed, inputs);
			Throttle = speedRulesSet.Evaluate(this.Speed, inputs); 
																  
			float dist2 = pathTracker.distanceTravelled + this.Speed * 0.37f;
			//Debug.Log("Difference distance btwn car and road future " + (transform.position - pathTracker.pathCreator.path.GetPointAtDistance(dist)).magnitude);
			//Debug.Log("Difference distance btwn car and road near " + (transform.position - pathTracker.pathCreator.path.GetPointAtDistance(dist2)).magnitude);

			//Debug.Log("Difference distance btwn car and road future DIRECTION " + (transform.position - pathTracker.pathCreator.path.GetDirectionAtDistance(dist)));
			//Debug.Log("Difference distance btwn car and road near DIRECTION " + (transform.position - pathTracker.pathCreator.path.GetDirectionAtDistance(dist2)));

			float curveAhead2 = Vector3.SignedAngle(transform.forward, pathTracker.pathCreator.path.GetPointAtDistance(dist2) - transform.position, Vector3.up);


			float curveDistance2 = Mathf.Sign(curveAhead2) * pathTracker.pathCreator.path.GetPointAtDistance(dist2).magnitude;
			nearSet.Evaluate(curveDistance2, inputs);
			if (curveAhead > 0)
			{
				//distanceFromRoad = Mathf.Sign(angle) * (carPosition - pathTracker.closestPointOnPath);
				Debug.Log("curve on right side future  : " + curveAhead);
			}
			else
			{
				//distanceFromRoad = -1 * (carPosition - pathTracker.closestPointOnPath);
				Debug.Log("on left  side future  :  " + curveAhead);

			}

			if (curveAhead2 > 0)
			{
				//distanceFromRoad = Mathf.Sign(angle) * (carPosition - pathTracker.closestPointOnPath);
				Debug.Log("curve on right side NEAR :  " + curveAhead2);
			}
			else
			{
				//distanceFromRoad = -1 * (carPosition - pathTracker.closestPointOnPath);
				Debug.Log("on left side NEAR:  " + curveAhead2);

			}



			Steering = steeringDirectionRuleSet.Evaluate(distance, inputs);


			DB_throttle = Throttle;
			DB_steering = Steering;



			

			base.Update();


		}

	}
}
