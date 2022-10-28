using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class CustomAnimator : MonoBehaviour
    {

        private float animationStartDifference;
        class CustomAnimation
        {

            internal Vector3 oldPosition { get; set; }
            internal Vector3 oldLocalPosition { get; set; }
            internal Vector3 newPosition { get; private set; }
            internal float animationTime { get; private set; }
            internal bool local { get; private set; }

            public CustomAnimation(Vector3 oldPosition, Vector3 oldLocalPosition, Vector3 newPosition, float animationTime, bool local)
            {
                this.oldPosition = oldPosition;
                this.oldLocalPosition = oldLocalPosition;
                this.newPosition = newPosition;
                this.animationTime = animationTime;
                this.local = local;
            }
        }

        private Queue<CustomAnimation> animations;

        private CustomAnimation currentAnimation;
        // Start is called before the first frame update
        void Start()
        {
            if (animations == null)
            {
                animations = new Queue<CustomAnimation>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (currentAnimation != null)
            {
                float interp = Mathf.Min(animationStartDifference / currentAnimation.animationTime, 1);
                Vector3 interpolPos;
                if (currentAnimation.local)
                {
                    interpolPos = Vector3.Lerp(currentAnimation.oldLocalPosition, currentAnimation.newPosition, interp);
                    transform.localPosition = interpolPos;
                }
                else
                {
                    interpolPos = Vector3.Lerp(currentAnimation.oldPosition, currentAnimation.newPosition, interp);
                    transform.position = interpolPos;
                }
                if (animationStartDifference > currentAnimation.animationTime) currentAnimation = null;
                animationStartDifference += Time.deltaTime;
            }
            else
            {
                animationStartDifference = 0;
                if (animations.Count > 0)
                {
                    currentAnimation = animations.Dequeue();
                    currentAnimation.oldLocalPosition = transform.localPosition;
                    currentAnimation.oldPosition = transform.position;
                }
            }


        }

        public void AddAnimation(Vector3 newPosition, float animationTime, bool local)
        {
            if (animations == null)
            {
                animations = new Queue<CustomAnimation>();
            }
            animations.Enqueue(new CustomAnimation(transform.position, transform.localPosition, newPosition, animationTime, local));
        }
    }
}
