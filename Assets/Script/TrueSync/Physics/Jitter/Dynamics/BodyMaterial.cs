namespace TrueSync.Physics3D {

    /**
     *  @brief Represents physical properties of a {@link RigidBody}. 
     *  表示{刚体}的物理属性。 
     **/
    public class BodyMaterial {

        internal FP kineticFriction = FP.One / 4;
        internal FP staticFriction = FP.One / 2;
        internal FP restitution = FP.Zero;

        public BodyMaterial() { }

        /**
         *  @brief Elastic restituion. 
         *  弹性恢复(回弹性)
         **/
        public FP Restitution {
            get { return restitution; }
            set { restitution = value; }
        }

        /**
         *  @brief Static friction. 
         *  静摩擦力 
         **/
        public FP StaticFriction {
            get { return staticFriction; }
            set { staticFriction = value; }
        }

        /**
         *  @brief Kinectic friction. 
         *  动摩擦 
         **/
        public FP KineticFriction {
            get { return kineticFriction; }
            set { kineticFriction = value; }
        }

    }

}