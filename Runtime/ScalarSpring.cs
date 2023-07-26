namespace AggroBird.UnityExtend
{
    // Source: http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/

    public struct ScalarSpring
    {
        public ScalarSpring(float target, float zeta, float omega, float current = 0, float velocity = 0)
        {
            this.current = current;
            this.velocity = velocity;
            this.target = target;
            this.zeta = zeta;
            this.omega = omega;
        }

        public float current;
        public float velocity;
        public float target;
        public float zeta;
        public float omega;

        public void Update(float delta)
        {
            float f = 1.0f + 2.0f * delta * zeta * omega;
            float oo = omega * omega;
            float hoo = delta * oo;
            float hhoo = delta * hoo;
            float detInv = 1.0f / (f + hhoo);
            float detX = f * current + delta * velocity + hhoo * target;
            float detV = velocity + hoo * (target - current);
            current = detX * detInv;
            velocity = detV * detInv;
        }
    }
}