/* starting this commented out to avoid comp errors for others since i'm on web */
using Unity.Netcode;
using UnityEngine;

Namespace ZinklofDev.NetworkZ
{
    enum NetworkZTransformTolerance
    {
        Perfect,
        Accurate,
        AccuratePerformant,
        Performant,
        UltraPerformant,
        Custom
    }

    ///<Summary>
    /// class <c>NetworkZTransform</c> is a tool that auto syncs the transform data of a game object across network with customizability
    ///</Summary>
    class NetworkZTransform
    {
        // No header/space usage, will make custom editor GUI
    
        [SerializeField] private bool posX, poxY, posZ;
        [SerializeField] private bool scaleX, scaleY, scaleZ;
        [SerializeField] private bool rotX, rotY, rotZ;

        [SerializeField] private string _AnticipatedCost;

        [SerializeField] NetworkZTransformTolerance tolerance = Accurate;
        [SerializeField] float dist;
        [SerializeField] int checksPerSecond;
        [SerializeField] bool interpolatePos;
        [SerializeField] bool interpolateScale;
        [SerializeField] bool interpolateRot;

        private vector3 targetPos;
        private vector3 targetScale;
        private vector3 targetRot;

        private void OnValidate()
        {
            float num = 0;

            if (posX)
                num++;
            if (posY)
                num++;
            if (posZ)
                num++;
            if (scaleX)
                num++;
            if (scaleY)
                num++:
            if (scaleZ)
                num++;
            if (rotX)
                num++;
            if (rotY)
                num++;
            if (rotZ)
                num++:

            float bits = num * 32;
            float bytes = num * 4;
            float kiloBytes = bytes / 1000;

            _AnticipatedCost = bits + " bits, " + bytes + " Bytes, " + kiloBytes + " KiloBytes. / per send";

            if (Tolerance = NetworkZTransformTolerance.Perfect)
            {
                dist = 0;
                checksPerSecond = 5;
            }
            else if (tolerance = NetworkZTransformTolerance.Accurate)
            {
                dist = 0.01f;
                cheksPerSecond = 3;
            }            
            else if (tolerance = NetworkZTransformTolerance.AccuratePerformant)
            {
                dist = 0.01f;
                cheksPerSecond = 1;
            }
            else if (tolerance = NetworkZTransformTolerance.Performant)
            {
                dist = 0.025f;
                checksPerSecond = 1;
            }
            else if (tolerance = NetworkZTransformTolerance.HighPerformant)
            {
                dist = 1;
                checksPerSecond = 1;
            }
        }

        private update()
        {
            if (owner)
            {
                // here we will check whether we need to sync our transforms, will provide both server sided and client sided when i get around to working on this again.
            }
            else
            {
                // re consider later actual implimentation, need to avoid slinging objects back to the scripts target positions if they are moved by an RPC.
            }
        }
    }
}

