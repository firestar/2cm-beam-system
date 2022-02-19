namespace BeamSystem.Physics
{
    /// <summary>
    /// https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/master/Sources/Sandbox.Game/Engine/Physics/MyPhysics.cs
    /// </summary>
    struct CollisionLayers
    {

        /// <summary>
        /// Layer that works like 'DefaultCollisionLayer' but do not return collision with voxels (ex. Planet ground/asteroid).
        /// </summary>
        public const int NoVoxelCollisionLayer = 9;

        public const int LightFloatingObjectCollisionLayer = 10;

        // Layer that doesn't collide with static grids and voxels
        public const int VoxelLod1CollisionLayer = 11;
        public const int NotCollideWithStaticLayer = 12;
        // Static grids
        public const int StaticCollisionLayer = 13;
        public const int CollideWithStaticLayer = 14;
        public const int DefaultCollisionLayer = 15;
        public const int DynamicDoubledCollisionLayer = 16;
        public const int KinematicDoubledCollisionLayer = 17;
        public const int CharacterCollisionLayer = 18;
        public const int NoCollisionLayer = 19;

        public const int DebrisCollisionLayer = 20;
        public const int GravityPhantomLayer = 21;

        public const int CharacterNetworkCollisionLayer = 22;
        public const int FloatingObjectCollisionLayer = 23;

        public const int ObjectDetectionCollisionLayer = 24;

        public const int VirtualMassLayer = 25;
        public const int CollectorCollisionLayer = 26;

        public const int AmmoLayer = 27;
        public const int VoxelCollisionLayer = 28;
        public const int ExplosionRaycastLayer = 29;
        public const int CollisionLayerWithoutCharacter = 30;

        // TODO: This layer should be removed, when character won't need both CharacterProxy's body with ragdoll enabled at one time i.e. jetpack
        public const int RagdollCollisionLayer = 31;
    }
}
