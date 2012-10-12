using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RealTimeStrategyEngine.Processor
{
    class Unit
    {
        public static double DefaultHP = 100.0;
        public static double DefaultDamage = 2.0;
        public static float DefaultSpeed = 25.0f;
        public static float DefaultRange = 500.0f;
        public enum StatDistribution
        {
            Default
            , Health
            , Range
            , Damage
            , Speed
            , End
        }

        //Variables that determine the Unit's status
        public Vector3 Position;
        public float[] IndexablePosition = new float[3];
        Vector3 TargetPosition;
        Vector3 Velocity;
        public float Speed;
        public double HitPoints;
        public double Damage;
        public float Range;

        //Variables that determine the Unit's processes
        public Unit Target;
        float Distance;
        double AttackTimer = 0.1;
        double DirectionTimer = 0.1;
        Unit OtherTarget;

        public Unit(Vector3 Position, StatDistribution Specialization, BehaviorType PrimitiveAI)
        {
            this.Position = Position;
            IndexablePosition[0] = Position.X;
            IndexablePosition[1] = Position.Y;
            IndexablePosition[2] = Position.Z;
            this.TargetPosition = Position;

            this.HitPoints = DefaultHP;
            this.Damage = DefaultDamage;
            this.Speed = DefaultSpeed;
            this.Range = DefaultRange;
            switch (Specialization)
            {
                case StatDistribution.Health:
                    this.HitPoints *= 2.0;
                    this.Speed *= 0.5f;
                    break;
                case StatDistribution.Range:
                    this.Range *= 1.5f;
                    this.Damage *= 0.5;
                    break;
                case StatDistribution.Damage:
                    this.Damage *= 2.0;
                    this.HitPoints *= 0.5;
                    break;
                case StatDistribution.Speed:
                    this.Speed *= 2.0f;
                    this.Range *= 0.75f;
                    break;
                case StatDistribution.Default:
                default:
                    break;
            }

            switch (PrimitiveAI)
            {
                case BehaviorType.BuddyUp:
                    this.Update = Behavior_BuddyUp;
                    break;
                case BehaviorType.Default:
                default:
                    this.Update = Behavior_Default;
                    break;
            }
        }

        /// <summary>
        /// Holds a particular method of the Unit that determines its AI
        /// </summary>
        public delegate void Behavior(GameTime gameTime, List<Unit_KDTree> EnemyTrees, Unit_KDTree AllyTree);
        public enum BehaviorType
        {
            Default
            , BuddyUp
            , End
        }
        /// <summary>
        /// By default, the Unit moves randomly and targets the closest enemy Unit
        /// </summary>
        private void Behavior_Default(GameTime gameTime, List<Unit_KDTree> EnemyTrees, Unit_KDTree AllyTree)
        {
            DirectionTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (DirectionTimer <= 0.0)
            {
                DirectionTimer = 5.0;
                TargetPosition = Manager.terrain.GetRandomPosition();
            }
            Move(gameTime);
            Attack(gameTime, EnemyTrees);
        }
        /// <summary>
        /// With BuddyUp, the Unit moves (with a bias) towards the first ally Unit it encounters
        /// The unit still targets the closest enemy Unit
        /// </summary>
        private void Behavior_BuddyUp(GameTime gameTime, List<Unit_KDTree> EnemyTrees, Unit_KDTree AllyTree)
        {
            DirectionTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (DirectionTimer <= 0.0)
            {
                DirectionTimer = 3.0;
                TargetPosition = Manager.terrain.GetRandomPosition();
            }
            if (OtherTarget == null || OtherTarget.HitPoints <= 0.0)
            {
                OtherTarget = null;
                Distance = float.PositiveInfinity;
                AllyTree.FindNearestUnit(this, ref OtherTarget, ref Distance);
            }
            else
            {
                TargetPosition = Vector3.Lerp(TargetPosition, OtherTarget.Position, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            Move(gameTime);
            Attack(gameTime, EnemyTrees);
        }
        public Behavior Update;

        private void Move(GameTime gameTime)
        {
            Vector3 NonNormalDirection = TargetPosition - Position;
            float DistanceToTarget = NonNormalDirection.Length();
            if (DistanceToTarget > 1.0f)
            {
                Velocity += Speed * NonNormalDirection / DistanceToTarget * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                Velocity *= 1.0f - (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            //Adjust position and speed by elevation
            Vector3 previousPosition = Position;
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float Ground = Manager.terrain.FindElevation(Position);
            if (Position.Y < Ground)
            {
                Position.Y = Ground;
                float retainedMomentum = Vector3.Dot(Vector3.Normalize(Velocity), Vector3.Normalize(Position - previousPosition));
                if (!float.IsNaN(retainedMomentum))
                {
                    float originalSpeed = Velocity.Length();
                    Velocity *= retainedMomentum;
                    Velocity.Y += 0.5f * originalSpeed * (1.0f - retainedMomentum);
                }
            }
            if (Position.X <= 0)
            {
                Position.X = 0;
                if (Velocity.X < 0)
                {
                    Velocity.X *= -0.5f;
                }
            }
            else if (Position.X >= Manager.terrain.Scale * Manager.terrain.GridWidth)
            {
                Position.X = Manager.terrain.Scale * Manager.terrain.GridWidth;
                if (Velocity.X > 0)
                {
                    Velocity.X *= -0.5f;
                }
            }
            if (Position.Z <= 0)
            {
                Position.Z = 0;
                if (Velocity.Z < 0)
                {
                    Velocity.Z *= -0.5f;
                }
            }
            else if (Position.Z >= Manager.terrain.Scale * Manager.terrain.GridHeight)
            {
                Position.Z = Manager.terrain.Scale * Manager.terrain.GridHeight;
                if (Velocity.Z > 0)
                {
                    Velocity.Z *= -0.5f;
                }
            }
            IndexablePosition[0] = Position.X;
            IndexablePosition[1] = Position.Y;
            IndexablePosition[2] = Position.Z;
        }
        private void Attack(GameTime gameTime, List<Unit_KDTree> EnemyTrees)
        {
            AttackTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (AttackTimer <= 0.0)
            {
                AttackTimer = 0.1;
                if (Target == null || Target.HitPoints <= 0.0 || (Target.Position - Position).Length() > Range)
                {
                    Target = null;
                    Distance = Range;
                    for (int i = 0; i < EnemyTrees.Count; i++)
                    {
                        EnemyTrees[i].FindNearestUnit(this, ref Target, ref Distance);
                    }
                    if (Target != null)
                    {
                        Target.HitPoints -= Damage;
                    }
                }
                else
                {
                    Target.HitPoints -= Damage;
                }
            }
        }
    }
}
