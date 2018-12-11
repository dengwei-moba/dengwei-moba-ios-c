using UnityEngine;
using TrueSync.Physics3D;

namespace TrueSync {

    /**
    *  @brief Represents information about a contact point
    **/
    public class TSContactPoint {

        /**
        *  @brief Contact point between two bodies
         *  DV 两个物体间的接触点 
        **/
        public TSVector point;
        public TSVector point2;

        /**
        *  @brief Normal vector from the contact point
         *  DV 接触点法线向量 
        **/
        public TSVector normal;
        public FP Penetration;
    }

    /**
    *  @brief Represents information about a contact between two 3D bodies
    *  DV 表示关于两个3D体之间的接触的信息。 
    **/
    public class TSCollision {

        /**
        *  @brief An array of {@link TSContactPoint}
        **/
        public TSContactPoint[] contacts = new TSContactPoint[1];

        /**
        *  @brief {@link TSCollider} of the body hit
        **/
        public TSCollider collider;

        /**
        *  @brief GameObject of the body hit
        **/
        public GameObject gameObject;

        /**
        *  @brief {@link TSRigidBody} of the body hit, if there is one attached
        **/
        public TSRigidBody rigidbody;

        /**
        *  @brief {@link TSTransform} of the body hit
        **/
        public TSTransform transform;

        /**
        *  @brief The {@link TSTransform} of the body hit
        **/
        public TSVector relativeVelocity;

        internal void Update(GameObject otherGO, Contact c) {
            if (this.gameObject == null) {
                this.gameObject = otherGO;
                this.collider = this.gameObject.GetComponent<TSCollider>();
                this.rigidbody = this.gameObject.GetComponent<TSRigidBody>();
                this.transform = this.collider.tsTransform;
            }

            if (c != null)
            {
                if (contacts[0] == null)
                {
                    contacts[0] = new TSContactPoint();
                }

                this.relativeVelocity = c.CalculateRelativeVelocity();

                contacts[0].normal = c.Normal;
                contacts[0].point = c.p1;
                contacts[0].point2 = c.p2;
                contacts[0].Penetration = c.Penetration;
            }
        }

    }

}