using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
#if UNITY_DOTSRUNTIME
using Unity.Tiny.Rendering;
using Unity.Tiny.Input;

#endif

namespace TinyPhysics.Systems
{
    /// <summary>
    ///     Update camera position and rotation to follow the player.
    /// </summary>
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    public class UpdateCameraFollow : SystemBase
    {
        private float3 DefaultCameraPosition;
        private quaternion DefaultCameraRot;
        private bool IsDefaultCameraPositionSet;

#if !UNITY_DOTSRUNTIME
        UnityEngine.Transform cameraTransform;
#endif
        protected override void OnStartRunning()
        {
#if !UNITY_DOTSRUNTIME
            cameraTransform = UnityEngine.Camera.main.transform;
#endif
        }

        protected override void OnUpdate()
        {
#if UNITY_DOTSRUNTIME
            // if shift is held, don't do anything
            if (World.GetExistingSystem<InputSystem>().GetKey(KeyCode.LeftShift))
                return;
#endif
            // Get player car position and direction
            var charPosition = float3.zero;
            var charDirection = float3.zero;
            var charRotation = quaternion.identity;
            Entities.WithAny<Moveable>().ForEach(
                //Use ref for components that you write to, 
                //in for components that you only read.
                (in Translation translation, in LocalToWorld localToWorld,
                    in Rotation rotation) =>
                {
                    charPosition = translation.Value;
                    //charDirection = localToWorld.Forward;
                    //charRotation = rotation.Value;
                }).Run();
            // Position the camera behind the car

            var targetPosition = charPosition + new float3(-20f, 24f, -28.6f);

            // TODO: Find camera with entity query once there's a pure component for cameras
#if !UNITY_DOTSRUNTIME
            var cameraPos = (float3)cameraTransform.position;
            //var cameraRot = quaternion.Euler(0f, 0f, 0f);
            var cameraRot = (quaternion)cameraTransform.rotation;
#else
            var cameraEntity = GetSingletonEntity<Camera>();
            var cameraPos = EntityManager.GetComponentData<Translation>(cameraEntity).Value;
            var cameraRot = EntityManager.GetComponentData<Rotation>(cameraEntity).Value;
#endif

            var deltaTime = math.clamp(Time.DeltaTime * 7f, 0, 1);
            deltaTime=1f;

                cameraPos = math.lerp(cameraPos, targetPosition, deltaTime);
                //cameraRot = quaternion.Euler(45f, 45f, 0f);
                //cameraRot = math.slerp(cameraRot, charRotation, deltaTime);

#if !UNITY_DOTSRUNTIME
            cameraTransform.position = cameraPos;
            cameraTransform.rotation = cameraRot;
#else
            EntityManager.SetComponentData(cameraEntity, new Translation {Value = cameraPos});
            EntityManager.SetComponentData(cameraEntity, new Rotation {Value = cameraRot});
#endif

        }
    }
}
