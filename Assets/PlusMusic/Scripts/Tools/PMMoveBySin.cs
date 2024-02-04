
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlusMusic
{
    public class PMMoveBySin : MonoBehaviour
    {

        public bool useX = false;
        [Range(0, 10)]
        public float xspeed = 0.0f;
        [Range(0, 10)]
        public float xdistance = 0.0f;

        public bool useY = true;
        [Range(0,10)]
        public float yspeed = 1.0f;
        [Range(0, 10)]
        public float ydistance = 1.0f;

        public bool useZ = false;
        [Range(0, 10)]
        public float zspeed = 0.0f;
        [Range(0, 10)]
        public float zdistance = 0.0f;


        private Vector3 myLocation;

        // Start is called before the first frame update
        void Start()
        {
            myLocation = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (useX)
                myLocation.x = xdistance * Mathf.Sin(Time.time * xspeed);
            if (useY)
                myLocation.y = ydistance * Mathf.Sin(Time.time * yspeed);
            if (useZ)
                myLocation.z = zdistance * Mathf.Sin(Time.time * zspeed);
            transform.position = myLocation;
        }
    }
}
