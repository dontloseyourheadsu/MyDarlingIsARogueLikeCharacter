using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RoguelikeDarling.Core.Skeleton2D
{
    public sealed class Skeleton2DBase
    {
        private readonly List<SkeletonMemberDefinition2D> members;
        private readonly Dictionary<string, SkeletonMemberDefinition2D> membersByName;
        private readonly Dictionary<string, SkeletonMemberPose2D> poses;

        public Skeleton2DBase()
        {
            members = new List<SkeletonMemberDefinition2D>();
            membersByName = new Dictionary<string, SkeletonMemberDefinition2D>(StringComparer.Ordinal);
            poses = new Dictionary<string, SkeletonMemberPose2D>(StringComparer.Ordinal);
        }

        public IReadOnlyList<SkeletonMemberDefinition2D> Members => members;

        public void RegisterMember(SkeletonMemberDefinition2D definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Name))
            {
                throw new ArgumentException("Invalid skeleton member definition.");
            }

            if (membersByName.ContainsKey(definition.Name))
            {
                throw new InvalidOperationException($"Skeleton member '{definition.Name}' is already registered.");
            }

            if (!string.IsNullOrWhiteSpace(definition.ParentName) && !membersByName.ContainsKey(definition.ParentName))
            {
                throw new InvalidOperationException($"Parent member '{definition.ParentName}' must be registered before child '{definition.Name}'.");
            }

            members.Add(definition);
            membersByName[definition.Name] = definition;
            poses[definition.Name] = new SkeletonMemberPose2D();
        }

        public bool TryGetDefinition(string memberName, out SkeletonMemberDefinition2D definition)
        {
            return membersByName.TryGetValue(memberName, out definition);
        }

        public bool TryGetPose(string memberName, out SkeletonMemberPose2D pose)
        {
            return poses.TryGetValue(memberName, out pose);
        }

        public void SetPose(string memberName, Vector2 localJointOffset, float localRotation, Vector2 localScale)
        {
            if (!poses.TryGetValue(memberName, out SkeletonMemberPose2D pose) || pose == null)
            {
                throw new InvalidOperationException($"Skeleton member '{memberName}' is not registered.");
            }

            pose.LocalJointOffset = localJointOffset;
            pose.LocalRotation = localRotation;
            pose.LocalScale = localScale;
        }

        public void SetPose(string memberName, Vector2 localJointOffset, float localRotation)
        {
            SetPose(memberName, localJointOffset, localRotation, Vector2.One);
        }

        public void ResetAllPoses()
        {
            foreach (KeyValuePair<string, SkeletonMemberPose2D> entry in poses)
            {
                SkeletonMemberPose2D pose = entry.Value;
                pose.LocalJointOffset = Vector2.Zero;
                pose.LocalRotation = 0f;
                pose.LocalScale = Vector2.One;
            }
        }

        public IReadOnlyDictionary<string, SkeletonMemberWorldPose2D> EvaluateWorldPoses(Vector2 rootPosition, Vector2 rootScale)
        {
            var worldPoses = new Dictionary<string, SkeletonMemberWorldPose2D>(members.Count, StringComparer.Ordinal);

            for (int i = 0; i < members.Count; i++)
            {
                SkeletonMemberDefinition2D definition = members[i];
                SkeletonMemberPose2D pose = poses[definition.Name];

                Vector2 jointPosition;
                float worldRotation;

                if (!string.IsNullOrWhiteSpace(definition.ParentName))
                {
                    SkeletonMemberWorldPose2D parentPose = worldPoses[definition.ParentName];
                    Vector2 jointOffset = definition.BindJointOffset + pose.LocalJointOffset;
                    Vector2 scaledJointOffset = new Vector2(jointOffset.X * rootScale.X, jointOffset.Y * rootScale.Y);

                    jointPosition = parentPose.JointWorldPosition + Rotate(scaledJointOffset, parentPose.WorldRotation);
                    worldRotation = parentPose.WorldRotation + pose.LocalRotation;
                }
                else
                {
                    Vector2 rootJointOffset = definition.BindJointOffset + pose.LocalJointOffset;
                    jointPosition = rootPosition + new Vector2(rootJointOffset.X * rootScale.X, rootJointOffset.Y * rootScale.Y);
                    worldRotation = pose.LocalRotation;
                }

                Vector2 worldScale = new Vector2(rootScale.X * pose.LocalScale.X, rootScale.Y * pose.LocalScale.Y);
                worldPoses[definition.Name] = new SkeletonMemberWorldPose2D(definition, jointPosition, worldRotation, worldScale);
            }

            return worldPoses;
        }

        private static Vector2 Rotate(Vector2 value, float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            return new Vector2(
                (value.X * cos) - (value.Y * sin),
                (value.X * sin) + (value.Y * cos));
        }
    }
}
