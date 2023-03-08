namespace Kenedia.Modules.Core.Utility
{
    public static class Tweening
    {
        public static class Quartic
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="t"></param> 
            /// <param name="b"></param>
            /// <param name="c"></param>
            /// <param name="d"></param>
            /// <returns></returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return c * (t /= d) * t * t * t + b;
            }
            public static float EaseOut(float t, float b, float c, float d)
            {
                return -c * ((t = t / d - 1) * t * t * t - 1) + b;
            }
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if ((t /= d / 2) < 1)
                {
                    return (c / 2 * t * t * t * t) + b;
                }
                return (-c / 2 * (((t -= 2) * t * t * t) - 2)) + b;
            }
            public static float EaseOutBounce(float currentTime, float startValue, float changeInValue, float totalTime)
            {
                float magic1 = 7.5625f;
                float magic2 = 2.75f;
                float magic3 = 1.5f;
                float magic4 = 2.25f;
                float magic5 = 2.625f;
                float magic6 = 0.75f;
                float magic7 = 0.9375f;
                float magic8 = 0.984375f;

                if ((currentTime /= totalTime) < (1 / magic2)) //0.36%
                {
                    return changeInValue * (magic1 * currentTime * currentTime) + startValue;
                }
                else if (currentTime < (2 / magic2)) //0.72%
                {
                    return changeInValue * (magic1 * (currentTime -= (magic3 / magic2)) * currentTime + magic6) + startValue;
                }
                else if (currentTime < (2.5 / magic2)) //0.91%
                {
                    return changeInValue * (magic1 * (currentTime -= (magic4 / magic2)) * currentTime + magic7) + startValue;
                }
                else
                {
                    return changeInValue * (magic1 * (currentTime -= (magic5 / magic2)) * currentTime + magic8) + startValue;
                }
            }
        }
    }
}
